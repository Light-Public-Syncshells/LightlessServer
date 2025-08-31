using LightlessSync.API.Routes;
using LightlessSyncShared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LightlessSyncAuthService.Controllers;

[Route(LightlessAuth.User)]
public class UserController : Controller
{
    protected readonly ILogger Logger;
    protected readonly IDbContextFactory<LightlessDbContext> LightlessDbContextFactory;
    public UserController(ILogger<UserController> logger, , IDbContextFactory<LightlessDbContext> lightlessDbContext)
    {
        Logger = logger;
        LightlessDbContextFactory = lightlessDbContext;
    }

    [Authorize(Policy = "Internal")]
    [HttpPost(LightlessAuth.User_Unban_Uid)]
    public async Task UnBanUserByUid(string uid)
    {
        using var dbContext = await LightlessDbContextFactory.CreateDbContextAsync();

        Logger.LogInformation("Unbanning user with UID {UID}", uid);

        //Mark User as not banned, and not marked for ban (if marked)
        var auth = await dbContext.Auth.FirstOrDefaultAsync(f => f.UserUID == uid);
        if (auth != null)
        {
            auth.IsBanned = false;
            auth.MarkForBan = false;
        }

        // Remove all bans associated with this user
        var bannedFromLightlessIds = dbContext.BannedUsers.Where(b => b.BannedUid == uid);
        dbContext.BannedUsers.RemoveRange(bannedFromLightlessIds);

        // Remove all character/discord bans associated with this user
        var lodestoneAuths = dbContext.LodeStoneAuth.Where(l => l.User != null && l.User.UID == uid).ToList();
        foreach (var lodestoneAuth in lodestoneAuths)
        {
            var bannedRegs = dbContext.BannedRegistrations.Where(b => b.DiscordIdOrLodestoneAuth == lodestoneAuth.HashedLodestoneId || b.DiscordIdOrLodestoneAuth == lodestoneAuth.DiscordId.ToString());
            dbContext.BannedRegistrations.RemoveRange(bannedRegs);
        }

        await dbContext.SaveChangesAsync();
    }

    [Authorize(Policy = "Internal")]
    [HttpPost(LightlessAuth.User_Unban_Discord)]
    public async Task UnBanUserByDiscordId(string discordId)
    {
        Logger.LogInformation("Unbanning user with discordId: {discordId}", discordId);
        using var dbContext = await LightlessDbContextFactory.CreateDbContextAsync();

        var userByDiscord = await dbContext.LodeStoneAuth.Include(l => l.User).FirstOrDefaultAsync(l => l.DiscordId.ToString() == discordId);

        if (userByDiscord?.User == null)
        {
            Logger.LogInformation("Unbanning user with discordId: {discordId} but no user found", discordId);
            return;
        }
        var bannedRegs = dbContext.BannedRegistrations.Where(b => b.DiscordIdOrLodestoneAuth == discordId || b.DiscordIdOrLodestoneAuth == userByDiscord.HashedLodestoneId);
        //Mark User as not banned, and not marked for ban (if marked)
        var auth = await dbContext.Auth.FirstOrDefaultAsync(f => f.UserUID == userByDiscord.User.UID);
        if (auth != null)
        {
            auth.IsBanned = false;
            auth.MarkForBan = false;
        }
        // Remove all bans associated with this user
        var bannedFromLightlessIds = dbContext.BannedUsers.Where(b => b.BannedUid == auth.UserUID || b.BannedUid == auth.PrimaryUserUID);
        dbContext.BannedUsers.RemoveRange(bannedFromLightlessIds);

        await dbContext.SaveChangesAsync();
    }
}

