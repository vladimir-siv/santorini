using System;

namespace etf.santorini.sv150155d.ioc
{
	using players;
	using ai;

	public static class InjectionMapper
	{
		static InjectionMapper()
		{
			System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

			Player.MapInitializer<Human>(InjectionParser.Empty);

			Player.MapInitializer<Dummy>(InjectionParser.Empty);

			var initializer = new InjectionParser("alpha-beta", "optimized", "level", "estimation", "threshold");
			initializer.MapParser("alpha-beta", Convert.ToBoolean);
			initializer.MapParser("optimized", Convert.ToBoolean);
			initializer.MapParser("level", Convert.ToInt32);
			initializer.MapParser("estimation", Estimator.Convert);
			initializer.MapParser("threshold", Convert.ToSingle);
			initializer.MapChoices("estimation", new string[]
			{
				typeof(Easy).FullName,
				typeof(Medium).FullName,
				typeof(Hard).FullName
			});
			Player.MapInitializer<MiniMax>(initializer);
		}

		public static void Initialize()
		{

		}
	}
}
