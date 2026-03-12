using Xunit.v3.IntegrationTesting.Exceptions;

namespace Xunit.v3.IntegrationTesting.Tests;

public class OrientedGraphTests
{
    [Fact]
    public void Validate_BuildGraph()
    {
        var graph = new OrientedGraph<string>(StringComparer.OrdinalIgnoreCase);
        graph.AddEdge("A", "B");
        graph.AddEdge("A", "C");
        graph.AddEdge("B", "D");
        graph.AddEdge("C", "D");
        graph.AddEdge("D", "E");

        Assert.True(graph.ContainsNode("A"));
        Assert.True(graph.ContainsNode("B"));
        Assert.True(graph.ContainsNode("C"));
        Assert.True(graph.ContainsNode("D"));
        Assert.True(graph.ContainsNode("E"));
        Assert.False(graph.ContainsNode("F"));

        Assert.Equal(new[] { "B", "C" }, graph.GetNeighbors("A").OrderBy(n => n));
        Assert.Equal(new[] { "D" }, graph.GetNeighbors("B"));
        Assert.Equal(new[] { "D" }, graph.GetNeighbors("C"));
        Assert.Equal(new[] { "E" }, graph.GetNeighbors("D"));
        Assert.Empty(graph.GetNeighbors("E"));
    }

    [Fact]
    public void Validate_GetSubtree()
    {
        var graph = new OrientedGraph<string>(StringComparer.OrdinalIgnoreCase);
        graph.AddEdge("A", "B");
        graph.AddEdge("A", "C");
        graph.AddEdge("B", "D");
        graph.AddEdge("C", "D");
        graph.AddEdge("D", "E");

        var subtree = graph.GetSubTree("A");

        Assert.Contains("A", subtree);
        Assert.Contains("B", subtree);
        Assert.Contains("C", subtree);
        Assert.Contains("D", subtree);
        Assert.Contains("E", subtree);
        Assert.DoesNotContain("F", subtree);
    }

    [Fact]
    public void Validate_TopologicalSort()
    {
        var graph = new OrientedGraph<string>(StringComparer.OrdinalIgnoreCase);
        graph.AddEdge("D", "E");
        graph.AddEdge("Y", "D");
        graph.AddEdge("X", "D");
        graph.AddEdge("Z", "Y");
        graph.AddEdge("Z", "X");

        var sorted = graph.TopologicalSort().ToList();

        // Dependencies should come before dependents (execution order)
        AssertInOrder(sorted, "E", "D");
        AssertInOrder(sorted, "D", "Y");
        AssertInOrder(sorted, "D", "X");
        AssertInOrder(sorted, "Y", "Z");
        AssertInOrder(sorted, "X", "Z");
    }

    [Fact]
    public void Validate_TopologicalSort_AllNodesPresent()
    {
        var graph = new OrientedGraph<string>(StringComparer.OrdinalIgnoreCase);
        graph.AddEdge("D", "E");
        graph.AddEdge("Y", "D");
        graph.AddEdge("X", "D");
        graph.AddEdge("Z", "Y");
        graph.AddEdge("Z", "X");

        var sorted = graph.TopologicalSort().ToList();

        Assert.Equal(5, sorted.Count);
        Assert.Contains("E", sorted);
        Assert.Contains("D", sorted);
        Assert.Contains("Y", sorted);
        Assert.Contains("X", sorted);
        Assert.Contains("Z", sorted);
    }

    [Fact]
    public void Validate_TopologicalSort_DeepChain()
    {
        // A -> B -> C -> D -> E (A depends on B, B depends on C, etc.)
        var graph = new OrientedGraph<string>(StringComparer.OrdinalIgnoreCase);
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");
        graph.AddEdge("C", "D");
        graph.AddEdge("D", "E");

        var sorted = graph.TopologicalSort().ToList();

        // Execution order: E first (no deps), then D, C, B, A
        AssertInOrder(sorted, "E", "D");
        AssertInOrder(sorted, "D", "C");
        AssertInOrder(sorted, "C", "B");
        AssertInOrder(sorted, "B", "A");
    }

    [Fact]
    public void Validate_TopologicalSort_IndependentNodes()
    {
        var graph = new OrientedGraph<string>(StringComparer.OrdinalIgnoreCase);
        graph.AddNode("A");
        graph.AddNode("B");
        graph.AddNode("C");

        var sorted = graph.TopologicalSort().ToList();

        Assert.Equal(3, sorted.Count);
        Assert.Contains("A", sorted);
        Assert.Contains("B", sorted);
        Assert.Contains("C", sorted);
    }

    [Fact]
    public void Validate_CircularDependency_ExceptionThrown()
    {
        var graph = new OrientedGraph<string>(StringComparer.OrdinalIgnoreCase);
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");
        graph.AddEdge("C", "A");

        var ex = Assert.Throws<CircularDependencyException<string>>(() => graph.TopologicalSort());

        Assert.Equal(new[] { "A", "B", "C", "A" }, ex.DependencyCycle);
    }

    private void AssertInOrder(List<string> sorted, string first, string second)
    {
        var firstIndex = sorted.IndexOf(first);
        var secondIndex = sorted.IndexOf(second);
        Assert.True(firstIndex >= 0, $"Item '{first}' not found in sorted list");
        Assert.True(secondIndex >= 0, $"Item '{second}' not found in sorted list");
        Assert.True(firstIndex < secondIndex, $"Item '{first}' should appear before '{second}'");
    }
}

