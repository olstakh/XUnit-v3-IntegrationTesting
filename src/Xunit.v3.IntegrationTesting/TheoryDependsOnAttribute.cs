using System.Runtime.CompilerServices;

namespace Xunit.v3.IntegrationTesting;

/// <summary>
/// Attribute that is applied to a method to indicate that it is a theory that should be run
/// by the default test runner.
/// Allows to specify test dependencies that must run and succeed for current test not to be skipped.
/// </summary>
[XunitTestCaseDiscoverer(typeof(TheoryDiscoverer))]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TheoryDependsOnAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1
    ) : DependsOnAttributeBase(sourceFilePath, sourceLineNumber), ITheoryAttribute
{
    /// <inheritdoc/>
    public bool DisableDiscoveryEnumeration { get; init; }

    /// <inheritdoc/>
    public bool SkipTestWithoutData { get; init; }
}
