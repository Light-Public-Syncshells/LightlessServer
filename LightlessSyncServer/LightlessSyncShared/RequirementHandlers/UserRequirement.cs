using Microsoft.AspNetCore.Authorization;

namespace LightlessSyncShared.RequirementHandlers;

public class UserRequirement : IAuthorizationRequirement
{
    public UserRequirement(UserRequirements requirements)
    {
        Requirements = requirements;
    }

    public UserRequirements Requirements { get; }
}
