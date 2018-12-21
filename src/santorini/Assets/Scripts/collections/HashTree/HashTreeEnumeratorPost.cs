using System;
using System.Collections;
using System.Collections.Generic;

namespace etf.santorini.sv150155d.collections
{
	public class HashTreeEnumeratorPost<TKey, TValue, TWeight> : HashCollectionEnumerator<TKey, TValue, TWeight> where TKey : IEquatable<TKey>
	{
		public override HashCollection<TKey, TValue, TWeight>.Node Node => (HashCollection<TKey, TValue, TWeight>.Node)Pointer.Current;
		
		internal HashTreeEnumeratorPost(HashTree<TKey, TValue, TWeight> container, TKey startingPoint) : base(container, startingPoint) { }

		Stack<(HashCollection<TKey, TValue, TWeight>.Node node, int child)> stack = new Stack<(HashCollection<TKey, TValue, TWeight>.Node, int)>();
		protected override IEnumerable Enumerate()
		{
			stack.Clear();
			stack.Push((Container[StartingPoint], 0));

			while (stack.Count > 0)
			{
				var e = stack.Pop();

				if (e.child < e.node.ChildrenCount)
				{
					// Expensive when HashSet indexing is used
					stack.Push((e.node[e.child], 0));
					stack.Push((e.node, e.child + 1));
				}
				else yield return e.node;
			}
		}
	}
}
