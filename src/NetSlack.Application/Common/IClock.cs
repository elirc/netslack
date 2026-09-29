namespace NetSlack.Application.Common;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

