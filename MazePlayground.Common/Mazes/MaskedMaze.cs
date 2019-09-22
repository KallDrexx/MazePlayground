using System;
using System.Collections.Generic;
using System.Linq;
using MazePlayground.Common.Solvers;
using MazePlayground.Common.WallSetup;

namespace MazePlayground.Common.Mazes
{
    public class MaskedMaze : IMaze
    {
        private readonly Random _random = new Random();
        private readonly Cell[] _cells;
        private readonly Dictionary<Cell, int> _cellIndexMap;
        
        public enum Direction { North, South, East, West }
        public int RowCount { get; }
        public int ColumnCount { get; }
        public Cell StartingCell { get; private set; }
        public Cell FinishingCell { get; private set; }
        public IReadOnlyList<Cell> AllCells => _cells.Where(x => x != null).ToArray();

        public MaskedMaze(int rowCount, int columnCount, IReadOnlyList<bool> mask, WallSetupAlgorithm algorithm)
        {
            if (mask == null) throw new ArgumentNullException(nameof(mask));
            if (rowCount * columnCount != mask.Count)
            {
                throw new ArgumentException($"Expected mask to have {rowCount * columnCount} values, instead had {mask.Count}");
            }

            RowCount = rowCount;
            ColumnCount = columnCount;
            _cells = new Cell[rowCount * columnCount];
            _cellIndexMap = new Dictionary<Cell, int>();
            for (var index = 0; index < mask.Count; index++)
            {
                if (mask[index])
                {
                    var cell = new Cell();
                    _cells[index] = cell;
                    _cellIndexMap.Add(cell, index);
                }
            }
            
            VerifyMazeIsValid();
            SetupWalls(algorithm);
            SetStartingAndFinishingCell();
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
        
        private (int row, int column) GetPositionFromIndex(int index)
        {
            var row = index / ColumnCount;
            var column = index % ColumnCount;

            return (row, column);
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

        private Cell GetCell(int row, int column)
        {
            if (row < 0 || column < 0 || row >= RowCount || column >= ColumnCount)
            {
                return null;
            }
            
            var index = (row * ColumnCount) + column;

            return index >= 0 && index < _cells.Length
                ? _cells[index]
                : null;
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

        private void VerifyMazeIsValid()
        {
            // Make sure every cell can be reached
            var distanceInfo = CellDistanceSolver.GetDistancesForUnlinkedMaze(this);
            if (distanceInfo.DistanceFromStartMap.Count != AllCells.Count)
            {
                throw new InvalidOperationException("Not all cells are reachable!");
            }
        }

        private void SetupWalls(WallSetupAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case WallSetupAlgorithm.Wilson:
                    new Wilson().Run(this);
                    break;
                
                case WallSetupAlgorithm.AldousBroder:
                    new AldousBroder().Run(this);
                    break;
                
                case WallSetupAlgorithm.HuntAndKill:
                    new HuntAndKill().Run(this);
                    break;
                
                case WallSetupAlgorithm.RecursiveBackTracker:
                    new RecursiveBackTracker().Run(this);
                    break;
                
                default:
                    throw new NotSupportedException($"Masked mazes does not support the {algorithm} algorithm");
            }
        }

        private void SetStartingAndFinishingCell()
        {
            // Starting cell should always be a random cell in the leftmost column
            Cell[] candidates;
            var column = 0;
            do
            {
                candidates = GetCellsInColumn(column);
                column++;
            } while (!candidates.Any());

            StartingCell = candidates[_random.Next(0, candidates.Length)];
            
            var distanceInfo = CellDistanceSolver.GetDistancesFromCell(StartingCell);
            FinishingCell = FindFarthestEdgeCell(distanceInfo);
        }

        private Cell FindFarthestEdgeCell(DistanceInfo distanceInfo)
        {
            var leftCandidates = new Cell[0];
            var rightCandidates = new Cell[0];
            var topCandidates = new Cell[0];
            var bottomCandidates = new Cell[0];

            for (var x = 0; x < ColumnCount; x++)
            {
                var candidates = GetCellsInColumn(x);
                if (candidates.Any())
                {
                    if (!leftCandidates.Any())
                    {
                        leftCandidates = candidates;
                    }

                    rightCandidates = candidates;
                }
            }

            for (var x = 0; x < ColumnCount; x++)
            {
                var candidates = GetCellsInRow(x);
                if (candidates.Any())
                {
                    if (!topCandidates.Any())
                    {
                        topCandidates = candidates;
                    }

                    bottomCandidates = candidates;
                }
            }

            return leftCandidates.Union(rightCandidates).Union(topCandidates).Union(bottomCandidates)
                .Select(x => new {cell = x, distance = distanceInfo.DistanceFromStartMap[x]})
                .OrderByDescending(x => x.distance)
                .Select(x => x.cell)
                .First();
        }

        private Cell[] GetCellsInColumn(int columnIndex)
        {
            return Enumerable.Range(0, RowCount)
                .Select(x => GetCell(x, columnIndex))
                .Where(x => x != null)
                .ToArray();
        }

        private Cell[] GetCellsInRow(int rowIndex)
        {
            return Enumerable.Range(0, ColumnCount)
                .Select(x => GetCell(rowIndex, x))
                .Where(x => x != null)
                .ToArray();
        }
    }
}