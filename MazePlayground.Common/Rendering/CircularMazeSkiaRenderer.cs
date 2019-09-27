using System;
using System.Collections.Generic;
using System.Linq;
using MazePlayground.Common.Mazes;
using MazePlayground.Common.Solvers;
using SkiaSharp;

namespace MazePlayground.Common.Rendering
{
    public static class CircularMazeSkiaRenderer
    {
        private const int Margin = 10;
        private const int CellLineWidth = 1;
        private const int CellWidth = 25;
        private const int CenterCellRadius = 25;
        
        public static SKImage RenderWithSkia(this CircularMaze maze,
            RenderOptions renderOptions,
            DistanceInfo distanceInfo,
            ShortestPathInfo shortestPathInfo)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));
            
            renderOptions = renderOptions ?? new RenderOptions();

            var imageWidth = (GetRadiusAtRing(maze.RingCount) * 2) + (Margin * 2);
            var imageCenter = imageWidth / 2;

            var imageInfo = new SKImageInfo(imageWidth, imageWidth, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(imageInfo))
            {
                surface.Canvas.Clear(SKColors.Black);

                var whitePaint = new SKPaint {Color = SKColors.White, StrokeWidth = CellLineWidth, Style = SKPaintStyle.Stroke};

                var renderedWalls = new HashSet<CellWall>();
                foreach (var cell in maze.AllCells)
                {
                    var position = maze.GetPositionOfCell(cell);
                    DrawRenderOptions(renderOptions, distanceInfo, cell, position, imageCenter, surface);
                    RenderWallsOfCell(maze, cell, renderedWalls, position, surface, imageCenter, whitePaint);
                }

                return surface.Snapshot();
            }
        }

        private static void DrawRenderOptions(RenderOptions renderOptions, DistanceInfo distanceInfo, Cell cell,
            CircularMaze.CircularPosition position, int imageCenter, SKSurface surface)
        {
            if (renderOptions.ShowGradientOfDistanceFromStart)
            {
                var finishingCellDistance = distanceInfo.DistanceFromStartMap[distanceInfo.FarthestCell];
                var currentCellDistance = distanceInfo.DistanceFromStartMap[cell];
                var intensity = (byte) (255 * (currentCellDistance / (decimal) finishingCellDistance));
                var color = new SKColor(0, 0, intensity);
                var paint = new SKPaint {Color = color};

                var path = new SKPath();
                if (position.RingNumber > 0)
                {
                    var innerRadius = GetRadiusAtRing(position.RingNumber - 1);
                    var outerRadius = GetRadiusAtRing(position.RingNumber);
                    var innerBounds = new SKRect(imageCenter - innerRadius,
                        imageCenter - innerRadius,
                        imageCenter + innerRadius,
                        imageCenter + innerRadius);

                    var outerBounds = new SKRect(imageCenter - outerRadius,
                        imageCenter - outerRadius,
                        imageCenter + outerRadius,
                        imageCenter + outerRadius);

                    var degreeDifference = position.EndingDegree - position.StartingDegree;
                    var firstLineCoords = GetCoords(outerRadius, position.EndingDegree);
                    var secondLineCoords = GetCoords(innerRadius, position.StartingDegree);

                    path.AddArc(innerBounds, NormalizeAngle(position.StartingDegree), NormalizeAngle(degreeDifference));
                    path.LineTo(imageCenter + firstLineCoords.x, imageCenter + firstLineCoords.y);
                    path.ArcTo(outerBounds, position.EndingDegree, -degreeDifference, false);
                    path.LineTo(imageCenter + secondLineCoords.x, imageCenter + secondLineCoords.y);
                }

                surface.Canvas.DrawPath(path, paint);
            }
        }

        private static void RenderWallsOfCell(CircularMaze maze, 
            Cell cell, 
            HashSet<CellWall> renderedWalls, 
            CircularMaze.CircularPosition position,
            SKSurface surface, 
            int imageCenter, 
            SKPaint whitePaint)
        {
            foreach (var wall in cell.CellWalls)
            {
                if (renderedWalls.Contains(wall))
                {
                    continue;
                }

                if (!wall.IsPassable)
                {
                    var otherCellPosition = maze.GetPositionOfCell(wall.GetOtherCell(cell));
                    if (position.RingNumber == otherCellPosition.RingNumber)
                    {
                        // These cells border on the same ring, so find the angle that's the same and draw a line for it
                        var angle = Math.Abs(NormalizeAngle(position.StartingDegree) -
                                             NormalizeAngle(otherCellPosition.EndingDegree)) < 0.001
                            ? position.StartingDegree
                            : position.EndingDegree;

                        var lowerRing = Math.Min(position.RingNumber, otherCellPosition.RingNumber);
                        var innerRadius = GetRadiusAtRing(lowerRing - 1);
                        var outerRadius = GetRadiusAtRing(lowerRing);

                        var lineStart = GetCoords(innerRadius, angle);
                        var lineEnd = GetCoords(outerRadius, angle);
                        surface.Canvas.DrawLine(lineStart.x + imageCenter,
                            lineStart.y + imageCenter,
                            lineEnd.x + imageCenter,
                            lineEnd.y + imageCenter,
                            whitePaint);
                    }
                    else
                    {
                        var innerRing = Math.Min(position.RingNumber, otherCellPosition.RingNumber);
                        var radius = GetRadiusAtRing(innerRing);
                        var bounds = new SKRect(imageCenter - radius,
                            imageCenter - radius,
                            imageCenter + radius,
                            imageCenter + radius);

                        var startingDegree = Math.Max(position.StartingDegree, otherCellPosition.EndingDegree);
                        var endingDegree = Math.Min(position.EndingDegree, otherCellPosition.StartingDegree);

                        var path = new SKPath();
                        path.AddArc(bounds, startingDegree, endingDegree - startingDegree);
                        surface.Canvas.DrawPath(path, whitePaint);
                    }
                }

                renderedWalls.Add(wall);
            }
        }

        private static (float x, float y) GetCoords(int radius, float angle)
            => (radius * (float)Math.Cos(ToRadians(angle)), radius * (float)Math.Sin(ToRadians(angle)));

        private static int GetRadiusAtRing(int ring) => CellWidth * ring + CenterCellRadius;

        private static float ToRadians(float angle) => (float)(Math.PI / 180) * angle;

        private static float NormalizeAngle(float angle) => angle >= 360 ? angle - 360f : angle;
    }
}