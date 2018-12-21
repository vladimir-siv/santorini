using System;
using System.Collections.Generic;
using System.Text;

namespace etf.santorini.sv150155d.logic
{
	using game;
	using players;
	using pool;

	public sealed class BoardState : IPoolSupport<string>, IPoolSupport
	{
		public const string Empty = "-1:-1:-1:-1:-1:-1:-1:-1:-1:-1:-1:-1:-1:-1:-1:-1:-1:-1:-1:-1:-1:-1:-1:-1:-1|??:??:??:??|??";

		static BoardState()
		{
			var injector = new PoolInjector<string>(Empty);
			PoolInjector.Map<BoardState, string>(injector);
		}

		private readonly (int level, Player standing)[,] state = new (int level, Player standing)[5, 5]
		{
			{ (-1, null), (-1, null), (-1, null), (-1, null), (-1, null) },
			{ (-1, null), (-1, null), (-1, null), (-1, null), (-1, null) },
			{ (-1, null), (-1, null), (-1, null), (-1, null), (-1, null) },
			{ (-1, null), (-1, null), (-1, null), (-1, null), (-1, null) },
			{ (-1, null), (-1, null), (-1, null), (-1, null), (-1, null) }
		};
		private string[,] positions = new string[2, 2]
		{
			{ "??", "??" },
			{ "??", "??" }
		};
		public int? OnTurn { get; private set; } = null;

		private GameController controller = null;
		private StringBuilder sb = new StringBuilder();

		public void OnCreate() { controller = GameController.CurrentReference; for (var i = 0; i < 4; ++i) positions[i % 2, i / 2] = "??"; OnTurn = null; }
		public void OnCreate(string boardState) { OnCreate(); FromString(boardState); }
		public void OnDestroy() { }

		public (int level, Player standing) this[(char row, int col) position]
		{
			get => this[position.row, position.col];
			set => this[position.row, position.col] = value;
		}
		public (int level, Player standing) this[char row, int col]
		{
			get => this[row - 'A', col - 1];
			set => this[row - 'A', col - 1] = value;
		}
		public (int level, Player standing) this[int row, int col]
		{
			get => state[row, col];
			set => state[row, col] = value;
		}

		public List<(char row, int col)> FindAdjacentFields((char row, int col) position, bool constrainLevels = false, bool constrainBlockedOrFilled = false, bool constrainSelf = true)
		{
			var currentField = this[position.row, position.col];
			var adjacentFields = new List<(char row, int col)>(8);

			for (int i = -1; i <= 1; ++i)
			{
				for (int j = -1; j <= 1; ++j)
				{
					var row = (char)(position.row + i);
					var col = position.col + j;

					if (i != 0 || j != 0)
					{
						if (('A' <= row && row <= 'E') && (1 <= col && col <= 5))
						{
							var adjacentField = this[row, col];

							if (!constrainLevels || currentField.level + 1 >= adjacentField.level)
							{
								if (!constrainBlockedOrFilled || (adjacentField.level < Building.TILES_COUNT && adjacentField.standing == null))
								{
									adjacentFields.Add((row, col));
								}
							}
						}
					}
					else if (!constrainSelf) adjacentFields.Add((row, col));
				}
			}

			return adjacentFields;
		}
		public ((char, int) p1, (char, int) p2) FindFieldsWithPlayer(Player player)
		{
			int playerIndex = controller.FirstPlayer == player ? 0 : 1;

			if (positions[playerIndex, 0] == "??" || positions[playerIndex, 1] == "??")
			{
				throw new AccessViolationException("Player still doesn't have his both figures placed");
			}

			var p1 = positions[playerIndex, 0];
			var p2 = positions[playerIndex, 1];

			return ((p1[0], p1[1] - '0'), (p2[0], p2[1] - '0'));
		}
		public ((char, int) p1, (char, int) p2) FindFieldsWithPlayerOnTurn()
		{
			return FindFieldsWithPlayer(OnTurn == controller.FirstPlayer.No ? controller.FirstPlayer : controller.SecondPlayer);
		}
		public ((char, int) p1, (char, int) p2) FindFieldsWithPlayerNotOnTurn()
		{
			return FindFieldsWithPlayer(OnTurn != controller.FirstPlayer.No ? controller.FirstPlayer : controller.SecondPlayer);
		}

