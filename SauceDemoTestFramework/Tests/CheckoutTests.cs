using NUnit.Framework;
using FluentAssertions;
using Framework.Core;

namespace Tests;

/// <summary>
/// Test class for checkout functionality
/// </summary>
[TestFixture]
[Category("Checkout")]
public class CheckoutTests : BaseTest
{
    [Test]
    [Description("Verify complete checkout process with valid information")]
    public async Task CompleteCheckout_WithValidInformation_ShouldSucceed()
    {
        // Arrange
        var userType = "StandardUser";
        var productName = "Sauce Labs Backpack";
        
        // Act
        var checkoutCompletePage = await CompleteCheckoutAsync(userType, productName);
        
        // Assert
        var isCheckoutCompleteDisplayed = await checkoutCompletePage.IsCheckoutCompletePageDisplayedAsync();
        isCheckoutCompleteDisplayed.Should().BeTrue("Checkout complete page should be displayed");
        
        var isOrderComplete = await checkoutCompletePage.VerifyOrderCompletionAsync();
        isOrderComplete.Should().BeTrue("Order should be completed successfully");
        
        var completeHeader = await checkoutCompletePage.GetCompleteHeaderAsync();
        completeHeader.Should().Contain("THANK YOU FOR YOUR ORDER", "Should display thank you message");
    }

    [Test]
    [Description("Verify checkout step one form validation with empty first name")]
    public async Task CheckoutStepOne_WithEmptyFirstName_ShouldShowValidationError()
    {
        // Arrange
        await LoginAsync("StandardUser");
        await ProductsPage.AddProductToCartAsync("Sauce Labs Backpack");
        var cartPage = await ProductsPage.GoToCartAsync();
        var checkoutStepOnePage = await cartPage.ProceedToCheckoutAsync();
        
        // Act
        await checkoutStepOnePage.ContinueWithEmptyFormAsync();
        
        // Assert
        var isErrorMessageDisplayed = await checkoutStepOnePage.IsErrorMessageDisplayedAsync();
        isErrorMessageDisplayed.Should().BeTrue("Error message should be displayed for empty first name");
        
        var errorMessage = await checkoutStepOnePage.GetErrorMessageAsync();
        errorMessage.Should().Contain("First Name is required", "Error message should indicate first name is required");
    }

    [Test]
    [Description("Verify checkout step one form validation with empty last name")]
    public async Task CheckoutStepOne_WithEmptyLastName_ShouldShowValidationError()
    {
        // Arrange
        await LoginAsync("StandardUser");
        await ProductsPage.AddProductToCartAsync("Sauce Labs Backpack");
        var cartPage = await ProductsPage.GoToCartAsync();
        var checkoutStepOnePage = await cartPage.ProceedToCheckoutAsync();
        
        // Act
        await checkoutStepOnePage.FillCheckoutFormAsync(new Framework.Models.CustomerInfo 
        { 
            FirstName = "John", 
            LastName = "", 
            PostalCode = "12345" 
        });
        await checkoutStepOnePage.ContinueWithEmptyFormAsync();
        
        // Assert
        var isErrorMessageDisplayed = await checkoutStepOnePage.IsErrorMessageDisplayedAsync();
        isErrorMessageDisplayed.Should().BeTrue("Error message should be displayed for empty last name");
    }

    [Test]
    [Description("Verify checkout step one form validation with empty postal code")]
    public async Task CheckoutStepOne_WithEmptyPostalCode_ShouldShowValidationError()
    {
        // Arrange
        await LoginAsync("StandardUser");
        await ProductsPage.AddProductToCartAsync("Sauce Labs Backpack");
        var cartPage = await ProductsPage.GoToCartAsync();
        var checkoutStepOnePage = await cartPage.ProceedToCheckoutAsync();
        
        // Act
        await checkoutStepOnePage.FillCheckoutFormAsync(new Framework.Models.CustomerInfo 
        { 
            FirstName = "John", 
            LastName = "Doe", 
            PostalCode = "" 
        });
        await checkoutStepOnePage.ContinueWithEmptyFormAsync();
        
        // Assert
        var isErrorMessageDisplayed = await checkoutStepOnePage.IsErrorMessageDisplayedAsync();
        isErrorMessageDisplayed.Should().BeTrue("Error message should be displayed for empty postal code");
    }

    [Test]
    [Description("Verify checkout step two displays correct order summary")]
    public async Task CheckoutStepTwo_ShouldDisplayCorrectOrderSummary()
    {
        // Arrange
        await LoginAsync("StandardUser");
        var productName = "Sauce Labs Backpack";
        await ProductsPage.AddProductToCartAsync(productName);
        var cartPage = await ProductsPage.GoToCartAsync();
        var checkoutStepOnePage = await cartPage.ProceedToCheckoutAsync();
        
        var customerInfo = TestData.GetRandomCustomerInfo();
        await checkoutStepOnePage.FillCheckoutFormAsync(customerInfo);
        var checkoutStepTwoPage = await checkoutStepOnePage.ContinueToStepTwoAsync();
        
        // Act
        var checkoutItemNames = await checkoutStepTwoPage.GetCheckoutItemNamesAsync();
        var checkoutItemCount = await checkoutStepTwoPage.GetCheckoutItemCountAsync();
        var subtotal = await checkoutStepTwoPage.GetSubtotalAsync();
        var tax = await checkoutStepTwoPage.GetTaxAsync();
        var total = await checkoutStepTwoPage.GetTotalAsync();
        
        // Assert
        checkoutItemCount.Should().Be(1, "Checkout should contain 1 item");
        checkoutItemNames.Should().Contain(productName, "Checkout should contain the added product");
        subtotal.Should().NotBeNullOrEmpty("Subtotal should be displayed");
        tax.Should().NotBeNullOrEmpty("Tax should be displayed");
        total.Should().NotBeNullOrEmpty("Total should be displayed");
    }

