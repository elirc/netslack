using NetSlack.Api.Contracts;
using NetSlack.Application.Channels;

namespace NetSlack.Api.Endpoints;

public static class ChannelEndpoints
{
    public static IEndpointRouteBuilder MapChannelEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workspaces/{workspaceId:guid}/channels")
            .RequireAuthorization()
            .WithTags("Channels");

        group.MapGet("/", async (Guid workspaceId, ChannelService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetForWorkspaceAsync(workspaceId, cancellationToken)));

        group.MapPost("/", async (
            Guid workspaceId,
            CreateChannelRequest request,
            ChannelService service,
            CancellationToken cancellationToken) =>
        {
            var channel = await service.CreateAsync(
                workspaceId,
                new CreateChannelCommand(request.Name, request.Topic, request.Type),
                cancellationToken);

            return Results.Created($"/api/channels/{channel.Id}", channel);
        });

        return app;
    }
}

