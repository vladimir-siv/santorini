using TMPro;
using UnityEngine;

namespace etf.santorini.sv150155d.ui
{
	public static class UI
	{
		private static TextMeshProUGUI Player1UI = GameObject.FindGameObjectWithTag("UI-Player1Text").GetComponent<TextMeshProUGUI>();
		private static TextMeshProUGUI Player2UI = GameObject.FindGameObjectWithTag("UI-Player2Text").GetComponent<TextMeshProUGUI>();
		private static TextMeshProUGUI StatusUI = GameObject.FindGameObjectWithTag("UI-StatusText").GetComponent<TextMeshProUGUI>();
		private static TextMeshProUGUI OutcomeUI = GameObject.FindGameObjectWithTag("UI-OutcomeText").GetComponent<TextMeshProUGUI>();

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
