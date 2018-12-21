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

			var levels = param.Length == 2 ? Convert.ToInt32(param[0]) : 4;
			var children = param.Length == 2 ? Convert.ToInt32(param[1]) : 3;

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

			for (var e = dag.EnumerateLevelOrder(dag.Root.Key); e.IsValid; e.Next())
			{
				Debug.Log(e.Node.Key + ":" + e.Node.Value + ":" + e.Node.Level + "[" + e.Level + "]");
			}
		}
	}
}
