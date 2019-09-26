using System.Collections.Generic;

namespace MazePlayground.Common.Mazes
{
    /// <summary>
    /// Represents a single area of a maze with walls to adjacent cells.
    /// </summary>
    public class Cell
    {
        public List<CellWall> CellWalls { get; } = new List<CellWall>();
    }
}