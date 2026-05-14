namespace HRNexus.API.Security;

public interface IClientIpAddressProvider
{
    string? GetClientIpAddress();
}
