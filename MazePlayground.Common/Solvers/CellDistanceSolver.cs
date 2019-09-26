using System;
using System.Collections.Generic;
using System.Linq;
using MazePlayground.Common.Mazes;

namespace MazePlayground.Common.Solvers
{
    public static class CellDistanceSolver
    {
        public static DistanceInfo GetPassableDistancesFromCell(Cell startingCell)
        {
            if (startingCell == null) throw new ArgumentNullException(nameof(startingCell));
            
            var map = new Dictionary<Cell, int>();
            MapPassableCellDistance(startingCell, map, out var farthestCell);
            
            return new DistanceInfo(map, farthestCell);
        }

        public static DistanceInfo GetDistancesForUnlinkedMaze(IMaze maze)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));
            
            var map = new Dictionary<Cell, int>();
            MapUnlinkedCellDistance(maze, map, out var farthestCell);
            
            return new DistanceInfo(map, farthestCell);
        }
        
        private static void MapPassableCellDistance(Cell cellToCheck, Dictionary<Cell, int> map, out Cell farthestCell)
        {
            var distance = 0;
            var farthestCellDistance = 0;
            farthestCell = cellToCheck;

            var cellsToCheck = new[] {cellToCheck};    
            while (cellsToCheck.Any())
            {
                var nextCellsToCheck = new List<Cell>();
                foreach (var cell in cellsToCheck)
                {
                    if (map.ContainsKey(cell))
                    {
                        continue;
                    }

                    map.Add(cell, distance);
                    nextCellsToCheck.AddRange(cell.CellWalls.Where(x => x.IsPassable).Select(x => x.GetOtherCell(cell)));

                    if (distance > farthestCellDistance)
                    {
                        farthestCell = cell;
                        farthestCellDistance = distance;
                    }
                }

                cellsToCheck = nextCellsToCheck.ToArray();
                distance++;
            }
        }
        
        private static void MapUnlinkedCellDistance(IMaze maze, Dictionary<Cell, int> map, out Cell farthestCell)
        {
            var distance = 0;
            var farthestCellDistance = 0;
            farthestCell = maze.AllCells.First();

            var cellsToCheck = new[] {farthestCell};    
            while (cellsToCheck.Any())
            {
                var nextCellsToCheck = new List<Cell>();
                foreach (var cell in cellsToCheck)
                {
                    if (map.ContainsKey(cell))
                    {
                        continue;
                    }

                    map.Add(cell, distance);
                    nextCellsToCheck.AddRange(cell.CellWalls.Select(x => x.GetOtherCell(cell)));

                    if (distance > farthestCellDistance)
                    {
                        farthestCell = cell;
                        farthestCellDistance = distance;
                    }
                }

                cellsToCheck = nextCellsToCheck.ToArray();
                distance++;
            }
        }
    }
}