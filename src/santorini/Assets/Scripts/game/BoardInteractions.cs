using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class BoardInteractions : MonoBehaviour
{
	private Camera mainCamera = null;
	private (GameObject obj, Material mat, Color col) lastField = (null, null, default(Color));

	private SemaphoreSlim mutex = new SemaphoreSlim(1, 1);
	private SemaphoreSlim interactor = new SemaphoreSlim(0, 1);
	private bool interact = false;
	private Func<Field, bool> filter = null;
	public (char, int) Position { get; private set; } = ('A', 1);

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

	void Awake()
	{
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
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
				GameObject obj = null;
				var objectChildren = field.GetObjectChildren();
				var childCount = objectChildren.transform.childCount;

				if (childCount == 0)
				{
					var objectGraphics = field.GetObjectGraphics();
					obj = objectGraphics.GetChild(0);
				}
				else
				{
					obj = objectChildren.GetChild(childCount - 1);
					obj = obj.GetObjectGraphics().GetChild(0);
				}

				var material = obj.GetComponent<Renderer>().material;
				var color = material.GetColor("_Color");
				material.SetColor("_Color", Color.gray);
				lastField = (field, material, color);
			}
		}
	}
}
