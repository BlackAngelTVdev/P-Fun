namespace P_Fun.Models
{
    /// <summary>
    /// Une série temporelle importée depuis un fichier JSON :
    /// le prix de clôture de chaque bougie, avec son horodatage (ms Unix).
    /// </summary>
    public sealed record PriceSeries(string Name, double[] Timestamps, double[] Closes)
    {
        public override string ToString() => Name;
    }
}
