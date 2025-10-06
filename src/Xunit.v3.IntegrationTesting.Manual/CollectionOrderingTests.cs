using Xunit;
using Xunit.v3.IntegrationTesting;

namespace Xunit.v3.IntegrationTesting.Manual.CollectionOrderingTests;

[Collection<DefinitionA>]
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

[Collection<DefinitionB>]
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

[Collection<DefinitionC>]
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

[DependsOnCollections(typeof(DefinitionB), typeof(DefinitionC))]
[CollectionDefinition(DisableParallelization = true)]
public sealed class DefinitionA;

[CollectionDefinition(DisableParallelization = true)]
public sealed class DefinitionB;

[CollectionDefinition(DisableParallelization = true)]
[DependsOnCollections(typeof(DefinitionB))]
public sealed class DefinitionC;