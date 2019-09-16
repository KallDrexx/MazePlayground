using System.Collections.Generic;
using MazePlayground.Common.Mazes;

namespace MazePlayground.Common
{
    public class SolutionData
    {
        public Cell[] ShortestPath { get; }
        public HashSet<Cell> CellsInShortestPath { get; }
        public Dictionary<Cell, int> DistanceFromStartMap { get; }
        
        public SolutionData(Dictionary<Cell, int> distanceFromStartMap, Cell[] shortestPath)
        {
            DistanceFromStartMap = distanceFromStartMap;
            ShortestPath = shortestPath;
            CellsInShortestPath = shortestPath != null ? new HashSet<Cell>(shortestPath) : null;
        }
    }
}