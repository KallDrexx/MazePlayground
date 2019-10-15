﻿using System.IO;
using MazePlayground.Common;
using MazePlayground.Common.Mazes;
using MazePlayground.Common.Rendering;
using MazePlayground.Common.Solvers;
using MazePlayground.Common.WallSetup;
using SkiaSharp;

namespace MazePlayground.App.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            const string pngFileName = "rendering.png";
            
            var maze = new HexMaze(20, 20, WallSetupAlgorithm.RecursiveBackTracker);
            var mazeDistanceInfo = CellDistanceSolver.GetPassableDistancesFromCell(maze.StartingCell);
            var mazeShortestPathInfo = ShortestPathSolver.Solve(maze.FinishingCell, mazeDistanceInfo);
            var renderOptions = new RenderOptions
            {
                ShowGradientOfDistanceFromStart = true,
                HighlightShortestPath = true,
                //ShowAllDistances = true,
            };
            
            using (var image = maze.RenderWithSkia(renderOptions, mazeDistanceInfo, mazeShortestPathInfo))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite(pngFileName))
            {
                data.SaveTo(stream);
            }
        }
    }
}