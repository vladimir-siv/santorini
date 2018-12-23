namespace etf.santorini.sv150155d.ai
{
	using logic;
	using players;

	public interface IEstimator
	{
		float Threshold { get; set; }
		float EstimateMove(in Player me, in Player opponent, in BoardState state, Move move);
		float EstimateFinalState(in Player me, in Player opponent, in BoardState state);
	}
}
