using Microsoft.Playwright;
using Framework.Core;
using Framework.Models;

namespace Framework.Pages;

/// <summary>
/// Page Object Model for the SauceDemo Products/Inventory page
/// </summary>
public class ProductsPage : BasePage
{
    // Locators
    private readonly ILocator _productContainer;
    private readonly ILocator _shoppingCartBadge;
    private readonly ILocator _shoppingCartLink;
    private readonly ILocator _menuButton;
    private readonly ILocator _logoutLink;
    private readonly ILocator _resetAppStateLink;
    private readonly ILocator _sortDropdown;
    private readonly ILocator _productItems;
    
    // Footer locators
    private readonly ILocator _footer;
    private readonly ILocator _twitterLink;
    private readonly ILocator _facebookLink;
    private readonly ILocator _linkedinLink;
    private readonly ILocator _footerCopyright;

    public ProductsPage(IPage page, ILogger logger, IConfigurationManager config) : base(page, logger, config)
    {
        _productContainer = Page.Locator("#inventory_container").First;
        _shoppingCartBadge = Page.Locator(".shopping_cart_badge");
        _shoppingCartLink = Page.Locator(".shopping_cart_link");
        _menuButton = Page.Locator(".bm-burger-button button");
        _logoutLink = Page.Locator("#logout_sidebar_link");
        _resetAppStateLink = Page.Locator("#reset_sidebar_link");
        _sortDropdown = Page.Locator(".product_sort_container");
        _productItems = Page.Locator(".inventory_item");
        
        // Footer locators
        _footer = Page.Locator(".footer");
        _twitterLink = Page.Locator(".social_twitter");
        _facebookLink = Page.Locator(".social_facebook");
        _linkedinLink = Page.Locator(".social_linkedin");
        _footerCopyright = Page.Locator(".footer_copy");
    }

    public override string PageUrl => Config.BaseUrl.Replace("index.html", "inventory.html");
    public override string PageTitle => "Swag Labs";

    /// <summary>
    /// Adds a product to the cart by product name
    /// </summary>
    /// <param name="productName">Name of the product to add</param>
    public async Task<ProductsPage> AddProductToCartAsync(string productName)
    {
        try
        {
            Logger.LogStep($"Adding product to cart: {productName}");
            
            var productItem = _productItems.Filter(new LocatorFilterOptions { HasText = productName });
            var addToCartButton = productItem.Locator("button").Filter(new LocatorFilterOptions { HasText = "Add to cart" });
            
            await WaitForElementAsync(productItem, $"Product item: {productName}");
            await ClickElementAsync(addToCartButton, $"Add to cart button for {productName}");
            
            Logger.LogInfo($"Successfully added {productName} to cart");
            return this;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to add product {productName} to cart: {ex.Message}");
            await TakeScreenshotAsync($"add_product_failed_{productName.Replace(" ", "_")}");
            throw;
        }
    }

    /// <summary>
    /// Removes a product from the cart by product name
    /// </summary>
    /// <param name="productName">Name of the product to remove</param>
    public async Task<ProductsPage> RemoveProductFromCartAsync(string productName)
    {
        try
        {
            Logger.LogStep($"Removing product from cart: {productName}");
            
            var productItem = _productItems.Filter(new LocatorFilterOptions { HasText = productName });
            var removeButton = productItem.Locator("button").Filter(new LocatorFilterOptions { HasText = "Remove" });
            
            await WaitForElementAsync(productItem, $"Product item: {productName}");
            await ClickElementAsync(removeButton, $"Remove button for {productName}");
            
            Logger.LogInfo($"Successfully removed {productName} from cart");
            return this;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to remove product {productName} from cart: {ex.Message}");
            await TakeScreenshotAsync($"remove_product_failed_{productName.Replace(" ", "_")}");
            throw;
        }
    }

