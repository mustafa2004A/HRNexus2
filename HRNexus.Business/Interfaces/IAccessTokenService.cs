using HRNexus.Business.Models.Auth;

namespace HRNexus.Business.Interfaces;

public interface IAccessTokenService
{
    AccessTokenResult CreateAccessToken(AccessTokenUser user);
}
