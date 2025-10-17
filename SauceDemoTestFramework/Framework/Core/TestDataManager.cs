using System.Text.Json;
using Framework.Models;

namespace Framework.Core;

/// <summary>
/// Manages test data from JSON files
/// </summary>
public class TestDataManager : ITestDataManager
{
    private readonly JsonDocument _testData;
    private readonly Random _random;

    public TestDataManager()
    {
        var testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "TestData.json");
        var testDataJson = File.ReadAllText(testDataPath);
        _testData = JsonDocument.Parse(testDataJson);
        _random = new Random();
    }

    public List<Product> GetProducts()
    {
        try
        {
            var products = new List<Product>();
            var productsArray = _testData.RootElement.GetProperty("Products");
            
            foreach (var productElement in productsArray.EnumerateArray())
            {
                products.Add(new Product
                {
                    Name = productElement.GetProperty("Name").GetString() ?? string.Empty,
                    Price = productElement.GetProperty("Price").GetString() ?? string.Empty,
                    Description = productElement.GetProperty("Description").GetString() ?? string.Empty
                });
            }
            
            return products;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load products from test data: {ex.Message}", ex);
        }
    }

    public Product? GetProduct(string productName)
    {
        return GetProducts().FirstOrDefault(p => p.Name.Equals(productName, StringComparison.OrdinalIgnoreCase));
    }

    public List<CustomerInfo> GetCustomerInfo()
    {
        try
        {
            var customers = new List<CustomerInfo>();
            var customersArray = _testData.RootElement.GetProperty("CustomerInfo");
            
            foreach (var customerElement in customersArray.EnumerateArray())
            {
                customers.Add(new CustomerInfo
                {
                    FirstName = customerElement.GetProperty("FirstName").GetString() ?? string.Empty,
                    LastName = customerElement.GetProperty("LastName").GetString() ?? string.Empty,
                    PostalCode = customerElement.GetProperty("PostalCode").GetString() ?? string.Empty
                });
            }
            
            return customers;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load customer info from test data: {ex.Message}", ex);
        }
    }

    public CustomerInfo GetRandomCustomerInfo()
    {
        var customers = GetCustomerInfo();
        return customers[_random.Next(customers.Count)];
    }

    public List<string> GetSortOptions()
    {
        try
        {
            var sortOptions = new List<string>();
            var sortOptionsArray = _testData.RootElement.GetProperty("SortOptions");
            
            foreach (var optionElement in sortOptionsArray.EnumerateArray())
            {
                var option = optionElement.GetString();
                if (!string.IsNullOrEmpty(option))
                {
                    sortOptions.Add(option);
                }
            }
            
            return sortOptions;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load sort options from test data: {ex.Message}", ex);
        }
    }

    public string GetErrorMessage(string key)
    {
        try
        {
            var errorMessages = _testData.RootElement.GetProperty("ErrorMessages");
            return errorMessages.GetProperty(key).GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load error message for key '{key}': {ex.Message}", ex);
        }
    }

    public Dictionary<string, string> GetErrorMessages()
    {
        try
        {
            var errorMessages = new Dictionary<string, string>();
            var errorMessagesObject = _testData.RootElement.GetProperty("ErrorMessages");
            
            foreach (var property in errorMessagesObject.EnumerateObject())
            {
                errorMessages[property.Name] = property.Value.GetString() ?? string.Empty;
            }
            
            return errorMessages;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load error messages from test data: {ex.Message}", ex);
        }
    }

    public void Dispose()
    {
        _testData?.Dispose();
    }
}
