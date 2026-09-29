namespace NetSlack.Api.Contracts;

public sealed record DevTokenRequest(string Email, string DisplayName);

public sealed record AuthUserResponse(Guid Id, string Email, string DisplayName, string AvatarUrl);

public sealed record TokenResponse(string AccessToken, AuthUserResponse User);

