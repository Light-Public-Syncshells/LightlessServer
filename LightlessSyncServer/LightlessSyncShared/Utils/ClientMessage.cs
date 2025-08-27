using LightlessSync.API.Data.Enum;

namespace LightlessSyncShared.Utils;
public record ClientMessage(MessageSeverity Severity, string Message, string UID);
