using System;
using System.Collections;
using System.Collections.Generic;

namespace etf.santorini.sv150155d.collections
{
	public class HashTreeEnumeratorLevel<TKey, TValue, TWeight> : HashCollectionEnumerator<TKey, TValue, TWeight> where TKey : IEquatable<TKey>
	{
		public override HashCollection<TKey, TValue, TWeight>.Node Node => (HashCollection<TKey, TValue, TWeight>.Node)Pointer.Current;

		internal HashTreeEnumeratorLevel(HashTree<TKey, TValue, TWeight> container, TKey startingPoint) : base(container, startingPoint) { }

		Queue<HashCollection<TKey, TValue, TWeight>.Node> queue = new Queue<HashCollection<TKey, TValue, TWeight>.Node>();
		protected override IEnumerable Enumerate()
		{
			queue.Clear();

			queue.Enqueue(Container[StartingPoint]);
			while (queue.Count > 0)
			{
				var node = queue.Dequeue();

				yield return node;

				foreach (var child in node.EnumerateChildren())
				{
					queue.Enqueue(child);
				}
			}
		}
	}
}
