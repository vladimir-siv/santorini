using System;
using System.Reflection;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace etf.santorini.sv150155d.ioc
{
	public class InjectorNotFoundException : Exception
	{
		public InjectorNotFoundException() { }
		protected InjectorNotFoundException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) { }
		public InjectorNotFoundException(string message) : base(message) { }
		public InjectorNotFoundException(string message, Exception innerException) : base(message, innerException) { }
	}

	public static class DependencyInjector<T>
	{
		public static Func<T> Resolver { get; set; } = null;

		public static T Resolve()
		{
			if (Resolver == null) throw new InjectorNotFoundException("Dependency was not injected before using resolver.");
			return Resolver();
		}
	}

	public static class DependencyInjector
	{
		public static object Resolve(Type type)
		{
			Type injector = typeof(DependencyInjector<>).MakeGenericType(type);
			MethodInfo resolver = injector.GetMethod("Resolve", BindingFlags.Static);
			return resolver?.Invoke(null, null);
		}

		public static T Resolve<T>()
		{
			return (T)Resolve(typeof(T));
		}
	}
}
