using NUnit.Framework;
using FluentAssertions;
using Framework.Core;

namespace Tests;

/// <summary>
/// End-to-end test scenarios covering complete user workflows
/// </summary>
[TestFixture]
[Category("EndToEnd")]
public class EndToEndTests : BaseTest
{
    [Test]
    [Description("Complete user journey: Login -> Add Product -> Checkout -> Logout")]
    public async Task CompleteUserJourney_LoginAddProductCheckoutLogout_ShouldSucceed()
    {
        // Arrange
        var userType = "StandardUser";
        var productName = "Sauce Labs Backpack";
        
        // Act - Complete user journey
        Logger.LogStep("Starting complete user journey test");
        
        // 1. Login
        var productsPage = await LoginAsync(userType);
        var isOnProductsPage = await productsPage.IsOnPageAsync();
        isOnProductsPage.Should().BeTrue("User should be logged in and on products page");
        
        // 2. Add product to cart
        await productsPage.AddProductToCartAsync(productName);
        var cartCount = await productsPage.GetCartItemCountAsync();
        cartCount.Should().Be(1, "Product should be added to cart");
        
        // 3. Go to cart and verify
        var cartPage = await productsPage.GoToCartAsync();
        var cartItemNames = await cartPage.GetCartItemNamesAsync();
        cartItemNames.Should().Contain(productName, "Cart should contain the added product");
        
        // 4. Proceed to checkout
        var checkoutStepOnePage = await cartPage.ProceedToCheckoutAsync();
        var customerInfo = TestData.GetRandomCustomerInfo();
        await checkoutStepOnePage.FillCheckoutFormAsync(customerInfo);
        
        var checkoutStepTwoPage = await checkoutStepOnePage.ContinueToStepTwoAsync();
        var checkoutItemNames = await checkoutStepTwoPage.GetCheckoutItemNamesAsync();
        checkoutItemNames.Should().Contain(productName, "Checkout should contain the added product");
        
        // 5. Complete checkout
        var checkoutCompletePage = await checkoutStepTwoPage.FinishCheckoutAsync();
        var isOrderComplete = await checkoutCompletePage.VerifyOrderCompletionAsync();
        isOrderComplete.Should().BeTrue("Order should be completed successfully");
        
        // 6. Return to products and logout
        var finalProductsPage = await checkoutCompletePage.BackToProductsAsync();
        var loginPage = await finalProductsPage.LogoutAsync();
        var isOnLoginPage = await loginPage.IsOnPageAsync();
        isOnLoginPage.Should().BeTrue("User should be logged out and on login page");
        
        Logger.LogInfo("Complete user journey test completed successfully");
    }

    [Test]
    [Description("Multiple products checkout scenario")]
    public async Task MultipleProductsCheckout_ShouldProcessAllProducts()
    {
        // Arrange
        var userType = "StandardUser";
        var productNames = new[] { "Sauce Labs Backpack", "Sauce Labs Bike Light", "Sauce Labs Bolt T-Shirt" };
        
        // Act
        Logger.LogStep("Starting multiple products checkout test");
        
        // Login and add multiple products
        var productsPage = await LoginAsync(userType);
        
        foreach (var productName in productNames)
        {
            await productsPage.AddProductToCartAsync(productName);
        }
        
        var cartCount = await productsPage.GetCartItemCountAsync();
        cartCount.Should().Be(productNames.Length, "All products should be added to cart");
        
        // Go to cart and verify all products
        var cartPage = await productsPage.GoToCartAsync();
        var cartItemNames = await cartPage.GetCartItemNamesAsync();
        cartItemNames.Should().Contain(productNames, "Cart should contain all added products");
        
        // Complete checkout
        var checkoutStepOnePage = await cartPage.ProceedToCheckoutAsync();
        var customerInfo = TestData.GetRandomCustomerInfo();
        await checkoutStepOnePage.FillCheckoutFormAsync(customerInfo);
        
        var checkoutStepTwoPage = await checkoutStepOnePage.ContinueToStepTwoAsync();
        var checkoutItemCount = await checkoutStepTwoPage.GetCheckoutItemCountAsync();
        checkoutItemCount.Should().Be(productNames.Length, "Checkout should contain all products");
        
        var checkoutCompletePage = await checkoutStepTwoPage.FinishCheckoutAsync();
        var isOrderComplete = await checkoutCompletePage.VerifyOrderCompletionAsync();
        isOrderComplete.Should().BeTrue("Order with multiple products should be completed successfully");
        
        Logger.LogInfo("Multiple products checkout test completed successfully");
    }

    [Test]
    [Description("Product sorting and selection workflow")]
    public async Task ProductSortingAndSelection_ShouldWorkCorrectly()
    {
        // Arrange
        var userType = "StandardUser";
        
        // Act
        Logger.LogStep("Starting product sorting and selection test");
        
        // Login
        var productsPage = await LoginAsync(userType);
        
        // Test different sorting options
        var sortOptions = TestData.GetSortOptions();
        
        foreach (var sortOption in sortOptions)
        {
            await productsPage.SortProductsAsync(sortOption);
            var sortedProductNames = await productsPage.GetProductNamesAsync();
            sortedProductNames.Should().NotBeEmpty($"Products should be displayed after sorting by {sortOption}");
        }
        
        // Add products after sorting
        await productsPage.SortProductsAsync("Name (A to Z)");
        var firstProduct = (await productsPage.GetProductNamesAsync()).First();
        await productsPage.AddProductToCartAsync(firstProduct);
        
        var cartCount = await productsPage.GetCartItemCountAsync();
        cartCount.Should().Be(1, "Product should be added to cart after sorting");
        
        Logger.LogInfo("Product sorting and selection test completed successfully");
    }

