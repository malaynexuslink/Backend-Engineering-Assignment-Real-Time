using Microsoft.AspNetCore.Mvc;
using RideMatchingSystem.Application.DTOs;
using RideMatchingSystem.Application.Interfaces;
using RideMatchingSystem.Domain.Entities;
using RideMatchingSystem.Infrastructure.Data;

namespace RideMatchingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriversController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDriverLocationService _locationService;

        public DriversController(AppDbContext context, IDriverLocationService locationService)
        {
            _context = context;
            _locationService = locationService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDriverRequest request)
        {
            var driver = new Driver { Name = request.Name, Phone = request.Phone, FcmToken = request.FcmToken };
            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync();
            return Ok(new { driver.Id, driver.Name });
        }

        [HttpPost("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateDriverStatusRequest req)
        {
            await _locationService.UpdateStatusAsync(id, req.Status);
            return NoContent();
        }

        [HttpPost("{id:guid}/location")]
        public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] UpdateDriverLocationRequest req)
        {
            await _locationService.UpdateLocationAsync(id, new Coordinates(req.Latitude, req.Longitude));
            return NoContent();
        }
    }
}
