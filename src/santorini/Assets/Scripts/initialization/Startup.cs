namespace etf.santorini.sv150155d.initialization
{
	using ioc;

	public static class Global
	{
		static Global()
		{
			System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
		}

		public static void Initialize()
		{
			InjectionMapper.Initialize();
		}
	}
}
