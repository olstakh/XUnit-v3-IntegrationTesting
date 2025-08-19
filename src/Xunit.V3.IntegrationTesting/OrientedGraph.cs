namespace Xunit.V3.IntegrationTesting;

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
        var stack = new Stack<TNode>();

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
        var stack = new Stack<TNode>();

        Visit(node, visited, stack);

        return visited;
    }

    public void ValidateNoCycles()
    {
        var visited = new HashSet<TNode>(_comparer);
        var stack = new Stack<TNode>();

        foreach (var node in GetAllNodes())
        {
            if (!visited.Contains(node))
            {
                Visit(node, visited, stack);
            }
        }
    }

    private void Visit(TNode node, HashSet<TNode> visited, Stack<TNode> stack)
    {
        // TODO: make more efficient instead of traversing the stack
        if (stack.Contains(node))
        {
            throw new InvalidOperationException($"Cycle detected involving node: {node}");
        }

        if (visited.Contains(node))
        {
            return;
        }

        stack.Push(node);

        foreach (var neighbor in GetNeighbors(node))
        {
            Visit(neighbor, visited, stack);
        }

        visited.Add(node);
        stack.Pop();
    }
}