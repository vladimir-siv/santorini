using UnityEngine;

namespace etf.santorini.sv150155d.game
{
	public sealed class Building : MonoBehaviour
	{
		public const int TILES_COUNT = 3;
		private readonly Color[] tileColors = new Color[TILES_COUNT] { Color.cyan, Color.yellow, Color.green };
		private readonly Color domeColor = Color.red;

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
