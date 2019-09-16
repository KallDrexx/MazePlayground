using System.IO;
using MazePlayground.Common;
using MazePlayground.Common.Mazes;
using MazePlayground.Common.Rendering;
using MazePlayground.Common.Solvers;
using SkiaSharp;

namespace MazePlayground.App.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            const string pngFileName = "rendering.png";
            
            var maze = new GridMaze(20, 20, GridMaze.WallSetupAlgorithm.Sidewinder);
            var mazeDistanceInfo = CellDistanceSolver.GetDistancesFromCell(maze.StartingCell);
            var mazeShortestPathInfo = ShortestPathSolver.Solve(maze.FinishingCell, mazeDistanceInfo);
            using (var image = SkiaMazeRenderer.Render(maze, null, mazeDistanceInfo, mazeShortestPathInfo))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite(pngFileName))
            {
                data.SaveTo(stream);
            }
        }
    }
}