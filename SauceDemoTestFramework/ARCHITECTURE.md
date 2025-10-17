# Framework Architecture Documentation

## Overview

This document provides a detailed overview of the SauceDemo Test Automation Framework architecture, design decisions, and implementation patterns.

## Architecture Principles

### 1. SOLID Principles Implementation

#### Single Responsibility Principle (SRP)
- **ConfigurationManager**: Only handles configuration management
- **BrowserManager**: Only manages browser operations
- **TestLogger**: Only handles logging functionality
- **Page Objects**: Each page class handles only its specific page logic

#### Open/Closed Principle (OCP)
- **BasePage**: Open for extension, closed for modification
- **BaseTest**: Extensible for new test types without modifying existing code
- **Interface-based design**: Easy to extend with new implementations

#### Liskov Substitution Principle (LSP)
- All page objects can be substituted for BasePage
- All managers implement their respective interfaces correctly

#### Interface Segregation Principle (ISP)
- **IConfigurationManager**: Only configuration-related methods
- **IBrowserManager**: Only browser-related operations
- **ILogger**: Only logging functionality
- **ITestDataManager**: Only test data operations

#### Dependency Inversion Principle (DIP)
- High-level modules depend on abstractions (interfaces)
- Dependency injection through constructor parameters
- Framework components depend on interfaces, not concrete implementations

### 2. Design Patterns

#### Page Object Model (POM)
```csharp
public class LoginPage : BasePage
{
    // Locators
    private readonly ILocator _usernameField;
    private readonly ILocator _passwordField;
    
    // Actions
    public async Task<ProductsPage> LoginAsync(UserCredentials credentials)
    
    // Verifications
    public async Task<bool> IsLoginPageDisplayedAsync()
}
```

#### Factory Pattern
- Browser creation through BrowserManager
- Page object instantiation in BaseTest

#### Strategy Pattern
- Different browser types (Chromium, Firefox, WebKit)
- Different logging strategies (Console, File)

#### Template Method Pattern
- BaseTest provides template for test execution
- BasePage provides template for page operations

## Framework Layers

### 1. Core Layer (`Framework/Core/`)

#### Configuration Management
```csharp
public interface IConfigurationManager
{
    string BaseUrl { get; }
    string Browser { get; }
    bool Headless { get; }
    UserCredentials GetUserCredentials(string userType);
}
```

**Responsibilities:**
- Load configuration from `appsettings.json`
- Provide type-safe access to configuration values
- Manage user credentials and test settings

#### Browser Management
```csharp
public interface IBrowserManager
{
    IBrowser Browser { get; }
    IPage Page { get; }
    Task InitializeAsync();
    Task NavigateToAsync(string url);
    Task<string> TakeScreenshotAsync(string? name = null);
}
```

**Responsibilities:**
- Initialize and manage browser instances
- Handle navigation and page operations
- Provide screenshot and trace capabilities
- Manage browser lifecycle

#### Logging System
```csharp
public interface ILogger
{
    void LogInfo(string message);
    void LogError(string message);
    void LogStep(string step);
    void LogAssertion(string assertion, object? expected, object? actual, bool passed);
}
```

**Responsibilities:**
- Structured logging with multiple levels
- Test step tracking
- Assertion logging
- File and console output

#### Test Data Management
```csharp
public interface ITestDataManager
{
    List<Product> GetProducts();
    List<CustomerInfo> GetCustomerInfo();
    CustomerInfo GetRandomCustomerInfo();
    string GetErrorMessage(string key);
}
```

**Responsibilities:**
- Load test data from JSON files
- Provide type-safe access to test data
- Support for random data selection
- Error message management

### 2. Page Layer (`Framework/Pages/`)

#### Base Page Class
```csharp
public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly ILogger Logger;
    protected readonly IConfigurationManager Config;
    
    public abstract string PageUrl { get; }
    public abstract string PageTitle { get; }
    
    protected async Task ClickElementAsync(ILocator locator, string elementDescription);
    protected async Task FillInputAsync(ILocator locator, string value, string fieldDescription);
    protected async Task<string> GetElementTextAsync(ILocator locator, string elementDescription);
}
```

**Responsibilities:**
- Common page operations (click, fill, get text)
- Error handling and logging
- Screenshot capabilities
- Element waiting and verification

#### Specific Page Classes
Each page class extends BasePage and implements:
- **Locators**: Element selectors for the page
- **Actions**: Page-specific operations
- **Verifications**: Page state validation
- **Navigation**: Page-to-page navigation

### 3. Model Layer (`Framework/Models/`)

#### Data Models
```csharp
public class UserCredentials
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class CustomerInfo
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}

public class Product
{
    public string Name { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
```

**Responsibilities:**
- Type-safe data representation
- Validation and constraints
- Serialization/deserialization support

### 4. Test Layer (`Tests/`)

#### Base Test Class
```csharp
public abstract class BaseTest
{
    protected IConfigurationManager Config { get; private set; }
    protected ILogger Logger { get; private set; }
    protected IBrowserManager BrowserManager { get; private set; }
    protected ITestDataManager TestData { get; private set; }
    
    // Page objects
    protected LoginPage LoginPage { get; private set; }
    protected ProductsPage ProductsPage { get; private set; }
    // ... other page objects
}
```

**Responsibilities:**
- Test setup and teardown
- Component initialization
- Common test utilities
- Failure handling (screenshots, traces)

