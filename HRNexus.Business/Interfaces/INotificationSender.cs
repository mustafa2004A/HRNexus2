using HRNexus.Business.Models.Security;

namespace HRNexus.Business.Interfaces;

public interface INotificationSender
{
    Task SendVerificationCodeAsync(
        VerificationCodeDeliveryRequest request,
        CancellationToken cancellationToken = default);
}
