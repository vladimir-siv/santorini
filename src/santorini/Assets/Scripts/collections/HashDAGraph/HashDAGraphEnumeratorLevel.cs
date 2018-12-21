using System;
using System.Collections;
using System.Collections.Generic;

namespace etf.santorini.sv150155d.collections
{
	public class HashDAGraphEnumeratorLevel<TKey, TValue, TWeight> : HashCollectionEnumerator<TKey, TValue, TWeight> where TKey : IEquatable<TKey>
	{
		public override HashCollection<TKey, TValue, TWeight>.Node Node => (((HashCollection<TKey, TValue, TWeight>.Node node, int level))Pointer.Current).node;
		public int Level => (((HashCollection<TKey, TValue, TWeight>.Node node, int level))Pointer.Current).level;

		internal HashDAGraphEnumeratorLevel(HashDAGraph<TKey, TValue, TWeight> container, TKey startingPoint) : base(container, startingPoint) { }

		Queue<(HashCollection<TKey, TValue, TWeight>.Node node, int level)> queue = new Queue<(HashCollection<TKey, TValue, TWeight>.Node node, int level)>();
		HashSet<HashCollection<TKey, TValue, TWeight>.Node> visited = new HashSet<HashCollection<TKey, TValue, TWeight>.Node>();
		protected override IEnumerable Enumerate()
		{
			queue.Clear();
			visited.Clear();

			queue.Enqueue((Container[StartingPoint], 0));
			while (queue.Count > 0)
			{
				var entry = queue.Dequeue();

				if (visited.Contains(entry.node)) continue;

				yield return entry;

				visited.Add(entry.node);

				foreach (var child in entry.node.EnumerateChildren())
				{
					queue.Enqueue((child, entry.level + 1));
				}
			}
		}


	}
}
