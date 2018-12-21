using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace etf.santorini.sv150155d.players
{
	using ioc;

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

		public string Serialize()
		{
			var type = GetType();

			StringBuilder sb = new StringBuilder();
			sb.Append(type.FullName);
			sb.Append(';');
			sb.Append(No);

			var initializer = FindInitializer(type);
			foreach (var parameter in initializer.GetParameters())
			{
				sb.Append(';');
				sb.Append(parameter);
				sb.Append('=');
				sb.Append(initializer[parameter]);
			}

			return sb.ToString();
		}

		public static (Type type, int no) Deserialize(string log)
		{
			var split = log.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

			var type = Type.GetType(split[0]);
			var no = Convert.ToInt32(split[1]);

			var initializer = FindInitializer(type);
			for (var i = 2; i < split.Length; ++i)
			{
				var arg = split[i].Split('=');
				if (arg.Length == 2)
				{
					initializer[arg[0]] = arg[1];
				}
			}

			return (type, no);
		}

		public virtual void OnPlayerInitialize(InjectionParser initializer)
		{

		}

		public void CheckAutoPlayer()
		{
			if (IsAutoPlaying && !AutoPlayer.HasMore) AutoPlayer = null;
		}

		public virtual async Task PreparePlacement()
		{
			await AutoPlayer.PreparePlacement();
		}
		public virtual async Task<(char, int)> PlaceFigure1()
		{
			return await AutoPlayer.PlaceFigure1();
		}
		public virtual async Task<(char, int)> PlaceFigure2()
		{
			return await AutoPlayer.PlaceFigure2();
		}
		public virtual async Task PrepareTurn()
		{
			await AutoPlayer.PrepareTurn();
		}
		public virtual async Task<(char, int)> SelectFigure((char row, int col) p1, (char row, int col) p2)
		{
			return await AutoPlayer.SelectFigure(p1, p2);
		}
		public virtual async Task<(char, int)> MoveFigure((char row, int col) playerPosition, List<(char row, int col)> allowedMovements)
		{
			return await AutoPlayer.MoveFigure(playerPosition, allowedMovements);
		}
		public virtual async Task<(char, int)> BuildOn((char row, int col) playerPosition, List<(char row, int col)> allowedBuildings)
		{
			return await AutoPlayer.BuildOn(playerPosition, allowedBuildings);
		}
	}
}
