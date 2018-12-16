using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class BoardInteractions : MonoBehaviour
{
	private Board board = null;

	private Camera mainCamera = null;
	private (GameObject obj, Material mat, Color col) lastField = (null, null, default(Color));

	private readonly SemaphoreSlim mutex = new SemaphoreSlim(1, 1);
	private readonly SemaphoreSlim interactor = new SemaphoreSlim(0, 1);
	private bool interact = false;
	private Func<Field, bool> filter = null;
	public (char, int) Position { get; private set; } = ('A', 1);

	void Awake()
	{
		board = GameObject.FindGameObjectWithTag("Board").GetComponent<Board>();
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
	}

	public async Task<(char, int)> StartInteracting(Func<Field, bool> filter = null)
	{
		mutex.Wait();
		this.filter = filter;
		interact = true;

		return await Task.Run
		(
			() =>
			{
				interactor.Wait();

				interact = false;
				this.filter = null;
				mutex.Release();

				return Position;
			}
		);
	}

	public void ShowPossibleOptions(List<Field> adjacentFields)
	{
		for (var i = 0; i < adjacentFields.Count; ++i)
		{
			var obj = adjacentFields[i].ActiveObject;
			obj.GetComponent<Renderer>().material.SetColor("_Color", new Color(.102f, .373f, .576f));
		}
	}

	public void ClearPossibleOptions(List<Field> adjacentFields)
	{
		for (var i = 0; i < adjacentFields.Count; ++i)
		{
			var obj = adjacentFields[i].ActiveObject;
			var building = obj.FindObjectBuilding();
			if (building == null) obj.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
			else building.GetComponent<Building>().UpdateGraphics();
		}
	}

	void Update()
	{
		if (!interact) return;

		GameObject field = null;

		if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out var hitObject))
		{
			field = hitObject.transform.gameObject.FindObjectField();

			if (field != null && Input.GetMouseButtonDown(0))
			{
				// Hack :)))
				Position = (field.name[0], field.name[1] - '0');

				interactor.Release();
				field = null;
			}
		}

		if (field == lastField.obj || field == null && lastField.obj == null) return;

		if (lastField.obj != null)
		{
			// Mouse Leave
			lastField.mat.SetColor("_Color", lastField.col);
			lastField = (null, null, default(Color));
		}

		if (field != null)
		{
			// Mouse Enter
			if (!field.GetComponent<Field>().IsBlocked && (filter == null || filter(field.GetComponent<Field>())))
			{
				var obj = field.GetComponent<Field>().ActiveObject;
				var material = obj.GetComponent<Renderer>().material;
				var color = material.GetColor("_Color");
				material.SetColor("_Color", Color.gray);
				lastField = (field, material, color);
			}
		}
	}
}
