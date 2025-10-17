using Microsoft.Playwright;
using Framework.Core;
using Framework.Models;

namespace Framework.Pages;

/// <summary>
/// Page Object Model for the SauceDemo Checkout Step One page
/// </summary>
public class CheckoutStepOnePage : BasePage
{
    // Locators
    private readonly ILocator _checkoutContainer;
    private readonly ILocator _firstNameField;
    private readonly ILocator _lastNameField;
    private readonly ILocator _postalCodeField;
    private readonly ILocator _continueButton;
    private readonly ILocator _cancelButton;
    private readonly ILocator _errorMessage;

    public CheckoutStepOnePage(IPage page, ILogger logger, IConfigurationManager config) : base(page, logger, config)
    {
        _checkoutContainer = Page.Locator("#checkout_info_container");
        _firstNameField = Page.Locator("#first-name");
        _lastNameField = Page.Locator("#last-name");
        _postalCodeField = Page.Locator("#postal-code");
        _continueButton = Page.Locator("input.btn_primary.cart_button[type='submit']");
        _cancelButton = Page.Locator("a.cart_cancel_link.btn_secondary[href*='cart.html']");
        _errorMessage = Page.Locator("[data-test='error']");
    }

    public override string PageUrl => Config.BaseUrl.Replace("index.html", "checkout-step-one.html");
    public override string PageTitle => "Swag Labs";

    /// <summary>
    /// Fills the checkout form with customer information
    /// </summary>
    /// <param name="customerInfo">Customer information to fill</param>
    public async Task<CheckoutStepOnePage> FillCheckoutFormAsync(CustomerInfo customerInfo)
    {
        try
        {
            Logger.LogStep($"Filling checkout form for: {customerInfo.FirstName} {customerInfo.LastName}");
            
            await WaitForElementAsync(_checkoutContainer, "Checkout container");
            await FillInputAsync(_firstNameField, customerInfo.FirstName, "First name field");
            await FillInputAsync(_lastNameField, customerInfo.LastName, "Last name field");
            await FillInputAsync(_postalCodeField, customerInfo.PostalCode, "Postal code field");
            
            Logger.LogInfo("Successfully filled checkout form");
            return this;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to fill checkout form: {ex.Message}");
            await TakeScreenshotAsync("fill_checkout_form_failed");
            throw;
        }
    }

    /// <summary>
    /// Continues to checkout step two
    /// </summary>
    public async Task<CheckoutStepTwoPage> ContinueToStepTwoAsync()
    {
        try
        {
            Logger.LogStep("Continuing to checkout step two");
            await ClickElementAsync(_continueButton, "Continue button");
            await Page.WaitForURLAsync("**/checkout-step-two.html");
            
            Logger.LogInfo("Successfully navigated to checkout step two");
            return new CheckoutStepTwoPage(Page, Logger, Config);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to continue to step two: {ex.Message}");
            await TakeScreenshotAsync("continue_to_step_two_failed");
            throw;
        }
    }

    /// <summary>
    /// Cancels the checkout process and returns to cart
    /// </summary>
    public async Task<CartPage> CancelCheckoutAsync()
    {
        try
        {
            Logger.LogStep("Canceling checkout process");
            await ClickElementAsync(_cancelButton, "Cancel button");
            await Page.WaitForURLAsync("**/cart.html");
            
            Logger.LogInfo("Successfully canceled checkout and returned to cart");
            return new CartPage(Page, Logger, Config);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to cancel checkout: {ex.Message}");
            await TakeScreenshotAsync("cancel_checkout_failed");
            throw;
        }
    }

    /// <summary>
    /// Attempts to continue with empty form to trigger validation
    /// </summary>
    public async Task<CheckoutStepOnePage> ContinueWithEmptyFormAsync()
    {
        try
        {
            Logger.LogStep("Attempting to continue with empty form");
            await ClickElementAsync(_continueButton, "Continue button");
            
            // Wait for error message to appear
            await WaitForElementAsync(_errorMessage, "Error message");
            
            Logger.LogInfo("Validation error displayed as expected");
            return this;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Unexpected error during empty form validation: {ex.Message}");
            await TakeScreenshotAsync("empty_form_validation_error");
            throw;
        }
    }

    /// <summary>
    /// Gets the error message text
    /// </summary>
    public async Task<string> GetErrorMessageAsync()
    {
        try
        {
            var errorText = await GetElementTextAsync(_errorMessage, "Error message");
            Logger.LogInfo($"Error message displayed: {errorText}");
            return errorText;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get error message: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Checks if error message is displayed
    /// </summary>
    public async Task<bool> IsErrorMessageDisplayedAsync()
    {
        try
        {
            var isVisible = await IsElementVisibleAsync(_errorMessage, "Error message");
            Logger.LogDebug($"Error message visible: {isVisible}");
            return isVisible;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error checking error message visibility: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Clears the checkout form
    /// </summary>
    public async Task<CheckoutStepOnePage> ClearCheckoutFormAsync()
    {
        try
        {
            Logger.LogStep("Clearing checkout form");
            await _firstNameField.ClearAsync();
            await _lastNameField.ClearAsync();
            await _postalCodeField.ClearAsync();
            Logger.LogInfo("Checkout form cleared");
            return this;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to clear checkout form: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Verifies the checkout step one page is displayed correctly
    /// </summary>
    public async Task<bool> IsCheckoutStepOnePageDisplayedAsync()
    {
        try
        {
            var isContainerVisible = await IsElementVisibleAsync(_checkoutContainer, "Checkout container");
            var isFirstNameVisible = await IsElementVisibleAsync(_firstNameField, "First name field");
            var isLastNameVisible = await IsElementVisibleAsync(_lastNameField, "Last name field");
            var isPostalCodeVisible = await IsElementVisibleAsync(_postalCodeField, "Postal code field");
            var isContinueVisible = await IsElementVisibleAsync(_continueButton, "Continue button");
            var isCancelVisible = await IsElementVisibleAsync(_cancelButton, "Cancel button");
            
            var isDisplayed = isContainerVisible && isFirstNameVisible && isLastNameVisible && 
                            isPostalCodeVisible && isContinueVisible && isCancelVisible;
            
            Logger.LogAssertion("Checkout step one page elements visibility", "All elements visible", 
                $"Container: {isContainerVisible}, FirstName: {isFirstNameVisible}, LastName: {isLastNameVisible}, " +
                $"PostalCode: {isPostalCodeVisible}, Continue: {isContinueVisible}, Cancel: {isCancelVisible}", 
                isDisplayed);
            
            return isDisplayed;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error verifying checkout step one page display: {ex.Message}");
            return false;
        }
    }
}
