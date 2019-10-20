namespace MazePlayground.App.MonoGame.Config
{
    public class RowColumnMazeConfig
    {
        public int RowCount { get; }
        public int ColumnCount { get; }

        public RowColumnMazeConfig(int rowCount, int columnCount)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
        }
    }
}