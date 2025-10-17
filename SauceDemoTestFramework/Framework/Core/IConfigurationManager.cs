using System.Text.Json;
using Framework.Models;

namespace Framework.Core;

/// <summary>
/// Interface for managing test configuration settings
/// </summary>
public interface IConfigurationManager
{
    /// <summary>
    /// Gets the base URL for the application under test
    /// </summary>
    string BaseUrl { get; }
    
    /// <summary>
    /// Gets the browser type to use for testing
    /// </summary>
    string Browser { get; }
    
    /// <summary>
    /// Gets whether to run tests in headless mode
    /// </summary>
    bool Headless { get; }
    
    /// <summary>
    /// Gets the slow motion delay in milliseconds
    /// </summary>
    int SlowMo { get; }
    
    /// <summary>
    /// Gets the default timeout in milliseconds
    /// </summary>
    int Timeout { get; }
    
    /// <summary>
    /// Gets the implicit wait time in milliseconds
    /// </summary>
    int ImplicitWait { get; }
    
    /// <summary>
    /// Gets whether to take screenshots on test failure
    /// </summary>
    bool ScreenshotOnFailure { get; }
    
    /// <summary>
    /// Gets whether to record videos during test execution
    /// </summary>
    bool VideoRecording { get; }
    
    /// <summary>
    /// Gets whether to capture traces on test failure
    /// </summary>
    bool TraceOnFailure { get; }
    
    /// <summary>
    /// Gets test data for a specific user type
    /// </summary>
    /// <param name="userType">The type of user</param>
    /// <returns>User credentials</returns>
    UserCredentials GetUserCredentials(string userType);
    
    /// <summary>
    /// Gets a configuration value by key
    /// </summary>
    /// <param name="key">The configuration key</param>
    /// <returns>The configuration value</returns>
    string? GetValue(string key);
}
