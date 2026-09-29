using NetSlack.Domain.Users;

namespace NetSlack.Infrastructure.Auth;

public interface IJwtTokenService
{
    string CreateToken(UserProfile user);
}

