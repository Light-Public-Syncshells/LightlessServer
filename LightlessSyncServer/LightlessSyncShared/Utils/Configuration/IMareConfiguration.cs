namespace LightlessSyncShared.Utils.Configuration;

public interface ILightlessConfiguration
{
    T GetValueOrDefault<T>(string key, T defaultValue);
    T GetValue<T>(string key);
    string SerializeValue(string key, string defaultValue);
}
