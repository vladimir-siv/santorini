using System.Collections.Generic;
using UnityEngine;

namespace etf.santorini.sv150155d.logic
{
	using game;
	using players;
	using System;
	using util;

	public sealed class Board : MonoBehaviour
	{
		public event Action<Player> TurnPrepared;
		public event Action<Player, (char row, int col)> FigurePlaced;
		public event Action<Player, (char row, int col), (char row, int col)> FigureMoved;
		public event Action<(char row, int col)> BuildingBuilt;

		public Field this[char row, int col] => gameObject.GetObjectChildren().GetChild((row - 'A') * 5 + col - 1).GetComponent<Field>();

		public List<(char row, int col)> FindAdjacentFields((char row, int col) position, bool constrainLevels = false, bool constrainBlockedOrFilled = false, bool constrainSelf = true)
		{
			return BoardProxy.Reference.FindAdjacentFields(position, constrainLevels, constrainBlockedOrFilled, constrainSelf);
		}

		public ((char, int), (char, int)) FindFieldsWithPlayer(Player player)
		{
			return BoardProxy.Reference.FindFieldsWithPlayer(player);
		}

		public void PrepareTurn(Player onTurn)
		{
			TurnPrepared?.Invoke(onTurn);
		}

		public void PlaceFigure(char row, int col, GameObject playerObject, Player player)
		{
			this[row, col].PlaceFigure(playerObject, player);
			FigurePlaced?.Invoke(player, (row, col));
		}

		public bool MoveFigure((char row, int col) from, (char row, int col) to)
		{
			Player player = this[from.row, from.col].Standing;
			bool success = this[from.row, from.col] > this[to.row, to.col];
			FigureMoved?.Invoke(player, from, to);
			return success;
		}

		public void Build(char row, int col)
		{
			this[row, col].Build();
			BuildingBuilt?.Invoke((row, col));
		}
	}
}
