using System.Threading.Tasks;

public interface Player
{
    int No { get; }

	Task<(char, int)> PlaceFigure();
	Task<(char, int)> SelectPlayer();
	Task<(char, int)> MovePlayer((char row, int col) playerPosition);
	Task<(char, int)> BuildOn((char row, int col) playerPosition);
}
