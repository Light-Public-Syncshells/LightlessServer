using LightlessSync.API.SignalR;
using LightlessSyncServer.Hubs;
using LightlessSyncShared.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace LightlessSyncServer.Controllers;

[Route("/msgc")]
[Authorize(Policy = "Internal")]
public class ClientMessageController : Controller
{
    private ILogger<ClientMessageController> _logger;
    private IHubContext<LightlessHub, ILightlessHub> _hubContext;

    public ClientMessageController(ILogger<ClientMessageController> logger, IHubContext<LightlessHub, ILightlessHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    [Route("sendMessage")]
    [HttpPost]
    public async Task<IActionResult> SendMessage(ClientMessage msg)
    {
        bool hasUid = !string.IsNullOrEmpty(msg.UID);

        if (!hasUid)
        {
            _logger.LogInformation("Sending Message of severity {severity} to all online users: {message}", msg.Severity, msg.Message);
            await _hubContext.Clients.All.Client_ReceiveServerMessage(msg.Severity, msg.Message).ConfigureAwait(false);
        }
        else
        {
            _logger.LogInformation("Sending Message of severity {severity} to user {uid}: {message}", msg.Severity, msg.UID, msg.Message);
            await _hubContext.Clients.User(msg.UID).Client_ReceiveServerMessage(msg.Severity, msg.Message).ConfigureAwait(false);
        }

        return Empty;
    }
}
