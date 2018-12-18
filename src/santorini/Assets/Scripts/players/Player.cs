using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace etf.santorini.sv150155d.players
{
	using ioc;
	using game;

	public abstract class Player
	{
		private static readonly IDictionary<Type, InjectionParser> PlayerInitializers = new Dictionary<Type, InjectionParser>();
		public static ICollection<Type> Types => PlayerInitializers.Keys;

		public static void MapInitializer(Type type, InjectionParser initializer)
		{
			if (PlayerInitializers.ContainsKey(type)) PlayerInitializers[type] = initializer;
			else PlayerInitializers.Add(type, initializer);
		}
		public static void MapInitializer<T>(InjectionParser initializer)
		{
			MapInitializer(typeof(T), initializer);
		}
		public static InjectionParser FindInitializer(Type type)
		{
			if (PlayerInitializers.ContainsKey(type)) return PlayerInitializers[type];
			else return null;
		}
		
		public virtual string Description => "Player";

		public virtual int No { get; protected set; } = 0;
		private AutoPlayer AutoPlayer { get; set; } = null;
		public bool IsAutoPlaying => AutoPlayer != null;

		protected Player(int No) : this(No, null)
		{

		}

		protected Player(int No, AutoPlayer autoplayer)
		{
			this.No = No;
			this.AutoPlayer = autoplayer;

			if (PlayerInitializers.ContainsKey(this.GetType()))
			{
				OnPlayerInitialize(PlayerInitializers[this.GetType()]);
			}
		}

		public virtual void OnPlayerInitialize(InjectionParser initializer)
		{

		}

		public virtual async Task<(char, int)> PlaceFigure()
		{
			return await AutoPlayer.PlaceFigure();
		}
		public void CheckAutoPlayer()
		{
			if (IsAutoPlaying && !AutoPlayer.HasMore) AutoPlayer = null;
		}
		public virtual async Task PrepareTurn()
		{
			await AutoPlayer.PrepareTurn();
		}
		public virtual async Task<(char, int)> SelectFigure((char row, int col) p1, (char row, int col) p2)
		{
			return await AutoPlayer.SelectFigure(p1, p2);
		}
		public virtual async Task<(char, int)> MoveFigure((char row, int col) playerPosition, List<Field> allowedMovements)
		{
			return await AutoPlayer.MoveFigure(playerPosition, allowedMovements);
		}
		public virtual async Task<(char, int)> BuildOn((char row, int col) playerPosition, List<Field> allowedBuildings)
		{
			return await AutoPlayer.BuildOn(playerPosition, allowedBuildings);
		}
	}
}
