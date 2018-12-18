﻿using System;
using System.Collections.Generic;

namespace etf.santorini.sv150155d.ioc
{
	public sealed class InjectionParser
	{
		public static InjectionParser Empty { get; private set; } = new InjectionParser(new string[] { });

		private readonly IDictionary<string, string> parameters = new Dictionary<string, string>();
		private readonly IDictionary<string, (Type t, object p)> parsers = new Dictionary<string, (Type, object)>();

		public int Size { get; }

		public InjectionParser(string[] parameters)
		{
			for (var i = 0; i < parameters.Length; ++i)
			{
				this.parameters.Add(parameters[i], string.Empty);
				this.parsers.Add(parameters[i], (null, null));
			}

			Size = parameters.Length;
		}

		public string this[string parameter]
		{
			get
			{
				if (!parameters.ContainsKey(parameter)) throw new IndexOutOfRangeException("Invalid parameter name");
				return parameters[parameter];
			}
			set
			{
				if (!parameters.ContainsKey(parameter)) throw new IndexOutOfRangeException("Invalid parameter name");
				parameters[parameter] = value;
			}
		}

		public void MapParser<T>(string parameter, Func<string, T> parser)
		{
			if (!parsers.ContainsKey(parameter)) throw new IndexOutOfRangeException("Invalid parameter name");
			parsers[parameter] = (typeof(T), parser);
		}

		public Type ResolveType(string parameter)
		{
			if (!parameters.ContainsKey(parameter)) throw new IndexOutOfRangeException("Invalid parameter name");
			return parsers[parameter].t;
		}

		public T Resolve<T>(string parameter)
		{
			if (!parameters.ContainsKey(parameter)) throw new IndexOutOfRangeException("Invalid parameter name");
			if (parsers[parameter].t != typeof(T)) throw new InvalidCastException("Parameter type does not match the parser type");
			return ((Func<string, T>)parsers[parameter].p)(parameters[parameter]);
		}

		public IEnumerable<string> GetParameters()
		{
			foreach (var parameter in parameters)
			{
				yield return parameter.Key;
			}
		}
	}
}
