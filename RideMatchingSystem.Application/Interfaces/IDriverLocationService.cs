using RideMatchingSystem.Domain.Entities;
using RideMatchingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatchingSystem.Application.Interfaces
{
    public interface IDriverLocationService
    {
        Task UpdateLocationAsync(Guid driverId, Coordinates coordinates, CancellationToken ct = default);
        Task UpdateStatusAsync(Guid driverId, DriverStatus status, CancellationToken ct = default);
        Task<IReadOnlyList<Driver>> GetNearbyAvailableDriversAsync(Coordinates origin, double radiusInKm, CancellationToken ct = default);
    }
}
