using System.Collections.Generic;
using System.Threading.Tasks;

namespace etf.santorini.sv150155d.players
{
	using game;

	public abstract class Player
	{
		public virtual string Description => "Player";

		public virtual int No { get; protected set; } = 0;
		private AutoPlayer AutoPlayer { get; set; } = null;
		public bool IsAutoPlaying => AutoPlayer != null;

		protected Player(int No) { this.No = No; }
		protected Player(int No, AutoPlayer autoplayer) { this.No = No; this.AutoPlayer = autoplayer;  }

		public virtual async Task<(char, int)> PlaceFigure()
		{
			return await AutoPlayer.PlaceFigure();
		}
		public void CheckAutoPlayer()
		{
			if (IsAutoPlaying && !AutoPlayer.HasMore) AutoPlayer = null;
		}
		public virtual async Task PrepareTurn()
		{
			await AutoPlayer.PrepareTurn();
		}
		public virtual async Task<(char, int)> SelectFigure((char row, int col) p1, (char row, int col) p2)
		{
			return await AutoPlayer.SelectFigure(p1, p2);
		}
		public virtual async Task<(char, int)> MoveFigure((char row, int col) playerPosition, List<Field> allowedMovements)
		{
			return await AutoPlayer.MoveFigure(playerPosition, allowedMovements);
		}
		public virtual async Task<(char, int)> BuildOn((char row, int col) playerPosition, List<Field> allowedBuildings)
		{
			return await AutoPlayer.BuildOn(playerPosition, allowedBuildings);
		}
	}
}
