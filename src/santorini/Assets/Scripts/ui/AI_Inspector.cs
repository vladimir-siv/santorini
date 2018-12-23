using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace etf.santorini.sv150155d.ui
{
	using collections;
	using game;
	using players;
	using ai;

	public class AI_Inspector : EditorWindow
	{
		Vector2 scrollPos;

		private GameController controller = null;

		private int? no = null;
		private bool? alphabeta = null;
		private bool? optimized = null;
		private int? level = null;
		private IEstimator estimator = null;
		private float? threshold = null;

		private (float? estimation, string move)? chosenMove = null;
		private List<(float? estimation, string move)> nextMoves = new List<(float? estimation, string move)>();

		[MenuItem("Santorini/AI Inspector")]
		public static void Init()
		{
			AI_Inspector window = (AI_Inspector)EditorWindow.GetWindow(typeof(AI_Inspector));
			window.minSize = new Vector2(300, 500);
			window.maxSize = new Vector2(300, 500);
			window.Show();
			window.Refresh();
			if (window.controller != null)
			{
				window.controller.OnWaitSpace -= window.OnSpace;
				window.controller.OnWaitSpace += window.OnSpace;
			}
		}

		private void OnSpace()
		{
			Refresh();
			Repaint();
		}
		
		void OnGUI()
		{
			GUILayout.Label("    MiniMax Player Info", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("OnTurn:\t\t" + (no?.ToString() ?? "None"));
			EditorGUILayout.LabelField("Alpha-Beta:\t" + alphabeta);
			EditorGUILayout.LabelField("Optimized:\t\t" + optimized);
			EditorGUILayout.LabelField("Level:\t\t" + level);
			EditorGUILayout.LabelField("Estimator:\t\t" + estimator?.GetType().Name);
			EditorGUILayout.LabelField("Threshold:\t\t" + threshold);

			GUILayout.Label("    MiniMax DAG Inspector", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Estimation:\tFrom -> To -> Build", EditorStyles.boldLabel);

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(300), GUILayout.Height(300));

			if (chosenMove != null) EditorGUILayout.LabelField((chosenMove.Value.estimation?.ToString() ?? "Null") + ":\t\t" + chosenMove.Value.move, EditorStyles.boldLabel);
			for (var i = 0; i < nextMoves.Count; ++i)
			{
				EditorGUILayout.LabelField((nextMoves[i].estimation?.ToString() ?? "Null") + ":\t\t" + nextMoves[i].move);
			}

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Refresh")) Refresh();
		}

		private void Refresh()
		{
			no = null;
			alphabeta = null;
			optimized = null;
			level = null;
			estimator = null;
			threshold = null;

			chosenMove = null;
			nextMoves.Clear();

			controller = GameController.CurrentReference;
			if (controller == null) return;
			
			var onTurnType = controller.OnTurn.GetType();
			if (onTurnType != typeof(MiniMax)) return;

			// Warning: boxing
			var isWaitingOnSpace = (bool)controller.GetType().GetField("IsWaitingOnSpace", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller);
			if (!isWaitingOnSpace) return;

			// Warning: boxing
			no = controller.OnTurn.No;
			alphabeta = (bool)onTurnType.GetField("alphabeta", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller.OnTurn);
			optimized = (bool)onTurnType.GetField("optimized", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller.OnTurn);
			level = (int)onTurnType.GetField("level", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller.OnTurn);
			estimator = (IEstimator)onTurnType.GetField("estimator", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller.OnTurn);
			threshold = estimator.Threshold;

			var dag = (HashDAGraph<string, float?, Move>)onTurnType.GetField("dag", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller.OnTurn);

			// Warning: boxing
			var chosenFrom = ((char row, int col))onTurnType.GetField("select", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller.OnTurn);
			var chosenTo = ((char row, int col))onTurnType.GetField("move", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller.OnTurn);
			var chosenBuild = ((char row, int col))onTurnType.GetField("build", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller.OnTurn);

			var chosenFromStr = string.Empty + chosenFrom.row + chosenFrom.col;
			var chosenToStr = string.Empty + chosenTo.row + chosenTo.col;
			var chosenBuildStr = string.Empty + chosenBuild.row + chosenBuild.col;

			chosenMove = (null, chosenFromStr + " -> " + chosenToStr + " -> " + chosenBuildStr);

			foreach (var child in dag.Root.EnumerateChildren())
			{
				var move = dag.Root.GetWeight(child.Key);

				var from = string.Empty + move.FromPosition.row + move.FromPosition.col;
				var to = string.Empty + move.ToPosition.row + move.ToPosition.col;
				var build = string.Empty + move.BuildOn.row + move.BuildOn.col;

				nextMoves.Add((child.Value, from + " -> " + to + " -> " + build));

				if (move.FromPosition == chosenFrom && move.ToPosition == chosenTo && move.BuildOn == chosenBuild)
				{
					if (chosenMove.Value.estimation == null) chosenMove = (child.Value, chosenMove.Value.move);
					else Debug.LogWarning("Found multiple chosen moves???");
				}
			}

			nextMoves.Sort((v1, v2) =>
			{
				if (v1.estimation == null && v2.estimation == null) return 0;
				if (v1.estimation == null) return 1;
				if (v2.estimation == null) return -1;

				if
				(
					v1.estimation.Value == float.PositiveInfinity && v2.estimation.Value == float.PositiveInfinity
					||
					v1.estimation.Value == float.NegativeInfinity && v2.estimation.Value == float.NegativeInfinity
				)
					return 0;

				if (v1.estimation.Value == float.PositiveInfinity || v2.estimation.Value == float.NegativeInfinity) return -1;
				if (v2.estimation.Value == float.PositiveInfinity || v1.estimation.Value == float.NegativeInfinity) return 1;

				return Math.Sign((float)v2.estimation - (float)v1.estimation);
			});
		}
	}
}
