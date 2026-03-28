namespace Xunit.v3.IntegrationTesting;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class DependsOnCollectionsAttribute(params Type[] dependencies) : Attribute
{
    public Type[] Dependencies { get; } = dependencies;

    /// <summary>
    /// Test collection orderer that orders collections based on dependencies declared via <see cref="DependsOnCollectionsAttribute"/>.
    /// Delegates to <see cref="DependencyAwareTestCollectionOrderer"/> for backward compatibility.
    /// </summary>
    internal class Orderer : DependencyAwareTestCollectionOrderer
    {
    }
}