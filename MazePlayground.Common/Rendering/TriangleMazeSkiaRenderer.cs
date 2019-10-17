using System;
using System.Linq;
using MazePlayground.Common.Mazes;
using MazePlayground.Common.Solvers;
using SkiaSharp;

namespace MazePlayground.Common.Rendering
{
    public static class TriangleMazeSkiaRenderer
    {
        private const int Margin = 10;
        private const int CellLineWidth = 2;
        private const int CellSideLength = 40;
        private const int HalfWidth = CellSideLength / 2;
        private static readonly int CellHeight = (int)Math.Sqrt(Math.Pow(CellSideLength, 2) - Math.Pow(HalfWidth, 2));
        private static readonly int HalfHeight = CellHeight / 2;

        public static SKImage RenderWithSkia(this TriangleMaze maze,
            RenderOptions renderOptions,
            DistanceInfo distanceInfo,
            ShortestPathInfo shortestPathInfo)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));

            renderOptions = renderOptions ?? new RenderOptions();
            
            // All corner adjustments. Image starts 0,0 top left and goes downward, therefore top has to be adjusted
            // by negative from center while bottom adjusted by positive from center.  
            var topMid = (x: 0, y: -HalfHeight);
            var bottomRight = (x: HalfWidth, y: HalfHeight);
            var bottomLeft = (x: -HalfWidth, y: HalfHeight);
            var bottomMid = (x: 0, y: HalfHeight);
            var topLeft = (x: -HalfWidth, y: -HalfHeight);
            var topRight = (x: HalfWidth, y: -HalfHeight);

            var imageWidth = (Margin * 2) + (maze.ColumnCount * HalfWidth) + HalfWidth;
            var imageHeight = (Margin * 2) + (maze.RowCount * CellHeight);
            var imageInfo = new SKImageInfo(imageWidth, imageHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(imageInfo))
            {
                surface.Canvas.Clear(SKColors.Black);
                
                var whitePaint = new SKPaint {Color = SKColors.White, StrokeWidth = CellLineWidth, TextAlign = SKTextAlign.Center};
                var startPaint = new SKPaint {Color = SKColors.Green, StrokeWidth = 1, TextAlign = SKTextAlign.Center};
                var finishPaint = new SKPaint {Color = SKColors.Red, StrokeWidth = 1, TextAlign = SKTextAlign.Center};
                var pathPaint = new SKPaint {Color = SKColors.Yellow, StrokeWidth = 1, TextAlign = SKTextAlign.Center};

                foreach (var cell in maze.AllCells)
                {
                    var (row, column) = maze.GetPositionOfCell(cell);
                    var centerX = Margin + (column * HalfWidth) + HalfWidth;
                    var centerY = Margin + (row * CellHeight) + HalfHeight;
                    var center = (x: centerX, y: centerY);
                    
                    if (renderOptions.ShowGradientOfDistanceFromStart)
                    {
                        var finishingCellDistance = distanceInfo.DistanceFromStartMap[distanceInfo.FarthestCell];
                        var currentCellDistance = distanceInfo.DistanceFromStartMap[cell];
                        var intensity = (byte)(255 * (currentCellDistance / (decimal)finishingCellDistance));
                        var color = new SKColor(0, 0, intensity);
                        var paint = new SKPaint {Color = color, Style = SKPaintStyle.Fill};
                        
                        var path = new SKPath();
                        if (TriangleMaze.IsCellPointingUp(row, column))
                        {
                            path.MoveTo(topMid.x + centerX, topMid.y + centerY);
                            path.LineTo(bottomRight.x + centerX, bottomRight.y + centerY);
                            path.LineTo(bottomLeft.x + centerX, bottomLeft.y + centerY);
                            path.LineTo(topMid.x + centerX, topMid.y + centerY);
                        }
                        else
                        {
                            path.MoveTo(bottomMid.x + centerX, bottomMid.y + centerY);
                            path.LineTo(topLeft.x + centerX, topLeft.y + centerY);
                            path.LineTo(topRight.x + centerX, topRight.y + centerY);
                            path.LineTo(bottomMid.x + centerX, bottomMid.y + centerY);
                        }
                        
                        surface.Canvas.DrawPath(path, paint);
                    }

                    var passableWalls = new[] {false, false, false}; // left, right, vertical
                    foreach (var cellWall in cell.CellWalls.Where(x => x.IsPassable))
                    {
                        var otherCellPosition = maze.GetPositionOfCell(cellWall.GetOtherCell(cell));
                        if (otherCellPosition.row == row && otherCellPosition.column < column)
                        {
                            passableWalls[0] = true;
                        }
                        else if (otherCellPosition.row == row && otherCellPosition.column > column)
                        {
                            passableWalls[1] = true;
                        }
                        else
                        {
                            passableWalls[2] = true;
                        }
                    }
                    
                    if (TriangleMaze.IsCellPointingUp(row, column))
                    {
                        if (!passableWalls[0])
                        {
                            surface.Canvas.DrawLine(GetCenteredPoint(bottomLeft, center), GetCenteredPoint(topMid, center), whitePaint);
                        }

                        if (!passableWalls[1])
                        {
                            surface.Canvas.DrawLine(GetCenteredPoint(topMid, center), GetCenteredPoint(bottomRight, center), whitePaint);
                        }

                        if (!passableWalls[2])
                        {
                            surface.Canvas.DrawLine(GetCenteredPoint(bottomRight, center), GetCenteredPoint(bottomLeft, center), whitePaint);
                        }
                    }
                    else
                    {
                        if (!passableWalls[0])
                        {
                            surface.Canvas.DrawLine(GetCenteredPoint(bottomMid, center), GetCenteredPoint(topLeft, center), whitePaint);
                        }

                        if (!passableWalls[1])
                        {
                            surface.Canvas.DrawLine(GetCenteredPoint(topRight, center), GetCenteredPoint(bottomMid, center), whitePaint);
                        }

                        if (!passableWalls[2])
                        {
                            surface.Canvas.DrawLine(GetCenteredPoint(topLeft, center), GetCenteredPoint(topRight, center), whitePaint);
                        }
                    }

                    var textYAdjust = TriangleMaze.IsCellPointingUp(row, column)
                        ? HalfHeight / 3
                        : -HalfHeight / 3;
                    
                    if (renderOptions.HighlightShortestPath && shortestPathInfo.IsCellInPath(cell))
                    {
                        var paint = cell == maze.StartingCell ? startPaint
                            : cell == maze.FinishingCell ? finishPaint
                            : pathPaint;

                        var distance = distanceInfo.DistanceFromStartMap[cell];
                        surface.Canvas.DrawText(distance.ToString(), center.x, center.y + textYAdjust, paint);
                    }
                    else if (renderOptions.ShowAllDistances && distanceInfo.DistanceFromStartMap.ContainsKey(cell))
                    {
                        var distance = distanceInfo.DistanceFromStartMap[cell];
                        surface.Canvas.DrawText(distance.ToString(), center.x, center.y + textYAdjust, whitePaint);
                    }
                    else if (cell == maze.StartingCell)
                    {
                        surface.Canvas.DrawText("S", center.x, center.y + textYAdjust, startPaint);
                    }
                    else if (cell == maze.FinishingCell)
                    {
                        surface.Canvas.DrawText("E", center.x, center.y + textYAdjust, finishPaint);
                    }
                }
                
                return surface.Snapshot();
            }
        }

        private static SKPoint GetCenteredPoint((float x, float y) origin, (float x, float y) center)
            => new SKPoint(origin.x + center.x, origin.y + center.y);
    }
}