using UnityEngine;

public sealed class GameController : MonoBehaviour
{
	public GameObject player1Object;
	public GameObject player2Object;

	private Board board = null;

	private Player player1 = new Human(1);
	private Player player2 = new Human(2);
	private Player onTurn = null;

	private bool IsInitialized = false;
	private bool IsCalculating = false;
	
	void Awake()
	{
		board = GameObject.FindGameObjectWithTag("Board").GetComponent<Board>();
	}

	async void Start()
	{
		(char row, int col) position = ('A', 1);

		position = await player1.PlaceFigure();
		board.PlaceFigure(position.row, position.col, player1Object, player1);
		
		position = await player1.PlaceFigure();
		board.PlaceFigure(position.row, position.col, player1Object, player1);

		position = await player2.PlaceFigure();
		board.PlaceFigure(position.row, position.col, player2Object, player2);

		position = await player2.PlaceFigure();
		board.PlaceFigure(position.row, position.col, player2Object, player2);

		onTurn = player1;
		IsInitialized = true;
	}

	async void Update()
	{
		if (!IsInitialized || IsCalculating) return;
		IsCalculating = true;
		
		// Can be optimized!
		((char, int) p1, (char, int) p2) positions = board.FindFieldsWithPlayer(onTurn);

		if
		(
			board.FindAdjacentFields(positions.p1, constrainLevels: true, constrainBlockedOrFilled: true, constrainSelf: true).Count == 0
			&&
			board.FindAdjacentFields(positions.p2, constrainLevels: true, constrainBlockedOrFilled: true, constrainSelf: true).Count == 0
		)
		{
			onTurn = onTurn == player1 ? player2 : player1;
			Debug.Log($"Plyer {onTurn.No} won the game by blocking his opponent!");
			IsInitialized = false;
			goto ret;
		}
		
		(char row, int col) playerFrom;
		do playerFrom = await onTurn.SelectFigure(positions.p1, positions.p2); while (board[playerFrom.row, playerFrom.col].Standing != onTurn);
		
		var allowedMovements = board.FindAdjacentFields(playerFrom, constrainLevels: true, constrainBlockedOrFilled: true, constrainSelf: true);
		(char row, int col) moveTo;
		do moveTo = await onTurn.MoveFigure(playerFrom, allowedMovements); while (!allowedMovements.Contains(board[moveTo.row, moveTo.col]) && (playerFrom.row != moveTo.row || playerFrom.col != moveTo.col));
		if (playerFrom.row == moveTo.row && playerFrom.col == moveTo.col) goto ret;
		board.MoveFigure(playerFrom, moveTo);

		if (board[moveTo.row, moveTo.col].Level == Building.TILES_COUNT - 1)
		{
			Debug.Log($"Plyer {onTurn.No} won the game!");
			IsInitialized = false;
			goto ret;
		}

		var allowedBuildings = board.FindAdjacentFields(moveTo, constrainLevels: false, constrainBlockedOrFilled: true, constrainSelf: true);
		(char row, int col) buildOn;
		do buildOn = await onTurn.BuildOn(moveTo, allowedBuildings); while (!allowedBuildings.Contains(board[buildOn.row, buildOn.col]));
		board[buildOn.row, buildOn.col].Build();

		onTurn = onTurn == player1 ? player2 : player1;

	ret:
		IsCalculating = false;
	}
}
