using System.IO;
using MazePlayground.Common;
using MazePlayground.Common.Mazes;
using MazePlayground.Common.Rendering;
using SkiaSharp;

namespace MazePlayground.App.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            const string pngFileName = "rendering.png";
            
            var maze = new GridMaze(20, 20, GridMaze.WallSetupAlgorithm.Sidewinder);
            var solution = MazeSolver.Solve(maze);
            using (var image = SkiaMazeRenderer.Render(maze, null, solution))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite(pngFileName))
            {
                data.SaveTo(stream);
            }
        }
    }
}