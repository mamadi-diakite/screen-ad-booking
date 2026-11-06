namespace ScreenAdBooking.Application.Contracts.Auth;

/// <summary>Self sign-up creates an advertiser together with its company.</summary>
public sealed record RegisterRequest(
    string CompanyName,
    string DisplayName,
    string Email,
    string Password,
    string ConfirmPassword);

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, CurrentUserResponse User);

/// <param name="Role">"Advertiser" or "AgencyAdmin".</param>
/// <param name="CompanyId">Set for advertisers only.</param>
public sealed record CurrentUserResponse(Guid Id, string Email, string DisplayName, string Role, Guid? CompanyId);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
