using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MazePlayground.Common.Solvers;
using MazePlayground.Common.WallSetupAlgorithms;

namespace MazePlayground.Common.Mazes
{
    public class GridMaze : IMaze
    {
        private readonly Random _random = new Random();
        private readonly Dictionary<Cell, int> _cellIndexMap = new Dictionary<Cell, int>();
        private readonly List<KeyValuePair<string, string>> _stats = new List<KeyValuePair<string, string>>();
        
        public enum Direction { North, South, East, West }
        public enum WallSetupAlgorithm { AldousBroder, BinaryTree, HuntAndKill, RecursiveBackTracker, Sidewinder, Wilson }

        public Cell[] Cells { get; }
        public int RowCount { get; }
        public int ColumnCount { get; }
        public Cell StartingCell { get; private set; }
        public Cell FinishingCell { get; private set; }
        public IReadOnlyList<KeyValuePair<string, string>> Stats => _stats;
        public IReadOnlyList<Cell> AllCells => Cells;

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
            
            _stats.Add(new KeyValuePair<string, string>("Algorithm:", setupAlgorithm.ToString()));
            _stats.Add(new KeyValuePair<string, string>("Rows:", rowCount.ToString()));
            _stats.Add(new KeyValuePair<string, string>("Columns:", columnCount.ToString()));
            _stats.Add(new KeyValuePair<string, string>("Total Cells:", Cells.Length.ToString()));

            var stopwatch = Stopwatch.StartNew();
            SetupWalls(setupAlgorithm);
            stopwatch.Stop();
            
            SetStartingAndEndingCells();

            var deadEnds = 0;
            foreach (var cell in Cells)
            {
                if (cell.LinkIdToCellMap.Count == 1)
                {
                    deadEnds++;
                }
            }

            var percentage = (int) ((deadEnds / (decimal) Cells.Length) * 100);
            _stats.Add(new KeyValuePair<string, string>("Dead Ends:", $"{deadEnds} ({percentage}%%)"));
            _stats.Add(new KeyValuePair<string, string>("Wall Setup Time:", $"{stopwatch.ElapsedMilliseconds}ms"));
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
                
                case WallSetupAlgorithm.AldousBroder:
                    SetupWallsAldousBroder();
                    break;
                
                case WallSetupAlgorithm.Wilson:
                    SetupWallsWilson();
                    break;
                
                case WallSetupAlgorithm.HuntAndKill:
                    SetupWallsHuntAndKill();
                    break;
                
                case WallSetupAlgorithm.RecursiveBackTracker:
                    SetupWallsRecursiveBackTracker();
                    break;
                
                default:
                    throw new NotSupportedException($"Algorithm {setupAlgorithm} not supported");
            }
        }

        private void SetupWallsBinaryTree()
        {
            new BinaryTree().Run(this);
        }

        private void SetupWallsSidewinder()
        {
            new Sidewinder().Run(this);
        }

        private void SetupWallsAldousBroder()
        {
            new AldousBroder().Run(this);
        }

        private void SetupWallsWilson()
        {
            new Wilson().Run(this);
        }

        private void SetupWallsHuntAndKill()
        {
            new HuntAndKill().Run(this);
        }

        private void SetupWallsRecursiveBackTracker()
        {
            new RecursiveBackTracker().Run(this);
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

        private Direction GetDirectionOfCells(Cell source, Cell target)
        {
            if (source == target) throw new ArgumentException("Both cells are the same!");
            
            var sourcePosition = GetPositionFromIndex(_cellIndexMap[source]);
            var targetPosition = GetPositionFromIndex(_cellIndexMap[target]);

            var rowDifference = targetPosition.row - sourcePosition.row;
            var colDifference = targetPosition.column - sourcePosition.column;

            if (Math.Abs(rowDifference) > 1 ||
                Math.Abs(colDifference) > 1 ||
                (rowDifference != 0 && colDifference != 0))
            {
                throw new ArgumentException("Cells are not adjacent");
            }

            if (rowDifference == 1) return Direction.South;
            if (rowDifference == -1) return Direction.North;
            if (colDifference == 1) return Direction.East;
            return Direction.West;
        }
    }
}