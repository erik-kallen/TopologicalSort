﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TopologicalSort {
	public static class TopologicalSorter {
		// Tarjan's algorithm: http://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
		private class Algorithm<T> {
			private readonly ILookup<T, T> _edges;
			private readonly IEqualityComparer<T> _comparer;

			private Dictionary<T, int> _indices;
			private Dictionary<T, int> _lowlink;
			private List<IList<T>> _result;
			private Stack<T> _s;
			private int _index;

			public Algorithm(ILookup<T, T> edges, IEqualityComparer<T> comparer) {
				_edges = edges;
				_comparer = comparer ?? EqualityComparer<T>.Default;
			}

			public List<IList<T>> MainLoop(IEnumerable<T> vertices) {
				_result = new List<IList<T>>();
				_indices = new Dictionary<T, int>(_comparer);
				_lowlink = new Dictionary<T, int>(_comparer);
				_s = new Stack<T>();
				_index = 0;
				foreach (var v in vertices) {
					if (!_indices.ContainsKey(v)) {
						StrongConnect(v);
					}
				}
				return _result;
			}

			private void StrongConnect(T v) {
				_indices[v] = _index;
				_lowlink[v] = _index;
				_index++;
				_s.Push(v);

				foreach (var w in _edges[v]) {
					if (!_indices.ContainsKey(w)) {
						StrongConnect(w);
						_lowlink[v] = Math.Min(_lowlink[v], _lowlink[w]);
					}
					else if (_s.Contains(w)) {
						_lowlink[v] = Math.Min(_lowlink[v], _indices[w]);
					}
				}

				if (_lowlink[v] == _indices[v]) {
					var scc = new List<T>();
					T w;
					do {
						w = _s.Pop();
						scc.Add(w);
					} while (!_comparer.Equals(v, w));
					_result.Add(scc);
				}
			}
		}

		/// <summary>
		/// Identifies all strongly connected components (http://en.wikipedia.org/wiki/Strongly_connected_component) in a graph, and returns them in an order such that all SCCs are returned after all SCCs on which they are dependent.
		/// This can be seen as topological sorting that supports cycles in the graph, and all items in a cycle are returned as one entity in the result.
		/// </summary>
		/// <param name="vertices">Vertices in the graph</param>
		/// <param name="edges">Edges in the graph. If there is an edge such that (From, To) == (a, b), then b will appear before a in the result.</param>
		/// <param name="comparer">Comparer used to compare the vertices, or null for the default comparer.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="vertices"/> or <paramref name="edges"/> is null.</exception>
		public static IList<IList<T>> FindAndTopologicallySortStronglyConnectedComponents<T>(IEnumerable<T> vertices, IEnumerable<IEdge<T>> edges, IEqualityComparer<T> comparer = null) {
			if (vertices == null) throw new ArgumentNullException("vertices");
			if (edges == null) throw new ArgumentNullException("edges");

			var result = new Algorithm<T>(edges.ToLookup(e => e.From, e => e.To), comparer).MainLoop(vertices);
			return result;
		}

		/// <summary>
		/// Identifies all strongly connected components (http://en.wikipedia.org/wiki/Strongly_connected_component) in a graph, and returns them in an order such that all SCCs are returned after all SCCs on which they are dependent.
		/// This can be seen as topological sorting that supports cycles in the graph, and all items in a cycle are returned as one entity in the result.
		/// </summary>
		/// <param name="source">Objects to sort</param>
		/// <param name="getVertex">Function used to get from one of the objects being sorted to the corresponding vertex. All objects must have unique vertices.</param>
		/// <param name="edges">Edges in the graph. If there is an edge such that (From, To) == (a, b), then b will appear before a in the result.</param>
		/// <param name="comparer">Comparer used to compare the vertices, or null for the default comparer.</param>
		/// <exception cref="ArgumentNullException">If any of <paramref name="source"/>, <paramref name="getVertex"/> or <paramref name="edges"/> is null.</exception>
		public static IList<IList<TSource>> FindAndTopologicallySortStronglyConnectedComponents<TSource, TVertex>(IEnumerable<TSource> source, Func<TSource, TVertex> getVertex, IEnumerable<IEdge<TVertex>> edges, IEqualityComparer<TVertex> comparer = null) {
			if (source == null) throw new ArgumentNullException("source");
			if (getVertex == null) throw new ArgumentNullException("source");
			if (edges == null) throw new ArgumentNullException("edges");

			var backref = source.ToDictionary(getVertex, comparer ?? EqualityComparer<TVertex>.Default);
			return FindAndTopologicallySortStronglyConnectedComponents(backref.Keys, edges, comparer).Select(l => (IList<TSource>)l.Select(t => backref[t]).ToList()).ToList();
		}

		/// <summary>
		/// Topologically sorts the specified sequence. Throws if there are any cycles in the graph.
		/// </summary>
		/// <param name="vertices">Vertices in the graph</param>
		/// <param name="edges">Edges in the graph. If there is an edge such that (From, To) == (a, b), then b will appear before a in the result.</param>
		/// <param name="comparer">Comparer used to compare the vertices, or null for the default comparer.</param>
		/// <exception cref="ArgumentException">If there are any cycles in the graph.</exception>
		/// <exception cref="ArgumentNullException">If <paramref name="vertices"/> or <paramref name="edges"/> is null.</exception>
		public static IEnumerable<T> TopologicalSort<T>(IEnumerable<T> vertices, IEnumerable<IEdge<T>> edges, IEqualityComparer<T> comparer = null) {
			var result = FindAndTopologicallySortStronglyConnectedComponents(vertices, edges, comparer);
			if (result.Any(x => x.Count > 1))
				throw new ArgumentException("Cycles in graph", "edges");
			return result.Select(x => x[0]);
		}

		/// <summary>
		/// Topologically sorts the specified sequence. Throws if there are any cycles in the graph.
		/// </summary>
		/// <param name="source">Objects to sort</param>
		/// <param name="getVertex">Function used to get from one of the objects being sorted to the corresponding vertex. All objects must have unique vertices.</param>
		/// <param name="edges">Edges in the graph. If there is an edge such that (From, To) == (a, b), then b will appear before a in the result.</param>
		/// <param name="comparer">Comparer used to compare the vertices, or null for the default comparer.</param>
		/// <exception cref="ArgumentException">If there are any cycles in the graph.</exception>
		/// <exception cref="ArgumentNullException">If any of <paramref name="source"/>, <paramref name="getVertex"/> or <paramref name="edges"/> is null.</exception>
		public static IEnumerable<TSource> TopologicalSort<TSource, TVertex>(IEnumerable<TSource> source, Func<TSource, TVertex> getVertex, IEnumerable<IEdge<TVertex>> edges, IEqualityComparer<TVertex> comparer = null) {
			var backref = source.ToDictionary(getVertex, comparer ?? EqualityComparer<TVertex>.Default);
			return TopologicalSort(backref.Keys, edges, comparer).Select(t => backref[t]);
		}
	}
}
