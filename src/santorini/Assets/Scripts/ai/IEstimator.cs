namespace etf.santorini.sv150155d.ai
{
	using logic;

	public interface IEstimator
	{
		float EstimateMoving(BoardState state, (char row, int col) from, (char row, int col) to);
		float EstimateBuilding(BoardState state, (char row, int col) position);
	}
}
