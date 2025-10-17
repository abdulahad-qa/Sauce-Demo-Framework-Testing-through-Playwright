using Microsoft.Playwright;

namespace Framework.Core;

/// <summary>
/// Interface for managing browser instances and operations
/// </summary>
public interface IBrowserManager
{
    /// <summary>
    /// Gets the current browser instance
    /// </summary>
    IBrowser Browser { get; }
    
    /// <summary>
    /// Gets the current page instance
    /// </summary>
    IPage Page { get; }
    
    /// <summary>
    /// Gets the Playwright instance
    /// </summary>
    IPlaywright Playwright { get; }
    
    /// <summary>
    /// Initializes the browser with configuration
    /// </summary>
    Task InitializeAsync();
    
    /// <summary>
    /// Navigates to a specific URL
    /// </summary>
    /// <param name="url">The URL to navigate to</param>
    Task NavigateToAsync(string url);
    
    /// <summary>
    /// Takes a screenshot with optional custom name
    /// </summary>
    /// <param name="name">Optional custom name for the screenshot</param>
    Task<string> TakeScreenshotAsync(string? name = null);
    
    /// <summary>
    /// Closes the browser and disposes resources
    /// </summary>
    Task CloseAsync();
    
    /// <summary>
    /// Waits for page to be in a specific state
    /// </summary>
    /// <param name="state">The load state to wait for</param>
    Task WaitForLoadStateAsync(LoadState state);
    
    /// <summary>
    /// Gets the current page URL
    /// </summary>
    string CurrentUrl { get; }
    
    /// <summary>
    /// Gets the current page title
    /// </summary>
    string CurrentTitle { get; }
}
