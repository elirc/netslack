using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NetSlack.Application.Messages;

namespace NetSlack.Api.Hubs;

[Authorize]
public sealed class MessageHub : Hub<IMessageClient>
{
    public Task JoinChannel(Guid channelId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, GroupName(channelId));
    }

    public Task LeaveChannel(Guid channelId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(channelId));
    }

    public static string GroupName(Guid channelId) => $"channel:{channelId}";
}

public interface IMessageClient
{
    Task MessagePosted(MessageDto message);
}
