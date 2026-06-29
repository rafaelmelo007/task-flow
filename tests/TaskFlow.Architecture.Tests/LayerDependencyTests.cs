using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace TaskFlow.Architecture.Tests;

[Trait("Category", "Architecture")]
public class LayerDependencyTests
{
    private const string ApplicationNs = "TaskFlow.Application";
    private const string InfrastructureNs = "TaskFlow.Infrastructure";
    private const string ApiNs = "TaskFlow.Api";

    [Fact]
    public void Domain_ShouldNotDependOnAnyOtherLayer()
    {
        var result = Types.InAssembly(typeof(TaskFlow.Domain.Entities.TaskItem).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNs, InfrastructureNs, ApiNs)
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructureOrApi()
    {
        var result = Types.InAssembly(typeof(TaskFlow.Application.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNs, ApiNs)
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnApi()
    {
        var result = Types.InAssembly(typeof(TaskFlow.Infrastructure.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNs)
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }
}
