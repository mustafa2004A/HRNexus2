using System.Security.Claims;
using HRNexus.Business.Interfaces;

namespace HRNexus.API.Security;

public sealed class HeaderCurrentUserContext : ICurrentUserContext
{
    private const string UserIdHeaderName = "X-User-Id";
    private const string EmployeeIdHeaderName = "X-Employee-Id";
    private const string UsernameHeaderName = "X-Username";
    private const string EmployeeIdClaimName = "employee_id";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HeaderCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var claimValue = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(claimValue, out var claimUserId) && claimUserId > 0)
            {
                return claimUserId;
            }

            var rawValue = _httpContextAccessor.HttpContext?.Request.Headers[UserIdHeaderName].ToString();

            return int.TryParse(rawValue, out var userId) && userId > 0
                ? userId
                : null;
        }
    }

    public int? EmployeeId
    {
        get
        {
            var claimValue = _httpContextAccessor.HttpContext?.User.FindFirstValue(EmployeeIdClaimName);

            if (int.TryParse(claimValue, out var claimEmployeeId) && claimEmployeeId > 0)
            {
                return claimEmployeeId;
            }

            var rawValue = _httpContextAccessor.HttpContext?.Request.Headers[EmployeeIdHeaderName].ToString();

            return int.TryParse(rawValue, out var employeeId) && employeeId > 0
                ? employeeId
                : null;
        }
    }

    public string? Username
    {
        get
        {
            var claimValue = _httpContextAccessor.HttpContext?.User.Identity?.Name
                ?? _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);

            if (!string.IsNullOrWhiteSpace(claimValue))
            {
                return claimValue.Trim();
            }

            var rawValue = _httpContextAccessor.HttpContext?.Request.Headers[UsernameHeaderName].ToString();
            return string.IsNullOrWhiteSpace(rawValue) ? null : rawValue.Trim();
        }
    }

    public IReadOnlyList<string> Roles
    {
        get
        {
            var roles = _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role)
                .Select(claim => claim.Value)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return roles ?? [];
        }
    }
}
