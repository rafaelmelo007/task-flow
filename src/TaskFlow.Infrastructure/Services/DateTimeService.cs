using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}
