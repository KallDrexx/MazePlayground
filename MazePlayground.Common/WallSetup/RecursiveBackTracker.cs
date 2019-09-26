using System;
using System.Collections.Generic;
using System.Linq;
using MazePlayground.Common.Mazes;

namespace MazePlayground.Common.WallSetup
{
    public class RecursiveBackTracker
    {
        private readonly Random _random = new Random();

        public void Run(IMaze maze)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));
            
            // Start from any cell, carve out a random path only going through unvisited cells.  If
            // we reach a dead end backtrack until we find a cell with an unvisited neighbor and continue

            var allCells = maze.AllCells;
            var visitedCells = new HashSet<Cell>();
            var path = new Stack<Cell>();

            var currentCell = allCells[_random.Next(0, allCells.Count)];
            visitedCells.Add(currentCell);
            path.Push(currentCell);

            while (visitedCells.Count < allCells.Count)
            {
                var walls = currentCell.CellWalls
                    .Where(x => !visitedCells.Contains(x.GetOtherCell(currentCell)))
                    .ToArray();

                if (walls.Any())
                {
                    var wall = walls[_random.Next(0, walls.Length)];
                    wall.IsPassable = true;
                    var adjacentCell = wall.GetOtherCell(currentCell);
                    visitedCells.Add(adjacentCell);
                    path.Push(adjacentCell);
                    currentCell = adjacentCell;
                }
                else
                {
                    currentCell = path.Pop();
                }
            }
        }
    }
}