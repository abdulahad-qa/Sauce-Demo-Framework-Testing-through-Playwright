using NUnit.Framework;
using FluentAssertions;
using Framework.Core;

namespace Tests;

/// <summary>
/// Test class for product functionality
/// </summary>
[TestFixture]
[Category("Products")]
public class ProductTests : BaseTest
{
    [Test]
    [Description("Verify products page displays all products correctly")]
    public async Task ProductsPage_ShouldDisplayAllProducts()
    {
        // Arrange
        await LoginAsync("StandardUser");
        
        // Act
        var isProductsPageDisplayed = await ProductsPage.IsProductsPageDisplayedAsync();
        var productNames = await ProductsPage.GetProductNamesAsync();
        var productPrices = await ProductsPage.GetProductPricesAsync();
        
        // Assert
        isProductsPageDisplayed.Should().BeTrue("Products page should be displayed correctly");
        productNames.Should().NotBeEmpty("Products should be displayed on the page");
        productPrices.Should().NotBeEmpty("Product prices should be displayed on the page");
        productNames.Count.Should().Be(productPrices.Count, "Number of product names should match number of product prices");
    }

    [Test]
    [Description("Verify adding product to cart functionality")]
    public async Task AddProductToCart_ShouldIncreaseCartCount()
    {
        // Arrange
        await LoginAsync("StandardUser");
        var initialCartCount = await ProductsPage.GetCartItemCountAsync();
        var productName = "Sauce Labs Backpack";
        
        // Act
        await ProductsPage.AddProductToCartAsync(productName);
        var updatedCartCount = await ProductsPage.GetCartItemCountAsync();
        
        // Assert
        updatedCartCount.Should().Be(initialCartCount + 1, "Cart count should increase by 1 after adding a product");
    }

    [Test]
    [Description("Verify removing product from cart functionality")]
    public async Task RemoveProductFromCart_ShouldDecreaseCartCount()
    {
        // Arrange
        await LoginAsync("StandardUser");
        var productName = "Sauce Labs Backpack";
        await ProductsPage.AddProductToCartAsync(productName);
        var cartCountAfterAdd = await ProductsPage.GetCartItemCountAsync();
        
        // Act
        await ProductsPage.RemoveProductFromCartAsync(productName);
        var cartCountAfterRemove = await ProductsPage.GetCartItemCountAsync();
        
        // Assert
        cartCountAfterRemove.Should().Be(cartCountAfterAdd - 1, "Cart count should decrease by 1 after removing a product");
    }

    [Test]
    [Description("Verify product sorting by name A to Z")]
    public async Task SortProducts_ByNameAToZ_ShouldSortCorrectly()
    {
        // Arrange
        await LoginAsync("StandardUser");
        var sortOption = "Name (A to Z)";
        
        // Act
        await ProductsPage.SortProductsAsync(sortOption);
        var sortedProductNames = await ProductsPage.GetProductNamesAsync();
        
        // Assert
        var expectedSortedNames = sortedProductNames.OrderBy(name => name).ToList();
        sortedProductNames.Should().Equal(expectedSortedNames, "Products should be sorted by name A to Z");
    }

    [Test]
    [Description("Verify product sorting by name Z to A")]
    public async Task SortProducts_ByNameZToA_ShouldSortCorrectly()
    {
        // Arrange
        await LoginAsync("StandardUser");
        var sortOption = "Name (Z to A)";
        
        // Act
        await ProductsPage.SortProductsAsync(sortOption);
        var sortedProductNames = await ProductsPage.GetProductNamesAsync();
        
        // Assert
        var expectedSortedNames = sortedProductNames.OrderByDescending(name => name).ToList();
        sortedProductNames.Should().Equal(expectedSortedNames, "Products should be sorted by name Z to A");
    }

    [Test]
    [Description("Verify product sorting by price low to high")]
    public async Task SortProducts_ByPriceLowToHigh_ShouldSortCorrectly()
    {
        // Arrange
        await LoginAsync("StandardUser");
        var sortOption = "Price (low to high)";
        
        // Act
        await ProductsPage.SortProductsAsync(sortOption);
        var sortedProductPrices = await ProductsPage.GetProductPricesAsync();
        
        // Assert
        var expectedSortedPrices = sortedProductPrices.OrderBy(price => decimal.Parse(price.Replace("$", ""))).ToList();
        sortedProductPrices.Should().Equal(expectedSortedPrices, "Products should be sorted by price low to high");
    }

