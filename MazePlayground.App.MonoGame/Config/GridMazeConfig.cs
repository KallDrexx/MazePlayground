using MazePlayground.Common.Mazes;
using MazePlayground.Common.Mazes.Grid;

namespace MazePlayground.App.MonoGame.Config
{
    public class GridMazeConfig
    {
        public int RowCount { get; }
        public int ColumnCount { get; }
        public WallSetupAlgorithm WallSetupAlgorithm { get; }
        
        public GridMazeConfig(int rowCount, int columnCount, WallSetupAlgorithm wallSetupAlgorithm)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            WallSetupAlgorithm = wallSetupAlgorithm;
        }
    }
}