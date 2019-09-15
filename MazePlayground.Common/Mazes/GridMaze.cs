using System;
using System.Collections.Generic;
using System.Linq;

namespace MazePlayground.Common.Mazes
{
    public class GridMaze : IMaze
    {
        private readonly Random _random = new Random();
        private readonly Dictionary<Cell, int> _cellIndexMap = new Dictionary<Cell, int>();
        
        public enum Direction { North, South, East, West }
        public enum WallSetupAlgorithm { BinaryTree, Sidewinder }

        public Cell[] Cells { get; }
        public int RowCount { get; }
        public int ColumnCount { get; }
        public Cell StartingCell { get; }
        public Cell FinishingCell { get; }

        public GridMaze(int rowCount, int columnCount, WallSetupAlgorithm setupAlgorithm)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            
            Cells = new Cell[rowCount * columnCount];
            for (var row = 0; row < rowCount; row++)
            for (var column = 0; column < columnCount; column++)
            {
                var index = (row * columnCount) + column;
                var cell = new Cell();

                Cells[index] = cell;
                _cellIndexMap[cell] = index;
            }
            
            SetupWalls(setupAlgorithm);

            var startRow = _random.Next(0, RowCount);
            var endRow = _random.Next(0, RowCount);

            StartingCell = GetCell(startRow, 0);
            FinishingCell = GetCell(endRow, ColumnCount - 1);
        }
        
        public Cell GetLinkedCell(Cell linkedFromCell, byte wallIndex)
        {
            throw new NotImplementedException();
        }

        public (int row, int column) GetPositionOfCell(Cell cell)
        {
            var index = _cellIndexMap[cell];
            return GetPositionFromIndex(index);
        }

        public Cell GetCellLinkedInDirection(Cell source, Direction direction)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var linkId = GetLinkIdForDirection(direction);
            return source.LinkIdToCellMap.TryGetValue(linkId, out var otherCell)
                ? otherCell
                : null;
        }
        
        private Cell GetCellInDirection(int row, int column, Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    row -= 1;
                    break;
                
                case Direction.South:
                    row += 1;
                    break;
                
                case Direction.East:
                    column += 1;
                    break;
                
                case Direction.West:
                    column -= 1;
                    break;
                
                default:
                    throw new NotSupportedException($"Unsupported direction {direction}");
            }

            return GetCell(row, column);
        }

        private static Direction GetOppositeDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.North: return Direction.South;
                case Direction.South: return Direction.North;
                case Direction.East: return Direction.West;
                case Direction.West: return Direction.East;
                
                default:
                    throw new NotSupportedException($"Unsupported direction {direction}");
            }
        }

        private static byte GetLinkIdForDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.North: return 1;
                case Direction.South: return 2;
                case Direction.East: return 3;
                case Direction.West: return 4;
                
                default:
                    throw new NotSupportedException($"Unsupported direction {direction}");
            }
        }
        
        private static void OpenCellWall(Cell first, Cell second, Direction linkDirection)
        {
            var forwardLinkId = GetLinkIdForDirection(linkDirection);
            var reverseLinkId = GetLinkIdForDirection(GetOppositeDirection(linkDirection));
            
            first.LinkToOtherCell(second, forwardLinkId);
            second.LinkToOtherCell(first, reverseLinkId);
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
                    var eastCell = GetCellInDirection(row, column, Direction.East);
                    OpenCellWall(cell, eastCell, Direction.East);
                }
                else if (row != 0 && column == ColumnCount - 1)
                {
                    var northCell = GetCellInDirection(row, column, Direction.North);
                    OpenCellWall(cell, northCell, Direction.North);
                }
                else if (row != 0 && column != ColumnCount - 1)
                {
                    var direction = _random.Next(0, 2) == 0 ? Direction.East : Direction.North;
                    var otherCell = GetCellInDirection(row, column, direction);
                    OpenCellWall(cell, otherCell, direction);
                }
            }
        }

        private void SetupWallsSidewinder()
        {
            void LinkRandomCellFromSet(List<Cell> cells)
            {
                var index = _random.Next(0, cells.Count);
                var position = GetPositionFromIndex(_cellIndexMap[cells[index]]);
                var southCell = GetCell(position.row, position.column);
                var northCell = GetCellInDirection(position.row, position.column, Direction.North);
                OpenCellWall(southCell, northCell, Direction.North);
                cells.Clear();
            }

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
                            var eastCell = GetCellInDirection(row, column, Direction.East);
                            OpenCellWall(cell, eastCell, Direction.East);
                        }
                    }
                    else
                    {
                        rowSet.Add(cell);
                        
                        var carveEast = column < ColumnCount - 1 && _random.Next(0, 2) == 0;
                        if (carveEast)
                        {
                            var eastCell = GetCellInDirection(row, column, Direction.East);
                            OpenCellWall(cell, eastCell, Direction.East);
                        }
                        else
                        {
                            LinkRandomCellFromSet(rowSet);
                        }
                    }
                }

                if (rowSet.Any())
                {
                    LinkRandomCellFromSet(rowSet);
                }
            }
        }
        
        private (int row, int column) GetPositionFromIndex(int index)
        {
            var row = index / ColumnCount;
            var column = index % ColumnCount;

            return (row, column);
        }

        private Cell GetCell(int row, int column)
        {
            var index = (row * ColumnCount) + column;

            return index >= 0 && index < Cells.Length
                ? Cells[index]
                : null;
        }
    }
}