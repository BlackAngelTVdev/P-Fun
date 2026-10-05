#!/bin/bash
# ============================================
# Export du rapport Markdown vers PDF
# Usage : bash export-pdf.sh
# ============================================
#
# Note : ce fichier doit être en fins de ligne LF, pas CRLF.
# Avec CRLF, le « \r » se colle aux commandes et bash échoue sur
# « exit 1\r » ou « if [ ... ]\r ». Tout le reste du projet peut
# être en CRLF, pas ce script.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
INPUT="$SCRIPT_DIR/rapport.md"
OUTPUT="$SCRIPT_DIR/rapport.pdf"
CSS="$SCRIPT_DIR/style.css"

# --- Prérequis ---
if [ ! -f "$INPUT" ]; then
  echo "❌ Fichier introuvable : $INPUT" >&2
  exit 1
fi

if ! command -v node >/dev/null 2>&1; then
  echo "❌ Node.js est requis (https://nodejs.org) et n'est pas installé." >&2
  exit 1
fi

# --- Arguments md-to-pdf ---
# On garde une seule source de vérité pour la mise en page :
#   --stylesheet       : le style.css, qui porte aussi le @page (marges,
#                       format, pied de page). Le pagination est donc
#                       gérée à un seul endroit.
#   --pdf-options      : uniquement ce que Chrome ne peut pas deviner.
#       printBackground     : sans lui, les fonds de couleur disparaissent
#                            — donc le texte blanc des en-têtes de
#                            tableau sur fond bleu foncé part avec.
#       preferCSSPageSize   : les marges viennent du @page du CSS, pas
#                            d'ici. Évite d'avoir deux configuration
#                            qui se contredisent.
echo "▶ Export du rapport en PDF…"

if [ -f "$CSS" ]; then
  npx --yes md-to-pdf "$INPUT" \
    --stylesheet "$CSS" \
    --pdf-options '{"printBackground":true,"preferCSSPageSize":true}'
else
  echo "⚠️  style.css absent : export sans mise en forme personnalisée."
  npx --yes md-to-pdf "$INPUT" \
    --pdf-options '{"printBackground":true,"preferCSSPageSize":true}'
fi

# --- Récupérer le fichier produit ---
# md-to-pdf peut écrire « rapport.md.pdf » ou « rapport.pdf »
# selon la version : on couvre les deux, sans deviner.
GENERATED=""
for CANDIDATE in "$INPUT.pdf" "$OUTPUT"; do
  if [ -f "$CANDIDATE" ]; then
    GENERATED="$CANDIDATE"
    break
  fi
done

if [ -z "$GENERATED" ]; then
  echo "❌ Échec de la génération du PDF (aucun fichier produit)." >&2
  exit 1
fi

if [ "$GENERATED" != "$OUTPUT" ]; then
  mv "$GENERATED" "$OUTPUT"
fi

# --- Résumé ---
# Taille en bash pur : pas de dépendance à strings/du, qui ne sont pas
# garantis sur toutes les machines (et strings manquait ici).
OCTETS=$(wc -c < "$OUTPUT")

echo "✅ PDF généré : $OUTPUT"
printf '   %s Ko\n' "$((OCTETS / 1024))"