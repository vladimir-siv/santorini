using System;
using System.Collections.Generic;

namespace etf.santorini.sv150155d.ioc
{
	public sealed class InjectionParser
	{
		public static InjectionParser Empty { get; private set; } = new InjectionParser(new string[] { });

		private readonly IDictionary<string, string> parameters;
		private readonly IDictionary<string, (Type t, object p)> parsers;
		private readonly IDictionary<string, string[]> choices;

		public int Size { get; }

		private InjectionParser(IDictionary<string, string> parameters, IDictionary<string, (Type t, object p)> parsers, IDictionary<string, string[]> choices)
		{
			this.parameters = new Dictionary<string, string>(parameters);
			this.parsers = new Dictionary<string, (Type, object)>(parsers);
			this.choices = new Dictionary<string, string[]>();
			
			foreach (var c in choices)
			{
				MapChoices(c.Key, (string[])c.Value.Clone());
			}

			Size = this.parameters.Count;
		}

		public InjectionParser(params string[] parameters)
		{
			this.parameters = new Dictionary<string, string>();
			this.parsers = new Dictionary<string, (Type, object)>();
			this.choices = new Dictionary<string, string[]>();

			for (var i = 0; i < parameters.Length; ++i)
			{
				this.parameters.Add(parameters[i], string.Empty);
				this.parsers.Add(parameters[i], (null, null));
			}

			Size = parameters.Length;
		}

		public InjectionParser Clone()
		{
			return new InjectionParser(parameters, parsers, choices);
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

		public void MapChoices(string parameter, string[] choices)
		{
			if (!parsers.ContainsKey(parameter)) throw new IndexOutOfRangeException("Invalid parameter name");
			this.choices[parameter] = choices;
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

		public bool ResolveChoices(string parameter, out string[] choices)
		{
			choices = null;
			if (!this.choices.ContainsKey(parameter)) return false;
			choices = this.choices[parameter];
			return true;
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
