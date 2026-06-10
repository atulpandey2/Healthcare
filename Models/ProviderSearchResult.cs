namespace HealthcareProviderLocator.Models
{
    /// <summary>
    /// Result of provider search for a patient
    /// </summary>
    public class ProviderSearchResult
    {
        public Provider Provider { get; set; }
        public double Distance { get; set; }
        public int SearchRadius { get; set; }

        public override string ToString()
        {
            return $"Provider: {Provider.Name}, Distance: {Distance} miles, Radius: {SearchRadius} miles";
        }
    }
}
