using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace etf.santorini.sv150155d.players
{
	using game;
	using logic;

	public class Human : Player
	{
		public override string Description => "Human";

		public Human(int No) : base(No) { }
		public Human(int No, AutoPlayer autoplayer) : base(No, autoplayer) { }
		
		public override async Task<(char, int)> PlaceFigure()
		{
			if (IsAutoPlaying) return await base.PlaceFigure();
			return await GameObject.FindGameObjectWithTag("Board").GetComponent<BoardInteractions>().StartInteracting(field => field.Standing == null);
		}

		public override async Task PrepareTurn()
		{
			if (IsAutoPlaying) await base.PrepareTurn();
			else await Task.CompletedTask;
		}

		public override async Task<(char, int)> SelectFigure((char row, int col) p1, (char row, int col) p2)
		{
			if (IsAutoPlaying) return await base.SelectFigure(p1, p2);
			return await GameObject.FindGameObjectWithTag("Board").GetComponent<BoardInteractions>().StartInteracting(field => field.Standing == this);
		}

		public override async Task<(char, int)> MoveFigure((char row, int col) playerPosition, List<Field> allowedMovements)
		{
			if (IsAutoPlaying) return await base.MoveFigure(playerPosition, allowedMovements);
			var boardObject = GameObject.FindGameObjectWithTag("Board");
			var board = boardObject.GetComponent<Board>();
			var interactions = boardObject.GetComponent<BoardInteractions>();
			interactions.ShowPossibleOptions(allowedMovements);
			var interaction = await boardObject.GetComponent<BoardInteractions>().StartInteracting(field => board.FindAdjacentFields(playerPosition, constrainLevels: true, constrainBlockedOrFilled: true, constrainSelf: false).Contains(field));
			interactions.ClearPossibleOptions(allowedMovements);
			return interaction;
		}

		public override async Task<(char, int)> BuildOn((char row, int col) playerPosition, List<Field> allowedBuildings)
		{
			if (IsAutoPlaying) return await base.BuildOn(playerPosition, allowedBuildings);
			var boardObject = GameObject.FindGameObjectWithTag("Board");
			var board = boardObject.GetComponent<Board>();
			var interactions = boardObject.GetComponent<BoardInteractions>();
			interactions.ShowPossibleOptions(allowedBuildings);
			var interaction = await GameObject.FindGameObjectWithTag("Board").GetComponent<BoardInteractions>().StartInteracting(field => field.Standing == null && board.FindAdjacentFields(playerPosition).Contains(field));
			interactions.ClearPossibleOptions(allowedBuildings);
			return interaction;
		}
	}
}
