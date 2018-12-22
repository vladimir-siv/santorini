namespace etf.santorini.sv150155d.ai
{
	using logic;
	using players;

	public class Hard : IEstimator
	{
		public float EstimateMoving(Player me, Player opponent, BoardState state, (char row, int col) from, (char row, int col) to)
		{
			throw new System.NotImplementedException();
		}

		public float EstimateBuilding(Player me, Player opponent, BoardState state, (char row, int col) position)
		{
			throw new System.NotImplementedException();
		}
	}
}
