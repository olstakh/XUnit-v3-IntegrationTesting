using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit.Sdk;
using Xunit.v3;
using Xunit.v3.IntegrationTesting.Exceptions;
using Xunit.v3.IntegrationTesting.Extensions;

namespace Xunit.v3.IntegrationTesting;

/// <summary>
/// Attribute that is applied to a method to indicate that it is a fact that should be run
/// by the default test runner.
/// Allows to specify test dependencies that must run and succeed for current test not to be skipped
/// </summary>
[XunitTestCaseDiscoverer(typeof(FactDiscoverer))]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class FactDependsOnAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1    
    ) : Attribute, IFactAttribute
{
    /// <summary>
    /// Test case orderer that orders tests based on dependencies declared via <see cref="FactDependsOnAttribute"/>.
    /// </summary>
    internal class Orderer : ITestCaseOrderer
    {
        /// <inheritdoc />
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : notnull, ITestCase
        {
            try
            {
                var graph = testCases.ToOrientedGraph(out var issues);
                var orderedCases = graph.TopologicalSort();

                foreach (var issue in issues)
                {
                    TestContext.Current.SendDiagnosticMessage("[TEST CASE ORDERER WARNING] " + issue);
                }

                return orderedCases.Cast<TTestCase>();
            }
            catch (CircularDependencyException<IXunitTestCase> ex)
            {
                var circularDependencyMessage = string.Format(
                    CultureInfo.CurrentCulture,
                    "There's a circular dependency involving the following tests: {0}",
                    string.Join(" -> ", ex.DependencyCycle.Select(tc => $"{tc.TestClassName}.{tc.TestMethodName}")));

                TestContext.Current.SendDiagnosticMessage("[TEST CASE ORDERER ERROR] " +
                    circularDependencyMessage);

                // Return tests in original order instead of throwing, which causes
                // a catastrophic failure in xUnit. The diagnostic message above is
                // sufficient to surface the problem.
                // This will result in test cases to be skipped anyway, since their dependency hasn't completed.
                return testCases.ToArray();
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while ordering test cases: " + ex.Message;
                TestContext.Current.SendDiagnosticMessage("[TEST CASE ORDERER ERROR] " + message);

                // Return tests in original order instead of throwing.
                return testCases.ToArray();
            }
        }

        /// <inheritdoc />
        public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases) where TTestCase : notnull, ITestCase
        {
            return OrderTestCases(testCases.AsEnumerable()).ToList();
        }
    }

    /// <inheritdoc/>
    public string? DisplayName { get; init; }

    /// <inheritdoc/>
    public bool Explicit { get; init; }

    /// <inheritdoc/>
    public Type[]? SkipExceptions { get; init; }

    /// <inheritdoc/>
    public string? SourceFilePath { get; } = sourceFilePath;

    /// <inheritdoc/>
    public int? SourceLineNumber { get; } = sourceLineNumber < 1 ? null : sourceLineNumber;

    /// <inheritdoc/>
    public int Timeout { get; init; }

    /// <summary>
    /// Gets or sets the list of dependencies for the test.
    /// </summary>
    public string[] Dependencies { get; init; } = Array.Empty<string>();

    internal string? OriginalSkip;
    internal string? OriginalSkipWhen;
    internal string? OriginalSkipUnless;
    internal Type? OriginalSkipType;

    private const string _customSkip = "One or more dependencies were skipped or had failed.";

    /// <inheritdoc />
    public string? Skip
    {
        get
        {
            return OriginalSkip == null ? _customSkip : $"{OriginalSkip} or {_customSkip}";
        }
        init
        {
            OriginalSkip = value;
        }
    }

    /// <inheritdoc />
    public string? SkipWhen
    {
        get
        {
            return nameof(SkipValidator.ShouldSkip);
        }
        init
        {
            OriginalSkipWhen = value;
        }
    }

    /// <inheritdoc />
    public string? SkipUnless
    {
        get
        {
            return null;
        }
        init
        {
            OriginalSkipUnless = value;
        }
    }

    /// <inheritdoc />
    public Type? SkipType
    {
        get
        {
            return typeof(SkipValidator);
        }
        init
        {
            OriginalSkipType = value;
        }
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

    private static bool ShouldSkipTestOriginalSetup(FactDependsOnAttribute dependsOn, IXunitTestCase testCase)
    {
        var originalSkipWhenFieldValue = (string?)dependsOn.OriginalSkipWhen;
        var originalSkipUnlessFieldValue = (string?)dependsOn.OriginalSkipUnless;
        var originalSkipReasonFieldValue = (string?)dependsOn.OriginalSkip;
        var originalSkipTypeFieldValue = (Type?)dependsOn.OriginalSkipType;

        #region Original implementation of XunitTestRunnerBaseContext.GetRuntimeSkipReason

        if (originalSkipUnlessFieldValue is null && originalSkipWhenFieldValue is null)
        {
            return false;
        }

        if (originalSkipUnlessFieldValue is not null && originalSkipWhenFieldValue is not null)
        {
            throw new TestPipelineException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    "Both 'SkipUnless' and 'SkipWhen' are set on test method '{0}.{1}'; they are mutually exclusive",
                    testCase.TestClassName,
                    testCase.TestMethodName
                )
            );
        }

        if (originalSkipReasonFieldValue is null)
        {
            throw new TestPipelineException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    "You must set 'Skip' when you set 'SkipUnless' or 'SkipWhen' on test method '{0}.{1}' to set the message for conditional skips",
                    testCase.TestClassName,
                    testCase.TestMethodName
                )
            );
        }

        var propertyType = originalSkipTypeFieldValue ?? testCase.TestClass.Class;
        var propertyName = (originalSkipUnlessFieldValue ?? originalSkipWhenFieldValue)!;

        var property =
            propertyType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static)
                ?? throw new TestPipelineException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Cannot find public static property '{0}' on type '{1}' for dynamic skip on test method '{2}.{3}'",
                        propertyName,
                        propertyType,
                        testCase.TestClassName,
                        testCase.TestMethodName
                    )
                );

        var getMethod =
            property.GetGetMethod()
                ?? throw new TestPipelineException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Public static property '{0}' on type '{1}' must be readable for dynamic skip on test method '{2}.{3}'",
                        propertyName,
                        propertyType,
                        testCase.TestClassName,
                        testCase.TestMethodName
                    )
                );

        if (getMethod.ReturnType != typeof(bool) || getMethod.Invoke(null, []) is not bool result)
            throw new TestPipelineException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    "Public static property '{0}' on type '{1}' must return bool for dynamic skip on test method '{2}.{3}'",
                    propertyName,
                    propertyType,
                    testCase.TestClassName,
                    testCase.TestMethodName
                )
            );

        var shouldSkip = (originalSkipUnlessFieldValue, originalSkipWhenFieldValue, result) switch
        {
            (not null, _, false) => true,
            (_, not null, true) => true,
            _ => false,
        };

        return shouldSkip;

        #endregion
    }

    private static bool ShouldSkipTest()
    {
        var currentTestCase = TestContext.Current.TestCase as IXunitTestCase;

        if (currentTestCase == null)
            return false; // send diagnostic later

        if (ShouldSkipBasedOnCollectionDependencies(currentTestCase))
            return true;

        if (ShouldSkipBasedOnMethodDependencies(currentTestCase))
            return true;

        return false;
    }

    private static bool ShouldSkipBasedOnCollectionDependencies(IXunitTestCase currentTestCase)
    {
        // Get dependencyOn attribute
        var dependsOn = currentTestCase.TestCollection.CollectionDefinition?.GetCustomAttribute<DependsOnCollectionsAttribute>(false);

        if (dependsOn == null)
            return false;

         // Get dependencies
        var dependencies = dependsOn.Dependencies;

        if (dependencies == null || dependencies.Length == 0)
            return false;

        // Check if all dependent collections have passed
        foreach (var dependency in dependencies)
        {
            var collectionName = dependency.GetCollectionDefinitionName();

            var collectionResults = TestContext.Current.KeyValueStorage.Keys
                .Where(k => k.StartsWith($"{collectionName}.", StringComparison.Ordinal))
                .Select(k => TestContext.Current.KeyValueStorage[k])
                .OfType<string>()
                .Select(r => Enum.TryParse<TestResult>(r, out var tr) ? tr : (TestResult?)null)
                .Where(r => r.HasValue)
                .ToList();
            
            if (collectionResults.Count == 0 || collectionResults.Any(r => r != TestResult.Passed))
            {
                // One of the dependencies either didn't run or failed - skip current test.
                // Overriding skip reason won't take effect as it's been already cached by the framework

                return true;
            }
        }

        return false;
    }

    private static bool ShouldSkipBasedOnMethodDependencies(IXunitTestCase currentTestCase)
    {
        // Get dependencyOn attribute
        var dependsOn = currentTestCase.TestMethod.Method.GetCustomAttribute<FactDependsOnAttribute>(false);

        if (dependsOn == null)
            return false; // send diagnostic later, we shouldn't end up here

        if (ShouldSkipTestOriginalSetup(dependsOn, currentTestCase))
            return true;

        // Get dependencies
        var dependencies = dependsOn.Dependencies;

        if (dependencies == null || dependencies.Length == 0)
            return false;

        // Check if all dependent methods have passed
        foreach (var dependency in dependencies)
        {
            if (!TestContext.Current.KeyValueStorage.TryGetValue($"{currentTestCase.TestCollection.TestCollectionDisplayName}.{currentTestCase.TestClass.TestClassName}.{dependency}", out var result)
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