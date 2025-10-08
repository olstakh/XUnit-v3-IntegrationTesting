using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.v3.IntegrationTesting.Comparers;

internal class TestCaseComparer<TTestCase> : IEqualityComparer<TTestCase>
    where TTestCase : notnull, ITestCase
{
    public static readonly TestCaseComparer<TTestCase> Instance = new();
    public bool Equals(TTestCase? x, TTestCase? y)
    {
        if (x is null && y is null)
        {
            return true;
        }
        if (x is null || y is null)
        {
            return false;
        }

        return
            TestClassComparer.Instance.Equals(x.TestMethod?.TestClass, y.TestMethod?.TestClass) &&
            TestMethodComparer.Instance.Equals(x.TestMethod, y.TestMethod);
    }

    public int GetHashCode(TTestCase obj)
    {
        var hash = new HashCode();
        hash.Add(obj.TestMethod?.TestClass, TestClassComparer.Instance);
        hash.Add(obj.TestMethod, TestMethodComparer.Instance);

        return hash.ToHashCode();
    }
}