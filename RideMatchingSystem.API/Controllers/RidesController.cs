using Microsoft.AspNetCore.Mvc;
using RideMatchingSystem.Application.DTOs;
using RideMatchingSystem.Application.Services;
using RideMatchingSystem.Domain.Entities;
using RideMatchingSystem.Infrastructure.Data;

namespace RideMatchingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RidesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly RideMatchingChannel _channel;

        public RidesController(AppDbContext context, RideMatchingChannel channel)
        {
            _context = context;
            _channel = channel;
        }

        [HttpPost]
        public async Task<IActionResult> RequestRide([FromBody] CreateRideRequestDto dto)
        {
            var ride = new RideRequest
            {
                RiderId = dto.RiderId,
                RiderFcmToken = dto.RiderFcmToken,
                PickupLatitude = dto.PickupLatitude,
                PickupLongitude = dto.PickupLongitude,
                DropoffLatitude = dto.DropoffLatitude,
                DropoffLongitude = dto.DropoffLongitude
            };

            _context.RideRequests.Add(ride);
            await _context.SaveChangesAsync();

            await _channel.EnqueueAsync(ride.Id); // Async matching trigger
            return Accepted(new { ride.Id, ride.Status });
        }
    }
}
