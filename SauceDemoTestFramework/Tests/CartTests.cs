using NUnit.Framework;
using FluentAssertions;
using Framework.Core;

namespace Tests;

/// <summary>
/// Test class for shopping cart functionality
/// </summary>
[TestFixture]
[Category("Cart")]
public class CartTests : BaseTest
{
    [Test]
    [Description("Verify cart page displays added products correctly")]
    public async Task CartPage_ShouldDisplayAddedProducts()
    {
        // Arrange
        await LoginAsync("StandardUser");
        var productNames = new[] { "Sauce Labs Backpack", "Sauce Labs Bike Light" };
        
        foreach (var productName in productNames)
        {
            await ProductsPage.AddProductToCartAsync(productName);
        }
        
        var cartPage = await ProductsPage.GoToCartAsync();
        
        // Act
        var cartItemNames = await cartPage.GetCartItemNamesAsync();
        var cartItemCount = await cartPage.GetCartItemCountAsync();
        
        // Assert
        cartItemCount.Should().Be(productNames.Length, "Cart should contain the correct number of items");
        cartItemNames.Should().Contain(productNames, "Cart should contain all added products");
    }

    [Test]
    [Description("Verify removing item from cart")]
    public async Task RemoveItemFromCart_ShouldRemoveItem()
    {
        // Arrange
        await LoginAsync("StandardUser");
        var productName = "Sauce Labs Backpack";
        await ProductsPage.AddProductToCartAsync(productName);
        var cartPage = await ProductsPage.GoToCartAsync();
        var initialItemCount = await cartPage.GetCartItemCountAsync();
        
        // Act
        await cartPage.RemoveItemFromCartAsync(productName);
        var updatedItemCount = await cartPage.GetCartItemCountAsync();
        var remainingItemNames = await cartPage.GetCartItemNamesAsync();
        
        // Assert
        updatedItemCount.Should().Be(initialItemCount - 1, "Cart item count should decrease by 1");
        remainingItemNames.Should().NotContain(productName, "Removed product should not be in cart");
    }

    [Test]
    [Description("Verify removing all items from cart")]
    public async Task RemoveAllItemsFromCart_ShouldEmptyCart()
    {
        // Arrange
        await LoginAsync("StandardUser");
        var productNames = new[] { "Sauce Labs Backpack", "Sauce Labs Bike Light", "Sauce Labs Bolt T-Shirt" };
        
        foreach (var productName in productNames)
        {
            await ProductsPage.AddProductToCartAsync(productName);
        }
        
        var cartPage = await ProductsPage.GoToCartAsync();
        
        // Act
        await cartPage.RemoveAllItemsFromCartAsync();
        var finalItemCount = await cartPage.GetCartItemCountAsync();
        var isCartEmpty = await cartPage.IsCartEmptyAsync();
        
        // Assert
        finalItemCount.Should().Be(0, "Cart should be empty after removing all items");
        isCartEmpty.Should().BeTrue("Cart should be empty");
    }

    [Test]
    [Description("Verify continue shopping functionality")]
    public async Task ContinueShopping_ShouldReturnToProductsPage()
    {
        // Arrange
        await LoginAsync("StandardUser");
        await ProductsPage.AddProductToCartAsync("Sauce Labs Backpack");
        var cartPage = await ProductsPage.GoToCartAsync();
        
        // Act
        var productsPage = await cartPage.ContinueShoppingAsync();
        
        // Assert
        var isProductsPageDisplayed = await productsPage.IsProductsPageDisplayedAsync();
        isProductsPageDisplayed.Should().BeTrue("Should return to products page");
        
        var isOnProductsPage = await productsPage.IsOnPageAsync();
        isOnProductsPage.Should().BeTrue("Should be on products page");
    }

    [Test]
    [Description("Verify proceed to checkout functionality")]
    public async Task ProceedToCheckout_ShouldNavigateToCheckoutStepOne()
    {
        // Arrange
        await LoginAsync("StandardUser");
        await ProductsPage.AddProductToCartAsync("Sauce Labs Backpack");
        var cartPage = await ProductsPage.GoToCartAsync();
        
        // Act
        var checkoutStepOnePage = await cartPage.ProceedToCheckoutAsync();
        
        // Assert
        var isCheckoutStepOneDisplayed = await checkoutStepOnePage.IsCheckoutStepOnePageDisplayedAsync();
        isCheckoutStepOneDisplayed.Should().BeTrue("Checkout step one page should be displayed");
        
        var isOnCheckoutStepOne = await checkoutStepOnePage.IsOnPageAsync();
        isOnCheckoutStepOne.Should().BeTrue("Should be on checkout step one page");
    }

    [Test]
    [Description("Verify cart page displays correct product prices")]
    public async Task CartPage_ShouldDisplayCorrectProductPrices()
    {
        // Arrange
        await LoginAsync("StandardUser");
        var productName = "Sauce Labs Backpack";
        await ProductsPage.AddProductToCartAsync(productName);
        var cartPage = await ProductsPage.GoToCartAsync();
        
        // Act
        var cartItemPrices = await cartPage.GetCartItemPricesAsync();
        
        // Assert
        cartItemPrices.Should().NotBeEmpty("Cart should display product prices");
        cartItemPrices.Should().AllSatisfy(price => 
            price.Should().MatchRegex(@"^\d+\.\d{2}$"), "All prices should be in correct format (e.g., 29.99)");
    }

    [Test]
    [Description("Verify empty cart behavior")]
    public async Task EmptyCart_ShouldDisplayCorrectly()
    {
        // Arrange
        await LoginAsync("StandardUser");
        var cartPage = await ProductsPage.GoToCartAsync();
        
        // Act
        var cartItemCount = await cartPage.GetCartItemCountAsync();
        var isCartEmpty = await cartPage.IsCartEmptyAsync();
        
        // Assert
        cartItemCount.Should().Be(0, "Empty cart should have 0 items");
        isCartEmpty.Should().BeTrue("Cart should be empty");
    }

    [Test]
    [Description("Verify cart page elements are displayed correctly")]
    public async Task CartPage_ShouldDisplayAllRequiredElements()
    {
        // Arrange
        await LoginAsync("StandardUser");
        await ProductsPage.AddProductToCartAsync("Sauce Labs Backpack");
        var cartPage = await ProductsPage.GoToCartAsync();
        
        // Act & Assert
        var isCartPageDisplayed = await cartPage.IsCartPageDisplayedAsync();
        isCartPageDisplayed.Should().BeTrue("Cart page should display all required elements");
    }
}
