using System;
using MazePlayground.Common.Mazes;
using SkiaSharp;

namespace MazePlayground.Common.Rendering
{
    public static class SkiaMazeRenderer
    {
        public static SKImage Render(GridMaze maze, RenderOptions renderOptions, SolutionData solution)
        {
            const int margin = 10;
            const int cellLineWidth = 1;
            const int cellSize = 25;
            const int labelMarginX = (int) (cellSize * 0.33);
            const int labelMarginY = (int) (cellSize * 0.75);
            
            if (maze == null) throw new ArgumentNullException(nameof(maze));

            renderOptions = renderOptions ?? new RenderOptions();

            var imageWidth = (cellSize * maze.ColumnCount) + (margin * 2);
            var imageHeight = (cellSize * maze.RowCount) + (margin * 2);
            
            var imageInfo = new SKImageInfo(imageWidth, imageHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(imageInfo))
            {
                surface.Canvas.Clear(SKColors.Black);

                var whitePaint = new SKPaint {Color = SKColors.White, StrokeWidth = cellLineWidth};
                var startPaint = new SKPaint {Color = SKColors.Green, StrokeWidth = cellLineWidth};
                var finishPaint = new SKPaint {Color = SKColors.Red, StrokeWidth = cellLineWidth};
                var pathPaint = new SKPaint {Color = SKColors.Yellow, StrokeWidth = cellLineWidth};

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

                    if (renderOptions.HighlightShortestPath && solution.CellsInShortestPath.Contains(cell))
                    {
                        var paint = cell == maze.StartingCell ? startPaint
                            : cell == maze.FinishingCell ? finishPaint
                            : pathPaint;

                        var distance = solution.DistanceFromStartMap[cell];
                        surface.Canvas.DrawText(distance.ToString(), leftX + labelMarginX, topY + labelMarginY, paint);
                    }
                    else if (renderOptions.ShowAllDistances && solution.DistanceFromStartMap.ContainsKey(cell))
                    {
                        var distance = solution.DistanceFromStartMap[cell];
                        surface.Canvas.DrawText(distance.ToString(), leftX + labelMarginX, topY + labelMarginY, whitePaint);
                    }
                    else if (cell == maze.StartingCell)
                    {
                        surface.Canvas.DrawText("S", leftX + labelMarginX, topY + labelMarginY, startPaint);
                    }
                    else if (cell == maze.FinishingCell)
                    {
                        surface.Canvas.DrawText("E", leftX + labelMarginX, topY + labelMarginY, finishPaint);
                    }
                }

                return surface.Snapshot();
            }
        }
    }
}