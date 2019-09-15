namespace MazePlayground.Common.Mazes.Grid
{
    public class Cell
    {
        public int Row { get; }
        public int Column { get; }
        public Cell North { get; internal set; }
        public Cell South { get; internal set; }
        public Cell East { get; internal set; }
        public Cell West { get; internal set; }
        
        public Cell(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
}