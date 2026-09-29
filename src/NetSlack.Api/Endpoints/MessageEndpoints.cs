using Microsoft.AspNetCore.SignalR;
using NetSlack.Api.Contracts;
using NetSlack.Api.Hubs;
using NetSlack.Application.Messages;

namespace NetSlack.Api.Endpoints;

public static class MessageEndpoints
{
    public static IEndpointRouteBuilder MapMessageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/channels/{channelId:guid}/messages")
            .RequireAuthorization()
            .WithTags("Messages");

        group.MapGet("/", async (
            Guid channelId,
            int? take,
            MessageService service,
            CancellationToken cancellationToken) =>
        {
            var messages = await service.GetRecentAsync(channelId, take ?? 50, cancellationToken);
            return Results.Ok(messages);
        });

        group.MapPost("/", async (
            Guid channelId,
            PostMessageRequest request,
            MessageService service,
            IHubContext<MessageHub, IMessageClient> hub,
            CancellationToken cancellationToken) =>
        {
            var message = await service.PostAsync(channelId, new PostMessageCommand(request.Body), cancellationToken);
            await hub.Clients.Group(MessageHub.GroupName(channelId)).MessagePosted(message);
            return Results.Created($"/api/channels/{channelId}/messages/{message.Id}", message);
        });

        return app;
    }
}

