using Xunit.Sdk;

namespace Xunit.v3.IntegrationTesting.Comparers;

internal class TestCaseComparer<TTestCase> : IEqualityComparer<TTestCase>
    where TTestCase : notnull, ITestCase
{
    public static readonly TestCaseComparer<TTestCase> Instance = new();
    public bool Equals(TTestCase? x, TTestCase? y) =>
        (x is null && y is null) || (x is not null && y is not null && x.UniqueID == y.UniqueID);

    public int GetHashCode(TTestCase obj) =>
        obj.UniqueID.GetHashCode();
}