using Microsoft.EntityFrameworkCore;
using NetSlack.Api.Contracts;
using NetSlack.Domain.Users;
using NetSlack.Infrastructure.Auth;
using NetSlack.Infrastructure.Data;

namespace NetSlack.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/dev-token", async (
            DevTokenRequest request,
            AppDbContext db,
            IJwtTokenService tokens,
            CancellationToken cancellationToken) =>
        {
            var email = request.Email.Trim().ToUpperInvariant();
            var user = await db.Users.SingleOrDefaultAsync(item => item.Email == email, cancellationToken);
            if (user is null)
            {
                user = UserProfile.Create(request.DisplayName, email);
                db.Users.Add(user);
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                user.Rename(request.DisplayName);
                await db.SaveChangesAsync(cancellationToken);
            }

            return Results.Ok(new TokenResponse(
                tokens.CreateToken(user),
                new AuthUserResponse(user.Id, user.Email, user.DisplayName, user.AvatarUrl)));
        })
        .AllowAnonymous()
        .WithSummary("Issues a development JWT for local demos. Replace with OIDC in production.");

        return app;
    }
}

