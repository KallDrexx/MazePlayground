using System;
using System.Linq;
using MazePlayground.Common.Solvers;
using MazePlayground.Common.WallSetup;

namespace MazePlayground.Common.Mazes
{
    public class HexMaze : RowColumnBasedMaze
    {
        private readonly Random _random = new Random();

        public HexMaze(int rowCount, int columnCount, WallSetupAlgorithm algorithm)
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
                CellIndexMap[cell] = index;
                
                // Link cells together if they've been created so far.  Coordinates assume offset rows and straight
                // columns with the first cell top left most of all cells.
                var northWestCell = GetCell(column % 2 == 1 ? row : row - 1, column - 1);
                var northCell = GetCell(row - 1, column);
                var northEastCell = GetCell(column % 2 == 1 ? row : row - 1, column + 1);
                var southWestCell = GetCell(column % 2 == 1 ? row + 1 : row, column - 1);
                
                LinkCellsIfNotAlreadyLinked(cell, northWestCell);
                LinkCellsIfNotAlreadyLinked(cell, northCell);
                LinkCellsIfNotAlreadyLinked(cell, northEastCell);
                LinkCellsIfNotAlreadyLinked(cell, southWestCell);
            }
            
            SetupWalls(algorithm);
            SetStartAndFinishingCells();
        }

        private static void LinkCellsIfNotAlreadyLinked(Cell current, Cell neighbor)
        {
            if (neighbor == null)
            {
                return;
            }

            if (neighbor.CellWalls.Any(x => x.GetOtherCell(neighbor) == current))
            {
                return;
            }
            
            var wall = new CellWall(current, neighbor);
            current.CellWalls.Add(wall);
            neighbor.CellWalls.Add(wall);
        }

        private void SetupWalls(WallSetupAlgorithm setupAlgorithm)
        {
            switch (setupAlgorithm)
            {
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

        private void SetStartAndFinishingCells()
        {
            var startingCandidates = Enumerable.Range(0, RowCount).Select(x => GetCell(x, 0)).ToArray(); // left
            var finishingCandidates = startingCandidates
                .Concat(Enumerable.Range(0, RowCount).Select(x => GetCell(x, ColumnCount - 1))) // right
                .Concat(Enumerable.Range(0, ColumnCount).Select(x => GetCell(0, x))) // top
                .Concat(Enumerable.Range(0, ColumnCount).Select(x => GetCell(RowCount - 1, x))) // bottom
                .ToArray();

            StartingCell = startingCandidates[_random.Next(0, startingCandidates.Length)];

            var distanceInfo = CellDistanceSolver.GetPassableDistancesFromCell(StartingCell);
            FinishingCell = finishingCandidates.Select(x => new {cell = x, distance = distanceInfo.DistanceFromStartMap[x]})
                .OrderByDescending(x => x.distance)
                .Select(x => x.cell)
                .First();
        }
    }
}