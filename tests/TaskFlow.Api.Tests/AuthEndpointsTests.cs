using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.Application.Auth.Dtos;
using Xunit;

namespace TaskFlow.Api.Tests;

[Trait("Category", "Integration")]
public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        factory.EnsureDbCreated();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_Returns201()
    {
        var request = new RegisterRequest($"test{Guid.NewGuid()}@test.com", "Password1");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Token.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var email = $"dup{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password1"));
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password1"));
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithToken()
    {
        var email = $"login{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password1"));
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password1"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Token.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var email = $"wrong{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password1"));
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "WrongPass1"));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
