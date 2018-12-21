using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace etf.santorini.sv150155d.players
{
	public class Dummy : Player
	{
		private Random rnd = new Random();

		public override string Description => "Dummy";

		public Dummy(int No) : base(No) { }
		public Dummy(int No, AutoPlayer autoplayer) : base(No, autoplayer) { }

		public override async Task PreparePlacement()
		{
			if (IsAutoPlaying) await base.PrepareTurn();
			else await Task.CompletedTask;
		}

		public override async Task<(char, int)> PlaceFigure1()
		{
			if (IsAutoPlaying) return await base.PlaceFigure1();
			return ((char)('A' + rnd.Next(5)), 1 + rnd.Next(5));
		}

		public override async Task<(char, int)> PlaceFigure2()
		{
			if (IsAutoPlaying) return await base.PlaceFigure2();
			return ((char)('A' + rnd.Next(5)), 1 + rnd.Next(5));
		}

		public override async Task PrepareTurn()
		{
			if (IsAutoPlaying) await base.PrepareTurn();
			else await Task.CompletedTask;
		}

		public override async Task<(char, int)> SelectFigure((char row, int col) p1, (char row, int col) p2)
		{
			if (IsAutoPlaying) return await base.SelectFigure(p1, p2);
			if (rnd.Next(2) == 0) return p1;
			else return p2;
		}

		public override async Task<(char, int)> MoveFigure((char row, int col) playerPosition, List<(char row, int col)> allowedMovements)
		{
			if (IsAutoPlaying) return await base.MoveFigure(playerPosition, allowedMovements);
			return allowedMovements[rnd.Next(allowedMovements.Count)];
		}

		public override async Task<(char, int)> BuildOn((char row, int col) playerPosition, List<(char row, int col)> allowedBuildings)
		{
			if (IsAutoPlaying) return await base.BuildOn(playerPosition, allowedBuildings);
			return allowedBuildings[rnd.Next(allowedBuildings.Count)];
		}
	}
}
