namespace MazePlayground.Common.Mazes
{
    public class GridMaze
    {
        public Cell[] Cells { get; }
        public int RowCount { get; }
        public int ColumnCount { get; }

        public GridMaze(int rowCount, int columnCount)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            
            Cells = new Cell[rowCount * columnCount];
            for (var row = 0; row < rowCount; row++)
            {
                for (var column = 0; column < columnCount; column++)
                {
                    var index = (row * columnCount) + column;
                    Cells[index] = new Cell(row, column);
                }
            }
        }
        
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
}