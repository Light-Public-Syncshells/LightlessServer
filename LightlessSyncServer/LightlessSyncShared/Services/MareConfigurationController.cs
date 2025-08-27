using LightlessSyncShared.Utils.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LightlessSyncShared.Services;

[Route("configuration/[controller]")]
[Authorize(Policy = "Internal")]
public class LightlessConfigurationController<T> : Controller where T : class, ILightlessConfiguration
{
    private readonly ILogger<LightlessConfigurationController<T>> _logger;
    private IOptionsMonitor<T> _config;

    public LightlessConfigurationController(IOptionsMonitor<T> config, ILogger<LightlessConfigurationController<T>> logger)
    {
        _config = config;
        _logger = logger;
    }

    [HttpGet("GetConfigurationEntry")]
    [Authorize(Policy = "Internal")]
    public IActionResult GetConfigurationEntry(string key, string defaultValue)
    {
        var result = _config.CurrentValue.SerializeValue(key, defaultValue);
        _logger.LogInformation("Requested " + key + ", returning:" + result);
        return Ok(result);
    }
}

#pragma warning disable MA0048 // File name must match type name
public class LightlessStaticFilesServerConfigurationController : LightlessConfigurationController<StaticFilesServerConfiguration>
{
    public LightlessStaticFilesServerConfigurationController(IOptionsMonitor<StaticFilesServerConfiguration> config, ILogger<LightlessStaticFilesServerConfigurationController> logger) : base(config, logger)
    {
    }
}

public class LightlessBaseConfigurationController : LightlessConfigurationController<LightlessConfigurationBase>
{
    public LightlessBaseConfigurationController(IOptionsMonitor<LightlessConfigurationBase> config, ILogger<LightlessBaseConfigurationController> logger) : base(config, logger)
    {
    }
}

public class LightlessServerConfigurationController : LightlessConfigurationController<ServerConfiguration>
{
    public LightlessServerConfigurationController(IOptionsMonitor<ServerConfiguration> config, ILogger<LightlessServerConfigurationController> logger) : base(config, logger)
    {
    }
}

public class LightlessServicesConfigurationController : LightlessConfigurationController<ServicesConfiguration>
{
    public LightlessServicesConfigurationController(IOptionsMonitor<ServicesConfiguration> config, ILogger<LightlessServicesConfigurationController> logger) : base(config, logger)
    {
    }
}
#pragma warning restore MA0048 // File name must match type name
