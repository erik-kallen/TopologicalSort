using System;

namespace TopologicalSort
{
    public interface IEdge<TVertex>
    {
        TVertex From { get; }
        TVertex To { get; }
    }
}
