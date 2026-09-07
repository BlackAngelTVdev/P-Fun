namespace P_Fun.Models
{
    public sealed record CryptoSeries(string Pair, string Description)
    {
        public override string ToString() => Pair;
    }
}