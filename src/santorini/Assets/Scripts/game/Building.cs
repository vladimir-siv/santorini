using UnityEngine;

namespace etf.santorini.sv150155d.game
{
	public sealed class Building : MonoBehaviour
	{
		public const int TILES_COUNT = 3;
		private readonly Color[] tileColors = new Color[TILES_COUNT] { new Color(.231f, .698f, .839f), new Color(.776f, .776f, .224f), new Color(.188f, .627f, .157f) };
		private readonly Color domeColor = new Color(.627f, .165f, .157f);

		public static int BuildingCount => TILES_COUNT + 1;

		public Mesh tileMesh;
		public Mesh domeMesh;

		private GameObject building;

		public bool IsBlocking { get; private set; } = false;

		private int level = 0;
		public int Level
		{
			get => level;
			set
			{
				level = value;
				IsBlocking = level >= TILES_COUNT;
				UpdateGraphics();
			}
		}

		void Awake()
		{
			var objectGraphics = transform.GetChild(0);
			building = objectGraphics.GetChild(0).gameObject;
			UpdateGraphics();
		}

		public void UpdateGraphics()
		{
			if (Level < TILES_COUNT)
			{
				building.GetComponent<MeshFilter>().mesh = tileMesh;
				building.GetComponent<Renderer>().material.SetColor("_Color", tileColors[level]);
			}
			else
			{
				building.GetComponent<MeshFilter>().mesh = domeMesh;
				building.GetComponent<Renderer>().material.SetColor("_Color", domeColor);
			}
		}
	}
}
