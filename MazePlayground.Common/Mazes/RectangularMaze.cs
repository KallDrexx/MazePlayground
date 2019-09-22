using System;
using System.Collections.Generic;
using System.Linq;
using MazePlayground.Common.Solvers;
using MazePlayground.Common.WallSetup;

namespace MazePlayground.Common.Mazes
{
    public class RectangularMaze : IMaze
    {
        private readonly Random _random = new Random();
        private readonly Dictionary<Cell, int> _cellIndexMap = new Dictionary<Cell, int>();
        
        public enum Direction { North, South, East, West }

        public Cell[] Cells { get; }
        public int RowCount { get; }
        public int ColumnCount { get; }
        public Cell StartingCell { get; private set; }
        public Cell FinishingCell { get; private set; }
        public IReadOnlyList<Cell> AllCells => Cells;

        public RectangularMaze(int rowCount, int columnCount, WallSetupAlgorithm setupAlgorithm)
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
            SetStartingAndEndingCells();
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

        public IReadOnlyList<CellWall> GetWallsForCell(Cell cell)
        {
                if (cell == null) throw new ArgumentNullException(nameof(cell));

                var (row, column) = GetPositionOfCell(cell);
                return new[] {Direction.North, Direction.East, Direction.South, Direction.West}
                    .Select(x => new CellWall(GetLinkIdForDirection(x), GetCellInDirection(row, column, x)))
                    .Where(x => x.CellOnOtherSide != null)
                    .ToArray();
        }

        public byte GetOppositeLinkId(byte linkId)
        {
            var direction = GetDirectionForLinkId(linkId);
            var oppositeDirection = GetOppositeDirection(direction);
            return GetLinkIdForDirection(oppositeDirection);
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

        private static Direction GetDirectionForLinkId(byte linkId)
        {
            switch (linkId)
            {
                case 1: return Direction.North;
                case 2: return Direction.South;
                case 3: return Direction.East;
                case 4: return Direction.West;
                
                default:
                    throw new NotSupportedException($"Link id {linkId} not supported");
            }
        }

        private void SetupWalls(WallSetupAlgorithm setupAlgorithm)
        {
            switch (setupAlgorithm)
            {
                case WallSetupAlgorithm.BinaryTree:
                    new BinaryTree().Run(this);
                    break;
                
                case WallSetupAlgorithm.Sidewinder:
                    new Sidewinder().Run(this);
                    break;
                
                case WallSetupAlgorithm.AldousBroder:
                    new AldousBroder().Run(this);
                    break;
                
                case WallSetupAlgorithm.Wilson:
                    new Wilson().Run(this);
                    break;
                
                case WallSetupAlgorithm.HuntAndKill:
                    new HuntAndKill().Run(this);
                    break;
                
                case WallSetupAlgorithm.RecursiveBackTracker:
                    new RecursiveBackTracker().Run(this);
                    break;
                
                default:
                    throw new NotSupportedException($"Algorithm {setupAlgorithm} not supported");
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
            if (row < 0 || column < 0 || row >= RowCount || column >= ColumnCount)
            {
                return null;
            }
            
            var index = (row * ColumnCount) + column;

            return index >= 0 && index < Cells.Length
                ? Cells[index]
                : null;
        }

        private void SetStartingAndEndingCells()
        {
            var startingRow = _random.Next(0, RowCount);
            StartingCell = GetCell(startingRow, 0);
            
            var distanceInfo = CellDistanceSolver.GetDistancesFromCell(StartingCell);
            FinishingCell = FindFarthestEdgeCell(distanceInfo);
        }

        private Cell FindFarthestEdgeCell(DistanceInfo distanceInfo, bool onlyLeftEdge = false)
        {
            var farthestCell = (Cell) null;
            var farthestCellDistance = 0;

            var positionsToCheck = Enumerable.Range(0, RowCount).Select(x => (x, 0)); // left
            if (!onlyLeftEdge)
            {
                positionsToCheck = positionsToCheck.Concat(Enumerable.Range(0, ColumnCount).Select(x => (0, x))) // top
                    .Concat(Enumerable.Range(0, ColumnCount).Select(x => (RowCount - 1, x))) // bottom
                    .Concat(Enumerable.Range(0, RowCount).Select(x => (x, ColumnCount - 1))); // right
            }

            foreach (var (row, column) in positionsToCheck)
            {
                var cell = GetCell(row, column);
                if (distanceInfo.DistanceFromStartMap.TryGetValue(cell, out var distance))
                {
                    if (distance > farthestCellDistance)
                    {
                        farthestCell = cell;
                        farthestCellDistance = distance;
                    }
                }
            }

            return farthestCell;
        }
    }
}