using System;
using System.Collections.Generic;
using System.Linq;
using MazePlayground.Common.Mazes;

namespace MazePlayground.Common.WallSetup
{
    public class Wilson
    {
        private readonly Random _random = new Random();

        public void Run(IMaze maze)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));

            // Start with one cell marked as visited.  Pick a random non-visited cell and create a path of cells until
            // we get to a visited cell.  Carve out a path through the path to the visited cell, then repeat until all
            // cells are visited.  If a path loops around reset the path at the beginning of the loop.

            var allCells = maze.AllCells;
            var visitedCells = new HashSet<Cell>();
            var unvisitedCells = new HashSet<Cell>(allCells);
            var path = new List<CellWall>();

            var firstVisitedCell = unvisitedCells.ElementAt(_random.Next(0, unvisitedCells.Count));
            visitedCells.Add(firstVisitedCell);
            unvisitedCells.Remove(firstVisitedCell);

            var firstCellInPath = unvisitedCells.ElementAt(_random.Next(0, unvisitedCells.Count));
            var currentCell = firstCellInPath;
            while (unvisitedCells.Any())
            {
                var walls = currentCell.CellWalls;
                var wall = walls[_random.Next(0, walls.Count)];
                if (visitedCells.Contains(wall.GetOtherCell(currentCell)))
                {
                    // Carve out the path
                    var firstCell = firstCellInPath;
                    foreach (var wallToOpen in path)
                    {
                        wallToOpen.IsPassable = true;
                        visitedCells.Add(firstCell);
                        unvisitedCells.Remove(firstCell);
                        firstCell = wallToOpen.GetOtherCell(firstCell);
                    }

                    wall.IsPassable = true;
                    visitedCells.Add(firstCell);
                    unvisitedCells.Remove(firstCell);
                    path.Clear();
                    
                    // Choose a new starting cell
                    if (unvisitedCells.Any())
                    {
                        firstCellInPath = unvisitedCells.ElementAt(_random.Next(0, unvisitedCells.Count));
                        currentCell = firstCellInPath;
                    }
                    else
                    {
                        break;
                    }
                }
                else if (firstCellInPath == wall.GetOtherCell(currentCell))
                {
                    // Looped back to the beginning so clear the whole path
                    path.Clear();
                    currentCell = wall.GetOtherCell(currentCell);
                }
                else
                {
                    var pathIndex = path.Select((pathWall, index) => new {pathWall, index})
                        .Where(x => x.pathWall.GetOtherCell(currentCell) == wall.GetOtherCell(currentCell))
                        .Select(x => (int?) x.index)
                        .FirstOrDefault();

                    if (pathIndex != null)
                    {
                        // Path looped back to itself
                        for (var x = path.Count - 1; x > pathIndex; x--)
                        {
                            path.RemoveAt(x);
                        }
                    }
                    else
                    {
                        path.Add(wall);
                    }

                    currentCell = wall.GetOtherCell(currentCell);
                }
            }
        }
    }
}