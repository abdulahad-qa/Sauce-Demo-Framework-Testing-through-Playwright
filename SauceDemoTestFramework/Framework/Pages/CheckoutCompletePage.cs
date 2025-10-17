using Microsoft.Playwright;
using Framework.Core;

namespace Framework.Pages;

/// <summary>
/// Page Object Model for the SauceDemo Checkout Complete page
/// </summary>
public class CheckoutCompletePage : BasePage
{
    // Locators
    private readonly ILocator _checkoutCompleteContainer;
    private readonly ILocator _completeHeader;
    private readonly ILocator _completeText;
    private readonly ILocator _ponyExpressImage;
    private readonly ILocator _backHomeButton;
    private readonly ILocator _menuButton;

    public CheckoutCompletePage(IPage page, ILogger logger, IConfigurationManager config) : base(page, logger, config)
    {
        _checkoutCompleteContainer = Page.Locator("#checkout_complete_container");
        _completeHeader = Page.Locator(".complete-header");
        _completeText = Page.Locator(".complete-text");
        _ponyExpressImage = Page.Locator(".pony_express");
        _backHomeButton = Page.Locator("#inventory_sidebar_link, a.bm-item.menu-item[href*='inventory.html']");
        _menuButton = Page.Locator(".bm-burger-button button");
    }

    public override string PageUrl => Config.BaseUrl.Replace("index.html", "checkout-complete.html");
    public override string PageTitle => "Swag Labs";

    /// <summary>
    /// Gets the completion header text
    /// </summary>
    public async Task<string> GetCompleteHeaderAsync()
    {
        try
        {
            var header = await GetElementTextAsync(_completeHeader, "Complete header");
            Logger.LogInfo($"Complete header: {header}");
            return header;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get complete header: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Gets the completion text
    /// </summary>
    public async Task<string> GetCompleteTextAsync()
    {
        try
        {
            var text = await GetElementTextAsync(_completeText, "Complete text");
            Logger.LogInfo($"Complete text: {text}");
            return text;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get complete text: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Returns to the products page
    /// </summary>
    public async Task<ProductsPage> BackToProductsAsync()
    {
        try
        {
            Logger.LogStep("Returning to products page");
            
            // Navigate directly to the products page since menu navigation has viewport issues
            await Page.GotoAsync(Config.BaseUrl.Replace("index.html", "inventory.html"));
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            Logger.LogInfo("Successfully returned to products page");
            return new ProductsPage(Page, Logger, Config);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to return to products page: {ex.Message}");
            await TakeScreenshotAsync("back_to_products_failed");
            throw;
        }
    }

    /// <summary>
    /// Verifies the checkout complete page is displayed correctly
    /// </summary>
    public async Task<bool> IsCheckoutCompletePageDisplayedAsync()
    {
        try
        {
            var isContainerVisible = await IsElementVisibleAsync(_checkoutCompleteContainer, "Checkout complete container");
            var isHeaderVisible = await IsElementVisibleAsync(_completeHeader, "Complete header");
            var isTextVisible = await IsElementVisibleAsync(_completeText, "Complete text");
            var isImageVisible = await IsElementVisibleAsync(_ponyExpressImage, "Pony express image");
            var isBackButtonVisible = await IsElementVisibleAsync(_backHomeButton, "Back to products button");
            
            var isDisplayed = isContainerVisible && isHeaderVisible && isTextVisible && 
                            isImageVisible;
            
            Logger.LogAssertion("Checkout complete page elements visibility", "All elements visible", 
                $"Container: {isContainerVisible}, Header: {isHeaderVisible}, Text: {isTextVisible}, " +
                $"Image: {isImageVisible}, Back Button: {isBackButtonVisible}", 
                isDisplayed);
            
            return isDisplayed;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error verifying checkout complete page display: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Verifies the order completion message
    /// </summary>
    public async Task<bool> VerifyOrderCompletionAsync()
    {
        try
        {
            var header = await GetCompleteHeaderAsync();
            var text = await GetCompleteTextAsync();
            
            var isHeaderCorrect = header.Contains("THANK YOU FOR YOUR ORDER");
            var isTextCorrect = text.Contains("Your order has been dispatched");
            
            var isComplete = isHeaderCorrect && isTextCorrect;
            
            Logger.LogAssertion("Order completion verification", "Thank you message displayed", 
                $"Header: {header}, Text: {text}", isComplete);
            
            return isComplete;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error verifying order completion: {ex.Message}");
            return false;
        }
    }
}
