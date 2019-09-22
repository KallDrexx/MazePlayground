namespace MazePlayground.App.MonoGame.Config
{
    public class RectangularMazeConfig
    {
        public int RowCount { get; }
        public int ColumnCount { get; }

        public RectangularMazeConfig(int rowCount, int columnCount)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
        }
    }
}