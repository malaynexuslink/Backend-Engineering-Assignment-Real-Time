using RideMatchingSystem.Domain.Enums;

namespace RideMatchingSystem.Application.DTOs;

public record RegisterDriverRequest(string Name, string Phone, string FcmToken);
public record UpdateDriverStatusRequest(DriverStatus Status);
public record UpdateDriverLocationRequest(double Latitude, double Longitude);

public record CreateRideRequestDto(
    Guid RiderId,
    string RiderFcmToken,
    double PickupLatitude,
    double PickupLongitude,
    double DropoffLatitude,
    double DropoffLongitude
);

public record RideResponseDto(
    Guid RideId,
    Guid RiderId,
    Guid? DriverId,
    string Status,
    DateTime RequestedAt,
    DateTime? MatchedAt
);

public record DriverResponseDto(
    Guid Id,
    string Name,
    string Status,
    double Latitude,
    double Longitude,
    DateTime LastLocationUpdate
);
