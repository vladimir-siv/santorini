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
		
		private HashDAGraph<string, float?, Move> dag = new HashDAGraph<string, float?, Move>();
		private List<string> lastLevel = new List<string>();
		private List<string> clone = new List<string>();
		private Stack<(HashCollection<string, float?, Move>.Node node, IEnumerator<HashCollection<string, float?, Move>.Node> i)> stack = new Stack<(HashCollection<string, float?, Move>.Node node, IEnumerator<HashCollection<string, float?, Move>.Node> i)>();
		
		private (char row, int col) select;
		private (char row, int col) move;
		private (char row, int col) build;

		public override string Description => "MiniMax";

		public MiniMax(int No) : base(No) { }
		public MiniMax(int No, AutoPlayer autoplayer) : base(No, autoplayer) { }

		private void Generate(string boardState, int levels)
		{
			if (levels < 3) levels = 3;

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
				Generate(clone[i], 3);
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
				float extractValue(string boardState, float? nodeValue, float? childValue)
				{
					if (nodeValue != null)
					{
						// Hack to quickly get BoardState.OnTurn from string
						var onTurn = Convert.ToInt32(boardState.Substring(boardState.LastIndexOf('|') + 1));
						if (onTurn == No) return Math.Max((float)nodeValue, (float)childValue);
						else return Math.Min((float)nodeValue, (float)childValue);
					}
					else return (float)childValue;
				}

				if (dag.Root == null) Sync();
				else Resync();

				stack.Clear();

				var state = Pool<BoardState>.Create();
				var clone = Pool<BoardState>.Create();

				var me = this;
				var opponent = GameController.CurrentReference.Opponent(this);

				HashCollection<string, float?, Move>.Node node = dag.Root;
				IEnumerator<HashCollection<string, float?, Move>.Node> i = null;
				if (node != null)
				{
					node.Value = null;
					i = node.EnumerateChildren().GetEnumerator();
					i.MoveNext();
				}
				while (true)
				{
					while (node != null)
					{
						stack.Push((node, i));

						node = i.Current;
						if (node != null)
						{
							node.Value = null;
							i = node.EnumerateChildren().GetEnumerator();
							i.MoveNext();
						}
					}

					if (stack.Count > 0)
					{
						(node, i) = stack.Pop();

						if (node.HashChildren)
						{
							node.Value = extractValue(node.Key, node.Value, i.Current.Value);

							// node = next;
							if (!i.MoveNext()) node = null;
						}
						else
						{
							float? finalEstimation = null;

							state.FromString(node.Key);
							clone.FromString(node.Key);

							foreach (var possibility in state.PossibleOutcomes())
							{
								var estimation = estimator.EstimateMoving(me, opponent, clone, possibility.from, possibility.to);
								clone.MoveFigure(possibility.from, possibility.to);
								estimation += estimator.EstimateBuilding(me, opponent, clone, possibility.build);
								clone.MoveFigure(possibility.to, possibility.from);
								finalEstimation = extractValue(node.Key, finalEstimation, estimation);
							}

							node.Value = finalEstimation;
							
							// node = next;
							node = null;
						}
					}
					else break;
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
