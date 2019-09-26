using System;
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

                foreach (var cell in maze.AllCells)
                {
                    var position = maze.GetPositionOfCell(cell);
                    if (position.RingNumber == 0)
                    {
                        // Special logic for the center cell since it's the only cell with 6 walls.  Let the non-center
                        // cells deal with the wall logic and just do shading/labeling for this cell
                    }
                    else
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

                        var innerStart = GetCoords(innerRadius, position.StartingDegree);
                        var innerEnd = GetCoords(innerRadius, position.EndingDegree);
                        var outerStart = GetCoords(outerRadius, position.StartingDegree);
                        var outerEnd = GetCoords(outerRadius, position.EndingDegree);
                        
                        var path = new SKPath();
                        path.AddArc(innerBounds, position.StartingDegree, position.EndingDegree);
                        path.AddArc(outerBounds, position.StartingDegree, position.EndingDegree);
                        surface.Canvas.DrawPath(path, whitePaint);
                        surface.Canvas.DrawLine(innerStart.x + imageCenter, 
                            innerStart.y + imageCenter,
                            outerStart.x + imageCenter, 
                            outerStart.y + imageCenter, 
                            whitePaint);
                        
                        surface.Canvas.DrawLine(innerEnd.x + imageCenter, 
                            innerEnd.y + imageCenter,
                            outerEnd.x + imageCenter, 
                            outerEnd.y + imageCenter, 
                            whitePaint);
                    }
                }
                
                return surface.Snapshot();
            }
        }

        private static (float x, float y) GetCoords(int radius, float angle)
            => (radius * (float)Math.Sin(ToRadians(angle)), radius * (float)Math.Cos(ToRadians(angle)));

        private static int GetRadiusAtRing(int ring) => CellWidth * ring + CenterCellRadius;

        private static float ToRadians(float angle) => (float)(Math.PI / 180) * angle;
    }
}