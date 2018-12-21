using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace etf.santorini.sv150155d.players
{
	using ioc;
	using pool;
	using game;
	using logic;
	using ai;
	using collections;

	public class MiniMax : Player
	{
		private GameController controller = GameController.CurrentReference;
		private BoardProxy board = BoardProxy.Reference;
		
		private Random rnd = new Random();

		private bool alphabeta = false;
		private bool optimized = false;
		private int level = 0;
		private IEstimator estimator = null;
		
		private HashDAGraph<string, float, Move> dag = new HashDAGraph<string, float, Move>();
		private List<string> lastLevel = new List<string>();
		private List<string> clone = new List<string>();
		private Stack<(HashCollection<string, float, Move>.Node node, int level)> stack = new Stack<(HashCollection<string, float, Move>.Node node, int level)>();

		private (char row, int col) select;
		private (char row, int col) move;
		private (char row, int col) build;

		public override string Description => "MiniMax";

		public MiniMax(int No) : base(No) { }
		public MiniMax(int No, AutoPlayer autoplayer) : base(No, autoplayer) { }

		private void Generate(string boardState, int levels)
		{
			if (levels < 2) levels = 2;

			var state = Pool<BoardState>.Create();

			var enumerator = dag.EnumerateLevelOrder(boardState);
			for (var i = enumerator.Level; i <= levels - 2; enumerator.Next(), i = enumerator.Level)
			{
				if (enumerator.Node.HashChildren) continue;

				state.FromString(enumerator.Node.Key);

				foreach (var possibility in state.PossibleOutcomes())
				{
					enumerator.Node.AddChild(possibility.hashkey, new Move(possibility.from, possibility.to, possibility.build));
					
					if (i == levels - 2)
					{
						lastLevel.Add(possibility.hashkey);
					}
				}
			}

			Pool<BoardState>.Destroy(state);
		}
		private void Sync()
		{
			var boardState = board.StateHash;
			lastLevel.Clear();
			dag.Init(boardState);
			Generate(boardState, level);
		}
		private void Resync()
		{
			var boardState = board.StateHash;
			dag.Root = dag[boardState];

			clone.Clear();

			for (var i = 0; i < lastLevel.Count; ++i)
			{
				if (dag.ContainsKey(lastLevel[i]))
				{
					clone.Add(lastLevel[i]);
				}
			}

			lastLevel.Clear();

			for (var i = 0; i < clone.Count; ++i)
			{
				Generate(clone[i], 2);
			}
		}

		public override void OnPlayerInitialize(InjectionParser initializer)
		{
			alphabeta = initializer.Resolve<bool>("alpha-beta");
			optimized = initializer.Resolve<bool>("optimized");
			level = initializer.Resolve<int>("level");
			estimator = initializer.Resolve<IEstimator>("estimation");

			if (level < 3) level = 3;
		}

		public override async Task PreparePlacement()
		{
			if (IsAutoPlaying) await base.PrepareTurn();
			else await Task.CompletedTask;
		}

		public override async Task<(char, int)> PlaceFigure1()
		{
			if (IsAutoPlaying) return await base.PlaceFigure1();

			var field = rnd.Next(9);
			char row = (char)('B' + field / 3);
			int col = 2 + field % 3;

			return (row, col);
		}

		public override async Task<(char, int)> PlaceFigure2()
		{
			if (IsAutoPlaying) return await base.PlaceFigure2();

			var field = rnd.Next(9);
			char row = (char)('B' + field / 3);
			int col = 2 + field % 3;

			return (row, col);
		}

		public override async Task PrepareTurn()
		{
			if (IsAutoPlaying)
			{
				await base.PrepareTurn();
				return;
			}

			await Task.Run(() =>
			{
				if (dag.Root == null) Sync();
				else Resync();

				stack.Clear();
				
				for (var enumerator = dag.EnumerateLevelOrder(dag.Root.Key); enumerator.IsValid; enumerator.Next())
				{
					stack.Push((enumerator.Node, enumerator.Level));
				}

				int lastLevel = -1;

				var state = Pool<BoardState>.Create();
				var clone = Pool<BoardState>.Create();

				while (stack.Count > 0)
				{
					var entry = stack.Pop();

					if (lastLevel == -1) lastLevel = entry.level;

					float? finalEstimation = null;

					if (entry.level == lastLevel)
					{
						state.FromString(entry.node.Key);
						clone.FromString(entry.node.Key);
						
						foreach (var possibility in state.PossibleOutcomes())
						{
							var estimation =
								estimator.EstimateMoving(clone, possibility.from, possibility.to)
								+
								estimator.EstimateBuilding(clone, possibility.build);

							if (finalEstimation != null)
							{
								if (clone.OnTurn == No) finalEstimation = Math.Max((float)finalEstimation, estimation);
								else finalEstimation = Math.Min((float)finalEstimation, estimation);
							}
							else finalEstimation = estimation;
						}

					}
					else
					{
						state.FromString(entry.node.Key);

						foreach (var child in entry.node.EnumerateChildren())
						{
							if (finalEstimation != null)
							{
								if (clone.OnTurn == No) finalEstimation = Math.Max((float)finalEstimation, child.Value);
								else finalEstimation = Math.Min((float)finalEstimation, child.Value);
							}
							else finalEstimation = child.Value;
						}
					}

					entry.node.Value = (float)finalEstimation;
				}

				Pool<BoardState>.Destroy(clone);
				Pool<BoardState>.Destroy(state);

				foreach (var child in dag.Root.EnumerateChildren())
				{
					if (child.Value >= dag.Root.Value)
					{
						var bestMove = dag.Root.GetWeight(child.Key);
						select = bestMove.FromPosition;
						move = bestMove.ToPosition;
						build = bestMove.BuildOn;
						return;
					}
				}
			});
		}

		public override async Task<(char, int)> SelectFigure((char row, int col) p1, (char row, int col) p2)
		{
			if (IsAutoPlaying) return await base.SelectFigure(p1, p2);
			if (select != p1 && select != p2) throw new Exception("MINIMAX SELECT ERROR");
			return select;
		}

		public override async Task<(char, int)> MoveFigure((char row, int col) playerPosition, List<(char row, int col)> allowedMovements)
		{
			if (IsAutoPlaying) return await base.MoveFigure(playerPosition, allowedMovements);
			if (!allowedMovements.Contains(move)) throw new Exception("MINIMAX MOVE ERROR");
			return move;
		}

		public override async Task<(char, int)> BuildOn((char row, int col) playerPosition, List<(char row, int col)> allowedBuildings)
		{
			if (IsAutoPlaying) return await base.BuildOn(playerPosition, allowedBuildings);
			if (!allowedBuildings.Contains(build)) throw new Exception("MINIMAX BUILD ERROR");
			return build;
		}
	}
}
