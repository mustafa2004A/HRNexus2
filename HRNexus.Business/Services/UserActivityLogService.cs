using HRNexus.Business.Interfaces;
using HRNexus.Business.Validation;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Security;
using HRNexus.DataAccess.Repositories.Abstractions;

namespace HRNexus.Business.Services;

public sealed class UserActivityLogService : IUserActivityLogService
{
    private readonly IActivityTypeRepository _activityTypeRepository;
    private readonly IUserActivityLogRepository _userActivityLogRepository;
    private readonly IHRNexusDbContext _dbContext;

    public UserActivityLogService(
        IActivityTypeRepository activityTypeRepository,
        IUserActivityLogRepository userActivityLogRepository,
        IHRNexusDbContext dbContext)
    {
        _activityTypeRepository = activityTypeRepository;
        _userActivityLogRepository = userActivityLogRepository;
        _dbContext = dbContext;
    }

    public async Task LogAsync(
        int? userId,
        string activityTypeCode,
        bool isSuccess,
        string? details,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var activityType = await _activityTypeRepository.GetByCodeAsync(activityTypeCode, cancellationToken);

        if (activityType is null)
        {
            return;
        }

        var activityLog = new UserActivityLog
        {
            UserId = userId,
            ActivityTypeId = activityType.ActivityTypeId,
            ActivityDetails = Truncate(BusinessValidation.NormalizeOptionalText(details), 255),
            IpAddress = Truncate(BusinessValidation.NormalizeOptionalText(ipAddress), 45),
            OccurredAt = DateTime.UtcNow,
            IsSuccess = isSuccess
        };

        await _userActivityLogRepository.AddAsync(activityLog, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string? Truncate(string? value, int maxLength)
    {
        return value is null || value.Length <= maxLength
            ? value
            : value[..maxLength];
    }
}
