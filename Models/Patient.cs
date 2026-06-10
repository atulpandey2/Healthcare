namespace HealthcareProviderLocator.Models
{
    /// <summary>
    /// Represents a patient/member with location information
    /// </summary>
    public class Patient
    {
        public string MemberId { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }

        public override string ToString()
        {
            return $"ID: {MemberId}, Name: {Name}, Location: ({Latitude}, {Longitude})";
        }
    }
}
