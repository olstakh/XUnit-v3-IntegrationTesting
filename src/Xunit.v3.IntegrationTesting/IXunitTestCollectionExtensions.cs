using System.Text.RegularExpressions;

namespace Xunit.v3.IntegrationTesting;

internal static class IXunitTestCollectionExtensions
{
    private static readonly Regex s_collectionNameRegex = new(
        @"^Test collection for (?<className>.+?) \(id: (?<id>.+?)\)$",
        RegexOptions.Compiled);

    public static Type? TryGetCollectionDefinition(this IXunitTestCollection testCollection)
    {
        if (testCollection.CollectionDefinition is not null)
        {
            return testCollection.CollectionDefinition;
        }

        var match = s_collectionNameRegex.Match(testCollection.TestCollectionDisplayName);
        if (match.Success)
        {
            var className = match.Groups["className"].Value;
            return Type.GetType(className);
        }

        return null;
    }
}