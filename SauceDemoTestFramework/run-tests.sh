#!/bin/bash

echo "========================================"
echo "SauceDemo Test Automation Framework"
echo "========================================"
echo

echo "Installing Playwright browsers..."
dotnet run
echo

echo "Running all tests..."
dotnet test --logger "console;verbosity=detailed" --results-directory test-results
echo

echo "Test execution completed!"
echo
echo "Results available in:"
echo "- test-results/html-report/index.html"
echo "- logs/test-execution.log"
echo "- screenshots/ (if any failures)"
echo "- videos/ (if any failures)"
echo
