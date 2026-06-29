using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnType("TEXT");
        builder.Property(t => t.Title).IsRequired().HasMaxLength(200).HasColumnType("TEXT COLLATE NOCASE");
        builder.Property(t => t.Description).HasMaxLength(2000);
        builder.Property(t => t.Status).HasConversion<int>();
        builder.Property(t => t.Priority).HasConversion<int>();
        builder.Property(t => t.DueDate).HasColumnType("TEXT");
        builder.Property(t => t.CreatedAt).HasColumnType("TEXT");
        builder.Property(t => t.UpdatedAt).HasColumnType("TEXT");
        builder.Property(t => t.UserId).HasColumnType("TEXT");
        builder.HasIndex(t => t.UserId);
        builder.HasOne(t => t.User).WithMany(u => u.Tasks).HasForeignKey(t => t.UserId);
        builder.Ignore(t => t.IsOverdue);
    }
}
