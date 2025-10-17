namespace Framework.Models;

/// <summary>
/// Represents customer information for checkout process
/// </summary>
public class CustomerInfo
{
    /// <summary>
    /// Gets or sets the first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the postal code
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Returns a string representation of the customer info (without sensitive data)
    /// </summary>
    public override string ToString()
    {
        return $"Customer: {FirstName} {LastName}, Postal Code: {PostalCode}";
    }
}
