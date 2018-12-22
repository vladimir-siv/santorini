namespace etf.santorini.sv150155d.ai
{
	using logic;
	using players;

	public interface IEstimator
	{
		float EstimateMoving(Player me, Player opponent, BoardState state, (char row, int col) from, (char row, int col) to);
		float EstimateBuilding(Player me, Player opponent, BoardState state, (char row, int col) position);
	}
}
