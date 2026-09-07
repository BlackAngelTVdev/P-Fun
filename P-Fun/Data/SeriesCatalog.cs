using P_Fun.Models;

namespace P_Fun.Data
{
    public static class SeriesCatalog
    {
        public static readonly IReadOnlyList<CryptoSeries> All =
        [
            new CryptoSeries("BTC/USDT", "Bitcoin, la référence du marché"),
            new CryptoSeries("ETH/USDT", "Ethereum, la deuxième plus grosse capitalisation"),
            new CryptoSeries("BNB/USDT", "La monnaie de la plateforme Binance"),
            new CryptoSeries("SOL/USDT", "Solana, une alternative plus récente"),
            new CryptoSeries("XRP/USDT", "Ripple, orienté paiements"),
        ];
    }
}