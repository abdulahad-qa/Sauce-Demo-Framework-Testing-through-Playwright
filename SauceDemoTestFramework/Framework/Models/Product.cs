namespace Framework.Models;

/// <summary>
/// Represents a product in the SauceDemo application
/// </summary>
public class Product
{
    /// <summary>
    /// Gets or sets the product name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the product price
    /// </summary>
    public string Price { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the product description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Returns a string representation of the product
    /// </summary>
    public override string ToString()
    {
        return $"{Name} - {Price}";
    }
}
