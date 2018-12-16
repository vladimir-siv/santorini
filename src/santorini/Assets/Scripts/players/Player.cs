using System.Collections.Generic;
using System.Threading.Tasks;

public interface Player
{
    int No { get; }
	
	Task<(char, int)> PlaceFigure();
	Task<(char, int)> SelectFigure((char row, int col) p1, (char row, int col) p2);
	Task<(char, int)> MoveFigure((char row, int col) playerPosition, List<Field> allowedMovements);
	Task<(char, int)> BuildOn((char row, int col) playerPosition, List<Field> allowedBuildings);
}
