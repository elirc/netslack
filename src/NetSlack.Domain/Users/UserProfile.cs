using NetSlack.Domain.Common;

namespace NetSlack.Domain.Users;

public sealed class UserProfile : Entity
{
    private UserProfile()
    {
        DisplayName = string.Empty;
        Email = string.Empty;
        AvatarUrl = string.Empty;
    }

    private UserProfile(Guid id, string displayName, string email, string avatarUrl)
        : base(id)
    {
        DisplayName = displayName;
        Email = email;
        AvatarUrl = avatarUrl;
    }

    public string DisplayName { get; private set; }

    public string Email { get; private set; }

    public string AvatarUrl { get; private set; }

    public static UserProfile Create(string displayName, string email, string avatarUrl = "")
    {
        var normalizedEmail = Guard.AgainstEmpty(email, nameof(email), 256).ToUpperInvariant();
        return new UserProfile(
            Guid.NewGuid(),
            Guard.AgainstEmpty(displayName, nameof(displayName), 80),
            normalizedEmail,
            avatarUrl.Trim());
    }

    public void Rename(string displayName)
    {
        DisplayName = Guard.AgainstEmpty(displayName, nameof(displayName), 80);
    }
}
