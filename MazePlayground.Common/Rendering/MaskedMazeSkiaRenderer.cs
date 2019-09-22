using System;
using MazePlayground.Common.Mazes;
using MazePlayground.Common.Solvers;
using SkiaSharp;

namespace MazePlayground.Common.Rendering
{
    public static class MaskedMazeSkiaRenderer
    {
        private const int Margin = 10;
        private const int CellLineWidth = 1;
        private const int CellSize = 25;
        private const int LabelMarginX = (int) (CellSize * 0.25);
        private const int LabelMarginY = (int) (CellSize * 0.75);
        
        public static SKImage RenderWithSkia(this MaskedMaze maze,
            RenderOptions renderOptions,
            DistanceInfo distanceInfo,
            ShortestPathInfo shortestPathInfo)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));
            
            renderOptions = renderOptions ?? new RenderOptions();

            var imageWidth = (CellSize * maze.ColumnCount) + (Margin * 2);
            var imageHeight = (CellSize * maze.RowCount) + (Margin * 2);
            
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
                    var (row, column) =  maze.GetPositionOfCell(cell);
                    var leftX = (column * CellSize) + Margin;
                    var rightX = leftX + CellSize;
                    var topY = (row * CellSize) + Margin;
                    var bottomY = topY + CellSize;

                    var isNorthFacingExit = (maze.FinishingCell == cell || maze.StartingCell == cell) && row == 0 && column > 1;
                    var isSouthFacingExit = (maze.FinishingCell == cell || maze.StartingCell == cell) && row == maze.RowCount - 1 && column > 1;
                    var isEastFacingExit = (maze.FinishingCell == cell || maze.StartingCell == cell) && column == maze.ColumnCount - 1;
                    var isWestFacingExit = (maze.FinishingCell == cell || maze.StartingCell == cell) && column == 0;

                    if (maze.GetCellLinkedInDirection(cell, MaskedMaze.Direction.North) == null && !isNorthFacingExit)
                    {
                        surface.Canvas.DrawLine(leftX, topY, rightX, topY, whitePaint);
                    }

                    if (maze.GetCellLinkedInDirection(cell, MaskedMaze.Direction.South) == null && !isSouthFacingExit)
                    {
                        surface.Canvas.DrawLine(leftX, bottomY, rightX, bottomY, whitePaint);
                    }

                    if (maze.GetCellLinkedInDirection(cell, MaskedMaze.Direction.East) == null && !isEastFacingExit)
                    {
                        surface.Canvas.DrawLine(rightX, topY, rightX, bottomY, whitePaint);
                    }

                    if (maze.GetCellLinkedInDirection(cell, MaskedMaze.Direction.West) == null && !isWestFacingExit)
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
                        surface.Canvas.DrawText(distance.ToString(), leftX + LabelMarginX, topY + LabelMarginY, paint);
                    }
                    else if (renderOptions.ShowAllDistances && distanceInfo.DistanceFromStartMap.ContainsKey(cell))
                    {
                        var distance = distanceInfo.DistanceFromStartMap[cell];
                        surface.Canvas.DrawText(distance.ToString(), leftX + LabelMarginX, topY + LabelMarginY, whitePaint);
                    }
                    else if (cell == maze.StartingCell)
                    {
                        surface.Canvas.DrawText("S", leftX + LabelMarginX, topY + LabelMarginY, startPaint);
                    }
                    else if (cell == maze.FinishingCell)
                    {
                        surface.Canvas.DrawText("E", leftX + LabelMarginX, topY + LabelMarginY, finishPaint);
                    }
                }

                return surface.Snapshot();
            }
        }
    }
}