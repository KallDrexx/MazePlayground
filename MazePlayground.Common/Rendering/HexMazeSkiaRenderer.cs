using System;
using MazePlayground.Common.Mazes;
using MazePlayground.Common.Solvers;
using SkiaSharp;

namespace MazePlayground.Common.Rendering
{
    public static class HexMazeSkiaRenderer
    {
        private const int Margin = 10;
        private const int CellLineWidth = 1;
        private const int CornerRadius = 12;

        public static SKImage RenderWithSkia(this HexMaze maze,
            RenderOptions renderOptions,
            DistanceInfo distanceInfo,
            ShortestPathInfo shortestPathInfo)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));

            renderOptions = renderOptions ?? new RenderOptions();

            var imageWidth = (CornerRadius * 2 * maze.ColumnCount) + (Margin * 2);
            var imageHeight = (CornerRadius * 2 * maze.RowCount) + (Margin * 2);
            var imageInfo = new SKImageInfo(imageWidth, imageHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(imageInfo))
            {
                surface.Canvas.Clear(SKColors.Black);
                
                var whitePaint = new SKPaint {Color = SKColors.White, StrokeWidth = CellLineWidth};
                
                var rightCorner = GetCoords(CornerRadius, 0);
                var bottomRightCorner = GetCoords(CornerRadius, 60);
                var bottomLeftCorner = GetCoords(CornerRadius, 120);
                var leftCorner = GetCoords(CornerRadius, 180);
                var topLeftCorner = GetCoords(CornerRadius, 240);
                var topRightCorner = GetCoords(CornerRadius, 300);
                
                foreach (var cell in maze.AllCells)
                {
                    var (row, column) = maze.GetPositionOfCell(cell);

                    var centerX = (column * CornerRadius * 2) + CornerRadius + Margin;
                    var centerY = (row * CornerRadius * 2) + CornerRadius + Margin;
                    if (column % 2 == 1)
                    {
                        centerY += CornerRadius;
                        //centerX += CornerRadius;
                    }
                    
                    surface.Canvas.DrawLine(rightCorner.x + centerX, rightCorner.y + centerY, bottomRightCorner.x + centerX, bottomRightCorner.y + centerY, whitePaint);
                    surface.Canvas.DrawLine(bottomRightCorner.x + centerX, bottomRightCorner.y + centerY, bottomLeftCorner.x + centerX, bottomLeftCorner.y + centerY, whitePaint);
                    surface.Canvas.DrawLine(bottomLeftCorner.x + centerX, bottomLeftCorner.y + centerY, leftCorner.x + centerX, leftCorner.y + centerY, whitePaint);
                    surface.Canvas.DrawLine(leftCorner.x + centerX, leftCorner.y + centerY, topLeftCorner.x + centerX, topLeftCorner.y + centerY, whitePaint);
                    surface.Canvas.DrawLine(topLeftCorner.x + centerX, topLeftCorner.y + centerY, topRightCorner.x + centerX, topRightCorner.y + centerY, whitePaint);
                    surface.Canvas.DrawLine(topRightCorner.x + centerX, topRightCorner.y + centerY, rightCorner.x + centerX, rightCorner.y + centerY, whitePaint);
                }

                return surface.Snapshot();
            }
        }
        
        private static (float x, float y) GetCoords(int radius, float angle)
            => (radius * (float)Math.Cos(ToRadians(angle)), radius * (float)Math.Sin(ToRadians(angle)));
        
        private static float ToRadians(float angle) => (float)(Math.PI / 180) * angle;
    }
}