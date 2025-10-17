using Microsoft.Playwright;
using Framework.Core;

namespace Framework.Pages;

/// <summary>
/// Page Object Model for the SauceDemo Checkout Step Two page
/// </summary>
public class CheckoutStepTwoPage : BasePage
{
    // Locators
    private readonly ILocator _checkoutContainer;
    private readonly ILocator _cartItems;
    private readonly ILocator _subtotalLabel;
    private readonly ILocator _taxLabel;
    private readonly ILocator _totalLabel;
    private readonly ILocator _finishButton;
    private readonly ILocator _cancelButton;

    public CheckoutStepTwoPage(IPage page, ILogger logger, IConfigurationManager config) : base(page, logger, config)
    {
        _checkoutContainer = Page.Locator("#checkout_summary_container");
        _cartItems = Page.Locator(".cart_item");
        _subtotalLabel = Page.Locator(".summary_subtotal_label");
        _taxLabel = Page.Locator(".summary_tax_label");
        _totalLabel = Page.Locator(".summary_total_label");
        _finishButton = Page.Locator("a.btn_action.cart_button[href*='checkout-complete.html']");
        _cancelButton = Page.Locator("a.cart_cancel_link.btn_secondary[href*='inventory.html']");
    }

    public override string PageUrl => Config.BaseUrl.Replace("index.html", "checkout-step-two.html");
    public override string PageTitle => "Swag Labs";

    /// <summary>
    /// Gets the subtotal amount
    /// </summary>
    public async Task<string> GetSubtotalAsync()
    {
        try
        {
            var subtotal = await GetElementTextAsync(_subtotalLabel, "Subtotal label");
            Logger.LogInfo($"Subtotal: {subtotal}");
            return subtotal;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get subtotal: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Gets the tax amount
    /// </summary>
    public async Task<string> GetTaxAsync()
    {
        try
        {
            var tax = await GetElementTextAsync(_taxLabel, "Tax label");
            Logger.LogInfo($"Tax: {tax}");
            return tax;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get tax: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Gets the total amount
    /// </summary>
    public async Task<string> GetTotalAsync()
    {
        try
        {
            var total = await GetElementTextAsync(_totalLabel, "Total label");
            Logger.LogInfo($"Total: {total}");
            return total;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get total: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Gets all item names in the checkout summary
    /// </summary>
    public async Task<List<string>> GetCheckoutItemNamesAsync()
    {
        try
        {
            Logger.LogStep("Getting checkout item names");
            
            var itemNameElements = _cartItems.Locator(".inventory_item_name");
            var count = await itemNameElements.CountAsync();
            var itemNames = new List<string>();
            
            for (int i = 0; i < count; i++)
            {
                var name = await itemNameElements.Nth(i).TextContentAsync();
                if (!string.IsNullOrEmpty(name))
                {
                    itemNames.Add(name.Trim());
                }
            }
            
            Logger.LogInfo($"Checkout contains {itemNames.Count} items: {string.Join(", ", itemNames)}");
            return itemNames;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get checkout item names: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Gets all item prices in the checkout summary
    /// </summary>
    public async Task<List<string>> GetCheckoutItemPricesAsync()
    {
        try
        {
            Logger.LogStep("Getting checkout item prices");
            
            var itemPriceElements = _cartItems.Locator(".inventory_item_price");
            var count = await itemPriceElements.CountAsync();
            var itemPrices = new List<string>();
            
            for (int i = 0; i < count; i++)
            {
                var price = await itemPriceElements.Nth(i).TextContentAsync();
                if (!string.IsNullOrEmpty(price))
                {
                    itemPrices.Add(price.Trim());
                }
            }
            
            Logger.LogInfo($"Checkout item prices: {string.Join(", ", itemPrices)}");
            return itemPrices;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get checkout item prices: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Gets the number of items in the checkout summary
    /// </summary>
    public async Task<int> GetCheckoutItemCountAsync()
    {
        try
        {
            var count = await _cartItems.CountAsync();
            Logger.LogDebug($"Checkout contains {count} items");
            return count;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get checkout item count: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Finishes the checkout process
    /// </summary>
    public async Task<CheckoutCompletePage> FinishCheckoutAsync()
    {
        try
        {
            Logger.LogStep("Finishing checkout process");
            await ClickElementAsync(_finishButton, "Finish button");
            await Page.WaitForURLAsync("**/checkout-complete.html");
            
            Logger.LogInfo("Successfully completed checkout");
            return new CheckoutCompletePage(Page, Logger, Config);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to finish checkout: {ex.Message}");
            await TakeScreenshotAsync("finish_checkout_failed");
            throw;
        }
    }

    /// <summary>
    /// Cancels the checkout process and returns to products page
    /// </summary>
    public async Task<ProductsPage> CancelCheckoutAsync()
    {
        try
        {
            Logger.LogStep("Canceling checkout process");
            await ClickElementAsync(_cancelButton, "Cancel button");
            await Page.WaitForURLAsync("**/inventory.html");
            
            Logger.LogInfo("Successfully canceled checkout and returned to products page");
            return new ProductsPage(Page, Logger, Config);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to cancel checkout: {ex.Message}");
            await TakeScreenshotAsync("cancel_checkout_failed");
            throw;
        }
    }

    /// <summary>
    /// Verifies the checkout step two page is displayed correctly
    /// </summary>
    public async Task<bool> IsCheckoutStepTwoPageDisplayedAsync()
    {
        try
        {
            var isContainerVisible = await IsElementVisibleAsync(_checkoutContainer, "Checkout container");
            var isSubtotalVisible = await IsElementVisibleAsync(_subtotalLabel, "Subtotal label");
            var isTaxVisible = await IsElementVisibleAsync(_taxLabel, "Tax label");
            var isTotalVisible = await IsElementVisibleAsync(_totalLabel, "Total label");
            var isFinishVisible = await IsElementVisibleAsync(_finishButton, "Finish button");
            var isCancelVisible = await IsElementVisibleAsync(_cancelButton, "Cancel button");
            
            var isDisplayed = isContainerVisible && isSubtotalVisible && isTaxVisible && 
                            isTotalVisible && isFinishVisible && isCancelVisible;
            
            Logger.LogAssertion("Checkout step two page elements visibility", "All elements visible", 
                $"Container: {isContainerVisible}, Subtotal: {isSubtotalVisible}, Tax: {isTaxVisible}, " +
                $"Total: {isTotalVisible}, Finish: {isFinishVisible}, Cancel: {isCancelVisible}", 
                isDisplayed);
            
            return isDisplayed;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error verifying checkout step two page display: {ex.Message}");
            return false;
        }
    }
}
