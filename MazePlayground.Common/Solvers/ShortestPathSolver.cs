using System;
using System.Collections.Generic;
using System.Linq;
using MazePlayground.Common.Mazes;

namespace MazePlayground.Common.Solvers
{
    public static class ShortestPathSolver
    {
        public static ShortestPathInfo Solve(Cell finishingCell, DistanceInfo distanceInfo)
        {
            if (distanceInfo == null) throw new ArgumentNullException(nameof(distanceInfo));
            if (distanceInfo.DistanceFromStartMap == null) throw new ArgumentNullException(nameof(distanceInfo.DistanceFromStartMap));

            var reversedPath = new List<Cell>(new[] {finishingCell});
            var currentCell = finishingCell;
            var nextCellWasFound = true;
            while (nextCellWasFound)
            {
                nextCellWasFound = false;
                var distance = distanceInfo.DistanceFromStartMap[currentCell];
                foreach (var linkedCell in currentCell.LinkIdToCellMap.Values)
                {
                    if (distanceInfo.DistanceFromStartMap.ContainsKey(linkedCell))
                    {
                        var linkedDistance = distanceInfo.DistanceFromStartMap[linkedCell];
                        if (distance - linkedDistance == 1)
                        {
                            reversedPath.Add(linkedCell);
                            currentCell = linkedCell;
                            nextCellWasFound = true;
                            break;
                        }
                    }
                }
            }
            
            return new ShortestPathInfo(reversedPath.ToArray().Reverse().ToArray());
        }
    }
}