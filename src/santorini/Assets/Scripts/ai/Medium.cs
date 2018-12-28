using System;

namespace etf.santorini.sv150155d.ai
{
	using game;
	using logic;
	using players;

	public class Medium : IEstimator
	{
		private bool isSet = false;
		private float threshold = float.NegativeInfinity;
		public float Threshold { get => threshold; set { if (!isSet) threshold = value; isSet = true; } }

		public float EstimateMove(Player me, Player opponent, BoardState state, Move move)
		{
			float distance((char row, int col) from, (char row, int col) to) => Math.Max(Math.Abs(from.row - to.row), Math.Abs(from.col - to.col));

			var m = state[move.ToPosition].level + 1f;
			var l = state[move.BuildOn].level + 1f;

			var myPositions = state.FindFieldsWithPlayer(me);
			var opponentPositions = state.FindFieldsWithPlayer(opponent);

			var my1Distance = move.FromPosition == myPositions.p1 ? 1 : distance(myPositions.p1, move.BuildOn);
			var my2Distance = move.FromPosition == myPositions.p2 ? 1 : distance(myPositions.p2, move.BuildOn);

			var opponent1Distance = distance(opponentPositions.p1, move.BuildOn);
			var opponent2Distance = distance(opponentPositions.p2, move.BuildOn);

			var h1 = state[opponentPositions.p1].level + 1f;
			var h2 = state[opponentPositions.p2].level + 1f;

			var myDistance = Math.Min(my1Distance, my2Distance); // should be 1
			var opponentDistance = Math.Min(opponent1Distance, opponent2Distance);

			var invert = l == Building.TILES_COUNT ? -1 : 1;

			if (opponentDistance == myDistance)
			{
				var h = opponent1Distance == myDistance ? h1 : h2;
				if (opponent2Distance == myDistance && h2 > h) h = h2;
				l *= (m - h) * invert;
			}
			else l *= (opponentDistance - myDistance) * invert;

			var d1 = distance(opponentPositions.p1, move.ToPosition);
			var d2 = distance(opponentPositions.p2, move.ToPosition);

			var q1 = (h1 - m) * d1;
			var q2 = (h2 - m) * d2;

			var f = 2f * m + l - q1 - q2;
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
