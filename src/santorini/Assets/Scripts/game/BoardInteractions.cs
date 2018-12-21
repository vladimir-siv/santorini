using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace etf.santorini.sv150155d.game
{
	using logic;
	using util;

	public sealed class BoardInteractions : MonoBehaviour
	{
		private Board board = null;

		private Camera mainCamera = null;
		private (GameObject obj, Material mat, Color col) lastField = (null, null, default(Color));

		private readonly SemaphoreSlim mutex = new SemaphoreSlim(1, 1);
		private readonly SemaphoreSlim interactor = new SemaphoreSlim(0, 1);
		private bool interact = false;
		private Func<(char, int), bool> filter = null;
		public (char, int) Position { get; private set; } = ('A', 1);

		void Awake()
		{
			board = GameObject.FindGameObjectWithTag("Board").GetComponent<Board>();
			mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		}

		public async Task<(char, int)> StartInteracting(Func<(char row, int col), bool> filter = null)
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

		public void ShowPossibleOptions(List<(char row, int col)> adjacentFields)
		{
			for (var i = 0; i < adjacentFields.Count; ++i)
			{
				var field = adjacentFields[i];
				var obj = board[field.row, field.col].ActiveObject;
				obj.GetComponent<Renderer>().material.SetColor("_Color", new Color(.102f, .373f, .576f));
			}
		}

		public void ClearPossibleOptions(List<(char row, int col)> adjacentFields)
		{
			for (var i = 0; i < adjacentFields.Count; ++i)
			{
				var field = adjacentFields[i];
				var obj = board[field.row, field.col].ActiveObject;
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

				if (field != null)
				{
					// Hack :)))
					Position = (field.name[0], field.name[1] - '0');

					if (Input.GetMouseButtonDown(0))
					{
						interactor.Release();
						field = null;
					}
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
				if (!field.GetComponent<Field>().IsBlocked && (filter == null || filter(Position)))
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
}