    /// <summary>
    /// Gets the cart item count from the badge
    /// </summary>
    public async Task<int> GetCartItemCountAsync()
    {
        try
        {
            var isBadgeVisible = await IsElementVisibleAsync(_shoppingCartBadge, "Shopping cart badge");
            if (!isBadgeVisible)
            {
                Logger.LogDebug("Shopping cart badge not visible, cart is empty");
                return 0;
            }

            var badgeText = await GetElementTextAsync(_shoppingCartBadge, "Shopping cart badge");
            var count = int.TryParse(badgeText, out var result) ? result : 0;
            
            Logger.LogDebug($"Cart item count: {count}");
            return count;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get cart item count: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Navigates to the shopping cart
    /// </summary>
    public async Task<CartPage> GoToCartAsync()
    {
        try
        {
            Logger.LogStep("Navigating to shopping cart");
            await ClickElementAsync(_shoppingCartLink, "Shopping cart link");
            await Page.WaitForURLAsync("**/cart.html");
            
            Logger.LogInfo("Successfully navigated to shopping cart");
            return new CartPage(Page, Logger, Config);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to navigate to cart: {ex.Message}");
            await TakeScreenshotAsync("navigate_to_cart_failed");
            throw;
        }
    }

    /// <summary>
    /// Sorts products by the specified option
    /// </summary>
    /// <param name="sortOption">Sort option (Name (A to Z), Name (Z to A), Price (low to high), Price (high to low))</param>
    public async Task<ProductsPage> SortProductsAsync(string sortOption)
    {
        try
        {
            Logger.LogStep($"Sorting products by: {sortOption}");
            
            await WaitForElementAsync(_sortDropdown, "Sort dropdown");
            await _sortDropdown.SelectOptionAsync(new SelectOptionValue { Label = sortOption });
            
            // Wait for products to re-render
            await Page.WaitForTimeoutAsync(1000);
            
            Logger.LogInfo($"Successfully sorted products by: {sortOption}");
            return this;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to sort products by {sortOption}: {ex.Message}");
            await TakeScreenshotAsync($"sort_failed_{sortOption.Replace(" ", "_")}");
            throw;
        }
    }

    /// <summary>
    /// Gets all product names on the page
    /// </summary>
    public async Task<List<string>> GetProductNamesAsync()
    {
        try
        {
            Logger.LogStep("Getting all product names");
            
            var productNameElements = _productItems.Locator(".inventory_item_name");
            var count = await productNameElements.CountAsync();
            var productNames = new List<string>();
            
            for (int i = 0; i < count; i++)
            {
                var name = await productNameElements.Nth(i).TextContentAsync();
                if (!string.IsNullOrEmpty(name))
                {
                    productNames.Add(name.Trim());
                }
            }
            
            Logger.LogInfo($"Found {productNames.Count} products: {string.Join(", ", productNames)}");
            return productNames;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get product names: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Gets all product prices on the page
    /// </summary>
    public async Task<List<string>> GetProductPricesAsync()
    {
        try
        {
            Logger.LogStep("Getting all product prices");
            
            var productPriceElements = _productItems.Locator(".inventory_item_price");
            var count = await productPriceElements.CountAsync();
            var productPrices = new List<string>();
            
            for (int i = 0; i < count; i++)
            {
                var price = await productPriceElements.Nth(i).TextContentAsync();
                if (!string.IsNullOrEmpty(price))
                {
                    productPrices.Add(price.Trim());
                }
            }
            
            Logger.LogInfo($"Found {productPrices.Count} product prices: {string.Join(", ", productPrices)}");
            return productPrices;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get product prices: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Resets the app state (clears cart and resets product buttons)
    /// </summary>
    public async Task<ProductsPage> ResetAppStateAsync()
    {
        try
        {
            Logger.LogStep("Resetting app state");
            
            await ClickElementAsync(_menuButton, "Menu button");
            await WaitForElementAsync(_resetAppStateLink, "Reset app state link");
            await ClickElementAsync(_resetAppStateLink, "Reset app state link");
            
            // Wait for the menu to close and state to reset
            await Page.WaitForTimeoutAsync(1000);
            
            Logger.LogInfo("Successfully reset app state");
            return this;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to reset app state: {ex.Message}");
            await TakeScreenshotAsync("reset_app_state_failed");
            throw;
        }
    }

    /// <summary>
    /// Checks if a product shows "Remove" button (indicating it's in cart)
    /// </summary>
    /// <param name="productName">Name of the product to check</param>
    public async Task<bool> IsProductInCartAsync(string productName)
    {
        try
        {
            var productItem = _productItems.Filter(new LocatorFilterOptions { HasText = productName });
            var removeButton = productItem.Locator("button").Filter(new LocatorFilterOptions { HasText = "Remove" });
            
            var isRemoveButtonVisible = await IsElementVisibleAsync(removeButton, $"Remove button for {productName}");
            
            Logger.LogDebug($"Product {productName} in cart: {isRemoveButtonVisible}");
            return isRemoveButtonVisible;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to check if product {productName} is in cart: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Logs out from the application
    /// </summary>
    public async Task<LoginPage> LogoutAsync()
    {
        try
        {
            Logger.LogStep("Logging out from the application");
            
            await ClickElementAsync(_menuButton, "Menu button");
            await WaitForElementAsync(_logoutLink, "Logout link");
            await ClickElementAsync(_logoutLink, "Logout link");
            
            await Page.WaitForURLAsync("**/index.html");
            
            Logger.LogInfo("Successfully logged out");
            return new LoginPage(Page, Logger, Config);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to logout: {ex.Message}");
            await TakeScreenshotAsync("logout_failed");
            throw;
        }
    }

    /// <summary>
    /// Verifies the products page is displayed correctly
    /// </summary>
    public async Task<bool> IsProductsPageDisplayedAsync()
    {
        try
        {
            var isContainerVisible = await IsElementVisibleAsync(_productContainer, "Product container");
            var isCartVisible = await IsElementVisibleAsync(_shoppingCartLink, "Shopping cart link");
            
            // Check if we're on the products page by URL and key elements
            var currentUrl = Page.Url;
            var isOnProductsPage = currentUrl.Contains("inventory.html");
            
            var isDisplayed = isContainerVisible && isCartVisible && isOnProductsPage;
            
            Logger.LogAssertion("Products page elements visibility", "All elements visible", 
                $"Container: {isContainerVisible}, Cart: {isCartVisible}, URL: {isOnProductsPage}", 
                isDisplayed);
            
            return isDisplayed;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error verifying products page display: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Checks if footer is visible on the page
    /// </summary>
    public async Task<bool> IsFooterVisibleAsync()
    {
        try
        {
            var isFooterVisible = await IsElementVisibleAsync(_footer, "Footer");
            Logger.LogDebug($"Footer visibility: {isFooterVisible}");
            return isFooterVisible;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error checking footer visibility: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Checks if social media links are clickable (should not be clickable)
    /// </summary>
    public async Task<bool> IsSocialMediaLinkClickableAsync(string socialMedia)
    {
        try
        {
            ILocator socialLink;
            switch (socialMedia.ToLower())
            {
                case "twitter":
                    socialLink = _twitterLink;
                    break;
                case "facebook":
                    socialLink = _facebookLink;
                    break;
                case "linkedin":
                    socialLink = _linkedinLink;
                    break;
                default:
                    throw new ArgumentException($"Unknown social media: {socialMedia}");
            }

            var isVisible = await IsElementVisibleAsync(socialLink, $"{socialMedia} link");
            if (!isVisible)
            {
                Logger.LogDebug($"{socialMedia} link is not visible");
                return false;
            }

            // Check if the element has clickable attributes (href, onclick, etc.)
            var hasHref = await socialLink.GetAttributeAsync("href");
            var hasOnClick = await socialLink.GetAttributeAsync("onclick");
            var tagName = await socialLink.EvaluateAsync<string>("el => el.tagName.toLowerCase()");
            
            var isClickable = !string.IsNullOrEmpty(hasHref) || !string.IsNullOrEmpty(hasOnClick) || tagName == "a" || tagName == "button";
            
            Logger.LogDebug($"{socialMedia} link clickable: {isClickable} (href: {hasHref}, onclick: {hasOnClick}, tag: {tagName})");
            return isClickable;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error checking {socialMedia} link clickability: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the footer copyright text
    /// </summary>
    public async Task<string> GetFooterCopyrightTextAsync()
    {
        try
        {
            var copyrightText = await GetElementTextAsync(_footerCopyright, "Footer copyright");
            Logger.LogDebug($"Footer copyright text: {copyrightText}");
            return copyrightText;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error getting footer copyright text: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Checks if copyright year is outdated (still shows 2020)
    /// </summary>
    public async Task<bool> IsCopyrightYearOutdatedAsync()
    {
        try
        {
            var copyrightText = await GetFooterCopyrightTextAsync();
            var currentYear = DateTime.Now.Year;
            var isOutdated = copyrightText.Contains("2020") && !copyrightText.Contains(currentYear.ToString());
            
            Logger.LogDebug($"Copyright year outdated: {isOutdated} (text: {copyrightText}, current year: {currentYear})");
            return isOutdated;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error checking copyright year: {ex.Message}");
            return false;
        }
    }
}
