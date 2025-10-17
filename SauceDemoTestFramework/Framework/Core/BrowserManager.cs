using Microsoft.Playwright;
using Framework.Core;

namespace Framework.Core;

/// <summary>
/// Manages browser instances and operations using Playwright
/// </summary>
public class BrowserManager : IBrowserManager
{
    private readonly IConfigurationManager _config;
    private readonly ILogger _logger;
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;

    public BrowserManager(IConfigurationManager config, ILogger logger)
    {
        _config = config;
        _logger = logger;
    }

    public IBrowser Browser => _browser;
    public IPage Page => _page;
    public IPlaywright Playwright => _playwright;

    public string CurrentUrl => _page?.Url ?? string.Empty;
    public string CurrentTitle => _page?.TitleAsync().Result ?? string.Empty;

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInfo("Initializing browser...");
            
            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            
            var browserType = _config.Browser.ToLower() switch
            {
                "chromium" => _playwright.Chromium,
                "firefox" => _playwright.Firefox,
                "webkit" => _playwright.Webkit,
                _ => _playwright.Chromium
            };

            var launchOptions = new BrowserTypeLaunchOptions
            {
                Headless = _config.Headless,
                SlowMo = _config.SlowMo
            };

            _browser = await browserType.LaunchAsync(launchOptions);
            
            var contextOptions = new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
                RecordVideoDir = _config.VideoRecording ? "videos/" : null,
                RecordVideoSize = _config.VideoRecording ? new RecordVideoSize { Width = 1920, Height = 1080 } : null
            };

            var context = await _browser.NewContextAsync(contextOptions);
            _page = await context.NewPageAsync();
            
            _page.SetDefaultTimeout(_config.Timeout);
            _page.SetDefaultNavigationTimeout(_config.Timeout);
            
            _logger.LogInfo($"Browser initialized successfully. Type: {_config.Browser}, Headless: {_config.Headless}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to initialize browser: {ex.Message}");
            throw;
        }
    }

    public async Task NavigateToAsync(string url)
    {
        try
        {
            _logger.LogStep($"Navigating to: {url}");
            await _page.GotoAsync(url);
            await WaitForLoadStateAsync(LoadState.NetworkIdle);
            _logger.LogInfo($"Successfully navigated to: {url}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to navigate to {url}: {ex.Message}");
            throw;
        }
    }

    public async Task<string> TakeScreenshotAsync(string? name = null)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = name != null ? $"{name}_{timestamp}.png" : $"screenshot_{timestamp}.png";
            var screenshotPath = Path.Combine("screenshots", fileName);
            
            Directory.CreateDirectory("screenshots");
            await _page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });
            
            _logger.LogInfo($"Screenshot saved: {screenshotPath}");
            return screenshotPath;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to take screenshot: {ex.Message}");
            throw;
        }
    }

    public async Task WaitForLoadStateAsync(LoadState state)
    {
        try
        {
            await _page.WaitForLoadStateAsync(state);
            _logger.LogDebug($"Page load state '{state}' reached");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to wait for load state '{state}': {ex.Message}");
            throw;
        }
    }

    public async Task CloseAsync()
    {
        try
        {
            _logger.LogInfo("Closing browser...");
            
            if (_page != null)
            {
                await _page.CloseAsync();
            }
            
            if (_browser != null)
            {
                await _browser.CloseAsync();
            }
            
            _playwright?.Dispose();
            
            _logger.LogInfo("Browser closed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error closing browser: {ex.Message}");
        }
    }
}
