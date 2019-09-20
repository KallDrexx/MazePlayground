using System.Collections.Generic;

namespace MazePlayground.Common.Mazes
{
    public interface IMaze
    {
        Cell StartingCell { get; }
        Cell FinishingCell { get; }
        IReadOnlyList<KeyValuePair<string, string>> Stats { get; }
        IReadOnlyList<Cell> AllCells { get; }
        IReadOnlyList<CellWall> GetWallsForCell(Cell cell);
        byte GetOppositeLinkId(byte linkId);
    }
}