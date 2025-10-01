using Xunit;
using Xunit.v3.IntegrationTesting;

[assembly: TestCollectionOrderer(typeof(DependencyAwareTestCollectionOrderer))]

namespace Xunit.v3.IntegrationTesting.Manual;

[DependsOnClasses(typeof(ClassB), typeof(ClassC))]
public class ClassA
{
    public static int counter = 0;

    [Fact]
    public void TestMethod()
    {
        Interlocked.Increment(ref counter);
        Assert.Equal(1, ClassB.counter);
        Assert.Equal(1, ClassC.counter);
    }
}

public class ClassB
{
    public static int counter = 0;

    [Fact]
    public void TestMethod()
    {
        Interlocked.Increment(ref counter);
        Assert.Equal(0, ClassC.counter);
        Assert.Equal(0, ClassA.counter);
    }
}

[CollectionDefinition]
[DependsOnClasses(typeof(ClassB))]
public class ClassC
{
    public static int counter = 0;

    [Fact]
    public void TestMethod()
    {
        Interlocked.Increment(ref counter);
        Assert.Equal(0, ClassA.counter);
        Assert.Equal(1, ClassB.counter);
    }
}