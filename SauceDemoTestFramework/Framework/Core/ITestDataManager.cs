using Framework.Models;

namespace Framework.Core;

/// <summary>
/// Interface for managing test data
/// </summary>
public interface ITestDataManager
{
    /// <summary>
    /// Gets all products from test data
    /// </summary>
    List<Product> GetProducts();
    
    /// <summary>
    /// Gets a specific product by name
    /// </summary>
    /// <param name="productName">Name of the product</param>
    Product? GetProduct(string productName);
    
    /// <summary>
    /// Gets all customer information from test data
    /// </summary>
    List<CustomerInfo> GetCustomerInfo();
    
    /// <summary>
    /// Gets a random customer information
    /// </summary>
    CustomerInfo GetRandomCustomerInfo();
    
    /// <summary>
    /// Gets all sort options
    /// </summary>
    List<string> GetSortOptions();
    
    /// <summary>
    /// Gets error message by key
    /// </summary>
    /// <param name="key">Error message key</param>
    string GetErrorMessage(string key);
    
    /// <summary>
    /// Gets all error messages
    /// </summary>
    Dictionary<string, string> GetErrorMessages();
}
