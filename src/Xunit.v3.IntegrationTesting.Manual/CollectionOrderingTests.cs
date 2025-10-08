using Xunit;
using Xunit.v3.IntegrationTesting;

namespace Xunit.v3.IntegrationTesting.Manual.CollectionOrderingTests;

[DependsOnClasses(Dependencies = [typeof(ClassB), typeof(ClassC)], Name = "DefinitionA")]
public class ClassA
{
    public static int counter = 0;

    [FactDependsOn]
    public void TestMethod()
    {
        Interlocked.Increment(ref counter);
        Assert.Equal(1 + 1, ClassB.counter);
        Assert.Equal(1, ClassC.counter);
    }
}

[DependsOnClasses(Name = "DefinitionB")]
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

public class ClassD : ClassB { }

[DependsOnClasses(Dependencies = [typeof(ClassD)], Name = "DefinitionC")]
public class ClassC
{
    public static int counter = 0;

    [FactDependsOn]
    public void TestMethod()
    {
        Interlocked.Increment(ref counter);
        Assert.Equal(0, ClassA.counter);
        Assert.Equal(1 + 1, ClassB.counter);
    }
}