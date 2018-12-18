using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace etf.santorini.sv150155d.players
{
	using ioc;
	using game;

	public class DummyPlayer : Player
	{
		private bool alphabeta = false;
		private bool optimized = false;
		private int level = 0;

		private Random rnd = new Random();

		public override string Description => "Dummy";

		public DummyPlayer(int No) : base(No) { }
		public DummyPlayer(int No, AutoPlayer autoplayer) : base(No, autoplayer) { }

		public override void OnPlayerInitialize(InjectionParser initializer)
		{
			alphabeta = initializer.Resolve<bool>("alpha-beta");
			optimized = initializer.Resolve<bool>("optimized");
			level = initializer.Resolve<int>("level");
		}

		public override async Task<(char, int)> PlaceFigure()
		{
			if (IsAutoPlaying) return await base.PlaceFigure();

			char row = (char)('A' + rnd.Next(5));
			int col = 1 + rnd.Next(5);

			return (row, col);
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

		public override async Task<(char, int)> MoveFigure((char row, int col) playerPosition, List<Field> allowedMovements)
		{
			if (IsAutoPlaying) return await base.MoveFigure(playerPosition, allowedMovements);

			(char row, int col) moveTo;
			moveTo.row = playerPosition.row;
			moveTo.col = playerPosition.col;

			int rowd = rnd.Next(3) - 1;
			int cold = rnd.Next(3) - 1;

			moveTo.row = (char)(moveTo.row + rowd);
			if (moveTo.row < 'A') moveTo.row = 'A';
			if (moveTo.row > 'E') moveTo.row = 'E';

			moveTo.col += cold;
			if (moveTo.col < 1) moveTo.col = 1;
			if (moveTo.col > 5) moveTo.col = 5;

			return moveTo;
		}

		public override async Task<(char, int)> BuildOn((char row, int col) playerPosition, List<Field> allowedBuildings)
		{
			if (IsAutoPlaying) return await base.BuildOn(playerPosition, allowedBuildings);

			(char row, int col) buildOn;
			buildOn.row = playerPosition.row;
			buildOn.col = playerPosition.col;

			int rowd = rnd.Next(3) - 1;
			int cold = rnd.Next(3) - 1;

			buildOn.row = (char)(buildOn.row + rowd);
			if (buildOn.row < 'A') buildOn.row = 'A';
			if (buildOn.row > 'E') buildOn.row = 'E';

			buildOn.col += cold;
			if (buildOn.col < 1) buildOn.col = 1;
			if (buildOn.col > 5) buildOn.col = 5;

			return buildOn;
		}
	}
}
