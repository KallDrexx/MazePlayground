using System;
using System.Collections.Generic;
using System.Linq;
using MazePlayground.Common.Mazes;

namespace MazePlayground.Common
{
    public static class MazeSolver
    {
        public static Dictionary<Cell, int> GetDistanceFromStartMap(IMaze maze)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));
            if (maze.StartingCell == null) throw new ArgumentNullException(nameof(maze.StartingCell));

            var map = new Dictionary<Cell, int>();
            var cellsToCheck = new List<Cell>(new[] {maze.StartingCell});
            MapCellDistance(cellsToCheck, map);

            return map;
        }

        public static Cell[] GetShortestPath(IMaze maze)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));
            if (maze.StartingCell == null) throw new ArgumentNullException(nameof(maze.StartingCell));
            if (maze.FinishingCell == null) throw new ArgumentNullException(nameof(maze.FinishingCell));
            
            var map = new Dictionary<Cell, int>();
            var cellsToCheck = new List<Cell>(new[] {maze.StartingCell});
            MapCellDistance(cellsToCheck, map, maze.FinishingCell);
            
            // Find the path by going in decreasing distances
            var reversePath = new List<Cell>();
            var currentCell = maze.FinishingCell;
            while (currentCell != maze.StartingCell)
            {
                reversePath.Add(currentCell);
                var previousCell = (Cell) null;
                var previousCellDistance = int.MaxValue;
                foreach (var cell in currentCell.LinkIdToCellMap.Values)
                {
                    if (!map.ContainsKey(cell))
                    {
                        // We short circuited Dijkstra before we got to this cell, so this cell definitely isn't
                        // on the shortest path
                        continue;
                    }
                    
                    var distance = map[cell];
                    
                    if (previousCell == null)
                    {
                        previousCell = cell;
                        previousCellDistance = distance;
                    }
                    else
                    {
                        if (previousCellDistance > distance)
                        {
                            previousCell = cell;
                            previousCellDistance = distance;
                        }
                    }
                }

                if (previousCell == null)
                {
                    throw new InvalidOperationException("No previous cell found for current cell");
                }

                currentCell = previousCell;
            }

            return reversePath.ToArray().Reverse().ToArray();
        }

        private static void MapCellDistance(List<Cell> cellsToCheck, Dictionary<Cell, int> map, Cell endingCell = null)
        {
            var distance = 0;
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

                    if (endingCell != null && cell == endingCell)
                    {
                        // We've reached the end of the maze
                        return;
                    }
                    
                    nextCellsToCheck.AddRange(cell.LinkIdToCellMap.Values);
                }

                cellsToCheck = nextCellsToCheck;
                distance++;
            }
        }
    }
}