#### Test Classes
Each test class extends BaseTest and contains:
- **Test Methods**: Individual test scenarios
- **Test Categories**: Organized by functionality
- **Assertions**: Using FluentAssertions for readability

## Data Flow

### 1. Test Execution Flow
```
Test Start → Setup → Test Execution → Teardown → Test End
     ↓           ↓           ↓            ↓          ↓
  Initialize  Browser    Page Actions  Cleanup   Report
  Components  Setup      & Assertions  Resources  Results
```

### 2. Page Object Flow
```
Test Method → Page Object → Base Page → Browser Manager → Playwright
     ↓            ↓            ↓             ↓              ↓
  Test Logic  Page Actions  Common Ops  Browser Ops   Web Driver
```

### 3. Configuration Flow
```
appsettings.json → ConfigurationManager → Framework Components
     ↓                      ↓                      ↓
  JSON Config         Type-safe Access        Runtime Values
```

## Error Handling Strategy

### 1. Exception Hierarchy
```
Exception
├── FrameworkException (Custom)
│   ├── ConfigurationException
│   ├── BrowserException
│   └── TestDataException
└── PlaywrightException (Third-party)
```

### 2. Error Handling Levels

#### Framework Level
- Configuration validation
- Browser initialization
- Resource cleanup

#### Page Level
- Element interaction failures
- Navigation errors
- Verification failures

#### Test Level
- Test execution failures
- Assertion failures
- Setup/teardown errors

### 3. Recovery Strategies
- **Retry Logic**: Automatic retry for flaky operations
- **Screenshot Capture**: Visual debugging on failures
- **Trace Recording**: Detailed execution traces
- **Graceful Degradation**: Continue execution where possible

## Logging Architecture

### 1. Logging Levels
```
DEBUG → INFO → WARNING → ERROR
  ↓       ↓        ↓        ↓
Detailed  General  Issues   Failures
Info      Status   Warnings Errors
```

### 2. Logging Components
- **TestLogger**: Serilog-based implementation
- **Console Output**: Real-time test execution
- **File Output**: Persistent log storage
- **Structured Logging**: JSON format for analysis

### 3. Log Categories
- **Test Steps**: Individual test actions
- **Assertions**: Verification results
- **Errors**: Exception details
- **Performance**: Timing information

## Configuration Architecture

### 1. Configuration Sources
- **appsettings.json**: Primary configuration
- **Environment Variables**: Override support
- **Command Line**: Runtime overrides

### 2. Configuration Categories
- **Test Settings**: Browser, timeouts, flags
- **Test Data**: User credentials, product data
- **Logging**: Log levels, output destinations

### 3. Type Safety
- **Strongly Typed**: No magic strings
- **Validation**: Runtime configuration validation
- **Defaults**: Sensible default values

## Extensibility Points

### 1. Adding New Pages
```csharp
public class NewPage : BasePage
{
    public NewPage(IPage page, ILogger logger, IConfigurationManager config) 
        : base(page, logger, config) { }
    
    public override string PageUrl => "new-page-url";
    public override string PageTitle => "New Page Title";
    
    // Add page-specific methods
}
```

### 2. Adding New Tests
```csharp
[TestFixture]
[Category("NewFeature")]
public class NewFeatureTests : BaseTest
{
    [Test]
    public async Task NewFeature_ShouldWork()
    {
        // Test implementation
    }
}
```

### 3. Adding New Browsers
```csharp
// In BrowserManager.InitializeAsync()
var browserType = _config.Browser.ToLower() switch
{
    "chromium" => _playwright.Chromium,
    "firefox" => _playwright.Firefox,
    "webkit" => _playwright.Webkit,
    "edge" => _playwright.Chromium, // New browser
    _ => _playwright.Chromium
};
```

## Performance Considerations

### 1. Browser Management
- **Single Browser Instance**: Reuse browser across tests
- **Context Isolation**: Separate contexts for parallel execution
- **Resource Cleanup**: Proper disposal of resources

### 2. Element Interaction
- **Optimized Selectors**: Fast, reliable element selection
- **Smart Waits**: Appropriate wait strategies
- **Batch Operations**: Group related operations

### 3. Test Execution
- **Parallel Execution**: Support for concurrent test runs
- **Test Isolation**: Independent test execution
- **Resource Management**: Efficient memory usage

## Security Considerations

### 1. Credential Management
- **No Hardcoded Credentials**: All credentials in configuration
- **Environment Separation**: Different credentials per environment
- **Secure Storage**: Consider encrypted storage for production

### 2. Data Privacy
- **No Sensitive Data**: Avoid logging sensitive information
- **Data Masking**: Mask passwords in logs
- **Cleanup**: Remove temporary data after tests

## Maintenance Guidelines

### 1. Code Organization
- **Consistent Naming**: Follow established conventions
- **Documentation**: Maintain inline documentation
- **Version Control**: Proper commit messages and branching

### 2. Testing the Framework
- **Unit Tests**: Test framework components
- **Integration Tests**: Test component interactions
- **Smoke Tests**: Verify framework functionality

### 3. Updates and Upgrades
- **Dependency Updates**: Regular package updates
- **Breaking Changes**: Handle Playwright updates
- **Backward Compatibility**: Maintain compatibility where possible

This architecture provides a solid foundation for scalable, maintainable test automation while following industry best practices and design patterns.
