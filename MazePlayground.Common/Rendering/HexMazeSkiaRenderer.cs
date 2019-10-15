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
        private const int CornerRadius = 20;

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
                
                var whitePaint = new SKPaint {Color = SKColors.White, StrokeWidth = CellLineWidth, TextAlign = SKTextAlign.Center};
                var startPaint = new SKPaint {Color = SKColors.Green, StrokeWidth = 1, TextAlign = SKTextAlign.Center};
                var finishPaint = new SKPaint {Color = SKColors.Red, StrokeWidth = 1, TextAlign = SKTextAlign.Center};
                var pathPaint = new SKPaint {Color = SKColors.Yellow, StrokeWidth = 1, TextAlign = SKTextAlign.Center};

                foreach (var cell in maze.AllCells)
                {
                    var cellCenter = GetCellCenter(maze, cell, halfHeight, xOffset);

                    var corners = new[]
                    {
                        new SKPoint(rightCorner.x + cellCenter.x, rightCorner.y + cellCenter.y),
                        new SKPoint(bottomRightCorner.x + cellCenter.x, bottomRightCorner.y + cellCenter.y),
                        new SKPoint(bottomLeftCorner.x + cellCenter.x, bottomLeftCorner.y + cellCenter.y),
                        new SKPoint(leftCorner.x + cellCenter.x, leftCorner.y + cellCenter.y),
                        new SKPoint(topLeftCorner.x + cellCenter.x, topLeftCorner.y + cellCenter.y),
                        new SKPoint(topRightCorner.x + cellCenter.x, topRightCorner.y + cellCenter.y),
                    };
                    
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

                    if (renderOptions.ShowGradientOfDistanceFromStart)
                    {
                        var finishingCellDistance = distanceInfo.DistanceFromStartMap[distanceInfo.FarthestCell];
                        var currentCellDistance = distanceInfo.DistanceFromStartMap[cell];
                        var intensity = (byte)(255 * (currentCellDistance / (decimal)finishingCellDistance));
                        var color = new SKColor(0, 0, intensity);
                        var paint = new SKPaint {Color = color, Style = SKPaintStyle.Fill};
                        
                        var path = new SKPath();
                        path.MoveTo(corners[0]);
                        path.LineTo(corners[1]);
                        path.LineTo(corners[2]);
                        path.LineTo(corners[3]);
                        path.LineTo(corners[4]);
                        path.LineTo(corners[5]);
                        path.LineTo(corners[0]);
                        
                        surface.Canvas.DrawPath(path, paint);
                    }

                    for (var x = 0; x < isWallOpen.Length; x++)
                    {
                        if (!isWallOpen[x])
                        {
                            var first = corners[x];
                            var second = corners[x < 5 ? x + 1 : 0];
                            surface.Canvas.DrawLine(first, second, whitePaint);
                        }
                    }
                    
                    if (renderOptions.HighlightShortestPath && shortestPathInfo.IsCellInPath(cell))
                    {
                        var paint = cell == maze.StartingCell ? startPaint
                            : cell == maze.FinishingCell ? finishPaint
                            : pathPaint;

                        var distance = distanceInfo.DistanceFromStartMap[cell];
                        surface.Canvas.DrawText(distance.ToString(), cellCenter.x, cellCenter.y + halfHeight / 2, paint);
                    }
                    else if (renderOptions.ShowAllDistances && distanceInfo.DistanceFromStartMap.ContainsKey(cell))
                    {
                        var distance = distanceInfo.DistanceFromStartMap[cell];
                        surface.Canvas.DrawText(distance.ToString(), cellCenter.x, cellCenter.y + halfHeight / 2, whitePaint);
                    }
                    else if (cell == maze.StartingCell)
                    {
                        surface.Canvas.DrawText("S", cellCenter.x, cellCenter.y + halfHeight / 2, startPaint);
                    }
                    else if (cell == maze.FinishingCell)
                    {
                        surface.Canvas.DrawText("E", cellCenter.x, cellCenter.y + halfHeight / 2, finishPaint);
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