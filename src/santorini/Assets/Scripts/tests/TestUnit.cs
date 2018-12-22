using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace etf.santorini.sv150155d.tests
{
	using collections;

	public class TestUnit : MonoBehaviour
	{
		public TMP_InputField parameters;
		public Button runner;

		void Awake()
		{
			runner.onClick.AddListener(TestDAGraph);
		}

		void TestDAGraph()
		{
			var param = parameters.text.Split(':');

			var levels = param.Length > 0 ? Convert.ToInt32(param[0]) : 4;
			var children = param.Length > 1 ? Convert.ToInt32(param[1]) : 3;
			var newRoot = param.Length > 2 ? Convert.ToChar(param[2]) : 'A';

			char current = 'A';
			var turned = false;
			var dag = new HashDAGraph<string, float, float>();

			dag.Init(current.ToString(), 1);

			var enumerator = dag.EnumerateLevelOrder(dag.Root.Key);

			for (var i = enumerator.Level; i < levels - 1; enumerator.Next(), i = enumerator.Level)
			{
				if (enumerator.Node.HashChildren)
				{
					if (turned) break;
					else continue;
				}

				for (var j = 0; j < children; ++j)
				{
					if (++current > 'Z')
					{
						current = 'A';
						turned = true;
					}

					enumerator.Node.AddChild(current.ToString(), j);
				}
			}

			if (newRoot != 'A')
			{
				Debug.Log("Before: " + dag.Count);
				dag.Root = dag[newRoot.ToString()];
				Debug.Log("After: " + dag.Count);
			}

			for (var e = dag.EnumerateLevelOrder(dag.Root.Key); e.IsValid; e.Next())
			{
				Debug.Log(e.Node.Key + ":" + e.Node.Value + ":" + e.Node.Level + "[" + e.Level + "]");
			}
			
			Debug.Log("Separator");

			var extend = dag.Root.EnumerateChildren().GetEnumerator();
			Debug.Log(extend.Current == null ? "Null!" : "???" + extend.Current.Key);

			while (extend.MoveNext())
			{
				Debug.Log(extend.Current.Key);
			}

			Debug.Log(extend.Current == null ? "Null!" : "???" + extend.Current.Key);

			extend.MoveNext();

			Debug.Log(extend.Current == null ? "Null!" : "???" + extend.Current.Key);
		}
	}
}
