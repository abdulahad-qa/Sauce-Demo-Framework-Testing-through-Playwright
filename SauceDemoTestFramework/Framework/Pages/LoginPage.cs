using Microsoft.Playwright;
using Framework.Core;
using Framework.Models;

namespace Framework.Pages;

/// <summary>
/// Page Object Model for the SauceDemo Login page
/// </summary>
public class LoginPage : BasePage
{
    // Locators
    private readonly ILocator _usernameField;
    private readonly ILocator _passwordField;
    private readonly ILocator _loginButton;
    private readonly ILocator _errorMessage;
    private readonly ILocator _loginContainer;

    public LoginPage(IPage page, ILogger logger, IConfigurationManager config) : base(page, logger, config)
    {
        _usernameField = Page.Locator("#user-name");
        _passwordField = Page.Locator("#password");
        _loginButton = Page.Locator("#login-button");
        _errorMessage = Page.Locator("[data-test='error']");
        _loginContainer = Page.Locator("form");
    }

    public override string PageUrl => Config.BaseUrl;
    public override string PageTitle => "Swag Labs";

    /// <summary>
    /// Performs login with provided credentials
    /// </summary>
    /// <param name="credentials">User credentials</param>
    public async Task<ProductsPage> LoginAsync(UserCredentials credentials)
    {
        try
        {
            Logger.LogStep($"Logging in with user: {credentials.Username}");
            
            // Wait for page to load completely
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Try to find any form element first
            var formExists = await Page.Locator("form").CountAsync() > 0;
            Logger.LogInfo($"Form element found: {formExists}");
            
            // Wait for username field to be visible
            await WaitForElementAsync(_usernameField, "Username field", 10000);
            await FillInputAsync(_usernameField, credentials.Username, "Username field");
            
            // Wait for password field to be visible
            await WaitForElementAsync(_passwordField, "Password field", 10000);
            await FillInputAsync(_passwordField, credentials.Password, "Password field");
            
            // Wait for login button to be visible
            await WaitForElementAsync(_loginButton, "Login button", 10000);
            await ClickElementAsync(_loginButton, "Login button");
            
            // Wait for navigation to products page
            await Page.WaitForURLAsync("**/inventory.html", new PageWaitForURLOptions { Timeout = 10000 });
            
            Logger.LogInfo("Login successful");
            return new ProductsPage(Page, Logger, Config);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Login failed: {ex.Message}");
            await TakeScreenshotAsync("login_failed");
            throw;
        }
    }

    /// <summary>
    /// Attempts login with invalid credentials
    /// </summary>
    /// <param name="credentials">Invalid user credentials</param>
    public async Task<LoginPage> LoginWithInvalidCredentialsAsync(UserCredentials credentials)
    {
        try
        {
            Logger.LogStep($"Attempting login with invalid credentials: {credentials.Username}");
            
            await WaitForElementAsync(_loginContainer, "Login container");
            await FillInputAsync(_usernameField, credentials.Username, "Username field");
            await FillInputAsync(_passwordField, credentials.Password, "Password field");
            await ClickElementAsync(_loginButton, "Login button");
            
            // Wait for error message to appear
            await WaitForElementAsync(_errorMessage, "Error message");
            
            Logger.LogInfo("Login failed as expected with invalid credentials");
            return this;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Unexpected error during invalid login: {ex.Message}");
            await TakeScreenshotAsync("invalid_login_error");
            throw;
        }
    }

    /// <summary>
    /// Gets the error message text
    /// </summary>
    public async Task<string> GetErrorMessageAsync()
    {
        try
        {
            var errorText = await GetElementTextAsync(_errorMessage, "Error message");
            Logger.LogInfo($"Error message displayed: {errorText}");
            return errorText;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get error message: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Checks if error message is displayed
    /// </summary>
    public async Task<bool> IsErrorMessageDisplayedAsync()
    {
        try
        {
            var isVisible = await IsElementVisibleAsync(_errorMessage, "Error message");
            Logger.LogDebug($"Error message visible: {isVisible}");
            return isVisible;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error checking error message visibility: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Clears the login form
    /// </summary>
    public async Task ClearLoginFormAsync()
    {
        try
        {
            Logger.LogStep("Clearing login form");
            await _usernameField.ClearAsync();
            await _passwordField.ClearAsync();
            Logger.LogInfo("Login form cleared");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to clear login form: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Verifies the login page is displayed correctly
    /// </summary>
    public async Task<bool> IsLoginPageDisplayedAsync()
    {
        try
        {
            // Check if we're on the login page by URL
            var currentUrl = Page.Url;
            var isOnLoginPage = currentUrl.Contains("index.html") || currentUrl.Contains("saucedemo.com");
            
            // Check if key elements are visible
            var isUsernameVisible = await IsElementVisibleAsync(_usernameField, "Username field");
            var isPasswordVisible = await IsElementVisibleAsync(_passwordField, "Password field");
            var isLoginButtonVisible = await IsElementVisibleAsync(_loginButton, "Login button");
            
            var isDisplayed = isOnLoginPage && isUsernameVisible && isPasswordVisible && isLoginButtonVisible;
            
            Logger.LogAssertion("Login page elements visibility", "All elements visible", 
                $"URL: {isOnLoginPage}, Username: {isUsernameVisible}, Password: {isPasswordVisible}, Button: {isLoginButtonVisible}", 
                isDisplayed);
            
            return isDisplayed;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error verifying login page display: {ex.Message}");
            return false;
        }
    }
}
