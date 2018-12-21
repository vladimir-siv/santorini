using System;

namespace etf.santorini.sv150155d.ai
{
	using logic;

	public class Easy : IEstimator
	{
		private float Estimate(BoardState state, (char row, int col) position)
		{
			float m = state[position].level + 1f;

			float l = m + 1f;

			var onTurn = state.FindFieldsWithPlayerOnTurn();
			var notOnTurn = state.FindFieldsWithPlayerNotOnTurn();

			float distance((char row, int col) from, (char row, int col) to) => Math.Max(Math.Abs(from.row - to.row), Math.Abs(from.col - to.col));

			float onTurnDistance = Math.Min(distance(onTurn.p1, position), distance(onTurn.p2, position));
			float notOnTurnDistance = Math.Min(distance(notOnTurn.p1, position), distance(notOnTurn.p2, position));

			l *= notOnTurnDistance - onTurnDistance;

			return m + l;
		}

		public float EstimateMoving(BoardState state, (char row, int col) from, (char row, int col) to)
		{
			return Estimate(state, to);
		}

		public float EstimateBuilding(BoardState state, (char row, int col) position)
		{
			return Estimate(state, position);
		}
	}
}
