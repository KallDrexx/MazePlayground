using MazePlayground.Common.WallSetup;

namespace MazePlayground.App.MonoGame.Config
{
    public class RectangularMazeConfig
    {
        public int RowCount { get; }
        public int ColumnCount { get; }
        public WallSetupAlgorithm WallSetupAlgorithm { get; }

        public RectangularMazeConfig(int rowCount, int columnCount, WallSetupAlgorithm wallSetupAlgorithm)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            WallSetupAlgorithm = wallSetupAlgorithm;
        }
    }
}