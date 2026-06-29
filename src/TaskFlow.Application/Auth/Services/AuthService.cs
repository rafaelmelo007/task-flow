using FluentValidation;
using TaskFlow.Application.Auth.Dtos;
using TaskFlow.Application.Auth.Mapping;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IDateTime _dateTime;

    public AuthService(
        IUserRepository userRepo,
        IPasswordHasher hasher,
        IJwtTokenGenerator jwt,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IDateTime dateTime)
    {
        _userRepo = userRepo;
        _hasher = hasher;
        _jwt = jwt;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _dateTime = dateTime;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validation = await _registerValidator.ValidateAsync(request, ct).ConfigureAwait(false);
        if (!validation.IsValid)
            return Result<AuthResponse>.ValidationError(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var existing = await _userRepo.GetByEmailAsync(request.Email, ct).ConfigureAwait(false);
        if (existing is not null)
            return Result<AuthResponse>.Conflict("Email already registered.");

#pragma warning disable CA1308 // Normalize email to lowercase — uppercase is not suitable for emails
        var user = new User
        {
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = _hasher.Hash(request.Password),
            CreatedAt = _dateTime.UtcNow
        };
#pragma warning restore CA1308

        var created = await _userRepo.AddAsync(user, ct).ConfigureAwait(false);
        var token = _jwt.GenerateToken(created);
        return Result<AuthResponse>.Success(new AuthResponse(token, created.ToDto()));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validation = await _loginValidator.ValidateAsync(request, ct).ConfigureAwait(false);
        if (!validation.IsValid)
            return Result<AuthResponse>.ValidationError(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var user = await _userRepo.GetByEmailAsync(request.Email, ct).ConfigureAwait(false);
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            return Result<AuthResponse>.Unauthorized("Invalid email or password.");

        var token = _jwt.GenerateToken(user);
        return Result<AuthResponse>.Success(new AuthResponse(token, user.ToDto()));
    }

    public async Task<Result<UserDto>> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct).ConfigureAwait(false);
        if (user is null)
            return Result<UserDto>.NotFound("User not found.");
        return Result<UserDto>.Success(user.ToDto());
    }
}
