using RideMatchingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatchingSystem.Domain.Entities
{
    public class Driver
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? FcmToken { get; set; }
        public DriverStatus Status { get; set; } = DriverStatus.Offline;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime LastLocationUpdate { get; set; } = DateTime.UtcNow;

        public Coordinates CurrentLocation => new(Latitude, Longitude);
    }
}
