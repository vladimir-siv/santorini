using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Human : Player
{
	public int No { get; private set; } = 0;

	public Human(int No)
	{
		this.No = No;
	}
	
	public async Task<(char, int)> PlaceFigure()
	{
		return await GameObject.FindGameObjectWithTag("Board").GetComponent<BoardInteractions>().StartInteracting(field => field.Standing == null);
	}
	
	public async Task<(char, int)> SelectFigure((char row, int col) p1, (char row, int col) p2)
	{
		return await GameObject.FindGameObjectWithTag("Board").GetComponent<BoardInteractions>().StartInteracting(field => field.Standing == this);
	}
	
	public async Task<(char, int)> MoveFigure((char row, int col) playerPosition, List<Field> allowedMovements)
	{
		var boardObject = GameObject.FindGameObjectWithTag("Board");
		var board = boardObject.GetComponent<Board>();
		var interactions = boardObject.GetComponent<BoardInteractions>();
		interactions.ShowPossibleOptions(allowedMovements);
		var interaction = await boardObject.GetComponent<BoardInteractions>().StartInteracting(field => board.FindAdjacentFields(playerPosition, constrainLevels: true, constrainBlockedOrFilled: true, constrainSelf: false).Contains(field));
		interactions.ClearPossibleOptions(allowedMovements);
		return interaction;
	}
	
	public async Task<(char, int)> BuildOn((char row, int col) playerPosition, List<Field> allowedBuildings)
	{
		var boardObject = GameObject.FindGameObjectWithTag("Board");
		var board = boardObject.GetComponent<Board>();
		var interactions = boardObject.GetComponent<BoardInteractions>();
		interactions.ShowPossibleOptions(allowedBuildings);
		var interaction = await GameObject.FindGameObjectWithTag("Board").GetComponent<BoardInteractions>().StartInteracting(field => field.Standing == null && board.FindAdjacentFields(playerPosition).Contains(field));
		interactions.ClearPossibleOptions(allowedBuildings);
		return interaction;
	}
}
