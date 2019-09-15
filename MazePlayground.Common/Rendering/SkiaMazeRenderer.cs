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
            const int labelMarginX = (int) (cellSize * 0.33);
            const int labelMarginY = (int) (cellSize * 0.75);

            var imageWidth = (cellSize * maze.ColumnCount) + (margin * 2);
            var imageHeight = (cellSize * maze.RowCount) + (margin * 2);

            var imageInfo = new SKImageInfo(imageWidth, imageHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(imageInfo))
            {
                surface.Canvas.Clear(SKColors.Black);

                var whitePaint = new SKPaint {Color = SKColors.White, StrokeWidth = cellLineWidth};

                foreach (var cell in maze.Cells)
                {
                    var (row, column) =  maze.GetPositionOfCell(cell);
                    var leftX = (column * cellSize) + margin;
                    var rightX = leftX + cellSize;
                    var topY = (row * cellSize) + margin;
                    var bottomY = topY + cellSize;

                    if (maze.GetCellLinkedInDirection(cell, GridMaze.Direction.North) == null)
                    {
                        surface.Canvas.DrawLine(leftX, topY, rightX, topY, whitePaint);
                    }

                    if (maze.GetCellLinkedInDirection(cell, GridMaze.Direction.South) == null)
                    {
                        surface.Canvas.DrawLine(leftX, bottomY, rightX, bottomY, whitePaint);
                    }

                    if (maze.GetCellLinkedInDirection(cell, GridMaze.Direction.East) == null)
                    {
                        surface.Canvas.DrawLine(rightX, topY, rightX, bottomY, whitePaint);
                    }

                    if (maze.GetCellLinkedInDirection(cell, GridMaze.Direction.West) == null)
                    {
                        surface.Canvas.DrawLine(leftX, topY, leftX, bottomY, whitePaint);
                    }

                    if (cell == maze.StartingCell)
                    {
                        surface.Canvas.DrawText("S", leftX + labelMarginX, topY + labelMarginY, whitePaint);
                    }

                    if (cell == maze.FinishingCell)
                    {
                        surface.Canvas.DrawText("E", leftX + labelMarginX, topY + labelMarginY, whitePaint);
                    }
                }

                return surface.Snapshot();
            }
        }
    }
}