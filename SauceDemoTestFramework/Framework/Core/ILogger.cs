namespace Framework.Core;

/// <summary>
/// Interface for logging test execution information
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Logs an information message
    /// </summary>
    /// <param name="message">The message to log</param>
    void LogInfo(string message);
    
    /// <summary>
    /// Logs a warning message
    /// </summary>
    /// <param name="message">The message to log</param>
    void LogWarning(string message);
    
    /// <summary>
    /// Logs an error message
    /// </summary>
    /// <param name="message">The message to log</param>
    void LogError(string message);
    
    /// <summary>
    /// Logs a debug message
    /// </summary>
    /// <param name="message">The message to log</param>
    void LogDebug(string message);
    
    /// <summary>
    /// Logs test step information
    /// </summary>
    /// <param name="step">The test step description</param>
    void LogStep(string step);
    
    /// <summary>
    /// Logs test assertion information
    /// </summary>
    /// <param name="assertion">The assertion description</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actual">The actual value</param>
    /// <param name="passed">Whether the assertion passed</param>
    void LogAssertion(string assertion, object? expected, object? actual, bool passed);
}
