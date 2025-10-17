using Microsoft.Playwright;
using Framework.Core;

namespace Framework.Pages;

/// <summary>
/// Page Object Model for the SauceDemo Shopping Cart page
/// </summary>
public class CartPage : BasePage
{
    // Locators
    private readonly ILocator _cartContainer;
    private readonly ILocator _cartItems;
    private readonly ILocator _continueShoppingButton;
    private readonly ILocator _checkoutButton;
    private readonly ILocator _removeButtons;

    public CartPage(IPage page, ILogger logger, IConfigurationManager config) : base(page, logger, config)
    {
        _cartContainer = Page.Locator("#cart_contents_container");
        _cartItems = Page.Locator(".cart_item");
        _continueShoppingButton = Page.Locator("a.btn_secondary[href*='inventory.html']");
        _checkoutButton = Page.Locator("a.btn_action.checkout_button[href*='checkout-step-one.html']");
        _removeButtons = Page.Locator("button[data-test*='remove'], .cart_button");
    }

    public override string PageUrl => Config.BaseUrl.Replace("index.html", "cart.html");
    public override string PageTitle => "Swag Labs";

    /// <summary>
    /// Gets the number of items in the cart
    /// </summary>
    public async Task<int> GetCartItemCountAsync()
    {
        try
        {
            var count = await _cartItems.CountAsync();
            Logger.LogDebug($"Cart contains {count} items");
            return count;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get cart item count: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Gets all product names in the cart
    /// </summary>
    public async Task<List<string>> GetCartItemNamesAsync()
    {
        try
        {
            Logger.LogStep("Getting cart item names");
            
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
            
            Logger.LogInfo($"Cart contains {itemNames.Count} items: {string.Join(", ", itemNames)}");
            return itemNames;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get cart item names: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Gets all product prices in the cart
    /// </summary>
    public async Task<List<string>> GetCartItemPricesAsync()
    {
        try
        {
            Logger.LogStep("Getting cart item prices");
            
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
            
            Logger.LogInfo($"Cart item prices: {string.Join(", ", itemPrices)}");
            return itemPrices;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get cart item prices: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Removes a specific item from the cart by product name
    /// </summary>
    /// <param name="productName">Name of the product to remove</param>
    public async Task<CartPage> RemoveItemFromCartAsync(string productName)
    {
        try
        {
            Logger.LogStep($"Removing item from cart: {productName}");
            
            var itemToRemove = _cartItems.Filter(new LocatorFilterOptions { HasText = productName });
            var removeButton = itemToRemove.Locator("button[data-test*='remove'], .cart_button");
            
            // Wait for the item and remove button to be visible
            await itemToRemove.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
            await removeButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
            
            await ClickElementAsync(removeButton, $"Remove button for {productName}");
            
            Logger.LogInfo($"Successfully removed {productName} from cart");
            return this;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to remove item {productName} from cart: {ex.Message}");
            await TakeScreenshotAsync($"remove_cart_item_failed_{productName.Replace(" ", "_")}");
            throw;
        }
    }

    /// <summary>
    /// Removes all items from the cart
    /// </summary>
    public async Task<CartPage> RemoveAllItemsFromCartAsync()
    {
        try
        {
            Logger.LogStep("Removing all items from cart");
            
            var removeButtonCount = await _removeButtons.CountAsync();
            
            for (int i = removeButtonCount - 1; i >= 0; i--)
            {
                await _removeButtons.Nth(i).ClickAsync();
                await Page.WaitForTimeoutAsync(500); // Wait for item to be removed
            }
            
            Logger.LogInfo("Successfully removed all items from cart");
            return this;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to remove all items from cart: {ex.Message}");
            await TakeScreenshotAsync("remove_all_items_failed");
            throw;
        }
    }

    /// <summary>
    /// Continues shopping (goes back to products page)
    /// </summary>
    public async Task<ProductsPage> ContinueShoppingAsync()
    {
        try
        {
            Logger.LogStep("Continuing shopping");
            
            // Wait for the button to be visible and clickable
            await _continueShoppingButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
            await ClickElementAsync(_continueShoppingButton, "Continue shopping button");
            await Page.WaitForURLAsync("**/inventory.html");
            
            Logger.LogInfo("Successfully navigated back to products page");
            return new ProductsPage(Page, Logger, Config);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to continue shopping: {ex.Message}");
            await TakeScreenshotAsync("continue_shopping_failed");
            throw;
        }
    }

    /// <summary>
    /// Proceeds to checkout
    /// </summary>
    public async Task<CheckoutStepOnePage> ProceedToCheckoutAsync()
    {
        try
        {
            Logger.LogStep("Proceeding to checkout");
            
            // Wait for the button to be visible and clickable
            await _checkoutButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
            await ClickElementAsync(_checkoutButton, "Checkout button");
            await Page.WaitForURLAsync("**/checkout-step-one.html");
            
            Logger.LogInfo("Successfully navigated to checkout step one");
            return new CheckoutStepOnePage(Page, Logger, Config);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to proceed to checkout: {ex.Message}");
            await TakeScreenshotAsync("proceed_to_checkout_failed");
            throw;
        }
    }

    /// <summary>
    /// Checks if the cart is empty
    /// </summary>
    public async Task<bool> IsCartEmptyAsync()
    {
        try
        {
            var itemCount = await GetCartItemCountAsync();
            var isEmpty = itemCount == 0;
            
            Logger.LogDebug($"Cart is empty: {isEmpty}");
            return isEmpty;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error checking if cart is empty: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Verifies the cart page is displayed correctly
    /// </summary>
    public async Task<bool> IsCartPageDisplayedAsync()
    {
        try
        {
            // Check if we're on the cart page URL
            var currentUrl = Page.Url;
            var isOnCartPage = currentUrl.Contains("cart.html");
            
            // Check if cart container is visible
            var isContainerVisible = await IsElementVisibleAsync(_cartContainer, "Cart container");
            
            // For cart page, we only need the container to be visible
            // Buttons might not be visible if cart is empty
            var isDisplayed = isOnCartPage && isContainerVisible;
            
            Logger.LogAssertion("Cart page elements visibility", "Cart page displayed", 
                $"URL: {isOnCartPage}, Container: {isContainerVisible}", 
                isDisplayed);
            
            return isDisplayed;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error verifying cart page display: {ex.Message}");
            return false;
        }
    }
}
