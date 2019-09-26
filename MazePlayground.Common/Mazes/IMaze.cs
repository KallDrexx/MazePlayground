using System.Collections.Generic;

namespace MazePlayground.Common.Mazes
{
    public interface IMaze
    {
        Cell StartingCell { get; }
        Cell FinishingCell { get; }
        IReadOnlyList<Cell> AllCells { get; }
    }
}