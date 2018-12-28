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
		private readonly GameController controller = GameController.CurrentReference;
		private BoardProxy board = BoardProxy.Reference;
		
		private Random rnd = new Random();

		private bool alphabeta = false;
		private bool optimized = false;
		private int level = 0;
		private IEstimator estimator = null;
		
		private HashDAGraph<string, float?, Move> dag = new HashDAGraph<string, float?, Move>();
		private List<string> lastLevel = new List<string>();
		private List<string> clone = new List<string>();
		private Stack<(HashCollection<string, float?, Move>.Node node, IEnumerator<HashCollection<string, float?, Move>.Node> i, float alpha, float beta)> stack = new Stack<(HashCollection<string, float?, Move>.Node node, IEnumerator<HashCollection<string, float?, Move>.Node> i, float alpha, float beta)>();
		
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
			var clone = Pool<BoardState>.Create();

			var me = this;
			var opponent = GameController.CurrentReference.Opponent(this);
			var onTurn = 0;
			var hadOne = false;

			((string hashkey, (char row, int col) from, (char row, int col) to, (char row, int col) build) possibility, float estimation)? bestMove = null;

			var enumerator = dag.EnumerateLevelOrder(boardState);
			for (var i = enumerator.Level; i <= levels - 2 && enumerator.IsValid; enumerator.Next(), i = enumerator.Level)
			{
				if (enumerator.Node.HashChildren) continue;

				state.FromString(enumerator.Node.Key);

				if (state.HasPlayerStandingOnLastLevel) continue;

				if (optimized)
				{
					clone.FromString(enumerator.Node.Key);
					onTurn = BoardState.FastExtractOnTurn(enumerator.Node.Key);
					hadOne = false;
					bestMove = null;
				}

				foreach (var possibility in state.PossibleOutcomes())
				{
					var move = new Move(possibility.from, possibility.to, possibility.build);

					if (optimized && onTurn == No)
					{
						var estimation = estimator.EstimateMove(me, opponent, clone, move);
						if (estimation < estimator.Threshold)
						{
							if (!hadOne && (bestMove == null || estimation > bestMove.Value.estimation))
							{
								bestMove = (possibility, estimation);
							}

							continue;
						}
					}
					
					enumerator.Node.AddChild(possibility.hashkey, move);
					
					if (i == levels - 2)
					{
						lastLevel.Add(possibility.hashkey);
					}

					hadOne = true;
				}

				if (!hadOne)
				{
					var possibility = bestMove.Value.possibility;
					var move = new Move(possibility.from, possibility.to, possibility.build);

					enumerator.Node.AddChild(possibility.hashkey, move);

					if (i == levels - 2)
					{
						lastLevel.Add(possibility.hashkey);
					}
				}
			}

			Pool<BoardState>.Destroy(clone);
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
			estimator.Threshold = initializer.Resolve<float>("threshold");

			if (level < 3) level = 3;
		}

		public override async Task PreparePlacement()
		{
			if (IsAutoPlaying) await base.PrepareTurn();
			else await Task.CompletedTask;
		}
		
		private (char, int) GenerateRandomField()
		{
			var field = rnd.Next(9);
			char row = (char)('B' + field / 3);
			int col = 2 + field % 3;

			if (rnd.Next(100) < 10) row = 'A';
			else if (rnd.Next(100) < 10) row = 'E';

			if (rnd.Next(100) < 10) col = 1;
			else if (rnd.Next(100) < 10) col = 5;

			return (row, col);
		}

		public override async Task<(char, int)> PlaceFigure1()
		{
			if (IsAutoPlaying) return await base.PlaceFigure1();
			return GenerateRandomField();
		}

		public override async Task<(char, int)> PlaceFigure2()
		{
			if (IsAutoPlaying) return await base.PlaceFigure2();
			return GenerateRandomField();
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
				
				var chosenMove = new Move();

				if (dag.Root.ChildrenCount > 1)
				{
					float extractValue(int onTurn, float? nodeValue, float? childValue)
					{
						if (nodeValue != null)
						{
							if (onTurn == No) return Math.Max((float)nodeValue, (float)childValue);
							else return Math.Min((float)nodeValue, (float)childValue);
						}
						else return (float)childValue;
					}

					stack.Clear();

					var state = Pool<BoardState>.Create();
					var clone = Pool<BoardState>.Create();

					var me = this;
					var opponent = GameController.CurrentReference.Opponent(this);

					HashCollection<string, float?, Move>.Node node = dag.Root;
					IEnumerator<HashCollection<string, float?, Move>.Node> i = null;
					float alpha = float.NegativeInfinity;
					float beta = float.PositiveInfinity;
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
							stack.Push((node, i, alpha, beta));

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
							(node, i, alpha, beta) = stack.Pop();
							
							var onTurn = BoardState.FastExtractOnTurn(node.Key);

							if (node.HashChildren)
							{
								var oldValue = node.Value;
								node.Value = extractValue(onTurn, node.Value, i.Current.Value);

								if (node == dag.Root && oldValue != node.Value)
								{
									chosenMove = dag.Root.GetWeight(i.Current.Key);
								}

								if (alphabeta)
								{
									if (onTurn == No) alpha = (float)node.Value;
									else beta = (float)node.Value;
								}

								// node = next;
								if ((onTurn == No && node.Value == float.PositiveInfinity || onTurn != No && node.Value == float.NegativeInfinity) || (!i.MoveNext()) || (alphabeta && alpha >= beta)) node = null;
							}
							else
							{
								float? finalEstimation = null;
								var hadMove = false;

								state.FromString(node.Key);
								clone.FromString(node.Key);

								if (!state.HasPlayerStandingOnLastLevel)
								{
									foreach (var possibility in state.PossibleOutcomes())
									{
										var estimation = estimator.EstimateMove(me, opponent, clone, new Move(possibility.from, possibility.to, possibility.build));
										finalEstimation = extractValue(onTurn, finalEstimation, estimation);
										hadMove = true;
									}
								}

								if (hadMove) node.Value = finalEstimation;
								else node.Value = estimator.EstimateFinalState(me, opponent, clone);

								// node = next;
								node = null;
							}
						}
						else break;
					}

					Pool<BoardState>.Destroy(clone);
					Pool<BoardState>.Destroy(state);

					if (optimized) estimator.Threshold = (float)dag.Root.Value;
				}
				else chosenMove = dag.Root.GetWeight(dag.Root.FirstChild);

				select = chosenMove.FromPosition;
				move = chosenMove.ToPosition;
				build = chosenMove.BuildOn;
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
