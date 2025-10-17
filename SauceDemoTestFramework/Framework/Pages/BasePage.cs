using Microsoft.Playwright;
using Framework.Core;

namespace Framework.Pages;

/// <summary>
/// Base class for all page objects providing common functionality
/// </summary>
public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly ILogger Logger;
    protected readonly IConfigurationManager Config;

    protected BasePage(IPage page, ILogger logger, IConfigurationManager config)
    {
        Page = page;
        Logger = logger;
        Config = config;
    }

    /// <summary>
    /// Gets the page URL
    /// </summary>
    public abstract string PageUrl { get; }

    /// <summary>
    /// Gets the page title
    /// </summary>
    public abstract string PageTitle { get; }

    /// <summary>
    /// Navigates to the page
    /// </summary>
    public virtual async Task NavigateToPageAsync()
    {
        try
        {
            Logger.LogStep($"Navigating to {GetType().Name}");
            await Page.GotoAsync(PageUrl);
            await WaitForPageToLoadAsync();
            Logger.LogInfo($"Successfully navigated to {GetType().Name}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to navigate to {GetType().Name}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Waits for the page to load completely
    /// </summary>
    protected virtual async Task WaitForPageToLoadAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.WaitForSelectorAsync("body", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
    }

    /// <summary>
    /// Verifies that the current page is the expected page
    /// </summary>
    public virtual async Task<bool> IsOnPageAsync()
    {
        try
        {
            var currentUrl = Page.Url;
            var currentTitle = await Page.TitleAsync();
            
            var isCorrectUrl = currentUrl.Contains(PageUrl) || PageUrl.Contains(currentUrl);
            var isCorrectTitle = currentTitle.Contains(PageTitle) || PageTitle.Contains(currentTitle);
            
            Logger.LogAssertion($"Verify on {GetType().Name}", $"{PageUrl} with title '{PageTitle}'", 
                $"{currentUrl} with title '{currentTitle}'", isCorrectUrl && isCorrectTitle);
            
            return isCorrectUrl && isCorrectTitle;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error verifying page: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Clicks an element with retry logic
    /// </summary>
    protected async Task ClickElementAsync(ILocator locator, string elementDescription)
    {
        try
        {
            Logger.LogStep($"Clicking {elementDescription}");
            await locator.ClickAsync();
            Logger.LogInfo($"Successfully clicked {elementDescription}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to click {elementDescription}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Fills an input field with retry logic
    /// </summary>
    protected async Task FillInputAsync(ILocator locator, string value, string fieldDescription)
    {
        try
        {
            Logger.LogStep($"Filling {fieldDescription} with: {value}");
            await locator.FillAsync(value);
            Logger.LogInfo($"Successfully filled {fieldDescription}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to fill {fieldDescription}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Gets text from an element
    /// </summary>
    protected async Task<string> GetElementTextAsync(ILocator locator, string elementDescription)
    {
        try
        {
            var text = await locator.TextContentAsync();
            Logger.LogDebug($"Retrieved text from {elementDescription}: {text}");
            return text ?? string.Empty;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get text from {elementDescription}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Waits for an element to be visible
    /// </summary>
    protected async Task WaitForElementAsync(ILocator locator, string elementDescription, int timeoutMs = 5000)
    {
        try
        {
            Logger.LogDebug($"Waiting for {elementDescription} to be visible");
            await locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        }
        catch (Exception ex)
        {
            Logger.LogError($"Element {elementDescription} did not become visible within {timeoutMs}ms: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Checks if an element is visible
    /// </summary>
    protected async Task<bool> IsElementVisibleAsync(ILocator locator, string elementDescription)
    {
        try
        {
            var isVisible = await locator.IsVisibleAsync();
            Logger.LogDebug($"{elementDescription} visibility: {isVisible}");
            return isVisible;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error checking visibility of {elementDescription}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Takes a screenshot of the current page
    /// </summary>
    protected async Task<string> TakeScreenshotAsync(string? name = null)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = name != null ? $"{GetType().Name}_{name}_{timestamp}.png" : $"{GetType().Name}_{timestamp}.png";
            var screenshotPath = Path.Combine("screenshots", fileName);
            
            Directory.CreateDirectory("screenshots");
            await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });
            
            Logger.LogInfo($"Screenshot saved: {screenshotPath}");
            return screenshotPath;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to take screenshot: {ex.Message}");
            throw;
        }
    }
}
