namespace Xunit.v3.IntegrationTesting;

/// <summary>
/// Declares that a test collection depends on one or more other collections.
/// Apply to collection definition classes to ensure upstream collections run first.
/// If any upstream collection has a failed or skipped test, all tests in this collection
/// will be automatically skipped.
/// <para>
/// Each <paramref name="dependencies"/> entry must be a collection definition type
/// (a class decorated with <see cref="Xunit.CollectionDefinitionAttribute"/>).
/// </para>
/// </summary>
/// <param name="dependencies">Collection definition types that this collection depends on.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class DependsOnCollectionsAttribute(params Type[] dependencies) : Attribute
{
    /// <summary>
    /// The collection definition types that this collection depends on.
    /// </summary>
    public Type[] Dependencies { get; } = dependencies ?? [];

    /// <summary>
    /// Test collection orderer that orders collections based on dependencies declared via <see cref="DependsOnCollectionsAttribute"/>.
    /// Delegates to <see cref="DependencyAwareTestCollectionOrderer"/> for backward compatibility.
    /// </summary>
    internal class Orderer : DependencyAwareTestCollectionOrderer
    {
    }
}