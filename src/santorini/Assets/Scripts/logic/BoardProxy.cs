using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace etf.santorini.sv150155d.logic
{
	using players;

	public sealed class BoardProxy
	{
		// Singleton
		public static BoardProxy Reference { get; private set; } = new BoardProxy();
		private BoardProxy() { }

		static BoardProxy()
		{
			SceneManager.sceneUnloaded += scene =>
			{
				if (scene.name == "game")
				{
					Reference.Detach();
					Reference.board = null;
				}
			};
		}

		private Board board = null;

		private BoardState state = new BoardState();
		public (int level, Player standing) this[(char row, int col) position]
		{
			get => state[position];
			private set => state[position] = value;
		}
		public (int level, Player standing) this[char row, int col]
		{
			get => state[row, col];
			private set => state[row, col] = value;
		}
		public (int level, Player standing) this[int row, int col]
		{
			get => state[row, col];
			private set => state[row, col] = value;
		}

		public void Set(Board board)
		{
			Detach();

			state.OnCreate();

			this.board = board;

			for (char row = 'A'; row <= 'E'; ++row)
			{
				for (int col = 1; col <= 5; ++col)
				{
					var field = board[row, col];
					this[row, col] = (field.Level, field.Standing);
				}
			}

			Attach();
		}
		public string StateHash => state.ToString();

		public List<(char row, int col)> FindAdjacentFields((char row, int col) position, bool constrainLevels = false, bool constrainBlockedOrFilled = false, bool constrainSelf = true)
		{
			return state.FindAdjacentFields(position, constrainLevels, constrainBlockedOrFilled, constrainSelf);
		}
		public ((char, int), (char, int)) FindFieldsWithPlayer(Player player)
		{
			return state.FindFieldsWithPlayer(player);
		}
		
		private void Attach()
		{
			if (board != null)
			{
				board.TurnPrepared += OnTurnPrepared;
				board.FigurePlaced += OnFigurePlaced;
				board.FigureMoved += OnFigureMoved;
				board.BuildingBuilt += OnBuildingBuilt;
			}
		}
		private void Detach()
		{
			if (board != null)
			{
				board.TurnPrepared -= OnTurnPrepared;
				board.FigurePlaced -= OnFigurePlaced;
				board.FigureMoved -= OnFigureMoved;
				board.BuildingBuilt -= OnBuildingBuilt;
			}
		}

		private void OnTurnPrepared(Player player)
		{
			state.PrepareTurn(player);
		}
		private void OnFigurePlaced(Player player, (char row, int col) position)
		{
			state.PlaceFigure(player, position);
		}
		private void OnFigureMoved(Player player, (char row, int col) from, (char row, int col) to)
		{
			state.MoveFigure(from, to);
		}
		private void OnBuildingBuilt((char row, int col) position)
		{
			state.Build(position);
		}
	}
}
