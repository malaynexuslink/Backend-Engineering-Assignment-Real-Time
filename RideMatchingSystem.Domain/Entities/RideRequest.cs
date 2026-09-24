using RideMatchingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatchingSystem.Domain.Entities
{
    public class RideRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RiderId { get; set; }
        public string? RiderFcmToken { get; set; }
        public Guid? DriverId { get; set; }
        public double PickupLatitude { get; set; }
        public double PickupLongitude { get; set; }
        public double DropoffLatitude { get; set; }
        public double DropoffLongitude { get; set; }
        public RideStatus Status { get; set; } = RideStatus.Requested;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? MatchedAt { get; set; }

        public Coordinates PickupLocation => new(PickupLatitude, PickupLongitude);
    }
}
