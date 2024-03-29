using System;
using System.Collections.Generic;
using System.Linq;
using MazePlayground.Common.Solvers;
using MazePlayground.Common.WallSetup;

namespace MazePlayground.Common.Mazes
{
    public class MaskedMaze : RowColumnBasedMaze
    {
        private readonly Random _random = new Random();

        public override IReadOnlyList<Cell> AllCells => Cells.Where(x => x != null).ToArray();

        public MaskedMaze(int rowCount, int columnCount, IReadOnlyList<bool> mask, WallSetupAlgorithm algorithm)
        {
            if (mask == null) throw new ArgumentNullException(nameof(mask));
            if (rowCount * columnCount != mask.Count)
            {
                throw new ArgumentException($"Expected mask to have {rowCount * columnCount} values, instead had {mask.Count}");
            }

            RowCount = rowCount;
            ColumnCount = columnCount;
            Cells = new Cell[rowCount * columnCount];
            for (var row = 0; row < rowCount; row++)
            for (var column = 0; column < columnCount; column++)
            {
                var index = (row * columnCount) + column;
                if (mask[index])
                {
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
            }
            
            VerifyMazeIsValid();
            SetupWalls(algorithm);
            SetStartingAndFinishingCell();
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
            
            var distanceInfo = CellDistanceSolver.GetPassableDistancesFromCell(StartingCell);
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