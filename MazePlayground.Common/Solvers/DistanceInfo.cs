using System.Collections.Generic;
using MazePlayground.Common.Mazes;

namespace MazePlayground.Common.Solvers
{
    public class DistanceInfo
    {
        public Dictionary<Cell, int> DistanceFromStartMap { get; }
        public Cell FarthestCell { get; }
        
        public DistanceInfo(Dictionary<Cell, int> distanceFromStartMap, Cell farthestCell)
        {
            DistanceFromStartMap = distanceFromStartMap;
            FarthestCell = farthestCell;
        }
    }
}