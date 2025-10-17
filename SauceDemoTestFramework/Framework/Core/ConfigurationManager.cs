using System.Text.Json;
using Framework.Models;

namespace Framework.Core;

/// <summary>
/// Manages test configuration settings from appsettings.json
/// </summary>
public class ConfigurationManager : IConfigurationManager
{
    private readonly JsonDocument _config;
    private readonly JsonElement _testSettings;
    private readonly JsonElement _testData;

    public ConfigurationManager()
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        var configJson = File.ReadAllText(configPath);
        _config = JsonDocument.Parse(configJson);
        _testSettings = _config.RootElement.GetProperty("TestSettings");
        _testData = _config.RootElement.GetProperty("TestData");
    }

    public string BaseUrl => _testSettings.GetProperty("BaseUrl").GetString() ?? throw new InvalidOperationException("BaseUrl not configured");
    
    public string Browser => _testSettings.GetProperty("Browser").GetString() ?? "Chromium";
    
    public bool Headless => _testSettings.GetProperty("Headless").GetBoolean();
    
    public int SlowMo => _testSettings.GetProperty("SlowMo").GetInt32();
    
    public int Timeout => _testSettings.GetProperty("Timeout").GetInt32();
    
    public int ImplicitWait => _testSettings.GetProperty("ImplicitWait").GetInt32();
    
    public bool ScreenshotOnFailure => _testSettings.GetProperty("ScreenshotOnFailure").GetBoolean();
    
    public bool VideoRecording => _testSettings.GetProperty("VideoRecording").GetBoolean();
    
    public bool TraceOnFailure => _testSettings.GetProperty("TraceOnFailure").GetBoolean();

    public UserCredentials GetUserCredentials(string userType)
    {
        var users = _testData.GetProperty("Users");
        var user = users.GetProperty(userType);
        
        return new UserCredentials
        {
            Username = user.GetProperty("Username").GetString() ?? throw new InvalidOperationException($"Username not found for user type: {userType}"),
            Password = user.GetProperty("Password").GetString() ?? throw new InvalidOperationException($"Password not found for user type: {userType}")
        };
    }

    public string? GetValue(string key)
    {
        try
        {
            return _testSettings.GetProperty(key).GetString();
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        _config?.Dispose();
    }
}
