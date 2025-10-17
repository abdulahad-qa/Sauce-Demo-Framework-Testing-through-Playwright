using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Framework.Core
{
    /// <summary>
    /// Custom test report generator for creating detailed test execution reports
    /// </summary>
    public class TestReportGenerator
    {
        private readonly string _reportDirectory;
        private readonly List<TestResult> _testResults;
        private readonly ILogger _logger;
        private static readonly string _sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        public TestReportGenerator(ILogger logger)
        {
            _logger = logger;
            // Create TestResults folder in project root instead of bin folder
            var projectRoot = Directory.GetCurrentDirectory();
            // Navigate up from bin/Debug/net8.0 to project root if needed
            while (!File.Exists(Path.Combine(projectRoot, "PlaywrightTest.csproj")))
            {
                var parent = Directory.GetParent(projectRoot);
                if (parent == null) break;
                projectRoot = parent.FullName;
            }
            _reportDirectory = Path.Combine(projectRoot, "TestResults");
            _testResults = new List<TestResult>();
            
            // Create TestResults directory if it doesn't exist
            if (!Directory.Exists(_reportDirectory))
            {
                Directory.CreateDirectory(_reportDirectory);
                _logger.LogInfo($"Created TestResults directory: {_reportDirectory}");
            }
        }

        /// <summary>
        /// Adds a test result to the collection
        /// </summary>
        /// <param name="testResult">Test result data</param>
        public void AddTestResult(TestResult testResult)
        {
            _testResults.Add(testResult);
            _logger.LogInfo($"Added test result: {testResult.TestName} - {testResult.Status}");
        }

        /// <summary>
        /// Generates a comprehensive test report in TXT format
        /// </summary>
        /// <param name="fileName">Optional custom file name</param>
        public void GenerateTxtReport(string fileName = null)
        {
            try
            {
                var reportFileName = fileName ?? $"SauceDemoTestFramework_{_sessionId}.txt";
                var reportPath = Path.Combine(_reportDirectory, reportFileName);

                var report = new StringBuilder();
                
                // Header
                report.AppendLine("=".PadRight(80, '='));
                report.AppendLine("                    SAUCEDEMO TEST EXECUTION REPORT");
                report.AppendLine("=".PadRight(80, '='));
                report.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                report.AppendLine($"Total Tests: {_testResults.Count}");
                report.AppendLine($"Passed: {_testResults.Count(t => t.Status == "PASSED")}");
                report.AppendLine($"Failed: {_testResults.Count(t => t.Status == "FAILED")}");
                report.AppendLine($"Skipped: {_testResults.Count(t => t.Status == "SKIPPED")}");
                report.AppendLine($"Success Rate: {GetSuccessRate():F1}%");
                report.AppendLine("=".PadRight(80, '='));
                report.AppendLine();

                // Summary Table
                report.AppendLine("TEST EXECUTION SUMMARY");
                report.AppendLine("-".PadRight(80, '-'));
                report.AppendLine($"{"#".PadRight(4)} {"Test Name".PadRight(40)} {"Status".PadRight(10)} {"Duration".PadRight(10)} {"Browser".PadRight(10)}");
                report.AppendLine("-".PadRight(80, '-'));

                for (int i = 0; i < _testResults.Count; i++)
                {
                    var result = _testResults[i];
                    var testNumber = (i + 1).ToString().PadRight(4);
                    var testName = TruncateString(result.TestName, 40).PadRight(40);
                    var status = result.Status.PadRight(10);
                    var duration = $"{result.Duration:F2}s".PadRight(10);
                    var browser = result.Browser.PadRight(10);

                    report.AppendLine($"{testNumber} {testName} {status} {duration} {browser}");
                }

                report.AppendLine("-".PadRight(80, '-'));
                report.AppendLine();

                // Detailed Results
                report.AppendLine("DETAILED TEST RESULTS");
                report.AppendLine("=".PadRight(80, '='));

                for (int i = 0; i < _testResults.Count; i++)
                {
                    var result = _testResults[i];
                    report.AppendLine($"Test #{i + 1}: {result.TestName}");
                    report.AppendLine($"  Status: {result.Status}");
                    report.AppendLine($"  Category: {result.Category}");
                    report.AppendLine($"  Browser: {result.Browser}");
                    report.AppendLine($"  Start Time: {result.StartTime:yyyy-MM-dd HH:mm:ss}");
                    report.AppendLine($"  End Time: {result.EndTime:yyyy-MM-dd HH:mm:ss}");
                    report.AppendLine($"  Duration: {result.Duration:F2} seconds");
                    
                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        report.AppendLine($"  Error: {result.ErrorMessage}");
                    }
                    
                    if (!string.IsNullOrEmpty(result.ScreenshotPath))
                    {
                        report.AppendLine($"  Screenshot: {result.ScreenshotPath}");
                    }
                    
                    report.AppendLine();
                }

                // Failed Tests Summary
                var failedTests = _testResults.Where(t => t.Status == "FAILED").ToList();
                if (failedTests.Any())
                {
                    report.AppendLine("FAILED TESTS SUMMARY");
                    report.AppendLine("=".PadRight(80, '='));
                    foreach (var failedTest in failedTests)
                    {
                        report.AppendLine($"• {failedTest.TestName}");
                        report.AppendLine($"  Error: {failedTest.ErrorMessage}");
                        report.AppendLine();
                    }
                }

                // Footer
                report.AppendLine("=".PadRight(80, '='));
                report.AppendLine($"Report generated by SauceDemo Test Framework");
                report.AppendLine($"Framework Version: 1.0.0");
                report.AppendLine("=".PadRight(80, '='));

                File.WriteAllText(reportPath, report.ToString());
                _logger.LogInfo($"TXT report generated: {reportPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating TXT report: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates a test report in CSV format (Excel compatible)
        /// </summary>
        /// <param name="fileName">Optional custom file name</param>
        public void GenerateCsvReport(string fileName = null)
        {
            try
            {
                var reportFileName = fileName ?? $"SauceDemoTestFramework_{_sessionId}.csv";
                var reportPath = Path.Combine(_reportDirectory, reportFileName);

                var csv = new StringBuilder();
                
                // CSV Header
                csv.AppendLine("Test Number,Test Name,Category,Status,Browser,Start Time,End Time,Duration (seconds),Error Message,Screenshot Path");

                // CSV Data
                for (int i = 0; i < _testResults.Count; i++)
                {
                    var result = _testResults[i];
                    var testNumber = i + 1;
                    var testName = EscapeCsvField(result.TestName);
                    var category = EscapeCsvField(result.Category);
                    var status = EscapeCsvField(result.Status);
                    var browser = EscapeCsvField(result.Browser);
                    var startTime = result.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                    var endTime = result.EndTime.ToString("yyyy-MM-dd HH:mm:ss");
                    var duration = result.Duration.ToString("F2");
                    var errorMessage = EscapeCsvField(result.ErrorMessage ?? "");
                    var screenshotPath = EscapeCsvField(result.ScreenshotPath ?? "");

                    csv.AppendLine($"{testNumber},{testName},{category},{status},{browser},{startTime},{endTime},{duration},{errorMessage},{screenshotPath}");
                }

                File.WriteAllText(reportPath, csv.ToString());
                _logger.LogInfo($"CSV report generated: {reportPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating CSV report: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates both TXT and CSV reports
        /// </summary>
        public void GenerateAllReports()
        {
            GenerateTxtReport();
            GenerateCsvReport();
        }

        /// <summary>
        /// Clears all test results from memory
        /// </summary>
        public void ClearResults()
        {
            _testResults.Clear();
            _logger.LogInfo("Test results cleared from memory");
        }

        /// <summary>
        /// Gets the current success rate percentage
        /// </summary>
        private double GetSuccessRate()
        {
            if (_testResults.Count == 0) return 0;
            var passedCount = _testResults.Count(t => t.Status == "PASSED");
            return (double)passedCount / _testResults.Count * 100;
        }

        /// <summary>
        /// Truncates string to specified length
        /// </summary>
        private string TruncateString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
                return input;
            return input.Substring(0, maxLength - 3) + "...";
        }

        /// <summary>
        /// Escapes CSV field values
        /// </summary>
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";
            
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            
            return field;
        }

    }

    /// <summary>
    /// Test result data model
    /// </summary>
    public class TestResult
    {
        public string TestName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Browser { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ScreenshotPath { get; set; } = string.Empty;
    }
}
