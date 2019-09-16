using System;
using System.Collections.Generic;
using System.Linq;
using MazePlayground.Common.Mazes;

namespace MazePlayground.Common.Solvers
{
    public static class CellDistanceSolver
    {
        public static DistanceInfo GetDistancesFromCell(Cell startingCell)
        {
            if (startingCell == null) throw new ArgumentNullException(nameof(startingCell));
            
            var map = new Dictionary<Cell, int>();
            MapCellDistance(startingCell, map, out var farthestCell);
            
            return new DistanceInfo(map);
        }
        
        private static void MapCellDistance(Cell cellToCheck, Dictionary<Cell, int> map, out Cell farthestCell)
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
                    nextCellsToCheck.AddRange(cell.LinkIdToCellMap.Values);

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