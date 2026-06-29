using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.Application.Auth.Dtos;
using TaskFlow.Application.Common.Models;
using TaskFlow.Application.Tasks.Dtos;
using TaskFlow.Domain.Enums;
using Xunit;

namespace TaskFlow.Api.Tests;

[Trait("Category", "Integration")]
public class TaskEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TaskEndpointsTests(CustomWebApplicationFactory factory)
    {
        factory.EnsureDbCreated();
        _factory = factory;
    }

    private async Task<(HttpClient client, string token)> AuthorizedClientAsync()
    {
        var client = _factory.CreateClient();
        var email = $"tasktest{Guid.NewGuid()}@test.com";
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password1"));
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return (client, auth.Token);
    }

    [Fact]
    public async Task GetTasks_Unauthorized_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/tasks");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAndGetTask_Returns201Then200()
    {
        var (client, _) = await AuthorizedClientAsync();
        var req = new CreateTaskRequest("My Task", "desc", TaskItemStatus.Todo, TaskPriority.High, null);

        var createResp = await client.PostAsJsonAsync("/api/tasks", req);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var task = await createResp.Content.ReadFromJsonAsync<TaskDto>();
        task!.Title.Should().Be("My Task");

        var getResp = await client.GetAsync($"/api/tasks/{task.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteTask_Returns204()
    {
        var (client, _) = await AuthorizedClientAsync();
        var req = new CreateTaskRequest("To Delete", null, TaskItemStatus.Todo, TaskPriority.Low, null);
        var createResp = await client.PostAsJsonAsync("/api/tasks", req);
        var task = await createResp.Content.ReadFromJsonAsync<TaskDto>();

        var delResp = await client.DeleteAsync($"/api/tasks/{task!.Id}");
        delResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CrossOwnerAccess_Returns404()
    {
        var (clientA, _) = await AuthorizedClientAsync();
        var (clientB, _) = await AuthorizedClientAsync();

        var createResp = await clientA.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("Owner A Task", null, TaskItemStatus.Todo, TaskPriority.Low, null));
        var task = await createResp.Content.ReadFromJsonAsync<TaskDto>();

        var getResp = await clientB.GetAsync($"/api/tasks/{task!.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTask_WithEmptyTitle_Returns400()
    {
        var (client, _) = await AuthorizedClientAsync();
        var req = new CreateTaskRequest("", null, TaskItemStatus.Todo, TaskPriority.Low, null);
        var response = await client.PostAsJsonAsync("/api/tasks", req);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
