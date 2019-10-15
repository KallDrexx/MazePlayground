using System;
using System.Linq;
using MazePlayground.Common.Mazes;
using MazePlayground.Common.Solvers;
using SkiaSharp;

namespace MazePlayground.Common.Rendering
{
    public static class HexMazeSkiaRenderer
    {
        private const int Margin = 20;
        private const int CellLineWidth = 2;
        private const int CornerRadius = 12;

        public static SKImage RenderWithSkia(this HexMaze maze,
            RenderOptions renderOptions,
            DistanceInfo distanceInfo,
            ShortestPathInfo shortestPathInfo)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));

            renderOptions = renderOptions ?? new RenderOptions();
            
            var rightCorner = GetCoords(CornerRadius, 0);
            var bottomRightCorner = GetCoords(CornerRadius, 60);
            var bottomLeftCorner = GetCoords(CornerRadius, 120);
            var leftCorner = GetCoords(CornerRadius, 180);
            var topLeftCorner = GetCoords(CornerRadius, 240);
            var topRightCorner = GetCoords(CornerRadius, 300);
                
            var halfHeight = (int)Math.Abs(topRightCorner.y);
            var xOffset = (int) (Math.Cos(30) * (halfHeight * 2));

            var imageWidth = ((CornerRadius - xOffset) * 2 * maze.ColumnCount) + (Margin * 2);
            var imageHeight = (halfHeight * 2 * maze.RowCount) + (Margin * 2) + halfHeight;
            var imageInfo = new SKImageInfo(imageWidth, imageHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(imageInfo))
            {
                surface.Canvas.Clear(SKColors.Black);
                
                var whitePaint = new SKPaint {Color = SKColors.White, StrokeWidth = CellLineWidth};
                var startPaint = new SKPaint {Color = SKColors.Green, StrokeWidth = CellLineWidth};
                var finishPaint = new SKPaint {Color = SKColors.Red, StrokeWidth = CellLineWidth};
                var pathPaint = new SKPaint {Color = SKColors.Yellow, StrokeWidth = CellLineWidth};

                foreach (var cell in maze.AllCells)
                {
                    var cellCenter = GetCellCenter(maze, cell, halfHeight, xOffset);
                    
                    // All walls starting from bottom right and going clockwise
                    var isWallOpen = new[] {false, false, false, false, false, false};
                    foreach (var cellWall in cell.CellWalls)
                    {
                        if (cellWall.IsPassable)
                        {
                            var otherCellCenter = GetCellCenter(maze, cellWall.GetOtherCell(cell), halfHeight, xOffset);
                            var angle = GetNormalizedDegreesToCell(cellCenter, otherCellCenter);
                            var index = angle / 60;
                            isWallOpen[index] = true;
                        }
                    }

                    if (!isWallOpen[0])
                    {
                        surface.Canvas.DrawLine(rightCorner.x + cellCenter.x, rightCorner.y + cellCenter.y, bottomRightCorner.x + cellCenter.x, bottomRightCorner.y + cellCenter.y, whitePaint);
                    }
                    if (!isWallOpen[1])
                    {
                        surface.Canvas.DrawLine(bottomRightCorner.x + cellCenter.x, bottomRightCorner.y + cellCenter.y, bottomLeftCorner.x + cellCenter.x, bottomLeftCorner.y + cellCenter.y, whitePaint);
                    }
                    if (!isWallOpen[2])
                    {
                        surface.Canvas.DrawLine(bottomLeftCorner.x + cellCenter.x, bottomLeftCorner.y + cellCenter.y, leftCorner.x + cellCenter.x, leftCorner.y + cellCenter.y, whitePaint);
                    }
                    if (!isWallOpen[3])
                    {
                        surface.Canvas.DrawLine(leftCorner.x + cellCenter.x, leftCorner.y + cellCenter.y, topLeftCorner.x + cellCenter.x, topLeftCorner.y + cellCenter.y, whitePaint);
                    }
                    if (!isWallOpen[4])
                    {
                        surface.Canvas.DrawLine(topLeftCorner.x + cellCenter.x, topLeftCorner.y + cellCenter.y, topRightCorner.x + cellCenter.x, topRightCorner.y + cellCenter.y, whitePaint);
                    }
                    if (!isWallOpen[5])
                    {
                        surface.Canvas.DrawLine(topRightCorner.x + cellCenter.x, topRightCorner.y + cellCenter.y, rightCorner.x + cellCenter.x, rightCorner.y + cellCenter.y, whitePaint);
                    }
                }

                return surface.Snapshot();
            }
        }

        private static (int x, int y) GetCellCenter(HexMaze maze, Cell cell, int halfHeight, int xOffset)
        {
            var (row, column) = maze.GetPositionOfCell(cell);
            var centerX = (column * (CornerRadius - xOffset) * 2) + (CornerRadius / 2) + Margin;
            var centerY = (row * halfHeight * 2) + halfHeight + Margin;
            if (column % 2 == 1)
            {
                centerY += halfHeight;
            }

            return (centerX, centerY);
        }

        private static int GetNormalizedDegreesToCell((int x, int y) first, (int x, int y) second)
        {
            var height = second.y - first.y;
            var width = second.x - first.x;
            var angle = Math.Atan2(height, width) * (180 / Math.PI);
            
            // Normalize the angles since we've lost some precision due to x/y position being ints
            if (angle < 0) angle += 360;
            if (angle >= 360) angle -= 360;

            angle = Math.Round(angle);
            var expectedDegrees = Enumerable.Range(0, 6).Select(x => (x * 60) + 30).ToArray();
            foreach (var expectedDegree in expectedDegrees)
            {
                if (Math.Abs(expectedDegree - angle) <= 1)
                {
                    return expectedDegree;
                }
            }
            
            throw new InvalidOperationException($"Unexpected angle of {angle}");
        }
        
        private static (float x, float y) GetCoords(int radius, float angle)
            => (radius * (float)Math.Cos(ToRadians(angle)), radius * (float)Math.Sin(ToRadians(angle)));
        
        private static float ToRadians(float angle) => (float)(Math.PI / 180) * angle;
    }
}