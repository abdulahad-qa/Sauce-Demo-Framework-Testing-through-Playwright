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
    [Description("Verify Reset App State functionality - should clear cart and 'Remove' button must not persist")]
    public async Task ResetAppState_ShouldClearCartAndShowAddButton()
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
        
        // Assert - product should not show Remove button after reset (this test will fail if bug still exists)
        var isProductInCartAfterReset = await ProductsPage.IsProductInCartAsync(productName);
        isProductInCartAfterReset.Should().BeFalse($"BUG: Product '{productName}' still shows 'Remove' button after reset app state, but cart is empty. This is a UI synchronization issue that MUST be fixed.");

        Logger.LogInfo("Reset App State test completed - Remove button does NOT persist after reset (no bug present)");
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
    [Description("Verify Twitter social media link is clickable button - FAILS if Twitter is not clickable")]
    public async Task TwitterLink_ShouldBeClickable()
    {
        // Arrange
        await LoginAsync("StandardUser");
        
        // Act
        var isTwitterClickable = await ProductsPage.IsSocialMediaLinkClickableAsync("Twitter");
        
        // Assert - This test SHOULD FAIL if Twitter is NOT clickable (error case)
        isTwitterClickable.Should().BeTrue("ERROR: Twitter link should be a clickable button/link but it's not - this is a bug!");
        
        if (!isTwitterClickable)
        {
            Logger.LogError("ERROR DETECTED: Twitter link is not clickable when it should be a functional button - TEST FAILED");
        }
        else
        {
            Logger.LogInfo("Twitter link correctly shows as clickable button - TEST PASSED");
        }
    }

    [Test]
    [Description("Verify Facebook social media link is clickable button - FAILS if Facebook is not clickable")]
    public async Task FacebookLink_ShouldBeClickable()
    {
        // Arrange
        await LoginAsync("StandardUser");
        
        // Act
        var isFacebookClickable = await ProductsPage.IsSocialMediaLinkClickableAsync("Facebook");
        
        // Assert - This test SHOULD FAIL if Facebook is NOT clickable (error case)
        isFacebookClickable.Should().BeTrue("ERROR: Facebook link should be a clickable button/link but it's not - this is a bug!");
        
        if (!isFacebookClickable)
        {
            Logger.LogError("ERROR DETECTED: Facebook link is not clickable when it should be a functional button - TEST FAILED");
        }
        else
        {
            Logger.LogInfo("Facebook link correctly shows as clickable button - TEST PASSED");
        }
    }

    [Test]
    [Description("Verify LinkedIn social media link is clickable button - FAILS if LinkedIn is not clickable")]
    public async Task LinkedInLink_ShouldBeClickable()
    {
        // Arrange
        await LoginAsync("StandardUser");
        
        // Act
        var isLinkedInClickable = await ProductsPage.IsSocialMediaLinkClickableAsync("LinkedIn");
        
        // Assert - This test SHOULD FAIL if LinkedIn is NOT clickable (error case)
        isLinkedInClickable.Should().BeTrue("ERROR: LinkedIn link should be a clickable button/link but it's not - this is a bug!");
        
        if (!isLinkedInClickable)
        {
            Logger.LogError("ERROR DETECTED: LinkedIn link is not clickable when it should be a functional button - TEST FAILED");
        }
        else
        {
            Logger.LogInfo("LinkedIn link correctly shows as clickable button - TEST PASSED");
        }
    }

    [Test]
    [Description("Verify copyright year is updated (still shows 2020 instead of current year) - FAILS if copyright is outdated")]
    public async Task CopyrightYear_ShouldBeOutdated()
    {
        // Arrange
        await LoginAsync("StandardUser");
        
        // Act
        var copyrightText = await ProductsPage.GetFooterCopyrightTextAsync();
        var isCopyrightOutdated = await ProductsPage.IsCopyrightYearOutdatedAsync();
        
        // Assert - This test SHOULD FAIL if copyright is outdated (error case)
        copyrightText.Should().NotBeNullOrEmpty("Copyright text should be present");
        isCopyrightOutdated.Should().BeFalse($"ERROR: Copyright year is outdated - shows '2020' instead of current year {DateTime.Now.Year} - this is a bug!");
        
        if (isCopyrightOutdated)
        {
            Logger.LogError($"ERROR DETECTED: Copyright year is outdated - shows '2020' instead of current year {DateTime.Now.Year} - TEST FAILED");
            Logger.LogError($"Copyright text: {copyrightText}");
        }
        else
        {
            Logger.LogInfo("Copyright year is up to date - TEST PASSED");
        }
    }

   
}
