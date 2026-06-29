using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Infrastructure.Settings;

public class JwtSettings
{
    [Required, MinLength(32)]
    public string Secret { get; set; } = string.Empty;
    [Required]
    public string Issuer { get; set; } = string.Empty;
    [Required]
    public string Audience { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; } = 60;
}
