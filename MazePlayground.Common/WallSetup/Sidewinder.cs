using System;
using System.Collections.Generic;
using System.Linq;
using MazePlayground.Common.Mazes;

namespace MazePlayground.Common.WallSetup
{
    public class Sidewinder
    {
        private readonly Random _random = new Random();

        public void Run(IMaze maze)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));
            
            // Iterate through each cell and add it to a collection of visited cells.  We have a 50% chance of carving
            // out the straight wall.  If we succeed in carving straight then move onto the next cell.  If we don't
            // carve straight (either due to failing the random check or no straight wall exists) then look at the
            // previous cells in the visited collection, pick one at random and carve out in the non-straight direction.
            // Then clear the visited collection and continue on.
            //
            // This algorithm is only meant for rectangular mazes with no masking/or holes.
            
            var rectMaze = (RectangularMaze) maze;
            var potentialNorthernPasses = new List<CellWall>();
            foreach (var cell in rectMaze.Cells)
            {
                var neighbors = GetNeighbors(rectMaze, cell);
                if (neighbors.North == null && neighbors.East == null)
                {
                    // top-right cell, nothing we can do from here
                }
                else if (neighbors.North == null)
                {
                    // Top row, just go east.  No need to add to the visited list since this column can be ignored
                    neighbors.East.IsPassable = true;
                }
                else if (neighbors.East == null)
                {
                    // Last column in the non-northern row
                    potentialNorthernPasses.Add(neighbors.North);
                    var cellWallFromSet = potentialNorthernPasses[_random.Next(0, potentialNorthernPasses.Count)];
                    cellWallFromSet.IsPassable = true;
                    potentialNorthernPasses.Clear();
                }
                else
                {
                    // Non top row and not last column
                    potentialNorthernPasses.Add(neighbors.North);
                    var carveEast = _random.Next(0, 2) == 0;
                    if (carveEast)
                    {
                        neighbors.East.IsPassable = true;
                        potentialNorthernPasses.Add(neighbors.North);
                    }
                    else
                    {
                        var cellWallFromSet = potentialNorthernPasses[_random.Next(0, potentialNorthernPasses.Count)];
                        cellWallFromSet.IsPassable = true;
                        potentialNorthernPasses.Clear();
                    }
                }
            }
        }

        private static Neighbors GetNeighbors(RectangularMaze maze, Cell cell)
        {
            var position = maze.GetPositionOfCell(cell);
            var neighborPositions = cell.CellWalls
                .Select(x => new {wall = x, position = maze.GetPositionOfCell(x.GetOtherCell(cell))})
                .ToArray();

            var north = neighborPositions.Where(x => x.position.row == position.row - 1).Select(x => x.wall).FirstOrDefault();
            var east = neighborPositions.Where(x => x.position.column == position.column + 1).Select(x => x.wall).FirstOrDefault();
            return new Neighbors(north, east);
        }

        private struct Neighbors
        {
            public readonly CellWall North;
            public readonly CellWall East;

            public Neighbors(CellWall north, CellWall east)
            {
                North = north;
                East = east;
            }
        }
    }
}