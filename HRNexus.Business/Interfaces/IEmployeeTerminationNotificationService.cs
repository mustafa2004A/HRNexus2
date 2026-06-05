using HRNexus.Business.Models.Notifications;

namespace HRNexus.Business.Interfaces;

public interface IEmployeeTerminationNotificationService
{
    Task NotifyEmployeeTerminatedAsync(
        EmployeeTerminationNotification notification,
        CancellationToken cancellationToken = default);
}
