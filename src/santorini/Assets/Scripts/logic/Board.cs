using System.Collections.Generic;
using UnityEngine;

public sealed class Board : MonoBehaviour
{
	public Field this[char row, int col] => gameObject.GetObjectChildren().GetChild((row - 'A') * 5 + col - 1).GetComponent<Field>();

	public List<Field> FindAdjacentFields((char row, int col) position, bool constrainLevels = false, bool constrainBlockedOrFilled = false, bool constrainSelf = true)
	{
		var currentField = this[position.row, position.col];
		var adjacentFields = new List<Field>(8);

		for (int i = -1; i <= 1; ++i)
		{
			for (int j = -1; j <= 1; ++j)
			{
				var row = (char)(position.row + i);
				var col = position.col + j;

				if (i != 0 || j != 0)
				{
					if (('A' <= row && row <= 'E') && (1 <= col && col <= 5))
					{
						var adjacentField = this[row, col];

						if (!constrainLevels || currentField.Level + 1 >= adjacentField.Level)
						{
							if (!constrainBlockedOrFilled || (!adjacentField.IsBlocked && adjacentField.Standing == null))
							{
								adjacentFields.Add(adjacentField);
							}
						}
					}
				}
				else if (!constrainSelf) adjacentFields.Add(this[row, col]);
			}
		}

		return adjacentFields;
	}

	public ((char, int), (char, int)) FindFieldsWithPlayer(Player player)
	{
		// Can be optimized!
		((char, int) p1, (char, int) p2) positions = (('A', 1), ('A', 1));
		bool firstFind = true;

		for (char row = 'A'; row <= 'E'; ++row)
		{
			for (int col = 1; col <= 5; ++col)
			{
				var field = this[row, col];
				if (field.Standing == player)
				{
					if (firstFind)
					{
						positions.p1 = (row, col);
						firstFind = false;
					}
					else
					{
						positions.p2 = (row, col);
						break;
					}
				}
			}
		}

		return positions;
	}

	public void PlaceFigure(char row, int col, GameObject playerObject, Player player)
	{
		this[row, col].PlaceFigure(playerObject, player);
	}

	public bool MoveFigure((char row, int col) from, (char row, int col) to)
	{
		return this[from.row, from.col] > this[to.row, to.col];
	}

	public void Build(char row, int col)
	{
		this[row, col].Build();
	}
}
