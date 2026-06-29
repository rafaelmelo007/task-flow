using FluentAssertions;
using FluentValidation;
using Moq;
using TaskFlow.Application.Auth.Dtos;
using TaskFlow.Application.Auth.Services;
using TaskFlow.Application.Auth.Validators;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Application.Tests.Common;
using Xunit;

namespace TaskFlow.Application.Tests.Auth;

[Trait("Category", "Unit")]
public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtMock = new();
    private readonly Mock<IDateTime> _dtMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _dtMock.Setup(d => d.UtcNow).Returns(DateTime.UtcNow);
        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<TaskFlow.Domain.Entities.User>())).Returns("test-token");

        _sut = new AuthService(
            _userRepoMock.Object,
            _hasherMock.Object,
            _jwtMock.Object,
            new RegisterRequestValidator(),
            new LoginRequestValidator(),
            _dtMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidEmail_ReturnsValidationError()
    {
        var result = await _sut.RegisterAsync(new RegisterRequest("not-an-email", "Password1"));
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task RegisterAsync_WithWeakPassword_ReturnsValidationError()
    {
        var result = await _sut.RegisterAsync(new RegisterRequest("user@test.com", "weak"));
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ReturnsConflict()
    {
        var existing = TestData.MakeUser("existing@test.com");
        _userRepoMock.Setup(r => r.GetByEmailAsync("existing@test.com", default)).ReturnsAsync(existing);

        var result = await _sut.RegisterAsync(new RegisterRequest("existing@test.com", "Password1"));

        result.ErrorType.Should().Be(ResultErrorType.Conflict);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsAuthResponse()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((TaskFlow.Domain.Entities.User?)null);
        _hasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<TaskFlow.Domain.Entities.User>(), default))
            .ReturnsAsync((TaskFlow.Domain.Entities.User u, CancellationToken _) => u);

        var result = await _sut.RegisterAsync(new RegisterRequest("new@test.com", "Password1"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("test-token");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsUnauthorized()
    {
        var user = TestData.MakeUser("user@test.com");
        _userRepoMock.Setup(r => r.GetByEmailAsync("user@test.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("wrongpassword", user.PasswordHash)).Returns(false);

        var result = await _sut.LoginAsync(new LoginRequest("user@test.com", "wrongpassword"));

        result.ErrorType.Should().Be(ResultErrorType.Unauthorized);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var user = TestData.MakeUser("user@test.com");
        _userRepoMock.Setup(r => r.GetByEmailAsync("user@test.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("Password1", user.PasswordHash)).Returns(true);

        var result = await _sut.LoginAsync(new LoginRequest("user@test.com", "Password1"));

        result.IsSuccess.Should().BeTrue();
    }
}
