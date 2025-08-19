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
    public string[] Dependencies { get; set; }

    private string? _originalSkip;
    private string? _originalSkipWhen;
    private string? _originalSkipUnless;
    private Type? _originalSkipType;

    /// <inheritdoc />
    public string? Skip
    {
        get
        {
            // Will be overriden via reflection if needed
            return "One or more dependencies were skipped or had failed.";
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
        TestContext.Current.KeyValueStorage[test.TestCase.UniqueID] = TestContext.Current.TestState.Result;
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
        return false;
    }

}

public class BB : BeforeAfterTestAttribute
{
    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        TestContext.Current.KeyValueStorage[test.TestCase.TestMethodName] = TestContext.Current.TestState.Result + " " + TestContext.Current.TestCase.TestMethodName;
        base.After(methodUnderTest, test);
    }
}

public class BB2 : BeforeAfterTestAttribute
{
    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        TestContext.Current.KeyValueStorage[test.TestCase.TestMethodName + "_"] = TestContext.Current.TestState.Result + " " + TestContext.Current.TestCase.TestMethodName;
        base.After(methodUnderTest, test);
    }
}