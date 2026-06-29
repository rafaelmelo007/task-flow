using TaskFlow.Application.Auth.Dtos;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Auth.Mapping;

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return new(user.Id, user.Email, user.CreatedAt);
    }
}
