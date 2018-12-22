using System;

namespace etf.santorini.sv150155d.ai
{
	using logic;
	using players;

	public class Easy : IEstimator
	{
		private float Estimate(Player me, Player opponent, BoardState state, (char row, int col) position)
		{
			float m = state[position].level + 1f;
			if (state.OnTurn == opponent.No) m *= -1;

			float l = m + 1f;

			var myPositions = state.FindFieldsWithPlayer(me);
			var opponentPositions = state.FindFieldsWithPlayer(opponent);

			float distance((char row, int col) from, (char row, int col) to) => Math.Max(Math.Abs(from.row - to.row), Math.Abs(from.col - to.col));

			float myDistance = Math.Min(distance(myPositions.p1, position), distance(myPositions.p2, position));
			float opponentDistance = Math.Min(distance(opponentPositions.p1, position), distance(opponentPositions.p2, position));

			l *= opponentDistance - myDistance;

			return m + l;
		}

		public float EstimateMoving(Player me, Player opponent, BoardState state, (char row, int col) from, (char row, int col) to)
		{
			return Estimate(me, opponent, state, to);
		}

		public float EstimateBuilding(Player me, Player opponent, BoardState state, (char row, int col) position)
		{
			return Estimate(me, opponent, state, position);
		}
	}
}
