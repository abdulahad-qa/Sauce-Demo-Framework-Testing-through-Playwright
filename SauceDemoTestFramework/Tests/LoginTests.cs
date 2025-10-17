using NUnit.Framework;
using FluentAssertions;
using Framework.Core;
using Framework.Models;

namespace Tests;

/// <summary>
/// Test class for login functionality
/// </summary>
[TestFixture]
[Category("Login")]
public class LoginTests : BaseTest
{
    [Test]
    [Description("Verify successful login with standard user")]
    public async Task Login_WithValidStandardUser_ShouldSucceed()
    {
        // Arrange
        var credentials = Config.GetUserCredentials("StandardUser");
        
        // Act
        var productsPage = await LoginPage.LoginAsync(credentials);
        
        // Assert
        var isOnProductsPage = await productsPage.IsOnPageAsync();
        isOnProductsPage.Should().BeTrue("User should be redirected to products page after successful login");
        
        var isProductsPageDisplayed = await productsPage.IsProductsPageDisplayedAsync();
        isProductsPageDisplayed.Should().BeTrue("Products page should be displayed correctly");
    }

    [Test]
    [Description("Verify login failure with locked out user")]
    public async Task Login_WithLockedOutUser_ShouldFail()
    {
        // Arrange
        var credentials = Config.GetUserCredentials("LockedOutUser");
        var expectedErrorMessage = TestData.GetErrorMessage("LockedOutUser");
        
        // Act
        var loginPage = await LoginPage.LoginWithInvalidCredentialsAsync(credentials);
        
        // Assert
        var isErrorMessageDisplayed = await loginPage.IsErrorMessageDisplayedAsync();
        isErrorMessageDisplayed.Should().BeTrue("Error message should be displayed for locked out user");
        
        var actualErrorMessage = await loginPage.GetErrorMessageAsync();
        actualErrorMessage.Should().Be(expectedErrorMessage, "Error message should match expected locked out user message");
    }

    [Test]
    [Description("Verify login failure with invalid credentials")]
    public async Task Login_WithInvalidCredentials_ShouldFail()
    {
        // Arrange
        var invalidCredentials = new UserCredentials { Username = "invalid_user", Password = "invalid_password" };
        var expectedErrorMessage = TestData.GetErrorMessage("InvalidCredentials");
        
        // Act
        var loginPage = await LoginPage.LoginWithInvalidCredentialsAsync(invalidCredentials);
        
        // Assert
        var isErrorMessageDisplayed = await loginPage.IsErrorMessageDisplayedAsync();
        isErrorMessageDisplayed.Should().BeTrue("Error message should be displayed for invalid credentials");
        
        var actualErrorMessage = await loginPage.GetErrorMessageAsync();
        actualErrorMessage.Should().Be(expectedErrorMessage, "Error message should match expected invalid credentials message");
    }

    [Test]
    [Description("Verify login failure with empty username")]
    public async Task Login_WithEmptyUsername_ShouldFail()
    {
        // Arrange
        var credentials = new UserCredentials { Username = "", Password = "secret_sauce" };
        var expectedErrorMessage = TestData.GetErrorMessage("LoginRequired");
        
        // Act
        var loginPage = await LoginPage.LoginWithInvalidCredentialsAsync(credentials);
        
        // Assert
        var isErrorMessageDisplayed = await loginPage.IsErrorMessageDisplayedAsync();
        isErrorMessageDisplayed.Should().BeTrue("Error message should be displayed for empty username");
        
        var actualErrorMessage = await loginPage.GetErrorMessageAsync();
        actualErrorMessage.Should().Be(expectedErrorMessage, "Error message should match expected empty username message");
    }

    [Test]
    [Description("Verify login failure with empty password")]
    public async Task Login_WithEmptyPassword_ShouldFail()
    {
        // Arrange
        var credentials = new UserCredentials { Username = "standard_user", Password = "" };
        var expectedErrorMessage = TestData.GetErrorMessage("PasswordRequired");
        
        // Act
        var loginPage = await LoginPage.LoginWithInvalidCredentialsAsync(credentials);
        
        // Assert
        var isErrorMessageDisplayed = await loginPage.IsErrorMessageDisplayedAsync();
        isErrorMessageDisplayed.Should().BeTrue("Error message should be displayed for empty password");
        
        var actualErrorMessage = await loginPage.GetErrorMessageAsync();
        actualErrorMessage.Should().Be(expectedErrorMessage, "Error message should match expected empty password message");
    }

    [Test]
    [Description("Verify login page elements are displayed correctly")]
    public async Task LoginPage_ShouldDisplayAllRequiredElements()
    {
        // Act & Assert
        var isLoginPageDisplayed = await LoginPage.IsLoginPageDisplayedAsync();
        isLoginPageDisplayed.Should().BeTrue("Login page should display all required elements");
        
        var isOnLoginPage = await LoginPage.IsOnPageAsync();
        isOnLoginPage.Should().BeTrue("Should be on login page");
    }

    [Test]
    [Description("Verify successful login with problem user")]
    public async Task Login_WithProblemUser_ShouldSucceed()
    {
        // Arrange
        var credentials = Config.GetUserCredentials("ProblemUser");
        
        // Act
        var productsPage = await LoginPage.LoginAsync(credentials);
        
        // Assert
        var isOnProductsPage = await productsPage.IsOnPageAsync();
        isOnProductsPage.Should().BeTrue("Problem user should be able to login successfully");
    }

    [Test]
    [Description("Verify successful login with performance glitch user")]
    public async Task Login_WithPerformanceGlitchUser_ShouldSucceed()
    {
        // Arrange
        var credentials = Config.GetUserCredentials("PerformanceGlitchUser");
        
        // Act
        var productsPage = await LoginPage.LoginAsync(credentials);
        
        // Assert
        var isOnProductsPage = await productsPage.IsOnPageAsync();
        isOnProductsPage.Should().BeTrue("Performance glitch user should be able to login successfully");
    }
}