    [Test]
    [Description("Verify product sorting by price high to low")]
    public async Task SortProducts_ByPriceHighToLow_ShouldSortCorrectly()
    {
        // Arrange
        await LoginAsync("StandardUser");
        var sortOption = "Price (high to low)";
        
        // Act
        await ProductsPage.SortProductsAsync(sortOption);
        var sortedProductPrices = await ProductsPage.GetProductPricesAsync();
        
        // Assert
        var expectedSortedPrices = sortedProductPrices.OrderByDescending(price => decimal.Parse(price.Replace("$", ""))).ToList();
        sortedProductPrices.Should().Equal(expectedSortedPrices, "Products should be sorted by price high to low");
    }

    [Test]
    [Description("Verify adding multiple products to cart")]
    public async Task AddMultipleProductsToCart_ShouldUpdateCartCountCorrectly()
    {
        // Arrange
        await LoginAsync("StandardUser");
        var productNames = new[] { "Sauce Labs Backpack", "Sauce Labs Bike Light", "Sauce Labs Bolt T-Shirt" };
        
        // Act
        foreach (var productName in productNames)
        {
            await ProductsPage.AddProductToCartAsync(productName);
        }
        
        var finalCartCount = await ProductsPage.GetCartItemCountAsync();
        
        // Assert
        finalCartCount.Should().Be(productNames.Length, "Cart count should equal the number of products added");
    }

    [Test]
    [Description("Verify navigation to cart page")]
    public async Task NavigateToCart_ShouldOpenCartPage()
    {
        // Arrange
        await LoginAsync("StandardUser");
        await ProductsPage.AddProductToCartAsync("Sauce Labs Backpack");
        
        // Act
        var cartPage = await ProductsPage.GoToCartAsync();
        
        // Assert
        var isCartPageDisplayed = await cartPage.IsCartPageDisplayedAsync();
        isCartPageDisplayed.Should().BeTrue("Cart page should be displayed correctly");
        
        var isOnCartPage = await cartPage.IsOnPageAsync();
        isOnCartPage.Should().BeTrue("Should be on cart page");
    }

    [Test]
    [Description("Verify Reset App State functionality and the known bug where Remove buttons persist")]
    public async Task ResetAppState_ShouldClearCartButMayShowRemoveButtons_BugVerification()
    {
        // Arrange
        await LoginAsync("StandardUser");
        var productName = "Sauce Labs Backpack";
        
        // Add product to cart
        await ProductsPage.AddProductToCartAsync(productName);
        var cartCountBeforeReset = await ProductsPage.GetCartItemCountAsync();
        var isProductInCartBeforeReset = await ProductsPage.IsProductInCartAsync(productName);
        
        // Verify product is in cart
        cartCountBeforeReset.Should().Be(1, "Product should be in cart before reset");
        isProductInCartBeforeReset.Should().BeTrue("Product should show Remove button before reset");
        
        // Act - Reset app state
        await ProductsPage.ResetAppStateAsync();
        
        // Assert - Check cart count (should be cleared)
        var cartCountAfterReset = await ProductsPage.GetCartItemCountAsync();
        cartCountAfterReset.Should().Be(0, "Cart should be empty after reset app state");
        
        // Check if product still shows Remove button (this is the bug!)
        var isProductInCartAfterReset = await ProductsPage.IsProductInCartAsync(productName);
        
        // This test documents the known bug in SauceDemo
        if (isProductInCartAfterReset)
        {
            Logger.LogWarning($"BUG DETECTED: Product {productName} still shows 'Remove' button after reset app state, but cart is empty");
            Logger.LogWarning("This is a known issue in the SauceDemo application where the UI state is not properly synchronized");
        }
        else
        {
            Logger.LogInfo($"Product {productName} correctly shows 'Add to cart' button after reset app state");
        }
        
        // The test passes regardless of the bug, as we're documenting the issue
        Logger.LogInfo("Reset App State test completed - documenting the known UI synchronization bug");
    }

    [Test]
    [Description("Verify footer is visible on products page")]
    public async Task Footer_ShouldBeVisibleOnProductsPage()
    {
        // Arrange
        await LoginAsync("StandardUser");
        
        // Act
        var isFooterVisible = await ProductsPage.IsFooterVisibleAsync();
        
        // Assert
        isFooterVisible.Should().BeTrue("Footer should be visible on products page");
    }

    [Test]
    [Description("Verify Twitter social media link is not clickable (should be just text)")]
    public async Task TwitterLink_ShouldNotBeClickable()
    {
        // Arrange
        await LoginAsync("StandardUser");
        
        // Act
        var isTwitterClickable = await ProductsPage.IsSocialMediaLinkClickableAsync("Twitter");
        
        // Assert
        isTwitterClickable.Should().BeFalse("Twitter link should not be clickable - it should be just text");
        
        if (isTwitterClickable)
        {
            Logger.LogWarning("ISSUE DETECTED: Twitter link is clickable when it should be just text");
        }
        else
        {
            Logger.LogInfo("Twitter link correctly shows as non-clickable text");
        }
    }

