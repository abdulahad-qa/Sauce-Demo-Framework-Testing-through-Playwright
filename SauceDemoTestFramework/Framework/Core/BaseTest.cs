using Microsoft.Playwright;
using NUnit.Framework;
using Framework.Pages;
using Framework.Models;

namespace Framework.Core;

/// <summary>
/// Base test class providing common setup and teardown functionality
/// </summary>
public abstract class BaseTest
{
    protected IConfigurationManager Config { get; private set; } = null!;
    protected ILogger Logger { get; private set; } = null!;
    protected IBrowserManager BrowserManager { get; private set; } = null!;
    protected ITestDataManager TestData { get; private set; } = null!;
    protected TestReportGenerator ReportGenerator { get; private set; } = null!;
    
    // Test execution tracking
    private DateTime _testStartTime;
    private string _currentTestName = string.Empty;
    private string _currentTestCategory = string.Empty;
    
    // Page objects
    protected LoginPage LoginPage { get; private set; } = null!;
    protected ProductsPage ProductsPage { get; private set; } = null!;
    protected CartPage CartPage { get; private set; } = null!;
    protected CheckoutStepOnePage CheckoutStepOnePage { get; private set; } = null!;
    protected CheckoutStepTwoPage CheckoutStepTwoPage { get; private set; } = null!;
    protected CheckoutCompletePage CheckoutCompletePage { get; private set; } = null!;

    [OneTimeSetUp]
    public virtual Task OneTimeSetUpAsync()
    {
        // Initialize core components
        Config = new ConfigurationManager();
        Logger = new TestLogger();
        BrowserManager = new BrowserManager(Config, Logger);
        TestData = new TestDataManager();
        ReportGenerator = new TestReportGenerator(Logger);
        
        Logger.LogInfo("=== Test Suite Started ===");
        Logger.LogInfo($"Test Framework: Playwright C#");
        Logger.LogInfo($"Target Application: {Config.BaseUrl}");
        Logger.LogInfo($"Browser: {Config.Browser}");
        Logger.LogInfo($"Headless Mode: {Config.Headless}");
        
        return Task.CompletedTask;
    }

    [SetUp]
    public virtual async Task SetUpAsync()
    {
        try
        {
            // Track test start
            _testStartTime = DateTime.Now;
            _currentTestName = TestContext.CurrentContext.Test.Name;
            _currentTestCategory = GetTestCategory();
            
            Logger.LogInfo($"--- Starting Test: {_currentTestName} ---");
            
            // Initialize browser
            await BrowserManager.InitializeAsync();
            
            // Initialize page objects
            LoginPage = new LoginPage(BrowserManager.Page, Logger, Config);
            ProductsPage = new ProductsPage(BrowserManager.Page, Logger, Config);
            CartPage = new CartPage(BrowserManager.Page, Logger, Config);
            CheckoutStepOnePage = new CheckoutStepOnePage(BrowserManager.Page, Logger, Config);
            CheckoutStepTwoPage = new CheckoutStepTwoPage(BrowserManager.Page, Logger, Config);
            CheckoutCompletePage = new CheckoutCompletePage(BrowserManager.Page, Logger, Config);
            
            // Navigate to the application
            await BrowserManager.NavigateToAsync(Config.BaseUrl);
            
            Logger.LogInfo("Test setup completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Test setup failed: {ex.Message}");
            await TakeFailureScreenshotAsync("setup_failed");
            throw;
        }
    }

    [TearDown]
    public virtual async Task TearDownAsync()
    {
        try
        {
            var testResult = TestContext.CurrentContext.Result.Outcome.Status;
            var testEndTime = DateTime.Now;
            var testDuration = (testEndTime - _testStartTime).TotalSeconds;
            
            if (testResult == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                Logger.LogError($"--- Test Failed: {_currentTestName} ---");
                await TakeFailureScreenshotAsync("test_failed");
                
                if (Config.TraceOnFailure)
                {
                    await TakeTraceAsync("test_failed");
                }
            }
            else
            {
                Logger.LogInfo($"--- Test Completed: {_currentTestName} ---");
            }
            
            // Close browser
            await BrowserManager.CloseAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Test teardown failed: {ex.Message}");
        }
        finally
        {
            // ALWAYS record test result in custom report (this is the key part!)
            RecordTestResultAsync();
        }
    }

