using System;
using System.Collections.Generic;
using System.Linq;

namespace MazePlayground.Common.Mazes.Grid
{
    public class GridMaze
    {
        private readonly Random _random = new Random();
        
        private enum Direction { North, South, East, West }

        public Cell[] Cells { get; }
        public int RowCount { get; }
        public int ColumnCount { get; }
        public Cell StartCell { get; }
        public Cell EndCell { get; }

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

            var startRow = _random.Next(0, RowCount);
            var endRow = _random.Next(0, RowCount);

            StartCell = GetCell(startRow, 0);
            EndCell = GetCell(endRow, ColumnCount - 1);
        }

        private void SetupWalls(WallSetupAlgorithm setupAlgorithm)
        {
            switch (setupAlgorithm)
            {
                case WallSetupAlgorithm.BinaryTree:
                    SetupWallsBinaryTree();
                    break;
                
                case WallSetupAlgorithm.Sidewinder:
                    SetupWallsSidewinder();
                    break;
                
                default:
                    throw new NotSupportedException($"Algorithm {setupAlgorithm} not supported");
            }
        }

        private void SetupWallsBinaryTree()
        {
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
                    var direction = _random.Next(0, 2) == 0 ? Direction.East : Direction.North;
                    OpenCellWall(cell, direction);
                }
            }
        }

        private void SetupWallsSidewinder()
        {
            var rowSet = new List<Cell>();
            for (var row = 0; row < RowCount; row++)
            {
                for (var column = 0; column < ColumnCount; column++)
                {
                    var cell = GetCell(row, column);
                    if (row == 0)
                    {
                        if (column != ColumnCount - 1)
                        {
                            OpenCellWall(cell, Direction.East);
                        }
                    }
                    else
                    {
                        rowSet.Add(cell);
                        
                        var carveEast = column < ColumnCount - 1 && _random.Next(0, 2) == 0;
                        if (carveEast)
                        {
                            OpenCellWall(cell, Direction.East);
                        }
                        else
                        {
                            var index = _random.Next(0, rowSet.Count);
                            OpenCellWall(rowSet[index], Direction.North);
                            rowSet.Clear();
                        }
                    }
                }

                if (rowSet.Any())
                {
                    var index = _random.Next(0, rowSet.Count);
                    OpenCellWall(rowSet[index], Direction.North);
                    rowSet.Clear();
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
    }
}