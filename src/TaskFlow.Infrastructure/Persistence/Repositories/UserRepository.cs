using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        // Emails are stored normalized to lower-case (see AuthService). EF Core translates this
        // comparison to SQL, so the StringComparison overloads CA1862 suggests are not
        // translatable, and CA1308's upper-case preference does not apply to this normalization.
#pragma warning disable CA1308, CA1862
        var normalized = email.ToLowerInvariant();
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == normalized, ct);
#pragma warning restore CA1308, CA1862
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return user;
    }
}
