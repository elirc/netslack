using Microsoft.Extensions.Options;

namespace NetSlack.Infrastructure.Auth;

public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.SigningKey) || options.SigningKey.Length < 32)
        {
            return ValidateOptionsResult.Fail("Jwt:SigningKey must be at least 32 characters.");
        }

        return ValidateOptionsResult.Success;
    }
}
