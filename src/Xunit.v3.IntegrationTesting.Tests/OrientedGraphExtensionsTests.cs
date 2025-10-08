using System.Reflection;
using Moq;
using Xunit.Sdk;
using Xunit.v3.IntegrationTesting.Extensions;

namespace Xunit.v3.IntegrationTesting.Tests;

public class OrientedGraphExtensionsTests
{
    [Fact]
    public void Validate_BuildGraph()
    {
        var testClass = CreateTestClass("TestClassName");
        var testCaseA = CreateTestCase(testClass, "A", dependencies: ["B", "C"]);
        var testCaseB = CreateTestCase(testClass, "B", dependencies: ["D"]);
        var testCaseC = CreateTestCase(testClass, "C", dependencies: ["D"]);
        var testCaseD = CreateTestCase(testClass, "D", dependencies: ["E"]);
        var testCaseE = CreateTestCase(testClass, "E");

        var lst = new List<IXunitTestCase>() { testCaseA, testCaseB, testCaseC, testCaseD, testCaseE };
        var graph = lst.ToOrientedGraph<IXunitTestCase>(out var issues);
        Assert.Empty(issues);

        Assert.Equal(new[] { testCaseB, testCaseC }, graph.GetNeighbors(testCaseA).OrderBy(n => n.TestMethodName));
        Assert.Equal(new[] { testCaseD }, graph.GetNeighbors(testCaseB).OrderBy(n => n.TestMethodName));
        Assert.Equal(new[] { testCaseD }, graph.GetNeighbors(testCaseC).OrderBy(n => n.TestMethodName));
        Assert.Equal(new[] { testCaseE }, graph.GetNeighbors(testCaseD).OrderBy(n => n.TestMethodName));
        Assert.Empty(graph.GetNeighbors(testCaseE));
    }

    [Fact]
    public void Validate_ThrowsIfMoreThanOneMethodWithSameName()
    {
        var testClass = CreateTestClass("TestClassName");
        var testCaseA = CreateTestCase(testClass, "A");
        var testCaseA_duplicate = CreateTestCase(testClass, "A");
        var testCaseB = CreateTestCase(testClass, "B", dependencies: ["A"]);

        var lst = new List<IXunitTestCase>() { testCaseA, testCaseA_duplicate, testCaseB };
        var ex = Assert.Throws<Exception>(() => lst.ToOrientedGraph<IXunitTestCase>(out var issues));
        Assert.Equal("Multiple tests found with the same name 'A' in class 'TestClassName'. Total test cases: '2'. This is not allowed.", ex.Message);
    }

    [Fact]
    public void Validate_IssuePresentIfDependencyNotFound()
    {
        var testClass = CreateTestClass("TestClassName");
        var testCaseA = CreateTestCase(testClass, "A");
        var testCaseB = CreateTestCase(testClass, "B", dependencies: ["C"]);

        var lst = new List<IXunitTestCase>() { testCaseA, testCaseB };
        var graph = lst.ToOrientedGraph<IXunitTestCase>(out var issues);

        var issue = Assert.Single(issues);
        Assert.Equal("Dependency 'C' for test 'TestClassName.B' not found.", issue);
        Assert.Empty(graph.GetNeighbors(testCaseA));
        Assert.Empty(graph.GetNeighbors(testCaseB));
        Assert.Equal(2, graph.GetAllNodes().Count());
    }

    [Fact]
    public void Validate_NoDependencyForMethodsFromDifferentClasses()
    {
        var testClassA = CreateTestClass("TestClassA");
        var testClassB = CreateTestClass("TestClassB");
        var testCaseA_A = CreateTestCase(testClassA, "A");
        var testCaseB_B = CreateTestCase(testClassB, "B", dependencies: ["A"]);

        var lst = new List<IXunitTestCase>() { testCaseA_A, testCaseB_B };
        var graph = lst.ToOrientedGraph<IXunitTestCase>(out var issues);

        var issue = Assert.Single(issues);
        Assert.Equal("Dependency 'A' for test 'TestClassB.B' not found.", issue);

        Assert.Empty(graph.GetNeighbors(testCaseA_A));
        Assert.Empty(graph.GetNeighbors(testCaseB_B));
        Assert.Equal(2, graph.GetAllNodes().Count());
    }

    private IXunitTestClass CreateTestClass(string className)
    {
        var testClassMock = new Mock<IXunitTestClass>(MockBehavior.Strict);
        testClassMock.SetupGet(tc => tc.TestClassName).Returns(className);
        testClassMock.As<ITestClass>().SetupGet(tc => tc.TestClassName).Returns(className);
        testClassMock.SetupGet(tc => tc.UniqueID).Returns(Guid.NewGuid().ToString());

        return testClassMock.Object;
    }

    private MethodInfo CreateMethodInfo(params string[] dependencies)
    {
        var methodInfoMock = new Mock<MethodInfo>(MockBehavior.Strict);

        methodInfoMock.SetupGet(m => m.MemberType).Returns(MemberTypes.Method);
        methodInfoMock.Setup(m => m.GetCustomAttributes(typeof(FactDependsOnAttribute), false)).Returns(new[]
        {
            new FactDependsOnAttribute()
            {
                Dependencies = dependencies
            }
        });

        return methodInfoMock.Object;
    }

    private IXunitTestMethod CreateTestMethod(IXunitTestClass testClass, string methodName, params string[] dependencies)
    {
        var testMethodMock = new Mock<IXunitTestMethod>(MockBehavior.Strict);
        var methodInfo = CreateMethodInfo(dependencies);

        testMethodMock.SetupGet(tm => tm.TestClass).Returns(testClass);
        testMethodMock.As<ITestMethod>().SetupGet(tm => tm.TestClass).Returns(testClass);
        testMethodMock.SetupGet(tm => tm.MethodName).Returns(methodName);
        testMethodMock.As<ITestMethod>().SetupGet(tm => tm.MethodName).Returns(methodName);
        testMethodMock.SetupGet(tm => tm.Method).Returns(methodInfo);
        testMethodMock.SetupGet(tm => tm.UniqueID).Returns(Guid.NewGuid().ToString());

        return testMethodMock.Object;
    }

    private IXunitTestCase CreateTestCase(IXunitTestClass testClass, string testMethodName, params string[] dependencies)
    {
        var testMethod = CreateTestMethod(testClass, testMethodName, dependencies);
        return CreateTestCase(testClass, testMethod);
    }

    private IXunitTestCase CreateTestCase(IXunitTestClass testClass, IXunitTestMethod testMethod)
    {
        var testCaseMock = new Mock<IXunitTestCase>(MockBehavior.Strict);

        testCaseMock.SetupGet(tc => tc.TestMethod).Returns(testMethod);
        testCaseMock.As<ITestCase>().SetupGet(tc => tc.TestMethod).Returns(testMethod);
        testCaseMock.SetupGet(tc => tc.TestClass).Returns(testClass);
        testCaseMock.As<ITestCase>().SetupGet(tc => tc.TestClass).Returns(testClass);
        testCaseMock.SetupGet(tc => tc.TestMethodName).Returns(testMethod.MethodName);
        testCaseMock.As<ITestCase>().SetupGet(tc => tc.TestMethodName).Returns(testMethod.MethodName);
        testCaseMock.SetupGet(tc => tc.TestClassName).Returns(testClass.TestClassName);
        testCaseMock.As<ITestCase>().SetupGet(tc => tc.TestClassName).Returns(testClass.TestClassName);

        return testCaseMock.Object;
    }
}

