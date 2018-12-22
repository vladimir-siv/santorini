using System;
using System.Collections.Generic;

namespace etf.santorini.sv150155d.collections
{
	public abstract class HashCollection<TKey, TValue, TWeight> where TKey : IEquatable<TKey>
	{
		public abstract class Node
		{
			public HashCollection<TKey, TValue, TWeight> Container { get; protected set; } = null;

			protected Dictionary<TKey, Node> nodes = null;
			protected Dictionary<TKey, TWeight> children = new Dictionary<TKey, TWeight>();
			
			public int Level { get; private set; } = 0;
			public bool IsRoot { get; protected internal set; } = false;
			public TKey Key { get; protected set; } = default;
			public TValue Value { get; set; } = default;
			public int ChildrenCount => children.Count;
			public bool HashChildren => ChildrenCount > 0;

			public Node this[TKey child] => Container.nodes[child];
			public Node this[int index]
			{
				get
				{
					int i = 0;
					foreach (var child in children)
					{
						if (i++ == index) return Container.nodes[child.Key];
					}

					throw new IndexOutOfRangeException();
				}
			}

			protected Node(HashCollection<TKey, TValue, TWeight> container, TKey key, int level = 0, bool isRoot = false) : this(container, key, default, level, isRoot) { }
			protected Node(HashCollection<TKey, TValue, TWeight> container, TKey key, TValue value, int level = 0, bool isRoot = false)
			{
				Container = container;
				nodes = container.nodes;
				Key = key;
				Level = level;
				IsRoot = isRoot;
				container.nodes[key] = this;
			}

			public abstract Node AddChild(TKey child, TWeight weight = default);
			public virtual void SetWeight(TKey child, TWeight weight)
			{
				if (children.ContainsKey(child))
				{
					children[child] = weight;
				}
			}
			public virtual TWeight GetWeight(TKey child)
			{
				return children[child];
			}
			public abstract void RemoveChild(TKey child);
			public abstract void ClearChildren();
			public virtual bool HasChild(TKey child)
			{
				return children.ContainsKey(child);
			}
			public virtual IEnumerable<Node> EnumerateChildren()
			{
				foreach (var child in children)
				{
					yield return nodes[child.Key];
				}
			}
		}

		protected Dictionary<TKey, Node> nodes = new Dictionary<TKey, Node>();

		protected Node root = null;
		public Node Root
		{
			get => root;
			set
			{
				if (root == value) return;

				if (value.Container != this) throw new ArgumentException("Given node does not belong to this container.");
				
				value.IsRoot = true;

				if (root != null)
				{
					if (nodes.ContainsKey(value.Key))
					{
						root.ClearChildren();
						nodes.Remove(root.Key);
					}
					else
					{
						root.IsRoot = false;
						value.AddChild(root.Key);
					}
				}

				root = value;
			}
		}

		public int Count => nodes.Count;
		public Node this[TKey key] => nodes[key];
		
		public abstract void Init(TKey key, TValue value = default);

		public bool ContainsKey(TKey key)
		{
			return nodes.ContainsKey(key);
		}

		public IEnumerable<TKey> EnumerateKeys()
		{
			foreach (var key in nodes.Keys)
			{
				yield return key;
			}
		}
	}
}
