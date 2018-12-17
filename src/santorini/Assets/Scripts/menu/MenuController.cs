using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace etf.santorini.sv150155d.menu
{
	using scenes;
	using util;

	public class MenuController : MonoBehaviour
	{
		public Button startGameBtn;
		public Button loadGameBtn;
		public Button exitGameBtn;

		void Start()
		{
			startGameBtn.onClick.AddListener(OnStartGameClick);
			loadGameBtn.onClick.AddListener(OnLoadGameClick);
			exitGameBtn.onClick.AddListener(OnExitGameClick);
		}

		private void OnStartGameClick()
		{
			var loader = new GameSceneLoader
			{
				Player1Class = "etf.santorini.sv150155d.players.Human",
				Player2Class = "etf.santorini.sv150155d.players.Human"
			};

			SceneLocalCache.AddLoader("game", loader);
			SceneManager.LoadScene("game");
		}

		private void OnLoadGameClick()
		{
			if (!File.Exists(Config.SAVE_GAME_FILE)) return;

			var loader = new GameSceneLoader
			{
				LoadGame = true,
				SaveGamePath = Config.SAVE_GAME_FILE
			};

			SceneLocalCache.AddLoader("game", loader);
			SceneManager.LoadScene("game");
		}

		private void OnExitGameClick()
		{
			Application.Quit();
		}
	}
}
