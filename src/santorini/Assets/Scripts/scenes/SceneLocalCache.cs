using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace etf.santorini.sv150155d.scenes
{
	public static class SceneLocalCache
	{
		private static IDictionary<string, Stack<SceneLoader>> sceneCache = new Dictionary<string, Stack<SceneLoader>>();

		public static void Touch() { }

		static SceneLocalCache()
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
			SceneManager.sceneUnloaded += OnSceneUnloaded;
		}

		private static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
		
		}

		private static void OnSceneUnloaded(Scene scene)
		{
			if (sceneCache.ContainsKey(scene.name))
			{
				sceneCache[scene.name].Clear();
			}
		}

		public static void AddLoader(string sceneName, SceneLoader loader)
		{
			if (!sceneCache.ContainsKey(sceneName))
			{
				sceneCache.Add(sceneName, new Stack<SceneLoader>());
			}

			sceneCache[sceneName].Push(loader);
		}

		public static SceneLoader GetLoader(string sceneName)
		{
			if (!sceneCache.ContainsKey(sceneName)) return null;
			else return sceneCache[sceneName].Count > 0 ? sceneCache[sceneName].Pop() : null;
		}

		public static T GetLoader<T>(string sceneName) where T : class, SceneLoader
		{
			return GetLoader(sceneName) as T;
		}
	}
}
