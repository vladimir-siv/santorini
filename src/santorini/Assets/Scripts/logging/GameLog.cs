using System.Collections.Generic;
using System.IO;

namespace etf.santorini.sv150155d.logging
{
	public sealed class GameLog
	{
		private readonly List<string> log = new List<string>();

		public string[] Present => log.ToArray();

		public void Players(string player1, string player2)
		{
			log.Add(player1);
			log.Add(player2);
		}

		public void Placement((char row, int col) figure1, (char row, int col) figure2)
		{
			log.Add(string.Empty + figure1.row + figure1.col + " " + figure2.row + figure2.col);
		}

		public void Turn((char row, int col) from, (char row, int col) to, (char row, int col) build)
		{
			log.Add(string.Empty + from.row + from.col + " " + to.row + to.col + " " + build.row + build.col);
		}

		public void Save(string path)
		{
			File.WriteAllLines(path, log);
		}

		public void Load(string path)
		{
			var lines = File.ReadAllLines(path);

			for (var i = 0; i < lines.Length; ++i)
			{
				log.Add(lines[i]);
			}
		}
	}
}
