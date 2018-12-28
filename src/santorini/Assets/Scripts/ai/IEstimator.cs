namespace etf.santorini.sv150155d.ai
{
	using logic;
	using players;

	public interface IEstimator
	{
		float Threshold { get; set; }
		float EstimateMove(Player me, Player opponent, BoardState state, Move move);
		float EstimateFinalState(Player me, Player opponent, BoardState state);
	}
}
