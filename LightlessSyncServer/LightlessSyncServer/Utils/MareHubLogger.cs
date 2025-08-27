using LightlessSync.API.SignalR;
using LightlessSyncServer.Hubs;
using System.Runtime.CompilerServices;

namespace LightlessSyncServer.Utils;

public class LightlessHubLogger
{
    private readonly LightlessHub _hub;
    private readonly ILogger<LightlessHub> _logger;

    public LightlessHubLogger(LightlessHub hub, ILogger<LightlessHub> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public static object[] Args(params object[] args)
    {
        return args;
    }

    public void LogCallInfo(object[] args = null, [CallerMemberName] string methodName = "")
    {
        string formattedArgs = args != null && args.Length != 0 ? "|" + string.Join(":", args) : string.Empty;
        _logger.LogInformation("{uid}:{method}{args}", _hub.UserUID, methodName, formattedArgs);
    }

    public void LogCallWarning(object[] args = null, [CallerMemberName] string methodName = "")
    {
        string formattedArgs = args != null && args.Length != 0 ? "|" + string.Join(":", args) : string.Empty;
        _logger.LogWarning("{uid}:{method}{args}", _hub.UserUID, methodName, formattedArgs);
    }
}
