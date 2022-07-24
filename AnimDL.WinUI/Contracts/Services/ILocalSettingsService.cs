namespace AnimDL.WinUI.Contracts.Services;

public interface ILocalSettingsService
{
    T ReadSetting<T>(string key);
    void SaveSetting<T>(string key, T value);
}
