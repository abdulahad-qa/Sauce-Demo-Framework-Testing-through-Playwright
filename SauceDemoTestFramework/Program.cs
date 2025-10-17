using Microsoft.Playwright;

namespace PlaywrightTest;

/// <summary>
/// Main program entry point for the test automation framework
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== SauceDemo Test Automation Framework ===");
        Console.WriteLine("Built with Playwright C# and NUnit");
        Console.WriteLine();
        
        try
        {
            // Install Playwright browsers if not already installed
            Console.WriteLine("Installing Playwright browsers...");
            Microsoft.Playwright.Program.Main(new string[] { "install" });
            Console.WriteLine("Playwright browsers installed successfully!");
            Console.WriteLine();
            
            // Display available commands
            Console.WriteLine("Available commands:");
            Console.WriteLine("  dotnet test                    - Run all tests");
            Console.WriteLine("  dotnet test --filter Login     - Run login tests only");
            Console.WriteLine("  dotnet test --filter Products  - Run product tests only");
            Console.WriteLine("  dotnet test --filter Cart      - Run cart tests only");
            Console.WriteLine("  dotnet test --filter Checkout  - Run checkout tests only");
            Console.WriteLine("  dotnet test --filter EndToEnd  - Run end-to-end tests only");
            Console.WriteLine();
            Console.WriteLine("Test results will be available in:");
            Console.WriteLine("  - Console output (real-time)");
            Console.WriteLine("  - logs/test-execution.log (detailed logs)");
            Console.WriteLine("  - test-results/html-report/ (HTML report)");
            Console.WriteLine("  - screenshots/ (failure screenshots)");
            Console.WriteLine("  - videos/ (test recordings)");
            Console.WriteLine("  - traces/ (failure traces)");
            Console.WriteLine();
            Console.WriteLine("Framework Features:");
            Console.WriteLine("  ✓ Modular Page Object Model");
            Console.WriteLine("  ✓ Comprehensive logging with Serilog");
            Console.WriteLine("  ✓ Robust error handling and retry logic");
            Console.WriteLine("  ✓ Multiple browser support (Chrome, Firefox, Safari)");
            Console.WriteLine("  ✓ Screenshot and video recording on failures");
            Console.WriteLine("  ✓ Test data management with JSON");
            Console.WriteLine("  ✓ FluentAssertions for readable test assertions");
            Console.WriteLine("  ✓ Allure reporting integration");
            Console.WriteLine("  ✓ SOLID principles and clean architecture");
            Console.WriteLine();
            Console.WriteLine("Ready to run tests! Use 'dotnet test' to start.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during setup: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
