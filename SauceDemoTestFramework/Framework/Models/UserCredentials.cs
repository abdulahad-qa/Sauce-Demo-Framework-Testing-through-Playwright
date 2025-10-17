namespace Framework.Models;

/// <summary>
/// Represents user credentials for test authentication
/// </summary>
public class UserCredentials
{
    /// <summary>
    /// Gets or sets the username
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the password
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Returns a string representation of the credentials (without password for security)
    /// </summary>
    public override string ToString()
    {
        return $"Username: {Username}";
    }
}
