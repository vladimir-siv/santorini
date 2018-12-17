using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace etf.santorini.sv150155d.players
{
	using game;
	using logging;

	public sealed class AutoPlayer : Player
	{
		public Player Player1 { get; private set; } = null;
		public Player Player2 { get; private set; } = null;

		private readonly GameLog gameLog = null;

		private readonly IEnumerator<string> enumerator = null;
		public bool HasMore { get; private set; }
		private IEnumerable<string> Tokens
		{
			get
			{
				var logs = gameLog.Present;

				// Warning: boxing
				Player1 = (Player)System.Activator.CreateInstance(System.Type.GetType(logs[0]), 1, this);
				Player2 = (Player)System.Activator.CreateInstance(System.Type.GetType(logs[1]), 2, this);
				
				for (var i = 2; i < logs.Length; ++i)
				{
					var tokens = logs[i].Split(' ');

					for (var j = 0; j < tokens.Length; ++j)
					{
						yield return tokens[j];
					}
				}
			}
		}

		public AutoPlayer(GameLog gameLog) : base(0)
		{
			this.gameLog = gameLog;
			enumerator = Tokens.GetEnumerator();
			Next();
		}

		private void Next()
		{
			HasMore = enumerator.MoveNext();
			if (!HasMore) enumerator.Dispose();
		}

		private (char, int) GetNextToken()
		{
			if (!HasMore) throw new OverflowException("No more tokens found");

			string current = enumerator.Current;
			Next();
			return (current[0], current[1] - '0');
		}

		public override async Task<(char, int)> PlaceFigure()
		{
			return await Task.FromResult(GetNextToken());
		}

		public override async Task PrepareTurn()
		{
			await Task.CompletedTask;
		}

		public override async Task<(char, int)> SelectFigure((char row, int col) p1, (char row, int col) p2)
		{
			return await Task.FromResult(GetNextToken());
		}

		public override async Task<(char, int)> MoveFigure((char row, int col) playerPosition, List<Field> allowedMovements)
		{
			return await Task.FromResult(GetNextToken());
		}

		public override async Task<(char, int)> BuildOn((char row, int col) playerPosition, List<Field> allowedBuildings)
		{
			return await Task.FromResult(GetNextToken());
		}
	}
}
