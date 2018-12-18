using TMPro;
using UnityEngine;

namespace etf.santorini.sv150155d.ui
{
	public static class UI
	{
		private static TextMeshProUGUI Player1UI;
		private static TextMeshProUGUI Player2UI;
		private static TextMeshProUGUI StatusUI;
		private static TextMeshProUGUI OutcomeUI;

		public static void Init()
		{
			Player1UI = GameObject.FindGameObjectWithTag("UI-Player1Text").GetComponent<TextMeshProUGUI>();
			Player2UI = GameObject.FindGameObjectWithTag("UI-Player2Text").GetComponent<TextMeshProUGUI>();
			StatusUI = GameObject.FindGameObjectWithTag("UI-StatusText").GetComponent<TextMeshProUGUI>();
			OutcomeUI = GameObject.FindGameObjectWithTag("UI-OutcomeText").GetComponent<TextMeshProUGUI>();
		}

		public static string Player1
		{
			get => Player1UI.text;
			set => Player1UI.text = value;
		}

		public static string Player2
		{
			get => Player2UI.text;
			set => Player2UI.text = value;
		}

		public static string Status
		{
			get => StatusUI.text;
			set => StatusUI.text = value;
		}

		public static string Outcome
		{
			get => OutcomeUI.text;
			set => OutcomeUI.text = value;
		}
	}
}
