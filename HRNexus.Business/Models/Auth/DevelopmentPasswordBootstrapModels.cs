using System.ComponentModel.DataAnnotations;

namespace HRNexus.Business.Models.Auth;

public sealed class DevelopmentPasswordBootstrapRequest
{
    [MaxLength(20)]
    public List<string> Usernames { get; set; } = [];

    [Required]
    [StringLength(128, MinimumLength = 10)]
    public string Password { get; set; } = string.Empty;
}

public sealed record DevelopmentPasswordBootstrapResultDto(
    int UpdatedUserCount,
    IReadOnlyList<string> UpdatedUsernames,
    IReadOnlyList<string> MissingUsernames,
    DateTime GeneratedAt);