    [OneTimeTearDown]
    public virtual Task OneTimeTearDownAsync()
    {
        try
        {
            Logger.LogInfo("=== Test Suite Completed ===");
            
            // Generate final test reports
            ReportGenerator.GenerateAllReports();
            Logger.LogInfo("Custom test reports generated successfully");
            
            // Dispose resources
            (Config as IDisposable)?.Dispose();
            (Logger as IDisposable)?.Dispose();
            (TestData as IDisposable)?.Dispose();
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Logger.LogError($"One-time teardown failed: {ex.Message}");
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Takes a screenshot on test failure
    /// </summary>
    protected async Task TakeFailureScreenshotAsync(string name)
    {
        if (Config.ScreenshotOnFailure)
        {
            try
            {
                await BrowserManager.TakeScreenshotAsync(name);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to take failure screenshot: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Takes a trace on test failure
    /// </summary>
    protected async Task TakeTraceAsync(string name)
    {
        try
        {
            var tracePath = Path.Combine("traces", $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
            Directory.CreateDirectory("traces");
            await BrowserManager.Page.Context.Tracing.StopAsync(new TracingStopOptions { Path = tracePath });
            Logger.LogInfo($"Trace saved: {tracePath}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to take trace: {ex.Message}");
        }
    }

    /// <summary>
    /// Performs login with specified user type
    /// </summary>
    protected async Task<ProductsPage> LoginAsync(string userType)
    {
        var credentials = Config.GetUserCredentials(userType);
        return await LoginPage.LoginAsync(credentials);
    }

    /// <summary>
    /// Performs complete checkout process
    /// </summary>
    protected async Task<CheckoutCompletePage> CompleteCheckoutAsync(string userType, string productName)
    {
        // Login
        var productsPage = await LoginAsync(userType);
        
        // Add product to cart
        await productsPage.AddProductToCartAsync(productName);
        
        // Go to cart
        var cartPage = await productsPage.GoToCartAsync();
        
        // Proceed to checkout
        var checkoutStepOne = await cartPage.ProceedToCheckoutAsync();
        
        // Fill checkout form
        var customerInfo = TestData.GetRandomCustomerInfo();
        await checkoutStepOne.FillCheckoutFormAsync(customerInfo);
        
        // Continue to step two
        var checkoutStepTwo = await checkoutStepOne.ContinueToStepTwoAsync();
        
        // Finish checkout
        return await checkoutStepTwo.FinishCheckoutAsync();
    }

    /// <summary>
    /// Performs logout
    /// </summary>
    protected async Task<LoginPage> LogoutAsync()
    {
        return await ProductsPage.LogoutAsync();
    }

    /// <summary>
    /// Records test result in custom report (called in finally block)
    /// </summary>
    private Task RecordTestResultAsync()
    {
        try
        {
            var testResult = TestContext.CurrentContext.Result.Outcome.Status;
            var testEndTime = DateTime.Now;
            var testDuration = (testEndTime - _testStartTime).TotalSeconds;
            
            var result = new TestResult
            {
                TestName = _currentTestName,
                Category = _currentTestCategory,
                Status = GetStatusString(testResult),
                Browser = Config.Browser,
                StartTime = _testStartTime,
                EndTime = testEndTime,
                Duration = testDuration,
                ErrorMessage = GetErrorMessage(testResult),
                ScreenshotPath = GetScreenshotPath(testResult)
            };
            
            ReportGenerator.AddTestResult(result);
            Logger.LogInfo($"Test result recorded: {_currentTestName} - {result.Status}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to record test result: {ex.Message}");
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets test category from test attributes
    /// </summary>
    private string GetTestCategory()
    {
        try
        {
            var categoryAttribute = TestContext.CurrentContext.Test.Properties.Get("Category");
            return categoryAttribute?.ToString() ?? "General";
        }
        catch
        {
            return "General";
        }
    }

    /// <summary>
    /// Converts NUnit test status to string
    /// </summary>
    private string GetStatusString(NUnit.Framework.Interfaces.TestStatus status)
    {
        return status switch
        {
            NUnit.Framework.Interfaces.TestStatus.Passed => "PASSED",
            NUnit.Framework.Interfaces.TestStatus.Failed => "FAILED",
            NUnit.Framework.Interfaces.TestStatus.Skipped => "SKIPPED",
            NUnit.Framework.Interfaces.TestStatus.Inconclusive => "INCONCLUSIVE",
            _ => "UNKNOWN"
        };
    }

    /// <summary>
    /// Gets error message for failed tests
    /// </summary>
    private string GetErrorMessage(NUnit.Framework.Interfaces.TestStatus status)
    {
        if (status == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            return TestContext.CurrentContext.Result.Message ?? "Test failed";
        }
        return string.Empty;
    }

    /// <summary>
    /// Gets screenshot path for failed tests
    /// </summary>
    private string GetScreenshotPath(NUnit.Framework.Interfaces.TestStatus status)
    {
        if (status == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            var screenshotsDir = Path.Combine("screenshots");
            var screenshotFile = $"test_failed_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            return Path.Combine(screenshotsDir, screenshotFile);
        }
        return string.Empty;
    }
}
