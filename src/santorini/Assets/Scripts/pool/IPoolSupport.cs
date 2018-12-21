namespace etf.santorini.sv150155d.pool
{
	public interface IPoolSupport
	{
		void OnCreate();
		void OnDestroy();
	}

	public interface IPoolSupport<T>
	{
		void OnCreate(T values);
		void OnDestroy();
	}
}
