using System;
using System.Collections.Generic;

namespace etf.santorini.sv150155d.ai
{
	using game;
	using logic;
	using players;

	public class Hard : IEstimator
	{
		public float Threshold { get; set; } = float.NegativeInfinity;
		
		private float EstimateEnvironment(BoardState state, (char row, int col) position, ((char row, int col) p1, (char row, int col) p2) opPositions)
		{
			float distance((char row, int col) from, (char row, int col) to) => Math.Max(Math.Abs(from.row - to.row), Math.Abs(from.col - to.col));
			
			var est = 0f;
			var myLevel = state[position.row, position.col].level + 1f;

			for (var i = -1; i <= 1; ++i)
			{
				for (var j = -1; j <= 1; ++j)
				{
					if (i == 0 && j == 0) continue;

					var row = (char)(position.row + i);
					var col = position.col + j;

					if (('A' <= row && row <= 'E') && (1 <= col && col <= 5))
					{
						var adjacentField = state[row, col];

						if (adjacentField.level < Building.TILES_COUNT)
						{
							var level = adjacentField.level + 1f;
							var dist = Math.Min(distance((row, col), opPositions.p1), distance((row, col), opPositions.p2));
							if (dist == 2) dist = 1; else if (dist == 1) dist = 2;
							var points = level * level * dist;
							points *= myLevel + 1f;
							est += points;
						}
						else est -= 3;
					}
				}
			}

			return est;
		}

		public float EstimateMove(Player me, Player opponent, BoardState state, Move move)
		{
			float distance((char row, int col) from, (char row, int col) to) => Math.Max(Math.Abs(from.row - to.row), Math.Abs(from.col - to.col));
			
			//var p = state[move.FromPosition].level + 1f;
			var m = state[move.ToPosition].level + 1f;
			var l = state[move.BuildOn].level + 1f;

			var myPositions = state.FindFieldsWithPlayer(me);
			var opPositions = state.FindFieldsWithPlayer(opponent);

			if (move.FromPosition == myPositions.p1) myPositions.p1 = move.ToPosition;
			if (move.FromPosition == myPositions.p2) myPositions.p2 = move.ToPosition;

			var my1Distance = distance(myPositions.p1, move.BuildOn);
			var my2Distance = distance(myPositions.p2, move.BuildOn);

			var op1Distance = distance(opPositions.p1, move.BuildOn);
			var op2Distance = distance(opPositions.p2, move.BuildOn);

			var myDistance = Math.Min(my1Distance, my2Distance); // should be 1
			var opDistance = Math.Min(op1Distance, op2Distance);

			var h1op = state[opPositions.p1].level + 1f;
			var h2op = state[opPositions.p2].level + 1f;

			var invert = l == Building.TILES_COUNT ? -1 : 1;

			if (opDistance == myDistance)
			{
				var h = op1Distance == myDistance ? h1op : h2op;
				if (op2Distance == myDistance && h2op > h) h = h2op;
				l *= (m - h) * invert;
			}
			else l *= (opDistance - myDistance) * invert;
			
			state.MoveFigure(move.FromPosition, move.ToPosition);
			state.Build(move.BuildOn);

			var q = 0f;
			q += EstimateEnvironment(state, myPositions.p1, opPositions);
			q += EstimateEnvironment(state, myPositions.p2, opPositions);
			q -= EstimateEnvironment(state, opPositions.p1, myPositions);
			q -= EstimateEnvironment(state, opPositions.p2, myPositions);
			
			state.Destroy(move.BuildOn);
			state.MoveFigure(move.ToPosition, move.FromPosition);

			var d1 = distance(opPositions.p1, move.ToPosition);
			var d2 = distance(opPositions.p2, move.ToPosition);

			var q1 = (h1op - m) * d1;
			var q2 = (h2op - m) * d2;

			var f = 2f * m + l + q - (q1 + q2);
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
