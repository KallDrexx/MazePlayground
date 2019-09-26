using System;
using System.Collections.Generic;
using System.IO;
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

                var centerCell = maze.AllCells[0];
                var wallsToRender = new Queue<CellWall>(centerCell.CellWalls);
                var renderedWalls = new HashSet<CellWall>();
                while (wallsToRender.Any())
                {
                    var wall = wallsToRender.Dequeue();
                    if (renderedWalls.Contains(wall))
                    {
                        continue;
                    }

                    var firstPosition = maze.GetPositionOfCell(wall.First);
                    var secondPosition = maze.GetPositionOfCell(wall.Second);
                    if (firstPosition.RingNumber == secondPosition.RingNumber)
                    {
                        // These cells border on the same ring, so find the angle that's the same and draw a line for it
                        var angle = Math.Abs(NormalizeAngle(firstPosition.StartingDegree) - NormalizeAngle(secondPosition.EndingDegree)) < 0.001
                            ? firstPosition.StartingDegree
                            : firstPosition.EndingDegree;

                        var lowerRing = Math.Min(firstPosition.RingNumber, secondPosition.RingNumber);
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
                        var innerRing = Math.Min(firstPosition.RingNumber, secondPosition.RingNumber);
                        var radius = GetRadiusAtRing(innerRing);
                        var bounds = new SKRect(imageCenter - radius, 
                            imageCenter - radius, 
                            imageCenter + radius, 
                            imageCenter + radius);
                        
                        var startingDegree = Math.Max(firstPosition.StartingDegree, secondPosition.EndingDegree);
                        var endingDegree = Math.Min(firstPosition.EndingDegree, secondPosition.StartingDegree);
                        
                        var path = new SKPath();
                        path.AddArc(bounds, startingDegree, endingDegree - startingDegree);
                        surface.Canvas.DrawPath(path, whitePaint);
                    }

                    renderedWalls.Add(wall);
                    foreach (var nextWall in wall.First.CellWalls.Concat(wall.Second.CellWalls))
                    {
                        wallsToRender.Enqueue(nextWall);
                    }
                }
                
                // Draw outer boundary
                // TOOD: add exit
                surface.Canvas.DrawCircle(imageCenter, imageCenter, GetRadiusAtRing(maze.RingCount - 1), whitePaint);
                
                return surface.Snapshot();
            }
        }

        private static (float x, float y) GetCoords(int radius, float angle)
            => (radius * (float)Math.Cos(ToRadians(angle)), radius * (float)Math.Sin(ToRadians(angle)));

        private static int GetRadiusAtRing(int ring) => CellWidth * ring + CenterCellRadius;

        private static float ToRadians(float angle) => (float)(Math.PI / 180) * angle;

        private static float NormalizeAngle(float angle) => angle >= 360 ? angle - 360f : angle;
    }
}