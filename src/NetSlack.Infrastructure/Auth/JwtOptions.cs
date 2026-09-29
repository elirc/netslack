namespace NetSlack.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "NetSlack";

    public string Audience { get; init; } = "NetSlack.Client";

    public string SigningKey { get; init; } = string.Empty;

    public int ExpirationMinutes { get; init; } = 480;
}

