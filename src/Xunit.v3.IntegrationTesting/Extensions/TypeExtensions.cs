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

        var collectionAttr = type.GetCustomAttributes(true).OfType<ICollectionAttribute>().FirstOrDefault();
        if (collectionAttr != null)
        {
            return collectionAttr.Name;
        }

        // Fallback: the type has neither [CollectionDefinition] nor ICollectionAttribute.
        // This shouldn't happen with valid usage (XIT0009 enforces it), but return a
        // deterministic name rather than throwing.
        return CollectionAttribute.GetCollectionNameForType(type);
    }
}