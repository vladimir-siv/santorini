using System;

namespace etf.santorini.sv150155d.collections
{
	public class HashTree<TKey, TValue, TWeight> : HashCollection<TKey, TValue, TWeight> where TKey : IEquatable<TKey>
	{
		public class TreeNode : Node
		{
			internal TreeNode(HashCollection<TKey, TValue, TWeight> container, TKey key, int level = 0, bool isRoot = false) : this(container, key, default, level, isRoot) { }
			internal TreeNode(HashCollection<TKey, TValue, TWeight> container, TKey key, TValue value, int level = 0, bool isRoot = false) : base(container, key, value, level, isRoot) { }

			public override Node AddChild(TKey child, TWeight weight = default)
			{
				if (children.ContainsKey(child)) return nodes[child];
				TreeNode node = null;
				if (nodes.ContainsKey(child)) node = (TreeNode)nodes[child];
				else node = new TreeNode(Container, child, Level + 1);
				children[child] = weight;
				return node;
			}
			public override void RemoveChild(TKey child)
			{
				if (children.ContainsKey(child))
				{
					var childNode = this[child];
					if (childNode.IsRoot) return;
					childNode.ClearChildren();
					nodes.Remove(child);
					children.Remove(child);
				}
			}
			public override void ClearChildren()
			{
				foreach (var child in children)
				{
					var childNode = this[child.Key];
					if (childNode.IsRoot) continue;
					childNode.ClearChildren();
					nodes.Remove(child.Key);
				}

				children.Clear();
			}
		}

		public override void Init(TKey key, TValue value = default)
		{
			Root = new TreeNode(this, key, value);
		}

		public HashCollectionEnumerator<TKey, TValue, TWeight> EnumeratePreOrder(TKey startingPoint) { return new HashTreeEnumeratorPre<TKey, TValue, TWeight>(this, startingPoint); }
		public HashCollectionEnumerator<TKey, TValue, TWeight> EnumeratePostOrder(TKey startingPoint) { return new HashTreeEnumeratorPost<TKey, TValue, TWeight>(this, startingPoint); }
		public HashCollectionEnumerator<TKey, TValue, TWeight> EnumerateLevelOrder(TKey startingPoint) { return new HashTreeEnumeratorLevel<TKey, TValue, TWeight>(this, startingPoint); }
	}
}
