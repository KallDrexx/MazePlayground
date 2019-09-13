using System;

namespace MazePlayground.Common.Mazes
{
    public class GridMaze
    {
        private enum Direction { North, South, East, West }
        public enum WallSetupAlgorithm { BinaryTree }
        
        public Cell[] Cells { get; }
        public int RowCount { get; }
        public int ColumnCount { get; }

        public GridMaze(int rowCount, int columnCount, WallSetupAlgorithm setupAlgorithm)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            
            Cells = new Cell[rowCount * columnCount];
            for (var row = 0; row < rowCount; row++)
            for (var column = 0; column < columnCount; column++)
            {
                var index = (row * columnCount) + column;
                var cell = new Cell(row, column);

                Cells[index] = cell;
            }
            
            SetupWalls(setupAlgorithm);
        }

        private void SetupWalls(WallSetupAlgorithm setupAlgorithm)
        {
            switch (setupAlgorithm)
            {
                case WallSetupAlgorithm.BinaryTree:
                    SetupWallsBinaryTree();
                    break;
                
                default:
                    throw new NotSupportedException($"Algorithm {setupAlgorithm} not supported");
            }
        }

        private void SetupWallsBinaryTree()
        {
            var random = new Random();
            
            for (var row = 0; row < RowCount; row++)
            for (var column = 0; column < ColumnCount; column++)
            {
                var cell = GetCell(row, column);
                if (row == 0 && column != ColumnCount - 1)
                {
                    OpenCellWall(cell, Direction.East);
                }
                else if (row != 0 && column == ColumnCount - 1)
                {
                    OpenCellWall(cell, Direction.North);
                }
                else if (row != 0 && column != ColumnCount - 1)
                {
                    var direction = random.Next(0, 2) == 0 ? Direction.East : Direction.North;
                    OpenCellWall(cell, direction);
                }
            }
        }

        private void OpenCellWall(Cell cell, Direction direction)
        {
            var otherCell = GetCell(cell.Row, cell.Column, direction);
            switch (direction)
            {
                case Direction.North:
                    cell.North = otherCell;
                    otherCell.South = cell;
                    break;
                
                case Direction.South:
                    cell.South = otherCell;
                    otherCell.North = cell;
                    break;
                
                case Direction.East:
                    cell.East = otherCell;
                    otherCell.West = cell;
                    break;
                
                case Direction.West:
                    cell.West = otherCell;
                    otherCell.East = cell;
                    break;
            }
        }

        private Cell GetCell(int row, int column, Direction? direction = null)
        {
            if (direction != null)
            {
                switch (direction)
                {
                    case Direction.North:
                        row--;
                        break;
                    
                    case Direction.South:
                        row++;
                        break;
                    
                    case Direction.East:
                        column++;
                        break;
                    
                    case Direction.West:
                        column--;
                        break;
                }
            }
            
            var index = (row * ColumnCount) + column;
            return Cells[index];
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