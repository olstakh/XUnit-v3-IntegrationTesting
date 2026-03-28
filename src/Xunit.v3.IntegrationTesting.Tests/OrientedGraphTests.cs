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

    [Fact]
    public void Validate_TopologicalSort_WithTieBreaker_RespectsOrder()
    {
        // Graph: A depends on nothing, B depends on nothing, C depends on nothing
        // Tiebreaker: alphabetical order
        var graph = new OrientedGraph<string>(StringComparer.OrdinalIgnoreCase);
        graph.AddNode("C");
        graph.AddNode("A");
        graph.AddNode("B");

        var sorted = graph.TopologicalSort((x, y) => string.Compare(x, y, StringComparison.Ordinal)).ToList();

        Assert.Equal(new[] { "A", "B", "C" }, sorted);
    }

    [Fact]
    public void Validate_TopologicalSort_WithTieBreaker_DependenciesOverridePriority()
    {
        // A (priority 3) depends on B (priority 2) which depends on C (priority 1)
        // Even though C has highest priority, dependency chain is respected: C, B, A
        var graph = new OrientedGraph<string>(StringComparer.OrdinalIgnoreCase);
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");

        var priorities = new Dictionary<string, int> { ["A"] = 3, ["B"] = 2, ["C"] = 1 };
        var sorted = graph.TopologicalSort((x, y) => priorities[x].CompareTo(priorities[y])).ToList();

        Assert.Equal(new[] { "C", "B", "A" }, sorted);
    }

    [Fact]
    public void Validate_TopologicalSort_WithTieBreaker_SameLevelSortedByPriority()
    {
        // D depends on nothing (priority 1)
        // C depends on nothing (priority 2)
        // B depends on nothing (priority 3)
        // A depends on B, C, D (priority 4)
        var graph = new OrientedGraph<string>(StringComparer.OrdinalIgnoreCase);
        graph.AddEdge("A", "B");
        graph.AddEdge("A", "C");
        graph.AddEdge("A", "D");

        var priorities = new Dictionary<string, int> { ["A"] = 4, ["B"] = 3, ["C"] = 2, ["D"] = 1 };
        var sorted = graph.TopologicalSort((x, y) => priorities[x].CompareTo(priorities[y])).ToList();

        // D, C, B are all at level 0 (no deps), sorted by priority: D(1), C(2), B(3)
        // A is at level 1 (after all deps resolved)
        Assert.Equal(new[] { "D", "C", "B", "A" }, sorted);
    }

    [Fact]
    public void Validate_TopologicalSort_WithTieBreaker_MultiLevel()
    {
        // Level 0: E(1), F(2) - no deps
        // Level 1: C(1) depends on E, D(2) depends on F
        // Level 2: A(1) depends on C and D, B(2) depends on D
        var graph = new OrientedGraph<string>(StringComparer.OrdinalIgnoreCase);
        graph.AddEdge("A", "C");
        graph.AddEdge("A", "D");
        graph.AddEdge("B", "D");
        graph.AddEdge("C", "E");
        graph.AddEdge("D", "F");

        var priorities = new Dictionary<string, int>
        {
            ["E"] = 1, ["F"] = 2,
            ["C"] = 1, ["D"] = 2,
            ["A"] = 1, ["B"] = 2
        };
        var sorted = graph.TopologicalSort((x, y) => priorities[x].CompareTo(priorities[y])).ToList();

        // Level 0: E(1) before F(2)
        AssertInOrder(sorted, "E", "F");
        // Level 1: C(1) before D(2)
        AssertInOrder(sorted, "C", "D");
        // Dependencies respected
        AssertInOrder(sorted, "E", "C");
        AssertInOrder(sorted, "F", "D");
        AssertInOrder(sorted, "C", "A");
        AssertInOrder(sorted, "D", "A");
        AssertInOrder(sorted, "D", "B");
        // Level 2: A(1) before B(2)
        AssertInOrder(sorted, "A", "B");
    }

    [Fact]
    public void Validate_TopologicalSort_WithTieBreaker_CircularDependency_Throws()
    {
        var graph = new OrientedGraph<string>(StringComparer.OrdinalIgnoreCase);
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");
        graph.AddEdge("C", "A");

        Assert.Throws<CircularDependencyException<string>>(
            () => graph.TopologicalSort((x, y) => string.Compare(x, y, StringComparison.Ordinal)));
    }

    [Fact]
    public void Validate_TopologicalSort_WithTieBreaker_AllNodesPresent()
    {
        var graph = new OrientedGraph<string>(StringComparer.OrdinalIgnoreCase);
        graph.AddEdge("D", "E");
        graph.AddEdge("Y", "D");
        graph.AddEdge("X", "D");
        graph.AddEdge("Z", "Y");
        graph.AddEdge("Z", "X");

        var sorted = graph.TopologicalSort((x, y) => 0).ToList();

        Assert.Equal(5, sorted.Count);
        Assert.Contains("E", sorted);
        Assert.Contains("D", sorted);
        Assert.Contains("Y", sorted);
        Assert.Contains("X", sorted);
        Assert.Contains("Z", sorted);
    }

    [Fact]
    public void Validate_TopologicalSort_WithTieBreaker_RespectsDependencies()
    {
        var graph = new OrientedGraph<string>(StringComparer.OrdinalIgnoreCase);
        graph.AddEdge("D", "E");
        graph.AddEdge("Y", "D");
        graph.AddEdge("X", "D");
        graph.AddEdge("Z", "Y");
        graph.AddEdge("Z", "X");

        var sorted = graph.TopologicalSort((x, y) => 0).ToList();

        AssertInOrder(sorted, "E", "D");
        AssertInOrder(sorted, "D", "Y");
        AssertInOrder(sorted, "D", "X");
        AssertInOrder(sorted, "Y", "Z");
        AssertInOrder(sorted, "X", "Z");
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

