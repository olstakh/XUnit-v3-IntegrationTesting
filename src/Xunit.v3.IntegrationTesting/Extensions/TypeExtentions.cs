using System.Reflection;

namespace Xunit.v3.IntegrationTesting.Extensions;

internal static class TypeExtensions
{
    public static string GetCollectionDefinitionName(this Type type)
    {
        var collectionDefinitionAttr = type.GetCustomAttribute<CollectionDefinitionAttribute>(false);
        if (collectionDefinitionAttr != null)
        {
            return collectionDefinitionAttr.Name ?? CollectionAttribute.GetCollectionNameForType(type);
        }

        var collectionAttr = type.GetCustomAttribute<CollectionAttribute>(false);
        if (collectionAttr != null)
        {
            return collectionAttr.Name;
        }

        // TODO: Handle when CollectionBehavior is set to CollectionBehavior.CollectionPerAssembly.
        // In that case - the collection name will be "Test collection for " + TestAssembly.AssemblyName
        // Below returns assumes CollectionPerClass.
        return CollectionAttribute.GetCollectionNameForType(type);
    }
}