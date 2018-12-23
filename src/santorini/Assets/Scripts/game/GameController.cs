using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace etf.santorini.sv150155d.game
{
	using scenes;
	using logic;
	using players;
	using ui;
	using logging;
	using util;

	public sealed class GameController : MonoBehaviour
	{
		public static GameController CurrentReference { get; private set; } = null;

		public GameObject player1Object;
		public GameObject player2Object;

		private readonly GameLog gameLog = new GameLog();

		private Board board = null;

		private Player player1 = null;
		private Player player2 = null;
		private Player onTurn = null;

		private bool IsInitialized = false;
		private bool IsCalculating = false;

		public Player FirstPlayer => player1;
		public Player SecondPlayer => player2;
		public Player OnTurn => onTurn;

		public Player Opponent(Player player)
		{
			if (player == player1) return player2;
			else return player1;
		}
		
		void Awake()
		{
			CurrentReference = this;
			SceneManager.sceneUnloaded += scene => CurrentReference = scene.name == "game" ? null : CurrentReference;
			board = GameObject.FindGameObjectWithTag("Board").GetComponent<Board>();
		}

		void Start()
		{
			var loader = SceneLocalCache.GetLoader<GameSceneLoader>("game");

			if (loader == null)
			{
				UI.Status = "Status:\r\nUnable to load scene properly";
				return;
			}

			UI.Init();

			if (!loader.LoadGame)
			{
				// Warning: boxing
				player1 = (Player)Activator.CreateInstance(Type.GetType(loader.Player1Class), loader.Player1No);
				player2 = (Player)Activator.CreateInstance(Type.GetType(loader.Player2Class), loader.Player2No);
				gameLog.Players(player1.Serialize(), player2.Serialize());

				StartGame();
			}
			else LoadGame(loader);
		}

		async void StartGame()
		{
			BoardProxy.Reference.Set(board);

			UI.Player1 += '[' + player1.Description + ']';
			UI.Player2 += '[' + player2.Description + ']';

			onTurn = player1;
			await player1.PreparePlacement();

			UI.Status = $"Status:\r\nPlayer{player1.No} placing 1. figure";
			(char row, int col) position11;
			do position11 = await player1.PlaceFigure1(); while (board[position11.row, position11.col].Standing != null);
			board.PlaceFigure(position11.row, position11.col, player1Object, player1);

			UI.Status = $"Status:\r\nPlayer{player1.No} placing 2. figure";
			(char row, int col) position12;
			do position12 = await player1.PlaceFigure2(); while (board[position12.row, position12.col].Standing != null);
			board.PlaceFigure(position12.row, position12.col, player1Object, player1);

			if (!player1.IsAutoPlaying) gameLog.Placement(position11, position12);

			onTurn = player2;
			await player2.PreparePlacement();

			UI.Status = $"Status:\r\nPlayer{player2.No} placing 1. figure";
			(char row, int col) position21;
			do position21 = await player2.PlaceFigure1(); while (board[position21.row, position21.col].Standing != null);
			board.PlaceFigure(position21.row, position21.col, player2Object, player2);

			UI.Status = $"Status:\r\nPlayer{player2.No} placing 2. figure";
			(char row, int col) position22;
			do position22 = await player2.PlaceFigure2(); while (board[position22.row, position22.col].Standing != null);
			board.PlaceFigure(position22.row, position22.col, player2Object, player2);

			if (!player2.IsAutoPlaying) gameLog.Placement(position21, position22);

			onTurn = player1;
			IsInitialized = true;

			// AI INSPECTOR
			if (player1.GetType() != typeof(Human) && player2.GetType() != typeof(Human)) AI_Inspector.Init();
		}

		void LoadGame(GameSceneLoader loader)
		{
			gameLog.Load(loader.SaveGamePath);
			var autoplayer = new AutoPlayer(gameLog);
			player1 = autoplayer.Player1;
			player2 = autoplayer.Player2;
			StartGame();
		}

		// AI INSPECTOR
		public event Action OnWaitSpace;
		// AI INSPECTOR
		bool trap = true;
		// AI INSPECTOR
		bool IsWaitingOnSpace = false;
		// AI INSPECTOR
		System.Threading.SemaphoreSlim onSpace = new System.Threading.SemaphoreSlim(0, 1);
		async void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (File.Exists(Config.SAVE_GAME_FILE)) File.Delete(Config.SAVE_GAME_FILE);
				if (IsInitialized) gameLog.Save(Config.SAVE_GAME_FILE);
				SceneManager.LoadScene("menu");
				return;
			}

			// AI INSPECTOR
			if (!onTurn.IsAutoPlaying && IsWaitingOnSpace && Input.GetKeyDown(KeyCode.Space))
			{
				onSpace.Release();
				IsWaitingOnSpace = false;
			}

			// AI INSPECTOR
			if (!onTurn.IsAutoPlaying && Input.GetKeyDown(KeyCode.End))
			{
				trap = false;
			}

			if (!IsInitialized || IsCalculating) return;
			IsCalculating = true;

			onTurn.CheckAutoPlayer();
			
			((char, int) p1, (char, int) p2) positions = board.FindFieldsWithPlayer(onTurn);

			bool p1Blocked = board.FindAdjacentFields(positions.p1, constrainLevels: true, constrainBlockedOrFilled: true, constrainSelf: true).Count == 0;
			bool p2Blocked = board.FindAdjacentFields(positions.p2, constrainLevels: true, constrainBlockedOrFilled: true, constrainSelf: true).Count == 0;

			if (p1Blocked && p2Blocked)
			{
				onTurn = onTurn == player1 ? player2 : player1;
				UI.Status = "Status:\r\nGame end";
				UI.Outcome = $"Player{onTurn.No} wins!";
				IsInitialized = false;
				goto ret;
			}

			UI.Status = $"Status:\r\nPlayer{onTurn.No} preparing";
			board.PrepareTurn(onTurn);
			await onTurn.PrepareTurn();

			// AI INSPECTOR
			if (player1.GetType() != typeof(Human) && player2.GetType() != typeof(Human))
			{
				if (!onTurn.IsAutoPlaying && trap && !IsWaitingOnSpace)
				{
					UI.Status = $"Status:\r\nPress space to let Player{onTurn.No} continue";
					var spacePressed = System.Threading.Tasks.Task.Run(onSpace.Wait);

					IsWaitingOnSpace = true;
					OnWaitSpace?.Invoke();

					await spacePressed;
				}
			}

			UI.Status = $"Status:\r\nPlayer{onTurn.No} selecting figure";
			(char row, int col) playerFrom;
			if (p1Blocked) playerFrom = positions.p2;
			else if (p2Blocked) playerFrom = positions.p1;
			else do playerFrom = await onTurn.SelectFigure(positions.p1, positions.p2); while (board[playerFrom.row, playerFrom.col].Standing != onTurn);

			UI.Status = $"Status:\r\nPlayer{onTurn.No} moving figure";
			var allowedMovements = board.FindAdjacentFields(playerFrom, constrainLevels: true, constrainBlockedOrFilled: true, constrainSelf: true);
			(char row, int col) moveTo;
			do moveTo = await onTurn.MoveFigure(playerFrom, allowedMovements); while (!allowedMovements.Contains((moveTo.row, moveTo.col)) && ((p1Blocked || p2Blocked) || (playerFrom.row != moveTo.row || playerFrom.col != moveTo.col)));
			if (playerFrom.row == moveTo.row && playerFrom.col == moveTo.col) goto ret;
			board.MoveFigure(playerFrom, moveTo);

			if (board[moveTo.row, moveTo.col].Level == Building.TILES_COUNT - 1)
			{
				UI.Status = "Status:\r\nGame end";
				UI.Outcome = $"Player{onTurn.No} wins!";
				IsInitialized = false;
				goto ret;
			}

			UI.Status = $"Status:\r\nPlayer{onTurn.No} building";
			var allowedBuildings = board.FindAdjacentFields(moveTo, constrainLevels: false, constrainBlockedOrFilled: true, constrainSelf: true);
			(char row, int col) buildOn;
			do buildOn = await onTurn.BuildOn(moveTo, allowedBuildings); while (!allowedBuildings.Contains((buildOn.row, buildOn.col)));
			board.Build(buildOn.row, buildOn.col);

			if (!onTurn.IsAutoPlaying) gameLog.Turn(playerFrom, moveTo, buildOn);

			onTurn = onTurn == player1 ? player2 : player1;

		ret:
			IsCalculating = false;
		}
	}
}
