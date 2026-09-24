using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using RideMatchingSystem.API.BackgroundServices;
using RideMatchingSystem.Application.Interfaces;
using RideMatchingSystem.Application.Services;
using RideMatchingSystem.Infrastructure.Data;
using RideMatchingSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Firebase Admin SDK Initialization
// Safely wrap in try/catch for local testing without the physical JSON file.
try
{
    if (File.Exists("firebase-adminsdk.json"))
    {
        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile("firebase-adminsdk.json")
        });
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Firebase init skipped (Development mode): {ex.Message}");
}

// 2. Database
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("RideMatchingDb"));

// 3. Dependency Injection
builder.Services.AddSingleton<RideMatchingChannel>();
builder.Services.AddSingleton<DriverLockManager>();
builder.Services.AddScoped<IDriverLocationService, HaversineGeoLocationService>();
// Injecting FCM Notification Service instead of SignalR
builder.Services.AddScoped<IRideNotificationService, FcmNotificationService>();

// 4. Background Workers
builder.Services.AddHostedService<RideMatchingBackgroundWorker>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Seed initial test data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var locationService = scope.ServiceProvider.GetRequiredService<IDriverLocationService>();

    var driver = new RideMatchingSystem.Domain.Entities.Driver
    {
        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Name = "FCM Test Driver",
        Phone = "123456",
        FcmToken = "mock-driver-token",
        Status = RideMatchingSystem.Domain.Enums.DriverStatus.Available,
        Latitude = 23.0225,
        Longitude = 72.5714
    };
    context.Drivers.Add(driver);
    context.SaveChanges();

    // Warm up the spatial index
    locationService.UpdateStatusAsync(driver.Id, driver.Status).GetAwaiter().GetResult();
    locationService.UpdateLocationAsync(driver.Id, driver.CurrentLocation).GetAwaiter().GetResult();
}

app.Run();