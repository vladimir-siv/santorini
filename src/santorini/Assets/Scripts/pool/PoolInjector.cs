using System;
using System.Collections.Generic;

namespace etf.santorini.sv150155d.pool
{
	public class PoolInjector
	{
		private static IDictionary<(Type, Type), PoolInjector> injectors = new Dictionary<(Type, Type), PoolInjector>();
		public static void Map<T, R>(PoolInjector<R> injector) { injectors[(typeof(T), typeof(R))] = injector; }
		public static PoolInjector<R> Resolve<T, R>() { return (PoolInjector<R>)injectors[(typeof(T), typeof(R))]; }
	}

	public sealed class PoolInjector<T> : PoolInjector
	{
		public T CreateValues { get; private set; }
		
		public PoolInjector(T CreateValues)
		{
			this.CreateValues = CreateValues;
		}
	}
}
