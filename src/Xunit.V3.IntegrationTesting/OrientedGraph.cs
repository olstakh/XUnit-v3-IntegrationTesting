namespace Xunit.v3.IntegrationTesting;

public class OrientedGraph<TNode>
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
        var stack = new List<TNode>();

        foreach (var node in GetAllNodes())
        {
            if (!visited.Contains(node))
            {
                Visit(node, visited, stack);
            }
        }

        visited.Reverse();

        return visited;
    }

    public IEnumerable<TNode> GetSubTree(TNode node)
    {
        var visited = new HashSet<TNode>(_comparer);
        var stack = new List<TNode>();

        Visit(node, visited, stack);

        return visited;
    }

    public IReadOnlyList<TNode> FindCycle()
    {
        var visited = new HashSet<TNode>(_comparer);
        var stack = new List<TNode>();

        foreach (var node in GetAllNodes())
        {
            if (!visited.Contains(node))
            {
                var cycle = Visit(node, visited, stack);
                if (cycle.Count > 0)
                {
                    return cycle;
                }
            }
        }

        return Array.Empty<TNode>();
    }

    private IReadOnlyList<TNode> Visit(TNode node, HashSet<TNode> visited, List<TNode> stack)
    {
        // TODO: make more efficient instead of traversing the stack
        var idx = stack.FindIndex(n => _comparer.Equals(n, node));
        if (idx != -1)
        {
            return stack[idx ..].Append(node).ToList();
        }

        if (visited.Contains(node))
        {
            return Array.Empty<TNode>();
        }

        stack.Add(node);

        List<TNode> anyCycle = new();

        foreach (var neighbor in GetNeighbors(node))
        {
            var cycle = Visit(neighbor, visited, stack);
            if (cycle.Count > 0 && anyCycle.Count == 0)
            {
                anyCycle = cycle.ToList();
            }
        }

        visited.Add(node);
        stack.RemoveAt(stack.Count - 1);

        return anyCycle;
    }
}