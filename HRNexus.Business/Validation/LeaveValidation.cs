using HRNexus.Business.Exceptions;
using HRNexus.Business.Models.Leave;

namespace HRNexus.Business.Validation;

public static class LeaveValidation
{
    public static void EnsureValid(CreateLeaveRequestRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.EndDate < request.StartDate)
        {
            throw new BusinessRuleException("End date cannot be earlier than start date.");
        }
    }

    public static void EnsureValid(UpsertLeaveBalanceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.UsedDays > request.EntitledDays)
        {
            throw new BusinessRuleException("Used days cannot be greater than entitled days.");
        }
    }

}
