using RideMatchingSystem.Application.Interfaces;
using RideMatchingSystem.Application.Services;
using RideMatchingSystem.Domain.Enums;
using RideMatchingSystem.Infrastructure.Data;
using RideMatchingSystem.Infrastructure.Services;

namespace RideMatchingSystem.API.BackgroundServices
{
    public class RideMatchingBackgroundWorker : BackgroundService
    {
        private readonly RideMatchingChannel _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly DriverLockManager _lockManager;

        public RideMatchingBackgroundWorker(RideMatchingChannel channel, IServiceProvider serviceProvider, DriverLockManager lockManager)
        {
            _channel = channel;
            _serviceProvider = serviceProvider;
            _lockManager = lockManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var rideId in _channel.ReadAllAsync(stoppingToken))
            {
                await ProcessMatchAsync(rideId, stoppingToken);
            }
        }

        private async Task ProcessMatchAsync(Guid rideId, CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var locationService = scope.ServiceProvider.GetRequiredService<IDriverLocationService>();
            var notifier = scope.ServiceProvider.GetRequiredService<IRideNotificationService>();

            var ride = await db.RideRequests.FindAsync(new object[] { rideId }, ct);
            if (ride == null || ride.Status != RideStatus.Requested) return;

            ride.Status = RideStatus.Searching;
            await db.SaveChangesAsync(ct);

            var drivers = await locationService.GetNearbyAvailableDriversAsync(ride.PickupLocation, 10.0, ct);
            bool matched = false;

            foreach (var driver in drivers)
            {
                var sem = _lockManager.GetLock(driver.Id);
                if (!await sem.WaitAsync(100, ct)) continue;

                try
                {
                    var freshDriver = await db.Drivers.FindAsync(new object[] { driver.Id }, ct);
                    if (freshDriver != null && freshDriver.Status == DriverStatus.Available)
                    {
                        freshDriver.Status = DriverStatus.Busy;
                        ride.DriverId = freshDriver.Id;
                        ride.Status = RideStatus.Matched;
                        ride.MatchedAt = DateTime.UtcNow;

                        await locationService.UpdateStatusAsync(freshDriver.Id, DriverStatus.Busy, ct);
                        await db.SaveChangesAsync(ct);

                        // FCM Dispatch
                        await notifier.NotifyRideMatchedAsync(ride, freshDriver, ct);
                        matched = true;
                        break;
                    }
                }
                finally { sem.Release(); }
            }

            if (!matched)
            {
                ride.Status = RideStatus.Failed;
                await db.SaveChangesAsync(ct);
                await notifier.NotifyRideStatusChangedAsync(ride.Id, "Failed - No drivers available", ct);
            }
        }
    }
}
