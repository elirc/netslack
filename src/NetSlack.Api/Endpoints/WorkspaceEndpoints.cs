using NetSlack.Api.Contracts;
using NetSlack.Application.Workspaces;

namespace NetSlack.Api.Endpoints;

public static class WorkspaceEndpoints
{
    public static IEndpointRouteBuilder MapWorkspaceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workspaces")
            .RequireAuthorization()
            .WithTags("Workspaces");

        group.MapGet("/", async (WorkspaceService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetMineAsync(cancellationToken)));

        group.MapPost("/", async (
            CreateWorkspaceRequest request,
            WorkspaceService service,
            CancellationToken cancellationToken) =>
        {
            var workspace = await service.CreateAsync(new CreateWorkspaceCommand(request.Name, request.Slug), cancellationToken);
            return Results.Created($"/api/workspaces/{workspace.Id}", workspace);
        });

        return app;
    }
}

