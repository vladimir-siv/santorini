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

	public async Task<(char, int)> SelectPlayer()
	{
		return await GameObject.FindGameObjectWithTag("Board").GetComponent<BoardInteractions>().StartInteracting(field => field.Standing == this);
	}

	public async Task<(char, int)> MovePlayer((char row, int col) playerPosition)
	{
		var board = GameObject.FindGameObjectWithTag("Board").GetComponent<Board>();
		return await GameObject.FindGameObjectWithTag("Board").GetComponent<BoardInteractions>().StartInteracting(field => board.FindAdjacentFields(playerPosition, constrainLevels: true, constrainBlockedOrFilled: true, constrainSelf: false).Contains(field));
	}

	public async Task<(char, int)> BuildOn((char row, int col) playerPosition)
	{
		var board = GameObject.FindGameObjectWithTag("Board").GetComponent<Board>();
		return await GameObject.FindGameObjectWithTag("Board").GetComponent<BoardInteractions>().StartInteracting(field => field.Standing == null && board.FindAdjacentFields(playerPosition).Contains(field));
	}
}
