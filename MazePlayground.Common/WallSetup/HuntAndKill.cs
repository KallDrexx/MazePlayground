using System;
using System.Collections.Generic;
using System.Linq;
using MazePlayground.Common.Mazes;

namespace MazePlayground.Common.WallSetup
{
    public class HuntAndKill
    {
        private readonly Random _random = new Random();

        public void Run(IMaze maze)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));
            
            // Start at a random cell and carve out a path going through other unvisited cells.  If we get stuck
            // (all neighbors are visited) then pick an unvisited cell that borders at least one visited cell, 
            // link them together, then carve a new path through unvisited cells until we get stuck again.

            var allCells = maze.AllCells;
            var visitedCells = new HashSet<Cell>();
            var currentCell = allCells[_random.Next(0, allCells.Count)];
            visitedCells.Add(currentCell);
            
            while (visitedCells.Count < allCells.Count)
            {
                var walls = currentCell.CellWalls
                    .Where(x => !visitedCells.Contains(x.GetOtherCell(currentCell)))
                    .ToArray();

                if (walls.Length == 0)
                {
                    foreach (var cell in allCells)
                    {
                        if (visitedCells.Contains(cell))
                        {
                            continue;
                        }

                        var visitedWalls = cell.CellWalls
                            .Where(x => visitedCells.Contains(x.GetOtherCell(cell)))
                            .ToArray();

                        if (visitedWalls.Any())
                        {
                            var visitedWall = visitedWalls[_random.Next(0, visitedWalls.Length)];
                            visitedWall.IsPassable = true;
                            visitedCells.Add(cell);
                            currentCell = cell;
                            break;
                        }
                    }
                }
                else
                {
                    var wall = walls[_random.Next(0, walls.Length)];
                    wall.IsPassable = true;
                    visitedCells.Add(wall.GetOtherCell(currentCell));
                    currentCell = wall.GetOtherCell(currentCell);
                }
            }
        }
    }
}