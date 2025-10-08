using Xunit.Sdk;

namespace Xunit.v3.IntegrationTesting.Comparers;

/// <summary>
/// Can't use built in <see cref="TestCollectionComparer{TTestCollection}"/> because it requires TTestCollection to be a class,
/// and <see cref="ITestCollectionOrderer.OrderTestCollections{TTestCollection}(IReadOnlyCollection{TTestCollection})"/> does not have such a restriction.
/// This is a local version that does not have that restriction.
/// </summary>
public class TestCollectionComparerLocal<TTestCollection> : IEqualityComparer<TTestCollection>
    where TTestCollection : ITestCollection
{
    public static readonly TestCollectionComparerLocal<TTestCollection> Instance = new();

    /// <inheritdoc/>
    public bool Equals(
        TTestCollection? x,
        TTestCollection? y) =>
            (x is null && y is null) || (x is not null && y is not null && x.UniqueID == y.UniqueID);

    /// <inheritdoc/>
    public int GetHashCode(TTestCollection obj) =>
        obj.UniqueID.GetHashCode();
}