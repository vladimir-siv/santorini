using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace etf.santorini.sv150155d.menu
{
	using initialization;
	using ioc;
	using scenes;
	using players;
	using ui;
	using util;

	public class MenuController : MonoBehaviour
	{
		public TMP_InputField inputField;
		public TMP_Dropdown dropdown;
		public Toggle toggle;

		public GameObject mainMenuPnl;
		public GameObject newGameMenuPnl;

		public Button newGameBtn;
		public Button loadGameBtn;
		public Button exitGameBtn;

		public Button startGameBtn;
		public Button backBtn;

		public TMP_Dropdown player1Dropdown;
		public GameObject player1PropPanel;

		public TMP_Dropdown player2Dropdown;
		public GameObject player2PropPanel;

		private InjectionParser initializer1 = null;
		private InjectionParser initializer2 = null;
		private readonly IDictionary<string, string> initialValues1 = new Dictionary<string, string>();
		private readonly IDictionary<string, string> initialValues2 = new Dictionary<string, string>();

		private ICollection<Type> types = null;

		void Awake()
		{
			Global.Initialize();
		}

		void Start()
		{
			types = Player.Types;

			foreach (var type in types)
			{
				player1Dropdown.options.Add(new TypedOptionData(type));
				player2Dropdown.options.Add(new TypedOptionData(type));
			}

			player1Dropdown.onValueChanged.AddListener(Player1DropdownValueChange);
			player2Dropdown.onValueChanged.AddListener(Player2DropdownValueChange);

			player1Dropdown.value = 0;
			player2Dropdown.value = 0;

			Player1DropdownValueChange(0);
			Player2DropdownValueChange(0);

			player1Dropdown.RefreshShownValue();
			player2Dropdown.RefreshShownValue();

			newGameBtn.onClick.AddListener(OnNewGameClick);
			loadGameBtn.onClick.AddListener(OnLoadGameClick);
			exitGameBtn.onClick.AddListener(OnExitGameClick);

			startGameBtn.onClick.AddListener(OnStartGameClick);
			backBtn.onClick.AddListener(OnBackClick);
		}

		private void OnNewGameClick()
		{
			mainMenuPnl.SetActive(false);
			newGameMenuPnl.SetActive(true);
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

		private void OnStartGameClick()
		{
			var player1Type = ((TypedOptionData)player1Dropdown.options[player1Dropdown.value]).Type;
			var player2Type = ((TypedOptionData)player2Dropdown.options[player2Dropdown.value]).Type;

			var loader = new GameSceneLoader
			{
				Player1Class = player1Type.FullName,
				Player2Class = player2Type.FullName,
				Player1No = Config.Player1No,
				Player2No = Config.Player2No
			};
			
			foreach (var parameter in initialValues1) initializer1[parameter.Key] = parameter.Value;
			foreach (var parameter in initialValues2) initializer2[parameter.Key] = parameter.Value;

			SceneLocalCache.AddLoader("game", loader);
			SceneManager.LoadScene("game");
		}

		private void OnBackClick()
		{
			newGameMenuPnl.SetActive(false);
			mainMenuPnl.SetActive(true);
		}

		private void Player1DropdownValueChange(int index)
		{
			initializer1 = OnDropdownValueChange(player1Dropdown, player1PropPanel, initialValues1, Config.Player1No, index);
		}

		private void Player2DropdownValueChange(int index)
		{
			initializer2 = OnDropdownValueChange(player2Dropdown, player2PropPanel, initialValues2, Config.Player2No, index);
		}

		private InjectionParser OnDropdownValueChange(TMP_Dropdown dropdown, GameObject propPanel, IDictionary<string, string> initialValues, int playerNo, int index)
		{
			foreach (Transform child in propPanel.transform) Destroy(child.gameObject);
			initialValues.Clear();

			InjectionParser initializer = Player.GenerateInitializer(((TypedOptionData)dropdown.options[index]).Type, playerNo.ToString());
			int yposition = 40;

			foreach (var parameter in initializer.GetParameters())
			{
				Selectable i = null;
				var resolvedType = initializer.ResolveType(parameter);

				if (resolvedType == typeof(bool))
				{
					var instance = Instantiate(toggle);
					instance.GetComponentInChildren<Text>().text = parameter;
					initialValues[parameter] = instance.isOn.ToString();
					instance.onValueChanged.AddListener(value => initialValues[parameter] = value.ToString());
					i = instance;
				}
				else if (initializer.ResolveChoices(parameter, out var choices))
				{
					var instance = Instantiate(this.dropdown);
					var defaultInitialValue = choices[0];

					if (choices != null)
					{
						for (var c = 0; c < choices.Length; ++c)
						{
							instance.options.Add(new TypedOptionData(Type.GetType(choices[c])));
						}
					}
					
					instance.value = 0;
					instance.GetComponentInChildren<TextMeshProUGUI>().text = parameter;
					initialValues[parameter] = defaultInitialValue;
					instance.onValueChanged.AddListener(c => initialValues[parameter] = ((TypedOptionData)instance.options[c]).Type.FullName);
					i = instance;
				}
				else
				{
					var instance = Instantiate(inputField);
					instance.placeholder.GetComponent<TextMeshProUGUI>().text = parameter;

					if (resolvedType == typeof(int))
					{
						instance.contentType = TMP_InputField.ContentType.IntegerNumber;
						instance.text = "0";
					}
					if (resolvedType == typeof(float))
					{
						instance.contentType = TMP_InputField.ContentType.DecimalNumber;
						instance.text = "0.0";
					}

					initialValues[parameter] = instance.text;
					instance.onValueChanged.AddListener(value => initialValues[parameter] = value.ToString());
					i = instance;
				}
				
				i.gameObject.transform.SetParent(propPanel.transform, false);
				var position = i.transform.localPosition;
				position.y += yposition;
				i.transform.localPosition = position;
				i.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

				yposition -= 60;
			}

			return initializer;
		}
	}
}
