using Microsoft.AspNetCore.Hosting;

namespace TaskFlow.Api.Extensions;

public static class CorsExtensions
{
    public const string PolicyName = "TaskFlowCors";

    public static IServiceCollection AddTaskFlowCors(
        this IServiceCollection services,
        IConfiguration config,
        IWebHostEnvironment env)
    {
        services.AddCors(opt =>
            opt.AddPolicy(PolicyName, p =>
                p.SetIsOriginAllowed(origin =>
                {
                    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;

                    // Only allow localhost in development
                    if (env.IsDevelopment() && uri.Host is "localhost" or "127.0.0.1") return true;

                    // All environments: check explicitly configured origins
                    var configured = config.GetSection("Cors:AllowedOrigins").Get<string[]>();
                    return configured?.Contains(origin, StringComparer.OrdinalIgnoreCase) ?? false;
                })
                .AllowAnyHeader()
                .AllowAnyMethod()));

        return services;
    }
}
