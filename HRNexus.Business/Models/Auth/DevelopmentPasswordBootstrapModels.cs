using System.ComponentModel.DataAnnotations;

namespace HRNexus.Business.Models.Auth;

public sealed class DevelopmentPasswordBootstrapRequest
{
    [MaxLength(20)]
    public List<string> Usernames { get; set; } = [];

    [StringLength(128)]
    public string? Password { get; set; }

    [MaxLength(20)]
    public List<DevelopmentDemoPasswordCredentialRequest> Credentials { get; set; } = [];
}

public sealed class DevelopmentDemoPasswordCredentialRequest
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string Password { get; set; } = string.Empty;
}

public sealed record DevelopmentPasswordBootstrapResultDto(
    int UpdatedUserCount,
    IReadOnlyList<string> UpdatedUsernames,
    IReadOnlyList<string> MissingUsernames,
    DateTime GeneratedAt,
    int CreatedUserCount = 0,
    IReadOnlyList<string>? CreatedUsernames = null,
    IReadOnlyList<string>? NormalizedUsernames = null,
    IReadOnlyList<string>? DuplicateUsernamesFound = null,
    IReadOnlyList<string>? DeactivatedDuplicateUsernames = null,
    IReadOnlyList<string>? RoleAssignmentsEnsured = null,
    IReadOnlyList<string>? MissingEmployeeCodes = null,
    IReadOnlyList<string>? MissingRoleNames = null,
    IReadOnlyList<string>? Warnings = null);
