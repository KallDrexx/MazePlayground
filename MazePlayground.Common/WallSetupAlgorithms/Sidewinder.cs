using System;
using System.Collections.Generic;
using System.Linq;
using MazePlayground.Common.Mazes;

namespace MazePlayground.Common.WallSetupAlgorithms
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
            // This algorithm is only meant for rectangular mazes with no masking/or holes.  It expects to start in a
            // corner with only 2 available directions and will pick which one is straight and which is not based on
            // those options.
            
            var allCells = maze.AllCells;
            var firstCell = allCells[0];
            var wallsForCell = maze.GetWallsForCell(firstCell);
            if (wallsForCell.Count != 2)
            {
                throw new InvalidOperationException("First cell did not have exactly 2 neighbors");
            }

            var straightLinkId = wallsForCell[0].LinkId;
            var nonStraightLinkId = wallsForCell[1].LinkId;
            var visitedCells = new List<Cell>();
            foreach (var cell in allCells)
            {
                wallsForCell = maze.GetWallsForCell(cell)
                    .Where(x => x.LinkId == straightLinkId || x.LinkId == nonStraightLinkId)
                    .ToArray();

                var hasStraightWall = wallsForCell.Any(x => x.LinkId == straightLinkId);
                var hasNonStraightWall = wallsForCell.Any(x => x.LinkId == nonStraightLinkId);

                if (!hasStraightWall && !hasNonStraightWall)
                {
                    // Most likely on last cell so ignore
                }
                else if (hasStraightWall && !hasNonStraightWall)
                {
                    // Most likely last section so just go straight
                    LinkCells(maze, cell, wallsForCell.Single(x => x.LinkId == straightLinkId));
                }
                else if (hasStraightWall && _random.Next(0, 2) == 0)
                {
                    // We have both a straight and non-straight wall, and randomization chose to go straight
                    visitedCells.Add(cell);
                    LinkCells(maze, cell, wallsForCell.Single(x => x.LinkId == straightLinkId));
                }
                else
                {
                    // We either don't have a straight wall (end of a row) or randomization told us not to go straight.
                    // If we got here it also means we *do* have a non-straight wall, and
                    visitedCells.Add(cell);
                    var cellFromSet = visitedCells[_random.Next(0, visitedCells.Count)];
                    var cellNonStraightWall = maze.GetWallsForCell(cellFromSet).Single(x => x.LinkId == nonStraightLinkId);
                    LinkCells(maze, cell, cellNonStraightWall);
                    
                    visitedCells.Clear();
                }
            }
        }

        private static void LinkCells(IMaze maze, Cell cell, CellWall wall)
        {
            var oppositeLinkId = maze.GetOppositeLinkId(wall.LinkId);
            cell.LinkToOtherCell(wall.CellOnOtherSide, wall.LinkId);
            wall.CellOnOtherSide.LinkToOtherCell(cell, oppositeLinkId);
        }
    }
}