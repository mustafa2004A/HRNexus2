using HRNexus.Business.Exceptions;

namespace HRNexus.Business.Validation;

public static class BusinessValidation
{
    public static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public static string NormalizeRequiredText(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BusinessRuleException($"{fieldName} is required.");
        }

        return value.Trim();
    }
}
