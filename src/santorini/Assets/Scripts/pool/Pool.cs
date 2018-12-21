using System.Collections.Generic;

namespace etf.santorini.sv150155d.pool
{
	public static class Pool<T> where T : class, IPoolSupport, new()
	{
		private static Stack<T> pool = new Stack<T>();

		public static T Create()
		{
			T obj = pool.Count == 0 ? new T() : pool.Pop();
			obj.OnCreate();
			return obj;
		}

		public static void Destroy(T obj)
		{
			obj.OnDestroy();
			pool.Push(obj);
		}
	}

	public static class Pool<T, R> where T : class, IPoolSupport<R>, new()
	{
		private static Stack<T> pool = new Stack<T>();

		public static T Create()
		{
			T obj = pool.Count == 0 ? new T() : pool.Pop();
			obj.OnCreate(PoolInjector.Resolve<T, R>().CreateValues);
			return obj;
		}

		public static T Create(R values)
		{
			T obj = pool.Count == 0 ? new T() : pool.Pop();
			obj.OnCreate(values);
			return obj;
		}

		public static void Destroy(T obj)
		{
			obj.OnDestroy();
			pool.Push(obj);
		}
	}
}
