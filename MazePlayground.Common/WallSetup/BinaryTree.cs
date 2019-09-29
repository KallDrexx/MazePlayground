using System;
using System.Linq;
using MazePlayground.Common.Mazes;

namespace MazePlayground.Common.WallSetup
{
    public class BinaryTree
    {
        private readonly Random _random = new Random();
        
        public void Run(IMaze maze)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));
            
            // For each cell we want a 50% chance to go one of two directions.  This algorithm only works for
            // unmasked rectangular mazes with no holes. 

            var rectMaze = (RectangularMaze) maze;
            foreach (var cell in rectMaze.AllCells)
            {
                var neighbors = GetNeighbors(rectMaze, cell);
                if (neighbors.North != null && neighbors.East == null)
                {
                    neighbors.North.IsPassable = true;
                }
                else if (neighbors.North == null && neighbors.East != null)
                {
                    neighbors.East.IsPassable = true;
                }
                else if (neighbors.North != null)
                {
                    var carveEast = _random.Next(0, 2) == 0;
                    if (carveEast)
                    {
                        neighbors.East.IsPassable = true;
                    }
                    else
                    {
                        neighbors.North.IsPassable = true;
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