using UnityEngine;

namespace etf.santorini.sv150155d.game
{
	using players;
	using util;

	public sealed class Field : MonoBehaviour
	{
		public GameObject building;
	
		private Transform objectChildren = null;

		public bool IsBlocked { get; private set; } = false;
		public int Level { get; private set; } = -1;
		public Player Standing { get; private set; } = null;
		public GameObject ActiveObject
		{
			get
			{
				GameObject obj = null;
				var objectChildren = gameObject.GetObjectChildren();
				var childCount = objectChildren.transform.childCount;

				if (childCount == 0)
				{
					var objectGraphics = gameObject.GetObjectGraphics();
					obj = objectGraphics.GetChild(0);
				}
				else
				{
					obj = objectChildren.GetChild(childCount - 1);
					obj = obj.GetObjectGraphics().GetChild(0);
				}

				return obj;
			}
		}

		void Awake()
		{
			objectChildren = transform.GetChild(2);
		}

		private Vector3 delta(bool increment)
		{
			return new Vector3(0, (Level + 1 + (increment ? 1 : 0)) * 0.2f, 0);
		}

		public void PlaceFigure(GameObject playerObject, Player player)
		{
			var obj = Instantiate(playerObject, Vector3.zero, Quaternion.identity, objectChildren);
			obj.transform.localPosition = delta(true);
			Standing = player;
		}

		private static bool Move(Field from, Field to)
		{
			if (from.Standing != null)
			{
				var player = from.gameObject.GetObjectChildren().GetLastChild();
				player.transform.parent = to.gameObject.GetObjectChildren().transform;
				player.transform.localPosition = to.delta(true);
				to.Standing = from.Standing;
				from.Standing = null;
				return true;
			}

			return false;
		}
		public static bool operator >(Field from, Field to)
		{
			return Move(from, to);
		}
		public static bool operator <(Field to, Field from)
		{
			return Move(from, to);
		}

		public void Build()
		{
			if (Level + 1 < Building.BuildingCount)
			{
				var obj = Instantiate(building, Vector3.zero, Quaternion.identity, objectChildren);
				var builtBuilding = obj.GetComponent<Building>();
				builtBuilding.Level = ++Level;
				obj.transform.localPosition = delta(false);
				IsBlocked = builtBuilding.IsBlocking;
			}
		}
	}
}
