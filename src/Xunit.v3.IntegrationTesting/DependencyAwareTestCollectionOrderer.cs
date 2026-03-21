using Xunit.Sdk;

namespace Xunit.v3.IntegrationTesting;

/// <summary>
/// Delegates to <see cref="DependsOnCollectionsAttribute.Orderer"/>.
/// Kept for backward compatibility and assembly-level attribute registration.
/// </summary>
public class DependencyAwareTestCollectionOrderer : ITestCollectionOrderer
{
    private readonly DependsOnCollectionsAttribute.Orderer _inner = new();

    /// <inheritdoc />
    public IEnumerable<TTestCollection> OrderTestCollections<TTestCollection>(IEnumerable<TTestCollection> testCollections)
        where TTestCollection : ITestCollection
        => _inner.OrderTestCollections(testCollections);

    /// <inheritdoc />
    public IReadOnlyCollection<TTestCollection> OrderTestCollections<TTestCollection>(IReadOnlyCollection<TTestCollection> testCollections)
        where TTestCollection : ITestCollection
        => _inner.OrderTestCollections(testCollections);
}