using System;
using System.Collections;
using System.Collections.Generic;

namespace etf.santorini.sv150155d.collections
{
	public class HashTreeEnumeratorPre<TKey, TValue, TWeight> : HashCollectionEnumerator<TKey, TValue, TWeight> where TKey : IEquatable<TKey>
	{
		public override HashCollection<TKey, TValue, TWeight>.Node Node => (HashCollection<TKey, TValue, TWeight>.Node)Pointer.Current;
		
		internal HashTreeEnumeratorPre(HashTree<TKey, TValue, TWeight> container, TKey startingPoint) : base(container, startingPoint) { }

		Stack<HashCollection<TKey, TValue, TWeight>.Node> stack = new Stack<HashCollection<TKey, TValue, TWeight>.Node>();
		protected override IEnumerable Enumerate()
		{
			stack.Clear();

			stack.Push(Container[StartingPoint]);
			while (stack.Count > 0)
			{
				var node = stack.Pop();

				yield return node;

				foreach (var child in node.EnumerateChildren())
				{
					stack.Push(child);
				}
			}
		}
	}
}
