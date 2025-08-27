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

        var sorted = graph.TopologicalSort().Reverse().ToList();

        AssertInOrder(sorted, "Z", "Y");
        AssertInOrder(sorted, "Y", "D");
        AssertInOrder(sorted, "D", "E");
        AssertInOrder(sorted, "X", "D");
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