    [Test]
    [Description("Verify canceling checkout from step one returns to cart")]
    public async Task CancelCheckout_FromStepOne_ShouldReturnToCart()
    {
        // Arrange
        await LoginAsync("StandardUser");
        await ProductsPage.AddProductToCartAsync("Sauce Labs Backpack");
        var cartPage = await ProductsPage.GoToCartAsync();
        var checkoutStepOnePage = await cartPage.ProceedToCheckoutAsync();
        
        // Act
        var returnedCartPage = await checkoutStepOnePage.CancelCheckoutAsync();
        
        // Assert
        var isCartPageDisplayed = await returnedCartPage.IsCartPageDisplayedAsync();
        isCartPageDisplayed.Should().BeTrue("Should return to cart page after canceling checkout");
        
        var isOnCartPage = await returnedCartPage.IsOnPageAsync();
        isOnCartPage.Should().BeTrue("Should be on cart page");
    }

    [Test]
    [Description("Verify canceling checkout from step two returns to products page")]
    public async Task CancelCheckout_FromStepTwo_ShouldReturnToProducts()
    {
        // Arrange
        await LoginAsync("StandardUser");
        await ProductsPage.AddProductToCartAsync("Sauce Labs Backpack");
        var cartPage = await ProductsPage.GoToCartAsync();
        var checkoutStepOnePage = await cartPage.ProceedToCheckoutAsync();
        
        var customerInfo = TestData.GetRandomCustomerInfo();
        await checkoutStepOnePage.FillCheckoutFormAsync(customerInfo);
        var checkoutStepTwoPage = await checkoutStepOnePage.ContinueToStepTwoAsync();
        
        // Act
        var returnedProductsPage = await checkoutStepTwoPage.CancelCheckoutAsync();
        
        // Assert
        var isProductsPageDisplayed = await returnedProductsPage.IsProductsPageDisplayedAsync();
        isProductsPageDisplayed.Should().BeTrue("Should return to products page after canceling checkout");
    }

    [Test]
    [Description("Verify checkout step one page elements are displayed correctly")]
    public async Task CheckoutStepOnePage_ShouldDisplayAllRequiredElements()
    {
        // Arrange
        await LoginAsync("StandardUser");
        await ProductsPage.AddProductToCartAsync("Sauce Labs Backpack");
        var cartPage = await ProductsPage.GoToCartAsync();
        var checkoutStepOnePage = await cartPage.ProceedToCheckoutAsync();
        
        // Act & Assert
        var isCheckoutStepOneDisplayed = await checkoutStepOnePage.IsCheckoutStepOnePageDisplayedAsync();
        isCheckoutStepOneDisplayed.Should().BeTrue("Checkout step one page should display all required elements");
    }

    [Test]
    [Description("Verify checkout step two page elements are displayed correctly")]
    public async Task CheckoutStepTwoPage_ShouldDisplayAllRequiredElements()
    {
        // Arrange
        await LoginAsync("StandardUser");
        await ProductsPage.AddProductToCartAsync("Sauce Labs Backpack");
        var cartPage = await ProductsPage.GoToCartAsync();
        var checkoutStepOnePage = await cartPage.ProceedToCheckoutAsync();
        
        var customerInfo = TestData.GetRandomCustomerInfo();
        await checkoutStepOnePage.FillCheckoutFormAsync(customerInfo);
        var checkoutStepTwoPage = await checkoutStepOnePage.ContinueToStepTwoAsync();
        
        // Act & Assert
        var isCheckoutStepTwoDisplayed = await checkoutStepTwoPage.IsCheckoutStepTwoPageDisplayedAsync();
        isCheckoutStepTwoDisplayed.Should().BeTrue("Checkout step two page should display all required elements");
    }

    [Test]
    [Description("Verify returning to products from checkout complete page")]
    public async Task ReturnToProducts_FromCheckoutComplete_ShouldNavigateToProducts()
    {
        // Arrange
        var userType = "StandardUser";
        var productName = "Sauce Labs Backpack";
        var checkoutCompletePage = await CompleteCheckoutAsync(userType, productName);
        
        // Act
        var productsPage = await checkoutCompletePage.BackToProductsAsync();
        
        // Assert
        var isProductsPageDisplayed = await productsPage.IsProductsPageDisplayedAsync();
        isProductsPageDisplayed.Should().BeTrue("Should return to products page");
        
        var isOnProductsPage = await productsPage.IsOnPageAsync();
        isOnProductsPage.Should().BeTrue("Should be on products page");
    }
}
