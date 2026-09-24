using RideMatchingSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatchingSystem.Application.Interfaces
{
    public interface IRideNotificationService
    {
        Task NotifyRideMatchedAsync(RideRequest ride, Driver driver, CancellationToken ct = default);
        Task NotifyRideStatusChangedAsync(Guid rideId, string status, CancellationToken ct = default);
        Task NotifyDriverLocationUpdatedAsync(Guid rideId, Coordinates coordinates, CancellationToken ct = default);
    }
}
