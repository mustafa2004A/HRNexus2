using System.Net;

namespace HRNexus.API.Security;

public sealed class ClientIpAddressProvider : IClientIpAddressProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClientIpAddressProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetClientIpAddress()
    {
        IPAddress? ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;

        if (ipAddress is null)
        {
            return null;
        }

        if (ipAddress.IsIPv4MappedToIPv6)
        {
            ipAddress = ipAddress.MapToIPv4();
        }

        if (IPAddress.IsLoopback(ipAddress))
        {
            return "127.0.0.1";
        }

        return ipAddress.ToString();
    }
}
