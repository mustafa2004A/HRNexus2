using HRNexus.Business.Models.Auth;

namespace HRNexus.Business.Interfaces;

public interface IDevelopmentPasswordBootstrapService
{
    Task<DevelopmentPasswordBootstrapResultDto> ReseedDemoPasswordsAsync(
        DevelopmentPasswordBootstrapRequest request,
        CancellationToken cancellationToken = default);
}
