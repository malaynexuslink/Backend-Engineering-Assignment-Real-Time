using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using RideMatchingSystem.Application.Interfaces;
using RideMatchingSystem.Domain.Entities;
using RideMatchingSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatchingSystem.Infrastructure.Services
{
    public class FcmNotificationService : IRideNotificationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FcmNotificationService> _logger;

        public FcmNotificationService(AppDbContext context, ILogger<FcmNotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task NotifyRideMatchedAsync(RideRequest ride, Driver driver, CancellationToken ct = default)
        {
            try
            {
                // 1. Notify Rider via Push Notification
                if (!string.IsNullOrEmpty(ride.RiderFcmToken))
                {
                    var riderMsg = new Message
                    {
                        Token = ride.RiderFcmToken,
                        Notification = new Notification { Title = "Ride Matched!", Body = $"{driver.Name} is on the way." },
                        Data = new Dictionary<string, string> { { "rideId", ride.Id.ToString() }, { "driverId", driver.Id.ToString() } }
                    };
                    await FirebaseMessaging.DefaultInstance.SendAsync(riderMsg, ct);
                }

                // 2. Notify Driver via Push Notification
                if (!string.IsNullOrEmpty(driver.FcmToken))
                {
                    var driverMsg = new Message
                    {
                        Token = driver.FcmToken,
                        Notification = new Notification { Title = "New Ride Request", Body = "Tap to view pickup details." },
                        Data = new Dictionary<string, string> { { "rideId", ride.Id.ToString() }, { "pickupLat", ride.PickupLatitude.ToString() } }
                    };
                    await FirebaseMessaging.DefaultInstance.SendAsync(driverMsg, ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FCM Delivery Failed for Match {RideId}", ride.Id);
            }
        }

        public async Task NotifyRideStatusChangedAsync(Guid rideId, string status, CancellationToken ct = default)
        {
            try
            {
                var ride = await _context.RideRequests.FindAsync(new object[] { rideId }, ct);
                if (ride != null && !string.IsNullOrEmpty(ride.RiderFcmToken))
                {
                    var msg = new Message
                    {
                        Token = ride.RiderFcmToken,
                        Notification = new Notification { Title = "Ride Update", Body = $"Status changed to: {status}" },
                        Data = new Dictionary<string, string> { { "rideId", rideId.ToString() }, { "status", status } }
                    };
                    await FirebaseMessaging.DefaultInstance.SendAsync(msg, ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FCM Status Update Failed for Ride {RideId}", rideId);
            }
        }

        public async Task NotifyDriverLocationUpdatedAsync(Guid rideId, Coordinates coordinates, CancellationToken ct = default)
        {
            // FCM is generally not recommended for high-frequency live location streaming (battery/quota drain).
            // Send a silent data message only if necessary, so the client app can update the map.
            try
            {
                var ride = await _context.RideRequests.FindAsync(new object[] { rideId }, ct);
                if (ride != null && !string.IsNullOrEmpty(ride.RiderFcmToken))
                {
                    var msg = new Message
                    {
                        Token = ride.RiderFcmToken,
                        Data = new Dictionary<string, string>
                    {
                        { "action", "LOCATION_UPDATE" },
                        { "lat", coordinates.Latitude.ToString() },
                        { "lon", coordinates.Longitude.ToString() }
                    }
                    };
                    await FirebaseMessaging.DefaultInstance.SendAsync(msg, ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FCM Location Payload Failed");
            }
        }
    }
}
