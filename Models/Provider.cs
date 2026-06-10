namespace HealthcareProviderLocator.Models
{
    /// <summary>
    /// Represents a healthcare provider with location information
    /// </summary>
    public class Provider
    {
        public string ProviderId { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Specialty { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        public override string ToString()
        {
            return $"ID: {ProviderId}, Name: {Name}, Location: ({Latitude}, {Longitude})";
        }
    }
}
