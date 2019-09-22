using System;
using System.Diagnostics;
using MazePlayground.App.MonoGame.Ui;
using MazePlayground.Common;
using MazePlayground.Common.Mazes;
using MazePlayground.Common.Rendering;
using MazePlayground.Common.Solvers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkiaSharp;

namespace MazePlayground.App.MonoGame
{
    public class MazeRenderer
    {
        private const string RenderTimeStatKey = "Render Time";
        
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private Texture2D _currentMazeTexture;
        private Rectangle _renderTarget;
        private RenderOptions _renderOptions;
        private IMaze _currentMaze;
        private MazeStats _currentStats;
        private DistanceInfo _mazeDistanceInfo;
        private ShortestPathInfo _mazeShortestPathInfo;

        public MazeRenderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);
            ResetMazePositionAndScaling();
        }

        public void Draw()
        {
            if (_currentMazeTexture != null)
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(_currentMazeTexture, _renderTarget, Color.White);
                _spriteBatch.End();
            }
        }

        public void LoadMaze(IMaze maze, MazeStats mazeStats)
        {
            _currentMaze = maze;
            _currentStats = mazeStats;
            _mazeDistanceInfo = CellDistanceSolver.GetDistancesFromCell(_currentMaze.StartingCell);
            _mazeShortestPathInfo = ShortestPathSolver.Solve(_currentMaze.FinishingCell, _mazeDistanceInfo);
            
            UpdateMazeRendering();
            ResetMazePositionAndScaling();
        }

        public void MoveRenderedMaze(Point moveBy)
        {
            _renderTarget.X += moveBy.X;
            _renderTarget.Y += moveBy.Y;
        }

        public void ScaleRenderedMaze(int scaleBy)
        {
            scaleBy = scaleBy / 2;
            
            _renderTarget.Width += scaleBy;
            _renderTarget.Height += scaleBy;

            if (_renderTarget.Width < 0)
            {
                _renderTarget.Width = 10;
            }

            if (_renderTarget.Height < 0)
            {
                _renderTarget.Height = 10;
            }
        }

        public void SetRenderOptions(RenderOptions renderOptions)
        {
            _renderOptions = renderOptions;
            UpdateMazeRendering();
        }

        public void ResetMazePositionAndScaling()
        {
            const int buffer = 15;
            
            if (_currentMazeTexture != null)
            {
                var startX = MazeConfigWindow.WindowWidth + buffer;
                _renderTarget = new Rectangle(startX, buffer, _currentMazeTexture.Width, _currentMazeTexture.Height);
            }
        }

        private void UpdateMazeRendering()
        {
            if (_currentMaze == null)
            {
                return;
            }
            
            var stopwatch = Stopwatch.StartNew();
            using (var image = GetImageForCurrentMaze())
            {
                _currentMazeTexture = MonoGameUtils.RenderImageToTexture2D(image, _graphicsDevice);
            }
            
            stopwatch.Stop();
            _currentStats.AddCustomStat(RenderTimeStatKey, $"{stopwatch.ElapsedMilliseconds}ms");
        }

        private SKImage GetImageForCurrentMaze()
        {
            switch (_currentMaze)
            {
                case RectangularMaze rectangularMaze:
                    return rectangularMaze.RenderWithSkia(_renderOptions, _mazeDistanceInfo, _mazeShortestPathInfo);
                
                case MaskedMaze maskedMaze:
                    return maskedMaze.RenderWithSkia(_renderOptions, _mazeDistanceInfo, _mazeShortestPathInfo);
                
                default:
                    throw new NotSupportedException($"Maze type {_currentMaze.GetType()} cannot be rendered");
            }
        }
    }
}