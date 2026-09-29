using Microsoft.Extensions.DependencyInjection;
using NetSlack.Application.Channels;
using NetSlack.Application.Messages;
using NetSlack.Application.Workspaces;

namespace NetSlack.Application.Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<WorkspaceService>();
        services.AddScoped<ChannelService>();
        services.AddScoped<MessageService>();
        return services;
    }
}

