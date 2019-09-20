using System;
using System.Linq;
using MazePlayground.Common.Mazes;

namespace MazePlayground.Common.WallSetupAlgorithms
{
    public class BinaryTree
    {
        private readonly Random _random = new Random();
        
        public void Run(IMaze maze)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));
            
            // For each cell we want a 50% chance to go one of two directions.  This algorithm only works for
            // unmasked rectangular mazes with no holes.  We assume the first cell is a corner and use those
            // two directions as our cutout paths.

            var allCells = maze.AllCells;
            var firstCell = allCells[0];
            var wallsForCell = maze.GetWallsForCell(firstCell);
            if (wallsForCell.Count != 2)
            {
                throw new InvalidOperationException("First cell did not have exactly 2 neighbors");
            }

            var linkIds = new[] {wallsForCell[0].LinkId, wallsForCell[1].LinkId};
            foreach (var cell in allCells)
            {
                wallsForCell = maze.GetWallsForCell(cell)
                    .Where(x => x.LinkId == linkIds[0] || x.LinkId == linkIds[1])
                    .ToArray();

                CellWall wall;
                if (wallsForCell.Count == 2)
                {
                    wall = wallsForCell[_random.Next(0, 2)];
                }
                else if (wallsForCell.Count == 1)
                {
                    wall = wallsForCell[0];
                }
                else
                {
                    // Theoretically we should be bottom right
                    continue;
                }

                var oppositeLinkid = maze.GetOppositeLinkId(wall.LinkId);
                cell.LinkToOtherCell(wall.CellOnOtherSide, wall.LinkId);
                wall.CellOnOtherSide.LinkToOtherCell(cell, oppositeLinkid);
            }
        }
    }
}