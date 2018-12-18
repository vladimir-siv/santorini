using System;
using TMPro;
using UnityEngine;

namespace etf.santorini.sv150155d.ui
{
	public sealed class TypedOptionData : TMP_Dropdown.OptionData
	{
		public Type Type { get; } = null;

		public TypedOptionData(Type type) : base(type.Name)
		{
			Type = type;
		}

		public TypedOptionData(Type type, Sprite image) : base(type.Name, image)
		{
			Type = type;
		}
	}
}
