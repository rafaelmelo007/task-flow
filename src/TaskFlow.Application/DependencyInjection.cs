using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Auth.Services;
using TaskFlow.Application.Tasks.Services;

namespace TaskFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
