using System;
using MazePlayground.Common.Mazes;
using MazePlayground.Common.Solvers;
using SkiaSharp;

namespace MazePlayground.Common.Rendering
{
    public static class SkiaMazeRenderer
    {
        public static SKImage Render(GridMaze maze, RenderOptions renderOptions, DistanceInfo distanceInfo, ShortestPathInfo shortestPathInfo)
        {
            const int margin = 10;
            const int cellLineWidth = 1;
            const int cellSize = 25;
            const int labelMarginX = (int) (cellSize * 0.25);
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

                    var isNorthFacingExit = (maze.FinishingCell == cell || maze.StartingCell == cell) && row == 0 && column > 1;
                    var isSouthFacingExit = (maze.FinishingCell == cell || maze.StartingCell == cell) && row == maze.RowCount - 1 && column > 1;
                    var isEastFacingExit = (maze.FinishingCell == cell || maze.StartingCell == cell) && column == maze.ColumnCount - 1;
                    var isWestFacingExit = (maze.FinishingCell == cell || maze.StartingCell == cell) && column == 0;

                    if (maze.GetCellLinkedInDirection(cell, GridMaze.Direction.North) == null && !isNorthFacingExit)
                    {
                        surface.Canvas.DrawLine(leftX, topY, rightX, topY, whitePaint);
                    }

                    if (maze.GetCellLinkedInDirection(cell, GridMaze.Direction.South) == null && !isSouthFacingExit)
                    {
                        surface.Canvas.DrawLine(leftX, bottomY, rightX, bottomY, whitePaint);
                    }

                    if (maze.GetCellLinkedInDirection(cell, GridMaze.Direction.East) == null && !isEastFacingExit)
                    {
                        surface.Canvas.DrawLine(rightX, topY, rightX, bottomY, whitePaint);
                    }

                    if (maze.GetCellLinkedInDirection(cell, GridMaze.Direction.West) == null && !isWestFacingExit)
                    {
                        surface.Canvas.DrawLine(leftX, topY, leftX, bottomY, whitePaint);
                    }

                    if (renderOptions.ShowGradientOfDistanceFromStart)
                    {
                        const int shadingMargin = 2;
                        
                        var finishingCellDistance = distanceInfo.DistanceFromStartMap[distanceInfo.FarthestCell];
                        var currentCellDistance = distanceInfo.DistanceFromStartMap[cell];
                        var intensity = (byte)(255 * (currentCellDistance / (decimal)finishingCellDistance));
                        var color = new SKColor(0, 0, intensity);
                        var paint = new SKPaint {Color = color};
                        var shadeWidth = rightX - leftX - (shadingMargin * 2);
                        var shadeHeight = bottomY - topY - (shadingMargin * 2);
                        
                        surface.Canvas.DrawRect(leftX + shadingMargin, topY + shadingMargin, shadeWidth, shadeHeight, paint);
                    }
                    
                    if (renderOptions.HighlightShortestPath && shortestPathInfo.IsCellInPath(cell))
                    {
                        var paint = cell == maze.StartingCell ? startPaint
                            : cell == maze.FinishingCell ? finishPaint
                            : pathPaint;

                        var distance = distanceInfo.DistanceFromStartMap[cell];
                        surface.Canvas.DrawText(distance.ToString(), leftX + labelMarginX, topY + labelMarginY, paint);
                    }
                    else if (renderOptions.ShowAllDistances && distanceInfo.DistanceFromStartMap.ContainsKey(cell))
                    {
                        var distance = distanceInfo.DistanceFromStartMap[cell];
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