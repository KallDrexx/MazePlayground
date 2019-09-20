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
            // Start with a cell and carve a path through all other cells until every cell has been visited.  Only
            // carve out walls of a cell the first time it is visited.
            
            var visitedCells = new HashSet<Cell>();
            var currentCell = Cells[0];
            visitedCells.Add(currentCell);
            while (visitedCells.Count < Cells.Length)
            {
                var cellIndex = _cellIndexMap[currentCell];
                var currentPosition = GetPositionFromIndex(cellIndex);

                var directions = new List<Direction>();
                if (currentPosition.row != 0) directions.Add(Direction.North);
                if (currentPosition.column != 0) directions.Add(Direction.West);
                if (currentPosition.row < RowCount - 1) directions.Add(Direction.South);
                if (currentPosition.column < ColumnCount - 1) directions.Add(Direction.East);

                var nextDirection = directions[_random.Next(0, directions.Count)];
                var nextCell = GetCellInDirection(currentPosition.row, currentPosition.column, nextDirection);
                if (!visitedCells.Contains(nextCell))
                {
                    OpenCellWall(currentCell, nextCell, nextDirection);
                    visitedCells.Add(nextCell);
                }

                currentCell = nextCell;
            }
        }

        private void SetupWallsWilson()
        {
            // Start with one cell marked as visited.  Pick a random non-visited cell and create a path of cells until
            // we get to a visited cell.  Carve out a path through the path to the visited cell, then repeat until all
            // cells are visited.  If a path loops around reset the path at the beginning of the loop.
            
            void OpenCellInPath(Cell firstCell, Cell secondCell, HashSet<Cell> hashSet, HashSet<Cell> cells, List<int> ints)
            {
                var direction = GetDirectionOfCells(firstCell, secondCell);
                OpenCellWall(firstCell, secondCell, direction);

                hashSet.Add(firstCell);
                cells.Remove(firstCell);
                ints.Remove(_cellIndexMap[firstCell]);
            }

            var visitedCells = new HashSet<Cell>();
            var unvisitedCells = new HashSet<Cell>(Cells);
            var unvisitedCellIndexes = Enumerable.Range(1, Cells.Length - 1).ToList();
            var currentPath = new List<Cell>();

            visitedCells.Add(Cells[0]);
            unvisitedCells.Remove(Cells[0]);

            var currentCellIndex = unvisitedCellIndexes[_random.Next(0, unvisitedCellIndexes.Count)];
            var currentCell = Cells[currentCellIndex];
            currentPath.Add(currentCell);
            while (true)
            {
                var cellIndex = _cellIndexMap[currentCell];
                var currentPosition = GetPositionFromIndex(cellIndex);
                
                var directions = new List<Direction>();
                if (currentPosition.row != 0) directions.Add(Direction.North);
                if (currentPosition.column != 0) directions.Add(Direction.West);
                if (currentPosition.row < RowCount - 1) directions.Add(Direction.South);
                if (currentPosition.column < ColumnCount - 1) directions.Add(Direction.East);
                
                var nextDirection = directions[_random.Next(0, directions.Count)];
                var nextCell = GetCellInDirection(currentPosition.row, currentPosition.column, nextDirection);

                if (visitedCells.Contains(nextCell))
                {
                    // Walk the path
                    for (var x = 1; x < currentPath.Count; x++)
                    {
                        var firstCell = currentPath[x - 1];
                        var secondCell = currentPath[x];
                        OpenCellInPath(firstCell, secondCell, visitedCells, unvisitedCells, unvisitedCellIndexes);
                    }
                    
                    OpenCellInPath(currentCell, nextCell, visitedCells, unvisitedCells, unvisitedCellIndexes);

                    // Path completed, pick the next cell to start a new path
                    if (unvisitedCellIndexes.Count == 0)
                    {
                        break;
                    }
                    
                    currentCellIndex = unvisitedCellIndexes[_random.Next(0, unvisitedCellIndexes.Count)];
                    currentCell = Cells[currentCellIndex];
                    currentPath.Clear();
                }
                else
                {
                    var indexInPath = currentPath.IndexOf(nextCell);
                    if (indexInPath >= 0)
                    {
                        // We looped around.  Remove the loop from the path
                        for (var x = currentPath.Count - 1; x > indexInPath; x--)
                        {
                            currentPath.RemoveAt(x);
                        }
                    }
                    else
                    {
                        // Newly seen cell
                        currentPath.Add(nextCell);
                    }

                    currentCell = nextCell;
                }
            }
        }

        private void SetupWallsHuntAndKill()
        {
            // Start from the bottom left cell, carve out a random path only going through other unvisited cells.  If
            // we get stuck (all neighbors are visited cells) then pick the top-left most unvisited cell that borders a
            // a visited, link the two cells, and carve a new path until we hit another visited cell.  Repeat.
            
            var visitedCells = new HashSet<Cell>();
            var currentCell = GetCell(RowCount - 1, 0);
            visitedCells.Add(currentCell);
            while (visitedCells.Count < Cells.Length)
            {
                var currentPosition = GetPositionOfCell(currentCell);
                var adjacentCells = new[]
                    {
                        GetCellInDirection(currentPosition.row, currentPosition.column, Direction.North),
                        GetCellInDirection(currentPosition.row, currentPosition.column, Direction.South),
                        GetCellInDirection(currentPosition.row, currentPosition.column, Direction.East),
                        GetCellInDirection(currentPosition.row, currentPosition.column, Direction.West),
                    }
                    .Where(x => x != null)
                    .Where(x => !visitedCells.Contains(x))
                    .ToArray();

                if (adjacentCells.Length == 0)
                {
                    // We got caught in a loop
                    var newCellFound = false;
                    for (var index = 0; index < Cells.Length; index++)
                    {
                        var cell = Cells[index];
                        if (visitedCells.Contains(cell))
                        {
                            continue;
                        }
                        
                        var position = GetPositionOfCell(cell);
                        var northernCell = GetCellInDirection(position.row, position.column, Direction.North);
                        if (northernCell != null && visitedCells.Contains(northernCell))
                        {
                            OpenCellWall(cell, northernCell, Direction.North);
                            currentCell = cell;
                            newCellFound = true;
                            break;
                        }
                        
                        var southernCell = GetCellInDirection(position.row, position.column, Direction.South);
                        if (southernCell != null && visitedCells.Contains(southernCell))
                        {
                            OpenCellWall(cell, southernCell, Direction.South);
                            currentCell = cell;
                            newCellFound = true;
                            break;
                        }

                        var easternCell = GetCellInDirection(position.row, position.column, Direction.East);
                        if (easternCell != null && visitedCells.Contains(easternCell))
                        {
                            OpenCellWall(cell, easternCell, Direction.East);
                            currentCell = cell;
                            newCellFound = true;
                            break;
                        }
                        
                        var westernCell = GetCellInDirection(position.row, position.column, Direction.West);
                        if (westernCell != null && visitedCells.Contains(westernCell))
                        {
                            OpenCellWall(cell, westernCell, Direction.West);
                            currentCell = cell;
                            newCellFound = true;
                            break;
                        }
                        
                        // No neighbors that have been visited, so go try the next
                    }

                    if (!newCellFound)
                    {
                        throw new InvalidOperationException("Could not find any unvisited cells with adjacent visited cells");
                    }
                }
                else
                {
                    var index = _random.Next(0, adjacentCells.Length);
                    var nextCell = adjacentCells[index];
                    var direction = GetDirectionOfCells(currentCell, nextCell);
                    OpenCellWall(currentCell, nextCell, direction);

                    currentCell = nextCell;
                }
                
                visitedCells.Add(currentCell);
            }
        }

        private void SetupWallsRecursiveBackTracker()
        {
            // Start from the bottom left cell, carve out a random path only going through unvisited cells.  If
            // we reach a dead end backtrack until we find a cell with an unvisited neighbor and continue
            
            var visitedCells = new HashSet<Cell>();
            var path = new Stack<Cell>();
            var currentCell = GetCell(RowCount - 1, 0);
            visitedCells.Add(currentCell);
            path.Push(currentCell);
            while (true)
            {
                var currentPosition = GetPositionOfCell(currentCell);
                var adjacentCells = new[]
                    {
                        (GetCellInDirection(currentPosition.row, currentPosition.column, Direction.North), Direction.North),
                        (GetCellInDirection(currentPosition.row, currentPosition.column, Direction.South), Direction.South),
                        (GetCellInDirection(currentPosition.row, currentPosition.column, Direction.East), Direction.East),
                        (GetCellInDirection(currentPosition.row, currentPosition.column, Direction.West), Direction.West),
                    }
                    .Where(x => x.Item1 != null)
                    .Where(x => !visitedCells.Contains(x.Item1))
                    .ToArray();

                if (adjacentCells.Length > 0)
                {
                    var (cell, direction) = adjacentCells[_random.Next(adjacentCells.Length)];
                    OpenCellWall(currentCell, cell, direction);
                    visitedCells.Add(cell);
                    path.Push(cell);

                    currentCell = cell;
                }
                else
                {
                    // No unvisited neighbors
                    path.Pop();
                    if (path.Any())
                    {
                        currentCell = path.Peek();
                    }
                    else
                    {
                        // All cells should now have visited neighbors
                        break;
                    }
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