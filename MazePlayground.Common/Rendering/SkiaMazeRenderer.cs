using System;
using MazePlayground.Common.Mazes;
using SkiaSharp;

namespace MazePlayground.Common.Rendering
{
    public static class SkiaMazeRenderer
    {
        public static SKImage Render(GridMaze maze)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));
            
            const int margin = 10;
            const int cellLineWidth = 1;
            const int cellSize = 25;

            var imageWidth = (cellSize * maze.ColumnCount) + (margin * 2);
            var imageHeight = (cellSize * maze.RowCount) + (margin * 2);

            var imageInfo = new SKImageInfo(imageWidth, imageHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(imageInfo))
            {
                surface.Canvas.Clear(SKColors.Black);
                
                var whiteLine = new SKPaint {Color = SKColors.Green, StrokeWidth = cellLineWidth};

                foreach (var cell in maze.Cells)
                {
                    var leftX = (cell.Column * cellSize) + margin;
                    var rightX = leftX + cellSize;
                    var topY = (cell.Row * cellSize) + margin;
                    var bottomY = topY + cellSize;

                    if (cell.North == null)
                    {
                        surface.Canvas.DrawLine(leftX, topY, rightX, topY, whiteLine);
                    }

                    if (cell.South == null)
                    {
                        surface.Canvas.DrawLine(leftX, bottomY, rightX, bottomY, whiteLine);
                    }

                    if (cell.East == null)
                    {
                        surface.Canvas.DrawLine(rightX, topY, rightX, bottomY, whiteLine);
                    }

                    if (cell.West == null)
                    {
                        surface.Canvas.DrawLine(leftX, topY, leftX, bottomY, whiteLine);
                    }
                }

                return surface.Snapshot();
            }
        }
    }
}