using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit.v3;

namespace Xunit.V3.IntegrationTesting;

/// <summary>
/// Attribute that is applied to a method to indicate that it is a fact that should be run
/// by the default test runner.
/// Allows to specify test dependencies that must run and succeed for current test not to be skipped
/// </summary>
[XunitTestCaseDiscoverer(typeof(FactDiscoverer))]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class DependsOnAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1    
    ) : Attribute, IFactAttribute, IBeforeAfterTestAttribute
{
    /// <inheritdoc/>
    public string? DisplayName { get; set; }

    /// <inheritdoc/>
    public bool Explicit { get; set; }

    /// <inheritdoc/>
    public Type[]? SkipExceptions { get; set; }

    /// <inheritdoc/>
    public string? SourceFilePath { get; } = sourceFilePath;

    /// <inheritdoc/>
    public int? SourceLineNumber { get; } = sourceLineNumber < 1 ? null : sourceLineNumber;

    /// <inheritdoc/>
    public int Timeout { get; set; }

    /// <summary>
    /// Gets or sets the list of dependencies for the test.
    /// </summary>
    public string[] Dependencies { get; set; } = Array.Empty<string>();

    private string? _originalSkip;
    private string? _originalSkipWhen;
    private string? _originalSkipUnless;
    private Type? _originalSkipType;

    private const string _customSkip = "One or more dependencies were skipped or had failed.";

    /// <inheritdoc />
    public string? Skip
    {
        get
        {
            return _originalSkip == null ? _customSkip : $"{_originalSkip} or {_customSkip}";
        }
        set
        {
            _originalSkip = value;
        }
    }

    /// <inheritdoc />
    public string? SkipWhen
    {
        get
        {
            return nameof(SkipValidator.ShouldSkip);
        }
        set
        {
            _originalSkipWhen = value;
        }
    }

    /// <inheritdoc />
    public string? SkipUnless
    {
        get
        {
            return null;
        }
        set
        {
            _originalSkipUnless = value;
        }
    }

    /// <inheritdoc />
    public Type? SkipType
    {
        get
        {
            return typeof(SkipValidator);
        }
        set
        {
            _originalSkipType = value;
        }
    }

    public void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        TestContext.Current.KeyValueStorage[ReadableTestId(test)] = TestContext.Current.TestState?.Result.ToString() ?? "Unknown result";

        static string ReadableTestId(IXunitTest test)
        {
            return $"{test.TestCase.TestClassName}.{test.TestCase.TestMethodName}";
        }
    }

    public void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
    }
}

internal class SkipValidator
{
    public static bool ShouldSkip
    {
        get
        {
            return ShouldSkipTest();
        }
    }

    private static bool ShouldSkipTest()
    {
        var currentTestMethod = TestContext.Current.TestMethod as IXunitTestMethod;

        if (currentTestMethod == null)
            return false; // send diagnostic later

        // Get dependencyOn attribute
        var dependencyOn = currentTestMethod.Method.GetCustomAttribute<DependsOnAttribute>(false);

        if (dependencyOn == null)
            return false;

        // Get dependencies
        var dependencies = dependencyOn.Dependencies;

        if (dependencies == null || dependencies.Length == 0)
            return false;

        // Check if all dependent methods have passed
        foreach (var dependency in dependencies)
        {
            if (!TestContext.Current.KeyValueStorage.TryGetValue($"{currentTestMethod.TestClass.TestClassName}.{dependency}", out var result)
                || !Enum.TryParse<TestResult>((string?)result, out var testResult)
                || testResult != TestResult.Passed)
            {
                // One of the dependencies either didn't run or failed - skip current test.
                // Overriding skip reason won't take effect as it's been already cached by the framework

                return true;
            }
        }

        return false;
    }

}