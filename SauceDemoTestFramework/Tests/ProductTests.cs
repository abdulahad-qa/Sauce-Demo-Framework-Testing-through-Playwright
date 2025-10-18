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
}
