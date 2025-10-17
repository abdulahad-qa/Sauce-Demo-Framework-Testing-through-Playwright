using Serilog;
using Serilog.Events;

namespace Framework.Core;

/// <summary>
/// Implements logging functionality using Serilog
/// </summary>
public class TestLogger : ILogger
{
    private readonly Serilog.ILogger _serilogLogger;

    public TestLogger()
    {
        var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "test-execution.log");
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

        _serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(logPath, 
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    public void LogInfo(string message)
    {
        _serilogLogger.Information(message);
    }

    public void LogWarning(string message)
    {
        _serilogLogger.Warning(message);
    }

    public void LogError(string message)
    {
        _serilogLogger.Error(message);
    }

    public void LogDebug(string message)
    {
        _serilogLogger.Debug(message);
    }

    public void LogStep(string step)
    {
        _serilogLogger.Information($"STEP: {step}");
    }

    public void LogAssertion(string assertion, object? expected, object? actual, bool passed)
    {
        var status = passed ? "PASSED" : "FAILED";
        _serilogLogger.Information($"ASSERTION {status}: {assertion} | Expected: {expected} | Actual: {actual}");
    }

    public void Dispose()
    {
        (_serilogLogger as IDisposable)?.Dispose();
    }
}
