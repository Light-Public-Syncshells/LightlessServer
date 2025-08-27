using LightlessSyncShared.Utils;
using Microsoft.AspNetCore.Mvc;

namespace LightlessSyncStaticFilesServer.Controllers;

public class ControllerBase : Controller
{
    protected ILogger _logger;

    public ControllerBase(ILogger logger)
    {
        _logger = logger;
    }

    protected string LightlessUser => HttpContext.User.Claims.First(f => string.Equals(f.Type, LightlessClaimTypes.Uid, StringComparison.Ordinal)).Value;
    protected string Continent => HttpContext.User.Claims.FirstOrDefault(f => string.Equals(f.Type, LightlessClaimTypes.Continent, StringComparison.Ordinal))?.Value ?? "*";
    protected bool IsPriority => !string.IsNullOrEmpty(HttpContext.User.Claims.FirstOrDefault(f => string.Equals(f.Type, LightlessClaimTypes.Alias, StringComparison.Ordinal))?.Value ?? string.Empty);
}
