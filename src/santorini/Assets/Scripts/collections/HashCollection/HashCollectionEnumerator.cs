using System;
using System.Collections;

namespace etf.santorini.sv150155d.collections
{
	public abstract class HashCollectionEnumerator<TKey, TValue, TWeight> where TKey : IEquatable<TKey>
	{
		public HashCollection<TKey, TValue, TWeight> Container { get; private set; } = null;
		public TKey StartingPoint { get; private set; }
		protected IEnumerator Pointer { get; private set; } = null;
		public bool IsValid { get; private set; } = false;
		public abstract HashCollection<TKey, TValue, TWeight>.Node Node { get; }
		
		protected HashCollectionEnumerator(HashCollection<TKey, TValue, TWeight> container, TKey startingPoint)
		{
			Container = container;
			StartingPoint = startingPoint;
			Reset();
		}

		public void Reset()
		{
			Pointer = Enumerate().GetEnumerator();
			Next();
		}

		public void Next()
		{
			IsValid = Pointer.MoveNext();
		}

		protected abstract IEnumerable Enumerate();
	}
}