    [Test]
    [Description("Cart management workflow: Add, Remove, Continue Shopping")]
    public async Task CartManagementWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        var userType = "StandardUser";
        var productNames = new[] { "Sauce Labs Backpack", "Sauce Labs Bike Light", "Sauce Labs Bolt T-Shirt" };
        
        // Act
        Logger.LogStep("Starting cart management workflow test");
        
        // Login and add products
        var productsPage = await LoginAsync(userType);
        
        foreach (var productName in productNames)
        {
            await productsPage.AddProductToCartAsync(productName);
        }
        
        // Go to cart
        var cartPage = await productsPage.GoToCartAsync();
        var initialCartCount = await cartPage.GetCartItemCountAsync();
        initialCartCount.Should().Be(productNames.Length, "All products should be in cart");
        
        // Remove one product
        await cartPage.RemoveItemFromCartAsync(productNames[0]);
        var cartCountAfterRemove = await cartPage.GetCartItemCountAsync();
        cartCountAfterRemove.Should().Be(productNames.Length - 1, "Cart count should decrease after removing product");
        
        // Continue shopping
        var returnedProductsPage = await cartPage.ContinueShoppingAsync();
        var isOnProductsPage = await returnedProductsPage.IsOnPageAsync();
        isOnProductsPage.Should().BeTrue("Should return to products page");
        
        // Add another product
        await returnedProductsPage.AddProductToCartAsync("Sauce Labs Fleece Jacket");
        var finalCartCount = await returnedProductsPage.GetCartItemCountAsync();
        finalCartCount.Should().Be(productNames.Length, "Cart count should be correct after adding another product");
        
        Logger.LogInfo("Cart management workflow test completed successfully");
    }

    [Test]
    [Description("Different user types login and basic functionality")]
    public async Task DifferentUserTypes_ShouldHaveAppropriateAccess()
    {
        // Arrange
        var userTypes = new[] { "StandardUser", "ProblemUser", "PerformanceGlitchUser" };
        
        // Act & Assert
        Logger.LogStep("Starting different user types test");
        
        foreach (var userType in userTypes)
        {
            Logger.LogStep($"Testing user type: {userType}");
            
            // Login with different user types
            var productsPage = await LoginAsync(userType);
            var isOnProductsPage = await productsPage.IsOnPageAsync();
            isOnProductsPage.Should().BeTrue($"{userType} should be able to login and access products page");
            
            // Verify basic functionality
            var productNames = await productsPage.GetProductNamesAsync();
            productNames.Should().NotBeEmpty($"{userType} should see products on the page");
            
            // Test adding product to cart
            var firstProduct = productNames.First();
            await productsPage.AddProductToCartAsync(firstProduct);
            var cartCount = await productsPage.GetCartItemCountAsync();
            cartCount.Should().Be(1, $"{userType} should be able to add products to cart");
            
            // Logout for next iteration
            await productsPage.LogoutAsync();
        }
        
        Logger.LogInfo("Different user types test completed successfully");
    }

    [Test]
    [Description("Error handling workflow: Invalid login attempts")]
    public async Task ErrorHandlingWorkflow_InvalidLoginAttempts_ShouldShowAppropriateErrors()
    {
        // Arrange
        var invalidCredentials = new[]
        {
            new { Username = "", Password = "secret_sauce", ExpectedError = "Username is required" },
            new { Username = "standard_user", Password = "", ExpectedError = "Password is required" },
            new { Username = "invalid_user", Password = "invalid_password", ExpectedError = "Username and password do not match" },
            new { Username = "locked_out_user", Password = "secret_sauce", ExpectedError = "Sorry, this user has been locked out" }
        };
        
        // Act & Assert
        Logger.LogStep("Starting error handling workflow test");
        
        foreach (var credentials in invalidCredentials)
        {
            Logger.LogStep($"Testing invalid credentials: {credentials.Username}");
            
            var userCredentials = new Framework.Models.UserCredentials 
            { 
                Username = credentials.Username, 
                Password = credentials.Password 
            };
            
            var loginPage = await LoginPage.LoginWithInvalidCredentialsAsync(userCredentials);
            var isErrorMessageDisplayed = await loginPage.IsErrorMessageDisplayedAsync();
            isErrorMessageDisplayed.Should().BeTrue($"Error message should be displayed for invalid credentials: {credentials.Username}");
            
            var errorMessage = await loginPage.GetErrorMessageAsync();
            errorMessage.Should().Contain(credentials.ExpectedError, $"Error message should contain expected text for: {credentials.Username}");
            
            // Clear form for next iteration
            await loginPage.ClearLoginFormAsync();
        }
        
        Logger.LogInfo("Error handling workflow test completed successfully");
    }
}
