namespace HRNexus.Business.Options;

public sealed class HrNotificationOptions
{
    public string? TerminationResponsibleEmail { get; set; }
    public string? TerminationResponsiblePhoneNumber { get; set; }
    public bool EnableTerminationEmail { get; set; }
    public bool EnableTerminationSms { get; set; }
}
