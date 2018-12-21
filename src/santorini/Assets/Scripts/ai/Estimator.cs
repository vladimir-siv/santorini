using System;

namespace etf.santorini.sv150155d.ai
{
	public static class Estimator
	{
		public static IEstimator Convert(string estimator)
		{
			return (IEstimator)Activator.CreateInstance(Type.GetType(estimator));
		}
	}
}
