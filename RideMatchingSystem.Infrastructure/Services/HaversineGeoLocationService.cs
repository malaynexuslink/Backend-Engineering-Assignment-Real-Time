using Microsoft.EntityFrameworkCore;
using RideMatchingSystem.Application.Interfaces;
using RideMatchingSystem.Domain.Entities;
using RideMatchingSystem.Domain.Enums;
using RideMatchingSystem.Infrastructure.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatchingSystem.Infrastructure.Services
{
    public class HaversineGeoLocationService : IDriverLocationService
    {
        private readonly AppDbContext _context;
        // In-memory spatial index for high-performance proximity searches
        private static readonly ConcurrentDictionary<Guid, (Coordinates Coords, DriverStatus Status)> _spatialIndex = new();

        public HaversineGeoLocationService(AppDbContext context) => _context = context;

        public async Task UpdateLocationAsync(Guid driverId, Coordinates coordinates, CancellationToken ct = default)
        {
            var driver = await _context.Drivers.FindAsync(new object[] { driverId }, ct);
            if (driver != null)
            {
                driver.Latitude = coordinates.Latitude;
                driver.Longitude = coordinates.Longitude;
                driver.LastLocationUpdate = DateTime.UtcNow;

                _spatialIndex.AddOrUpdate(driverId, (coordinates, driver.Status), (_, val) => (coordinates, val.Status));
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task UpdateStatusAsync(Guid driverId, DriverStatus status, CancellationToken ct = default)
        {
            var driver = await _context.Drivers.FindAsync(new object[] { driverId }, ct);
            if (driver != null)
            {
                driver.Status = status;
                _spatialIndex.AddOrUpdate(driverId, (driver.CurrentLocation, status), (_, val) => (val.Coords, status));
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<IReadOnlyList<Driver>> GetNearbyAvailableDriversAsync(Coordinates origin, double radiusInKm, CancellationToken ct = default)
        {
            var candidateIds = _spatialIndex
                .Where(k => k.Value.Status == DriverStatus.Available)
                .Select(k => new { Id = k.Key, Dist = CalcDistance(origin, k.Value.Coords) })
                .Where(x => x.Dist <= radiusInKm)
                .OrderBy(x => x.Dist)
                .Take(10)
                .Select(x => x.Id)
                .ToList();

            return await _context.Drivers.Where(d => candidateIds.Contains(d.Id)).ToListAsync(ct);
        }

        private static double CalcDistance(Coordinates pos1, Coordinates pos2)
        {
            const double R = 6371.0; // Earth radius in KM
            double dLat = (pos2.Latitude - pos1.Latitude) * (Math.PI / 180.0);
            double dLon = (pos2.Longitude - pos1.Longitude) * (Math.PI / 180.0);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(pos1.Latitude * (Math.PI / 180.0)) * Math.Cos(pos2.Latitude * (Math.PI / 180.0)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * (2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));
        }
    }
}
