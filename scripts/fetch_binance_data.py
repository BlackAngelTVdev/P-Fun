#!/usr/bin/env python3
"""Récupère les bougies Binance des 5 séries du projet et remplit data/*.json.

Les fichiers produits gardent le format brut renvoyé par Binance (klines) :
un tableau de bougies, chaque bougie étant un tableau
[openTime, open, high, low, close, volume, closeTime, ...].
C'est exactement le format attendu par JsonFolderImporter / SeriesDatabase.

L'API Binance ne renvoie que 1000 bougies par requête : le script pagine donc
automatiquement avec startTime pour couvrir toute la plage demandée.

Usage :
    python scripts/fetch_binance_data.py
    python scripts/fetch_binance_data.py --start 2026-09-14 --end 2026-09-28
    python scripts/fetch_binance_data.py --start 2026-09-14 --interval 5m
"""

from __future__ import annotations

import argparse
import json
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
from datetime import datetime, timedelta, timezone
from pathlib import Path

API_BASE = "https://api.binance.com/api/v3/klines"

# Paire Binance -> fichier JSON dans le dossier data (les noms existants sont réutilisés).
SERIES: dict[str, str] = {
    "BTCUSDT": "btc.json",
    "ETHUSDT": "eth.json",
    "SOLUSDT": "solu.json",
    "PAXGUSDT": "gold.json",
    "EURUSDT": "eur.json",
}

TIMEOUT_SECONDS = 30
CANDLE_FIELDS = 12  # nombre de champs renvoyés par le endpoint klines
PAGE_SIZE_MAX = 1000  # maximum accepté par l'API
DEFAULT_START = "2026-09-14"
PAUSE_BETWEEN_PAGES = 0.05  # secondes, pour rester loin des limites de débit

# Durée d'un intervalle, en millisecondes (ceux que le projet utilise).
INTERVAL_MS: dict[str, int] = {
    "1m": 60_000,
    "3m": 3 * 60_000,
    "5m": 5 * 60_000,
    "15m": 15 * 60_000,
    "30m": 30 * 60_000,
    "1h": 60 * 60_000,
    "4h": 4 * 60 * 60_000,
    "1d": 24 * 60 * 60_000,
}


def interval_to_ms(interval: str) -> int:
    try:
        return INTERVAL_MS[interval]
    except KeyError:
        raise ValueError(
            f"intervalle inconnu : {interval!r} (supportés : {', '.join(INTERVAL_MS)})"
        ) from None


def parse_date(value: str) -> datetime:
    """Accepte AAAA-MM-JJ ou AAAA-MM-JJTHH:MM (interprété en UTC)."""
    for fmt in ("%Y-%m-%d", "%Y-%m-%dT%H:%M", "%Y-%m-%dT%H:%M:%S"):
        try:
            return datetime.strptime(value, fmt).replace(tzinfo=timezone.utc)
        except ValueError:
            continue

    raise ValueError(f"date invalide : {value!r} (attendu AAAA-MM-JJ ou AAAA-MM-JJTHH:MM)")


def to_millis(moment: datetime) -> int:
    return int(moment.timestamp() * 1000)


def build_url(symbol: str, interval: str, limit: int, start_ms: int, end_ms: int) -> str:
    query = urllib.parse.urlencode(
        {
            "symbol": symbol,
            "interval": interval,
            "limit": limit,
            "startTime": start_ms,
            "endTime": end_ms,
        }
    )
    return f"{API_BASE}?{query}"


def request_page(symbol: str, interval: str, limit: int, start_ms: int, end_ms: int) -> list[list[object]]:
    """Une page de bougies (au plus `limit`), entièrement validée."""
    request = urllib.request.Request(
        build_url(symbol, interval, limit, start_ms, end_ms),
        headers={"User-Agent": "P-Fun/1.0 (fetch_binance_data)"},
    )

    with urllib.request.urlopen(request, timeout=TIMEOUT_SECONDS) as response:
        payload = json.load(response)

    if not isinstance(payload, list):
        raise ValueError(f"{symbol} : réponse inattendue (objet JSON au lieu d'un tableau)")

    for index, candle in enumerate(payload):
        if not isinstance(candle, list) or len(candle) < CANDLE_FIELDS:
            raise ValueError(f"{symbol} : bougie {index} mal formée")

    return payload


