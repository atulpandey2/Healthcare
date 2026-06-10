using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthcareProviderLocator
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
    }

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
    }

    /// <summary>
    /// Result of provider search for a patient
    /// </summary>
    public class ProviderSearchResult
    {
        public Provider Provider { get; set; }
        public double Distance { get; set; }
        public int SearchRadius { get; set; }
    }

    /// <summary>
    /// Distance calculation and provider search utility
    /// </summary>
    public class ProviderDistanceUtility
    {
        private const double EarthRadiusMiles = 3959;

        /// <summary>
        /// Calculates distance between two geographical points using Haversine formula
        /// </summary>
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            if (!IsValidCoordinate(lat1, lon1) || !IsValidCoordinate(lat2, lon2))
            {
                throw new ArgumentException("Invalid coordinates provided");
            }

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a1 = ToRadians(lat1);
            var a2 = ToRadians(lat2);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(a1) * Math.Cos(a2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = EarthRadiusMiles * c;

            return Math.Round(distance, 2);
        }

        /// <summary>
        /// Validates if coordinates are within valid ranges
        /// </summary>
        private static bool IsValidCoordinate(double latitude, double longitude)
        {
            return latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;
        }

        /// <summary>
        /// Converts degrees to radians
        /// </summary>
        private static double ToRadians(double degrees)
        {
            return (degrees * Math.PI) / 180;
        }

        /// <summary>
        /// Finds the nearest provider(s) using progressive radius search
        /// Searches in order: 25 miles → 75 miles → increments of 5 miles up to 175 miles
        /// </summary>
        public static List<ProviderSearchResult> FindNearestProviders(
            List<Provider> providers,
            Patient patient,
            int maxResults = int.MaxValue)
        {
            if (providers == null || providers.Count == 0)
            {
                throw new ArgumentException("Providers list cannot be empty");
            }

            if (patient == null)
            {
                throw new ArgumentException("Patient cannot be null");
            }

            if (!IsValidCoordinate(patient.Latitude, patient.Longitude))
            {
                throw new ArgumentException("Invalid patient location coordinates");
            }

            // Define search radius strategy
            var searchRadii = new List<int> { 25, 75 };

            // Add incremental radii from 80 to 175 miles in 5-mile increments
            for (int radius = 80; radius <= 175; radius += 5)
            {
                searchRadii.Add(radius);
            }

            // Search progressively with increasing radii
            foreach (var radius in searchRadii)
            {
                var foundProviders = FindProvidersInRadius(providers, patient, radius);

                if (foundProviders.Count > 0)
                {
                    return foundProviders.Take(maxResults).ToList();
                }
            }

            // No providers found within maximum radius
            return new List<ProviderSearchResult>();
        }

        /// <summary>
        /// Finds all providers within a specified radius
        /// </summary>
        private static List<ProviderSearchResult> FindProvidersInRadius(
            List<Provider> providers,
            Patient patient,
            int radiusMiles)
        {
            return providers
                .Select(provider => new ProviderSearchResult
                {
                    Provider = provider,
                    Distance = CalculateDistance(
                        patient.Latitude,
                        patient.Longitude,
                        provider.Latitude,
                        provider.Longitude),
                    SearchRadius = radiusMiles
                })
                .Where(result => result.Distance <= radiusMiles)
                .OrderBy(result => result.Distance)
                .ToList();
        }

        /// <summary>
        /// Finds providers within a specified radius
        /// </summary>
        public static List<ProviderSearchResult> FindProvidersWithinRadius(
            List<Provider> providers,
            Patient patient,
            int radiusMiles)
        {
            if (radiusMiles <= 0)
            {
                throw new ArgumentException("Radius must be greater than 0");
            }

            return FindProvidersInRadius(providers, patient, radiusMiles);
        }
    }

    /// <summary>
    /// Main console application for healthcare provider locator
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("Healthcare Provider Locator");
            Console.WriteLine("========================================\n");

            try
            {
                // Initialize sample data
                var providers = GetSampleProviders();
                var patients = GetSamplePatients();

                // Display menu
                bool running = true;
                while (running)
                {
                    Console.WriteLine("\nOptions:");
                    Console.WriteLine("1. Find nearest provider for a patient");
                    Console.WriteLine("2. Find all providers within custom radius");
                    Console.WriteLine("3. Display all providers");
                    Console.WriteLine("4. Display all patients");
                    Console.WriteLine("5. Add new provider");
                    Console.WriteLine("6. Add new patient");
                    Console.WriteLine("7. Exit");
                    Console.Write("\nSelect option (1-7): ");

                    string option = Console.ReadLine();

                    switch (option)
                    {
                        case "1":
                            FindNearestProviderInteractive(providers, patients);
                            break;
                        case "2":
                            FindProvidersWithinRadiusInteractive(providers, patients);
                            break;
                        case "3":
                            DisplayProviders(providers);
                            break;
                        case "4":
                            DisplayPatients(patients);
                            break;
                        case "5":
                            AddNewProvider(providers);
                            break;
                        case "6":
                            AddNewPatient(patients);
                            break;
                        case "7":
                            running = false;
                            Console.WriteLine("\nThank you for using Healthcare Provider Locator!");
                            break;
                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void FindNearestProviderInteractive(List<Provider> providers, List<Patient> patients)
        {
            Console.WriteLine("\n--- Find Nearest Provider ---");
            Console.Write("Enter Patient ID (Member ID): ");
            string memberId = Console.ReadLine();

            var patient = patients.FirstOrDefault(p => p.MemberId == memberId);
            if (patient == null)
            {
                Console.WriteLine($"Patient with ID {memberId} not found.");
                return;
            }

            Console.WriteLine($"\nSearching for nearest provider to {patient.Name}...");
            Console.WriteLine($"Patient Location: ({patient.Latitude}, {patient.Longitude})");

            var results = ProviderDistanceUtility.FindNearestProviders(providers, patient);

            if (results.Count > 0)
            {
                Console.WriteLine($"\n✓ Found {results.Count} provider(s) within {results[0].SearchRadius} miles:\n");
                DisplaySearchResults(results);
            }
            else
            {
                Console.WriteLine($"\n✗ No providers found within 175 miles of the patient location.");
            }
        }

        static void FindProvidersWithinRadiusInteractive(List<Provider> providers, List<Patient> patients)
        {
            Console.WriteLine("\n--- Find Providers Within Custom Radius ---");
            Console.Write("Enter Patient ID (Member ID): ");
            string memberId = Console.ReadLine();

            var patient = patients.FirstOrDefault(p => p.MemberId == memberId);
            if (patient == null)
            {
                Console.WriteLine($"Patient with ID {memberId} not found.");
                return;
            }

            Console.Write("Enter search radius in miles: ");
            if (!int.TryParse(Console.ReadLine(), out int radius) || radius <= 0)
            {
                Console.WriteLine("Invalid radius. Please enter a positive number.");
                return;
            }

            Console.WriteLine($"\nSearching within {radius} miles...");
            var results = ProviderDistanceUtility.FindProvidersWithinRadius(providers, patient, radius);

            if (results.Count > 0)
            {
                Console.WriteLine($"\n✓ Found {results.Count} provider(s):\n");
                DisplaySearchResults(results);
            }
            else
            {
                Console.WriteLine($"\n✗ No providers found within {radius} miles.");
            }
        }

        static void DisplaySearchResults(List<ProviderSearchResult> results)
        {
            Console.WriteLine(new string('-', 100));
            Console.WriteLine(string.Format("{0,-15} {1,-30} {2,-25} {3,-12} {4,-10}",
                "Provider ID", "Name", "Specialty", "Distance", "Address"));
            Console.WriteLine(new string('-', 100));

            foreach (var result in results)
            {
                Console.WriteLine(string.Format("{0,-15} {1,-30} {2,-25} {3,-12} {4,-10}",
                    result.Provider.ProviderId,
                    result.Provider.Name,
                    result.Provider.Specialty ?? "N/A",
                    $"{result.Distance} mi",
                    result.Provider.Address ?? "N/A"));
            }
            Console.WriteLine(new string('-', 100));
        }

        static void DisplayProviders(List<Provider> providers)
        {
            Console.WriteLine("\n--- All Providers ---");
            Console.WriteLine(new string('-', 110));
            Console.WriteLine(string.Format("{0,-15} {1,-25} {2,-12} {3,-12} {4,-20} {5,-15}",
                "Provider ID", "Name", "Latitude", "Longitude", "Specialty", "Phone"));
            Console.WriteLine(new string('-', 110));

            foreach (var provider in providers)
            {
                Console.WriteLine(string.Format("{0,-15} {1,-25} {2,-12} {3,-12} {4,-20} {5,-15}",
                    provider.ProviderId,
                    provider.Name,
                    provider.Latitude,
                    provider.Longitude,
                    provider.Specialty ?? "N/A",
                    provider.Phone ?? "N/A"));
            }
            Console.WriteLine(new string('-', 110));
        }

        static void DisplayPatients(List<Patient> patients)
        {
            Console.WriteLine("\n--- All Patients ---");
            Console.WriteLine(new string('-', 90));
            Console.WriteLine(string.Format("{0,-15} {1,-25} {2,-12} {3,-12} {4,-20}",
                "Member ID", "Name", "Latitude", "Longitude", "Address"));
            Console.WriteLine(new string('-', 90));

            foreach (var patient in patients)
            {
                Console.WriteLine(string.Format("{0,-15} {1,-25} {2,-12} {3,-12} {4,-20}",
                    patient.MemberId,
                    patient.Name,
                    patient.Latitude,
                    patient.Longitude,
                    patient.Address ?? "N/A"));
            }
            Console.WriteLine(new string('-', 90));
        }

        static void AddNewProvider(List<Provider> providers)
        {
            Console.WriteLine("\n--- Add New Provider ---");
            
            var provider = new Provider();

            Console.Write("Provider ID: ");
            provider.ProviderId = Console.ReadLine();

            Console.Write("Provider Name: ");
            provider.Name = Console.ReadLine();

            Console.Write("Latitude: ");
            if (!double.TryParse(Console.ReadLine(), out double lat))
            {
                Console.WriteLine("Invalid latitude.");
                return;
            }
            provider.Latitude = lat;

            Console.Write("Longitude: ");
            if (!double.TryParse(Console.ReadLine(), out double lon))
            {
                Console.WriteLine("Invalid longitude.");
                return;
            }
            provider.Longitude = lon;

            Console.Write("Specialty (optional): ");
            provider.Specialty = Console.ReadLine();

            Console.Write("Phone (optional): ");
            provider.Phone = Console.ReadLine();

            Console.Write("Address (optional): ");
            provider.Address = Console.ReadLine();

            providers.Add(provider);
            Console.WriteLine($"\n✓ Provider {provider.Name} added successfully!");
        }

        static void AddNewPatient(List<Patient> patients)
        {
            Console.WriteLine("\n--- Add New Patient ---");

            var patient = new Patient();

            Console.Write("Member ID: ");
            patient.MemberId = Console.ReadLine();

            Console.Write("Patient Name: ");
            patient.Name = Console.ReadLine();

            Console.Write("Latitude: ");
            if (!double.TryParse(Console.ReadLine(), out double lat))
            {
                Console.WriteLine("Invalid latitude.");
                return;
            }
            patient.Latitude = lat;

            Console.Write("Longitude: ");
            if (!double.TryParse(Console.ReadLine(), out double lon))
            {
                Console.WriteLine("Invalid longitude.");
                return;
            }
            patient.Longitude = lon;

            Console.Write("Address (optional): ");
            patient.Address = Console.ReadLine();

            patients.Add(patient);
            Console.WriteLine($"\n✓ Patient {patient.Name} added successfully!");
        }

        static List<Provider> GetSampleProviders()
        {
            return new List<Provider>
            {
                new Provider
                {
                    ProviderId = "PRV001",
                    Name = "John Smith, MD",
                    Latitude = 40.7128,
                    Longitude = -74.0060,
                    Specialty = "Cardiology",
                    Phone = "(212) 555-0101",
                    Address = "123 Medical Plaza, New York, NY"
                },
                new Provider
                {
                    ProviderId = "PRV002",
                    Name = "Sarah Johnson, MD",
                    Latitude = 40.7614,
                    Longitude = -73.9776,
                    Specialty = "Orthopedics",
                    Phone = "(212) 555-0102",
                    Address = "456 Health Center, New York, NY"
                },
                new Provider
                {
                    ProviderId = "PRV003",
                    Name = "Michael Chen, MD",
                    Latitude = 40.6892,
                    Longitude = -74.0445,
                    Specialty = "General Practice",
                    Phone = "(718) 555-0103",
                    Address = "789 Care Clinic, Brooklyn, NY"
                },
                new Provider
                {
                    ProviderId = "PRV004",
                    Name = "Emily Davis, MD",
                    Latitude = 40.7282,
                    Longitude = -73.7949,
                    Specialty = "Pediatrics",
                    Phone = "(718) 555-0104",
                    Address = "321 Children's Hospital, Queens, NY"
                },
                new Provider
                {
                    ProviderId = "PRV005",
                    Name = "Robert Wilson, MD",
                    Latitude = 40.8448,
                    Longitude = -73.8648,
                    Specialty = "Neurology",
                    Phone = "(212) 555-0105",
                    Address = "654 Brain Institute, Manhattan, NY"
                }
            };
        }

        static List<Patient> GetSamplePatients()
        {
            return new List<Patient>
            {
                new Patient
                {
                    MemberId = "MEM001",
                    Name = "James Anderson",
                    Latitude = 40.7489,
                    Longitude = -73.9680,
                    Address = "100 Park Ave, New York, NY"
                },
                new Patient
                {
                    MemberId = "MEM002",
                    Name = "Patricia Martinez",
                    Latitude = 40.6501,
                    Longitude = -73.9496,
                    Address = "200 Atlantic Ave, Brooklyn, NY"
                },
                new Patient
                {
                    MemberId = "MEM003",
                    Name = "Michael Thompson",
                    Latitude = 40.7580,
                    Longitude = -73.9855,
                    Address = "300 5th Ave, New York, NY"
                },
                new Patient
                {
                    MemberId = "MEM004",
                    Name = "Linda White",
                    Latitude = 40.8176,
                    Longitude = -73.9282,
                    Address = "400 East 72nd St, New York, NY"
                },
                new Patient
                {
                    MemberId = "MEM005",
                    Name = "David Garcia",
                    Latitude = 40.6976,
                    Longitude = -74.2591,
                    Address = "500 Jersey St, Jersey City, NJ"
                }
            };
        }
    }
}