		public void SwitchTurn()
		{
			PrepareTurn(OnTurn == controller.FirstPlayer.No ? controller.SecondPlayer : controller.FirstPlayer);
		}
		public void PrepareTurn(Player player)
		{
			OnTurn = player.No;
		}
		public void PlaceFigure(Player player, (char row, int col) position)
		{
			if (player == null) return;
			var playerIndex = player == controller.FirstPlayer ? 0 : 1;
			this[position] = (this[position].level, player);
			var positionIndex = positions[playerIndex, 0] == "??" ? 0 : 1;
			positions[playerIndex, positionIndex] = string.Empty + position.row + position.col;
		}
		public void MoveFigure((char row, int col) from, (char row, int col) to)
		{
			var player = this[from].standing;
			if (player == null) return;
			var playerIndex = player == controller.FirstPlayer ? 0 : 1;
			this[from] = (this[from].level, null);
			this[to] = (this[to].level, player);
			var positionFrom = string.Empty + from.row + from.col;
			var positionTo = string.Empty + to.row + to.col;
			var positionIndex = positions[playerIndex, 0] == positionFrom ? 0 : 1;
			positions[playerIndex, positionIndex] = positionTo;
		}
		public void Build((char row, int col) position)
		{
			this[position] = (this[position].level + 1, this[position].standing);
		}
		public void Destroy((char row, int col) position)
		{
			this[position] = (this[position].level - 1, this[position].standing);
		}

		public IEnumerable<(string hashkey, (char row, int col) from, (char row, int col) to, (char row, int col) build)> PossibleOutcomes()
		{
			(char row, int col)[] Arrayify(((char, int) p1, (char, int) p2) ps) => new (char row, int col)[] { ps.p1, ps.p2 };
			var positions = Arrayify(FindFieldsWithPlayerOnTurn());

			SwitchTurn();

			for (var figure = 0; figure < positions.Length; ++figure)
			{
				var pAllowed = FindAdjacentFields(positions[figure], constrainLevels: true, constrainBlockedOrFilled: true, constrainSelf: true);

				for (var p = 0; p < pAllowed.Count; ++p)
				{
					MoveFigure(positions[figure], pAllowed[p]);

					var bAllowed = FindAdjacentFields(pAllowed[p], constrainLevels: false, constrainBlockedOrFilled: true, constrainSelf: true);

					for (var b = 0; b < bAllowed.Count; ++b)
					{
						Build(bAllowed[b]);
						
						yield return (ToString(), positions[figure], pAllowed[p], bAllowed[b]);

						Destroy(bAllowed[b]);
					}

					MoveFigure(pAllowed[p], positions[figure]);
				}
			}

			SwitchTurn();
		}

		public void FromString(string boardState)
		{
			var seperated = boardState.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

			if (seperated.Length > 0)
			{
				var levels = seperated[0].Split(':');
				if (levels.Length == 25)
				{
					for (var i = 0; i < levels.Length; ++i)
					{
						int level = -1;

						try { level = Convert.ToInt32(levels[i]); }
						catch { }

						this[i / 5, i % 5] = (level, null);
					}

					for (var i = 0; i < 4; ++i) positions[i % 2, i / 2] = "??";
					OnTurn = null;
				}
				else return;
			}

			if (seperated.Length > 1)
			{
				var standings = seperated[1].Split(':');
				if (standings.Length == 4)
				{
					(char row, int col) decode(string position) => (position[0], position[1] - '0');
					for (var i = 0; i < standings.Length; ++i)
					{
						if (standings[i].Length != 2 || standings[i] == "??") continue;
						var player = i % 2 == 0 ? controller.FirstPlayer : controller.SecondPlayer;
						var position = decode(standings[i]);
						if (this[position].standing != null || this[position].level >= Building.TILES_COUNT) continue;
						positions[i % 2, i / 2] = standings[i];
						this[position] = (this[position].level, player);
					}
				}
				else return;
			}

			if (seperated.Length > 2)
			{
				try { OnTurn = Convert.ToInt32(seperated[2]); }
				catch { }
			}
		}
		public override string ToString()
		{
			sb.Clear();

			for (char row = 'A'; row <= 'E'; ++row)
			{
				for (int col = 1; col <= 5; ++col)
				{
					var field = this[row, col];

					sb.Append(this[row, col].level);
					sb.Append(':');
				}
			}

			sb[sb.Length - 1] = '|';

			for (var i = 0; i < 4; ++i)
			{
				sb.Append(positions[i % 2, i / 2]);
				sb.Append(':');
			}

			sb[sb.Length - 1] = '|';

			sb.Append(OnTurn?.ToString() ?? "??");
			
			return sb.ToString();
		}
	}
}