def fetch_range(symbol: str, interval: str, start_ms: int, end_ms: int, page_size: int) -> list[list[object]]:
    """Pagine de start_ms à end_ms jusqu'à épuisement des données."""
    step_ms = interval_to_ms(interval)
    candles: dict[int, list[object]] = {}
    cursor = start_ms

    while cursor <= end_ms:
        page = request_page(symbol, interval, page_size, cursor, end_ms)
        if not page:
            break

        candles.update((int(candle[0]), candle) for candle in page)

        next_cursor = int(page[-1][0]) + step_ms

        # Sécurité : si l'API ne progresse plus, on s'arrête plutôt que de boucler.
        if next_cursor <= cursor or len(page) < page_size:
            break

        cursor = next_cursor
        time.sleep(PAUSE_BETWEEN_PAGES)

    return [candles[open_time] for open_time in sorted(candles)]


def write_json(path: Path, candles: list[list[object]]) -> None:
    """Écrit le fichier comme les JSON existants : indenté, encodage UTF-8."""
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="\n") as handle:
        json.dump(candles, handle, indent=4)
        handle.write("\n")


def format_moment(millis: int) -> str:
    return datetime.fromtimestamp(millis / 1000, tz=timezone.utc).strftime("%Y-%m-%d %H:%M")


def parse_args(argv: list[str]) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--start", default=DEFAULT_START, help=f"début de la plage (défaut : {DEFAULT_START})")
    parser.add_argument("--end", default=None, help="fin de la plage (défaut : maintenant)")
    parser.add_argument("--interval", default="1m", help="intervalle des bougies (défaut : 1m)")
    parser.add_argument(
        "--page-size",
        type=int,
        default=PAGE_SIZE_MAX,
        help=f"bougies par requête, max {PAGE_SIZE_MAX} (défaut : {PAGE_SIZE_MAX})",
    )
    parser.add_argument(
        "--out-dir",
        type=Path,
        default=Path(__file__).resolve().parent.parent / "data",
        help="dossier de sortie des JSON (défaut : data/ à la racine du projet)",
    )
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    args = parse_args(argv if argv is not None else sys.argv[1:])

    start = parse_date(args.start)
    end = parse_date(args.end) if args.end else datetime.now(timezone.utc)

    if start >= end:
        print(f"Plage invalide : {args.start} est postérieur à {args.end or 'maintenant'}.", file=sys.stderr)
        return 2

    # On s'arrête à la dernière bougie clôturée : la bougie en cours est incomplète.
    step_ms = interval_to_ms(args.interval)
    end_ms = to_millis(end) - step_ms
    start_ms = to_millis(start)

    print(f"Plage : {start:%Y-%m-%d %H:%M} -> {end:%Y-%m-%d %H:%M} UTC  (interval {args.interval})\n")

    failures: list[str] = []

    for symbol, filename in SERIES.items():
        destination = args.out_dir / filename
        try:
            candles = fetch_range(symbol, args.interval, start_ms, end_ms, args.page_size)
            if not candles:
                raise ValueError("aucune bougie sur cette plage")
            write_json(destination, candles)
        except (
            urllib.error.URLError,
            urllib.error.HTTPError,
            ValueError,
            OSError,
            json.JSONDecodeError,
        ) as error:
            failures.append(f"{symbol} -> {filename} : {error}")
            print(f"[ERREUR] {symbol} -> {filename} : {error}", file=sys.stderr)
            continue

        print(
            f"[OK] {symbol:<8} -> {filename:<10} {len(candles):>6} bougies  "
            f"({format_moment(int(candles[0][0]))} .. {format_moment(int(candles[-1][0]))})"
        )

    if failures:
        print(f"\n{len(failures)} série(s) en échec sur {len(SERIES)}.", file=sys.stderr)
        return 1

    print(f"\n{len(SERIES)} fichiers écrits dans {args.out_dir}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
