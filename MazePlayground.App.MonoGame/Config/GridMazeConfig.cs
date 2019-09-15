using MazePlayground.Common.Mazes;

namespace MazePlayground.App.MonoGame.Config
{
    public class GridMazeConfig
    {
        public int RowCount { get; }
        public int ColumnCount { get; }
        public GridMaze.WallSetupAlgorithm WallSetupAlgorithm { get; }

        public GridMazeConfig(int rowCount, int columnCount, GridMaze.WallSetupAlgorithm wallSetupAlgorithm)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            WallSetupAlgorithm = wallSetupAlgorithm;
        }
    }
}