using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.V3.IntegrationTesting;

public class DependencyResolver
{
    public static List<ITestCase> OrderTestsByDependencies(IEnumerable<IXunitTestCase> testCases)
    {
        var testCaseDict = testCases.ToDictionary(tc => GetTestName(tc), tc => tc);
        var dependencies = new Dictionary<string, IReadOnlySet<string>>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>();
        var ordered = new List<ITestCase>();

        // Build dependency graph
        foreach (var testCase in testCases)
        {
            var testName = GetTestName(testCase);
            var dependsOnAttrs = testCase.TestMethod.Method.GetCustomAttributes(typeof(DependsOnAttribute), false);

            var currentDependencies = new HashSet<string>();

            foreach (var attr in dependsOnAttrs)
            {
                if (attr is DependsOnAttribute dependsOn)
                {
                    foreach (var dependency in dependsOn.Dependencies)
                    {
                        currentDependencies.Add(dependency);
                    }
                }
            }

            dependencies[testName] = currentDependencies;
        }

        // Perform topological sort with cycle detection
        foreach (var testName in testCaseDict.Keys)
        {
            if (!visited.Contains(testName))
            {
                Visit(testName, testCaseDict, dependencies, visited, visiting, ordered);
            }
        }

        return ordered;
    }

    private static void Visit(
        string testName,
        IReadOnlyDictionary<string, IXunitTestCase> testCaseDict,
        IReadOnlyDictionary<string, IReadOnlySet<string>> dependencies,
        HashSet<string> visited,
        HashSet<string> visiting,
        List<ITestCase> ordered)
    {
        if (visiting.Contains(testName))
        {
            throw new InvalidOperationException($"Circular dependency detected involving test: {testName}");
        }

        if (visited.Contains(testName))
        {
            return;
        }

        visiting.Add(testName);

        if (dependencies.ContainsKey(testName))
        {
            foreach (var dependency in dependencies[testName])
            {
                if (testCaseDict.ContainsKey(dependency))
                {
                    Visit(dependency, testCaseDict, dependencies, visited, visiting, ordered);
                }
                else
                {
                    throw new InvalidOperationException($"Dependency '{dependency}' for test '{testName}' not found.");
                }
            }
        }

        visiting.Remove(testName);
        visited.Add(testName);

        if (testCaseDict.ContainsKey(testName))
        {
            ordered.Add(testCaseDict[testName]);
        }
    }

    private static string GetTestName(ITestCase testCase)
    {
        if (testCase.TestMethod == null)
        {
            // Should never happen
            throw new InvalidOperationException("Test method is not initialized in the current context.");
        }
        return $"{testCase.TestMethod.TestClass.TestClassName}.{testCase.TestMethodName}";
    }
}