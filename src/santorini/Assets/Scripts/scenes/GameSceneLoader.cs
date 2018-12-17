namespace etf.santorini.sv150155d.scenes
{
	public class GameSceneLoader : SceneLoader
	{
		public virtual string Player1Class { get; set; } = null;
		public virtual string Player2Class { get; set; } = null;

		public virtual bool LoadGame { get; set; } = false;
		public virtual string SaveGamePath { get; set; } = string.Empty;
	}
}
