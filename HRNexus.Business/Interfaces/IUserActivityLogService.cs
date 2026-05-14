namespace HRNexus.Business.Interfaces;

public interface IUserActivityLogService
{
    Task LogAsync(
        int? userId,
        string activityTypeCode,
        bool isSuccess,
        string? details,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
