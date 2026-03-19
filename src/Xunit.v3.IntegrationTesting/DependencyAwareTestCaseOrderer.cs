using Xunit.Sdk;

namespace Xunit.v3.IntegrationTesting;

/// <summary>
/// Delegates to <see cref="FactDependsOnAttribute.Orderer"/>.
/// Kept for backward compatibility and assembly-level attribute registration.
/// </summary>
public class DependencyAwareTestCaseOrderer : ITestCaseOrderer
{
    private readonly FactDependsOnAttribute.Orderer _inner = new();

    /// <inheritdoc />
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
        where TTestCase : notnull, ITestCase
        => _inner.OrderTestCases(testCases);

    /// <inheritdoc />
    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
        where TTestCase : notnull, ITestCase
        => _inner.OrderTestCases(testCases);
}