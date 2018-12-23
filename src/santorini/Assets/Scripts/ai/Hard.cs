namespace etf.santorini.sv150155d.ai
{
	using logic;
	using players;

	public class Hard : IEstimator
	{
		public float Threshold { get; set; } = float.NegativeInfinity;

		public float EstimateMove(in Player me, in Player opponent, in BoardState state, Move move)
		{
			throw new System.NotImplementedException();
		}

		public float EstimateFinalState(in Player me, in Player opponent, in BoardState state)
		{
			throw new System.NotImplementedException();
		}
	}
}
