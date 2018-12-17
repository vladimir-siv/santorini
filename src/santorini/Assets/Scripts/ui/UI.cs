using TMPro;
using UnityEngine;

public static class UI
{
	private static TextMeshProUGUI StatusUI = GameObject.FindGameObjectWithTag("UI-StatusText").GetComponent<TextMeshProUGUI>();
	private static TextMeshProUGUI OutcomeUI = GameObject.FindGameObjectWithTag("UI-OutcomeText").GetComponent<TextMeshProUGUI>();

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
