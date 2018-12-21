using System;

namespace etf.santorini.sv150155d.collections
{
	public class HashDAGraph<TKey, TValue, TWeight> : HashCollection<TKey, TValue, TWeight> where TKey : IEquatable<TKey>
	{
		public class DAGraphNode : Node
		{
			private int tracking = 0;

			internal DAGraphNode(HashCollection<TKey, TValue, TWeight> container, TKey key, int level = 0, bool isRoot = false) : this(container, key, default, level, isRoot) { }
			internal DAGraphNode(HashCollection<TKey, TValue, TWeight> container, TKey key, TValue value, int level = 0, bool isRoot = false) : base(container, key, value, level, isRoot) { }
			
			private void UpdateRemoval(TKey child)
			{
				var childNode = (DAGraphNode)nodes[child];
				--childNode.tracking;
				if (childNode.IsRoot) return;
				if (childNode.tracking == 0)
				{
					childNode.ClearChildren();
					nodes.Remove(child);
				}
			}
			
			public override Node AddChild(TKey child, TWeight weight = default)
			{
				if (children.ContainsKey(child)) return nodes[child];
				DAGraphNode node = null;
				if (nodes.ContainsKey(child)) node = (DAGraphNode)nodes[child];
				else node = new DAGraphNode(Container, child, Level + 1);
				++node.tracking;
				children[child] = weight;
				return node;
			}
			public override void RemoveChild(TKey child)
			{
				if (children.ContainsKey(child))
				{
					UpdateRemoval(child);
					children.Remove(child);
				}
			}
			public override void ClearChildren()
			{
				foreach (var child in children)
				{
					UpdateRemoval(child.Key);
				}

				children.Clear();
			}
		}

		public override void Init(TKey key, TValue value = default)
		{
			Root = new DAGraphNode(this, key, value);
		}

		public HashDAGraphEnumeratorLevel<TKey, TValue, TWeight> EnumerateLevelOrder(TKey startingPoint) { return new HashDAGraphEnumeratorLevel<TKey, TValue, TWeight>(this, startingPoint); }
	}
}
