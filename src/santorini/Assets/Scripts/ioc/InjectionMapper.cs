using System;

namespace etf.santorini.sv150155d.ioc
{
	using players;

	public static class InjectionMapper
	{
		static InjectionMapper()
		{
			InjectionParser initializer = null;

			Player.MapInitializer<Human>(InjectionParser.Empty);

			initializer = new InjectionParser(new[] { "alpha-beta", "optimized", "level" });
			initializer.MapParser("alpha-beta", Convert.ToBoolean);
			initializer.MapParser("optimized", Convert.ToBoolean);
			initializer.MapParser("level", Convert.ToInt32);
			Player.MapInitializer<DummyPlayer>(initializer);
		}

		public static void Initialize()
		{

		}
	}
}
