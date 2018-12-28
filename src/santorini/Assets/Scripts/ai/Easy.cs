using System;

namespace etf.santorini.sv150155d.ai
{
	using logic;
	using players;

	public class Easy : IEstimator
	{
		private bool isSet = false;
		private float threshold = float.NegativeInfinity;
		public float Threshold { get => threshold; set { if (!isSet) threshold = value; isSet = true; } }

		public float EstimateMove(Player me, Player opponent, BoardState state, Move move)
		{
			float distance((char row, int col) from, (char row, int col) to) => Math.Max(Math.Abs(from.row - to.row), Math.Abs(from.col - to.col));

			var m = state[move.ToPosition].level + 1f;
			var l = state[move.BuildOn].level + 2f;

			var myPositions = state.FindFieldsWithPlayer(me);
			var opponentPositions = state.FindFieldsWithPlayer(opponent);

			var myDistance = 1; //Math.Min(distance(myPositions.p1, move.BuildOn), distance(myPositions.p2, move.BuildOn));
			var opponentDistance = Math.Min(distance(opponentPositions.p1, move.BuildOn), distance(opponentPositions.p2, move.BuildOn));

			l *= opponentDistance - myDistance;

			var f = m + l;
			return f;
			//return state.OnTurn == me.No ? f : -f;
		}

		public float EstimateFinalState(Player me, Player opponent, BoardState state)
		{
			if (state.IsPlayerStandingOnLastLevel(me)) return float.PositiveInfinity;
			if (state.IsPlayerStandingOnLastLevel(opponent)) return float.NegativeInfinity;

			var myPositions = state.FindFieldsWithPlayer(me);
			var opponentPositions = state.FindFieldsWithPlayer(opponent);

			var myP1Allowed = state.FindAdjacentFields(myPositions.p1, constrainLevels: true, constrainBlockedOrFilled: true, constrainSelf: true);
			var myP2Allowed = state.FindAdjacentFields(myPositions.p2, constrainLevels: true, constrainBlockedOrFilled: true, constrainSelf: true);
			if (myP1Allowed.Count == 0 && myP2Allowed.Count == 0) return float.NegativeInfinity;

			var opponentP1Allowed = state.FindAdjacentFields(opponentPositions.p1, constrainLevels: true, constrainBlockedOrFilled: true, constrainSelf: true);
			var opponentP2Allowed = state.FindAdjacentFields(opponentPositions.p2, constrainLevels: true, constrainBlockedOrFilled: true, constrainSelf: true);
			if (opponentP1Allowed.Count == 0 && opponentP2Allowed.Count == 0) return float.PositiveInfinity;

			return 0.0f;
		}
	}
}