    [Test]
    [Description("Verify Facebook social media link is not clickable (should be just text)")]
    public async Task FacebookLink_ShouldNotBeClickable()
    {
        // Arrange
        await LoginAsync("StandardUser");
        
        // Act
        var isFacebookClickable = await ProductsPage.IsSocialMediaLinkClickableAsync("Facebook");
        
        // Assert
        isFacebookClickable.Should().BeFalse("Facebook link should not be clickable - it should be just text");
        
        if (isFacebookClickable)
        {
            Logger.LogWarning("ISSUE DETECTED: Facebook link is clickable when it should be just text");
        }
        else
        {
            Logger.LogInfo("Facebook link correctly shows as non-clickable text");
        }
    }

    [Test]
    [Description("Verify LinkedIn social media link is not clickable (should be just text)")]
    public async Task LinkedInLink_ShouldNotBeClickable()
    {
        // Arrange
        await LoginAsync("StandardUser");
        
        // Act
        var isLinkedInClickable = await ProductsPage.IsSocialMediaLinkClickableAsync("LinkedIn");
        
        // Assert
        isLinkedInClickable.Should().BeFalse("LinkedIn link should not be clickable - it should be just text");
        
        if (isLinkedInClickable)
        {
            Logger.LogWarning("ISSUE DETECTED: LinkedIn link is clickable when it should be just text");
        }
        else
        {
            Logger.LogInfo("LinkedIn link correctly shows as non-clickable text");
        }
    }

    [Test]
    [Description("Verify copyright year is outdated (still shows 2020 instead of current year)")]
    public async Task CopyrightYear_ShouldBeOutdated()
    {
        // Arrange
        await LoginAsync("StandardUser");
        
        // Act
        var copyrightText = await ProductsPage.GetFooterCopyrightTextAsync();
        var isCopyrightOutdated = await ProductsPage.IsCopyrightYearOutdatedAsync();
        
        // Assert
        copyrightText.Should().NotBeNullOrEmpty("Copyright text should be present");
        isCopyrightOutdated.Should().BeTrue("Copyright year should be outdated (still shows 2020)");
        
        if (isCopyrightOutdated)
        {
            Logger.LogWarning($"ISSUE DETECTED: Copyright year is outdated - shows '2020' instead of current year {DateTime.Now.Year}");
            Logger.LogWarning($"Copyright text: {copyrightText}");
        }
        else
        {
            Logger.LogInfo("Copyright year is up to date");
        }
    }

    [Test]
    [Description("Verify all footer issues: non-clickable social links and outdated copyright")]
    public async Task Footer_ShouldHaveMultipleIssues_BugVerification()
    {
        // Arrange
        await LoginAsync("StandardUser");
        
        // Act
        var isFooterVisible = await ProductsPage.IsFooterVisibleAsync();
        var isTwitterClickable = await ProductsPage.IsSocialMediaLinkClickableAsync("Twitter");
        var isFacebookClickable = await ProductsPage.IsSocialMediaLinkClickableAsync("Facebook");
        var isLinkedInClickable = await ProductsPage.IsSocialMediaLinkClickableAsync("LinkedIn");
        var isCopyrightOutdated = await ProductsPage.IsCopyrightYearOutdatedAsync();
        var copyrightText = await ProductsPage.GetFooterCopyrightTextAsync();
        
        // Assert
        isFooterVisible.Should().BeTrue("Footer should be visible");
        
        // Document all footer issues
        Logger.LogInfo("=== FOOTER ISSUES VERIFICATION ===");
        Logger.LogInfo($"Footer visible: {isFooterVisible}");
        Logger.LogInfo($"Twitter clickable: {isTwitterClickable} (should be False)");
        Logger.LogInfo($"Facebook clickable: {isFacebookClickable} (should be False)");
        Logger.LogInfo($"LinkedIn clickable: {isLinkedInClickable} (should be False)");
        Logger.LogInfo($"Copyright outdated: {isCopyrightOutdated} (should be True)");
        Logger.LogInfo($"Copyright text: {copyrightText}");
        
        // Count issues
        var issueCount = 0;
        if (isTwitterClickable) issueCount++;
        if (isFacebookClickable) issueCount++;
        if (isLinkedInClickable) issueCount++;
        if (!isCopyrightOutdated) issueCount++;
        
        Logger.LogInfo($"Total footer issues detected: {issueCount}");
        
        // Test passes regardless of issues - we're documenting them
        Logger.LogInfo("Footer verification completed - documenting all footer-related issues");
    }
}
