using System.Diagnostics;
using System.IO;
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
            
            var maze = new GridMaze(6, 10);
            using (var image = SkiaMazeRenderer.Render(maze))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite(pngFileName))
            {
                data.SaveTo(stream);
            }
        }
    }
}