using System;
using System.Collections.Generic;
using System.Linq;
using MazePlayground.Common.Mazes;

namespace MazePlayground.Common.WallSetup
{
    public class AldousBroder
    {
        private readonly Random _random = new Random();

        public void Run(IMaze maze)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));
            
            // Start with a cell and carve a path through all other cells until every cell has been visited.  Only
            // carve out walls of a cell the first time it is visited.

            var visitedCells = new HashSet<Cell>();
            var allCells = maze.AllCells;
            var currentCell = allCells[_random.Next(0, allCells.Count)];
            visitedCells.Add(currentCell);
            
            while (visitedCells.Count < allCells.Count)
            {
                var walls = currentCell.CellWalls.ToArray();
                var wall = walls[_random.Next(0, walls.Length)];
                if (!visitedCells.Contains(wall.GetOtherCell(currentCell)))
                {
                    wall.IsPassable = true;
                    visitedCells.Add(wall.GetOtherCell(currentCell));
                }

                currentCell = wall.GetOtherCell(currentCell);
            }
        }
    }
}