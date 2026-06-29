using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnType("TEXT");
        builder.Property(u => u.Email).IsRequired().HasColumnType("TEXT COLLATE NOCASE");
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.CreatedAt).HasColumnType("TEXT");
        builder.Property(u => u.UpdatedAt).HasColumnType("TEXT");
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
