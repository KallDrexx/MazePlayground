using System;
using System.Linq;
using MazePlayground.Common.Solvers;
using MazePlayground.Common.WallSetup;

namespace MazePlayground.Common.Mazes
{
    public class RectangularMaze : RowColumnBasedMaze
    {
        private readonly Random _random = new Random();

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
                
                // Setup adjacent walls
                var northernCell = row > 0 ? GetCell(row - 1, column) : null;
                if (northernCell != null)
                {
                    var wall = new CellWall(cell, northernCell);
                    cell.CellWalls.Add(wall);
                    northernCell.CellWalls.Add(wall);
                }
                
                var westernCell = column > 0 ? GetCell(row, column - 1) : null;
                if (westernCell != null)
                {
                    var wall = new CellWall(cell, westernCell);
                    cell.CellWalls.Add(wall);
                    westernCell.CellWalls.Add(wall);
                }

                Cells[index] = cell;
                CellIndexMap[cell] = index;
            }

            SetupWalls(setupAlgorithm);
            SetStartingAndEndingCells();
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

        private void SetStartingAndEndingCells()
        {
            var startingRow = _random.Next(0, RowCount);
            StartingCell = GetCell(startingRow, 0);
            
            var distanceInfo = CellDistanceSolver.GetPassableDistancesFromCell(StartingCell);
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