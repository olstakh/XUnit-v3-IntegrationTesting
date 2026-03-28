using Microsoft.CodeAnalysis;

namespace Xunit.v3.IntegrationTesting.Analyzers.Helpers;

internal static class TypeHierarchyHelper
{
    /// <summary>
    /// Returns true if <paramref name="type"/> is the same as or derives from <paramref name="baseType"/>.
    /// </summary>
    public static bool IsOrDerivesFrom(ITypeSymbol? type, ITypeSymbol? baseType)
    {
        if (type is null || baseType is null)
            return false;

        var current = type;
        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
                return true;
            current = current.BaseType;
        }

        return false;
    }
}
