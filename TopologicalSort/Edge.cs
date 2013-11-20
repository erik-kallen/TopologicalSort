using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TopologicalSort {
	public struct Edge<TVertex> {
		public TVertex From { get; private set; }
		public TVertex To { get; private set; }

		public Edge(TVertex from, TVertex to) : this() {
			From = from;
			To   = to;
		}
	}

	public static class Edge {
		public static Edge<T> Create<T>(T from, T to) {
			return new Edge<T>(from, to);
		}
	}
}
