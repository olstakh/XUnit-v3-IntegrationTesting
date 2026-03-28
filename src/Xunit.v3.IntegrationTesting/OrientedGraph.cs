using Xunit.v3.IntegrationTesting.Exceptions;

namespace Xunit.v3.IntegrationTesting;

internal class OrientedGraph<TNode>
    where TNode : notnull
{
    private readonly IEqualityComparer<TNode> _comparer;
    private readonly Dictionary<TNode, HashSet<TNode>> _adjacencyList;

    public OrientedGraph(IEqualityComparer<TNode> comparer)
    {
        _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        _adjacencyList = new Dictionary<TNode, HashSet<TNode>>(_comparer);
    }

    public void AddNode(TNode node)
    {
        if (!_adjacencyList.ContainsKey(node))
        {
            _adjacencyList[node] = new HashSet<TNode>(_comparer);
        }
    }

    public void AddEdge(TNode from, TNode to)
    {
        AddNode(from);
        AddNode(to);
        _adjacencyList[from].Add(to);
    }

    public IEnumerable<TNode> GetNeighbors(TNode node)
    {
        if (_adjacencyList.TryGetValue(node, out var neighbors))
        {
            return neighbors;
        }
        return Enumerable.Empty<TNode>();
    }

    public IEnumerable<TNode> GetAllNodes()
    {
        return _adjacencyList.Keys;
    }

    public bool ContainsNode(TNode node)
    {
        return _adjacencyList.ContainsKey(node);
    }

    public IEnumerable<TNode> TopologicalSort()
    {
        var visited = new HashSet<TNode>(_comparer);
        var sorted = new List<TNode>();
        var stack = new List<TNode>();

        foreach (var node in GetAllNodes())
        {
            if (!visited.Contains(node))
            {
                var cycle = Visit(node, visited, sorted, stack);
                if (cycle.Count > 0)
                {
                    throw new CircularDependencyException<TNode>("Graph contains a cycle; topological sort is not possible.")
                    {
                        DependencyCycle = cycle
                    };
                }
            }
        }

        return sorted;
    }

    /// <summary>
    /// Performs a topological sort using Kahn's algorithm with a tiebreaker comparison
    /// for nodes at the same dependency level. This allows secondary ordering
    /// (e.g., by priority) among nodes that have no dependency relationship.
    /// </summary>
    public IEnumerable<TNode> TopologicalSort(Comparison<TNode> tieBreaker)
    {
        // Build reverse adjacency (who depends on me?) and compute in-degrees
        var inDegree = new Dictionary<TNode, int>(_comparer);
        var dependents = new Dictionary<TNode, List<TNode>>(_comparer);

        foreach (var node in GetAllNodes())
        {
            if (!inDegree.ContainsKey(node))
                inDegree[node] = 0;
            if (!dependents.ContainsKey(node))
                dependents[node] = new List<TNode>();
        }

        foreach (var node in GetAllNodes())
        {
            foreach (var dep in GetNeighbors(node))
            {
                inDegree[node]++;
                dependents[dep].Add(node);
            }
        }

        // Ready queue: nodes with no dependencies, sorted by tiebreaker
        var ready = new List<TNode>();
        foreach (var kvp in inDegree)
        {
            if (kvp.Value == 0)
                ready.Add(kvp.Key);
        }
        ready.Sort(tieBreaker);

        var sorted = new List<TNode>();
        while (ready.Count > 0)
        {
            var current = ready[0];
            ready.RemoveAt(0);
            sorted.Add(current);

            foreach (var dependent in dependents[current])
            {
                inDegree[dependent]--;
                if (inDegree[dependent] == 0)
                {
                    ready.Add(dependent);
                    ready.Sort(tieBreaker);
                }
            }
        }

        if (sorted.Count != inDegree.Count)
        {
            // Cycle detected - delegate to DFS-based sort for proper cycle reporting
            TopologicalSort();
        }

        return sorted;
    }

    public IEnumerable<TNode> GetSubTree(TNode node)
    {
        var visited = new HashSet<TNode>(_comparer);
        var sorted = new List<TNode>();
        var stack = new List<TNode>();

        _ = Visit(node, visited, sorted, stack);

        return visited;
    }

    // returns first found cycle if any
    private IReadOnlyList<TNode> Visit(TNode node, HashSet<TNode> visited, List<TNode> sorted, List<TNode> stack)
    {
        // TODO: make more efficient instead of traversing the stack
        var idx = stack.FindIndex(n => _comparer.Equals(n, node));
        if (idx != -1)
        {
            return stack.Skip(idx).Append(node).ToList();
        }

        if (visited.Contains(node))
        {
            return Array.Empty<TNode>();
        }

        stack.Add(node);

        List<TNode> anyCycle = new();

        foreach (var neighbor in GetNeighbors(node))
        {
            var cycle = Visit(neighbor, visited, sorted, stack);
            if (cycle.Count > 0 && anyCycle.Count == 0)
            {
                anyCycle = cycle.ToList();
            }
        }

        visited.Add(node);
        sorted.Add(node);
        stack.RemoveAt(stack.Count - 1);

        return anyCycle;
    }
}