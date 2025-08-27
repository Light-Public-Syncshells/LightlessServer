using Microsoft.AspNetCore.SignalR;
using LightlessSync.API.SignalR;
using LightlessSyncServer.Hubs;

namespace LightlessSyncStaticFilesServer.Services;

public class MainClientReadyMessageService : IClientReadyMessageService
{
    private readonly ILogger<MainClientReadyMessageService> _logger;
    private readonly IHubContext<LightlessHub> _lightlessHub;

    public MainClientReadyMessageService(ILogger<MainClientReadyMessageService> logger, IHubContext<LightlessHub> lightlessHub)
    {
        _logger = logger;
        _lightlessHub = lightlessHub;
    }

    public async Task SendDownloadReady(string uid, Guid requestId)
    {
        _logger.LogInformation("Sending Client Ready for {uid}:{requestId} to SignalR", uid, requestId);
        await _lightlessHub.Clients.User(uid).SendAsync(nameof(ILightlessHub.Client_DownloadReady), requestId).ConfigureAwait(false);
    }
}
