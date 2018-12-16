using UnityEngine;

public static class GameHierarchy
{
	public static GameObject GetObjectGraphics(this GameObject obj)
	{
		return obj.transform.GetChild(0).gameObject;
	}

	public static GameObject GetObjectPhysics(this GameObject obj)
	{
		return obj.transform.GetChild(1).gameObject;
	}

	public static GameObject GetObjectChildren(this GameObject obj)
	{
		return obj.transform.GetChild(2).gameObject;
	}

	public static GameObject FindObjectField(this GameObject obj)
	{
		for (Transform current = obj.transform; current != null; current = current.parent)
			if (current.gameObject.tag == "Field") return current.gameObject;
		return null;
	}

	public static GameObject FindObjectBuilding(this GameObject obj)
	{
		for (Transform current = obj.transform; current != null; current = current.parent)
			if (current.gameObject.tag == "Building") return current.gameObject;
		return null;
	}

	public static GameObject GetChild(this GameObject obj, int child)
	{
		return obj.transform.GetChild(child).gameObject;
	}

	public static GameObject GetLastChild(this GameObject obj)
	{
		return obj.transform.GetChild(obj.transform.childCount - 1).gameObject;
	}
}
