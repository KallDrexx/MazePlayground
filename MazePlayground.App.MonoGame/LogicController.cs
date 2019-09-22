using System.Collections.Generic;
using System.Diagnostics;
using MazePlayground.App.MonoGame.Config;
using MazePlayground.App.MonoGame.Ui;
using MazePlayground.Common;
using MazePlayground.Common.Mazes;
using MazePlayground.Common.WallSetup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkiaSharp;

namespace MazePlayground.App.MonoGame
{
    public class LogicController
    {
        private readonly MazeConfigWindow _mazeConfigWindow;
        private readonly MaskCreationWindow _maskCreationWindow;
        private readonly MazeRenderer _mazeRenderer;
        private readonly GraphicsDevice _graphicsDevice;
        private Point? _mousePositionLastFrame;
        private int _scrollWheelLastFrame;
        private bool _f1WasDown, _f2WasDown;

        public LogicController(MazeConfigWindow mazeConfigWindow, 
            MazeRenderer mazeRenderer, 
            MaskCreationWindow maskCreationWindow, 
            GraphicsDevice graphicsDevice)
        {
            _mazeConfigWindow = mazeConfigWindow;
            _mazeRenderer = mazeRenderer;
            _maskCreationWindow = maskCreationWindow;
            _graphicsDevice = graphicsDevice;
        }

        public void ExecuteLogic()
        {
            HandleGuiEvents();
            HandleInput();
        }

        private void HandleGuiEvents()
        {
            if (_mazeConfigWindow.GenerateButtonPressed)
            {
                GenerateMaze();
            }

            if (_mazeConfigWindow.RenderingOptionsChanged)
            {
                _mazeRenderer.SetRenderOptions(_mazeConfigWindow.RenderingOptions);
            }

            if (_mazeConfigWindow.ResetMazePositionPressed)
            {
                _mazeRenderer.ResetMazePositionAndScaling();
            }

            if (_mazeConfigWindow.ShowMaskEditorButtonPressed)
            {
                _maskCreationWindow.ToggleWindow();
            }

            if (_maskCreationWindow.UpdateMaskButtonPressed)
            {
                _maskCreationWindow.ToggleWindow();
                RenderMask();
            }
        }

        private void HandleInput()
        {
            var mouseState = Mouse.GetState();
            var anyWindowHasFocus = _maskCreationWindow.WindowHasFocus || _mazeConfigWindow.WindowHasFocus;
            
            if (mouseState.LeftButton == ButtonState.Pressed && !anyWindowHasFocus)
            {
                // Either starting or continuing a drag
                if (_mousePositionLastFrame == null)
                {
                    _mousePositionLastFrame = mouseState.Position;
                }

                var moveAmount = mouseState.Position - _mousePositionLastFrame.Value;
                _mazeRenderer.MoveRenderedMaze(moveAmount);

                _mousePositionLastFrame = mouseState.Position;
            }
            else
            {
                // No longer panning
                _mousePositionLastFrame = null;
            }

            var scrollWheelChange = mouseState.ScrollWheelValue - _scrollWheelLastFrame;
            _scrollWheelLastFrame = mouseState.ScrollWheelValue;
            _mazeRenderer.ScaleRenderedMaze(scrollWheelChange);

            if (Keyboard.GetState().IsKeyDown(Keys.F1))
            {
                _f1WasDown = true;
            }
            else if (_f1WasDown)
            {
                _mazeConfigWindow.ToggleDemoWindow();
                _f1WasDown = false;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.F2))
            {
                _f2WasDown = true;
            }
            else if (_f2WasDown)
            {
                _mazeConfigWindow.ToggleMetricsWindow();
                _f2WasDown = false;
            }
        }

        private void GenerateMaze()
        {
            switch (_mazeConfigWindow.MazeType)
            {
                case MazeType.Rectangular:
                {
                    var config = _mazeConfigWindow.RectangularMazeConfig;
                    
                    var stopwatch = Stopwatch.StartNew();
                    var maze = new RectangularMaze(config.RowCount, config.ColumnCount, config.WallSetupAlgorithm);
                    stopwatch.Stop();
                    
                    var stats = new MazeStats(maze, config.WallSetupAlgorithm);
                    stats.AddCustomStat("Generation Time", $"{stopwatch.ElapsedMilliseconds}ms");
                    
                    _mazeRenderer.LoadMaze(maze, stats);
                    _mazeConfigWindow.SetMazeStats(stats.Entries);
                    break;
                }
                
                case MazeType.Masked:
                {
                    var maze = new MaskedMaze(_maskCreationWindow.RowCount, _maskCreationWindow.ColumnCount, _maskCreationWindow.MaskData, WallSetupAlgorithm.RecursiveBackTracker);
                    _mazeRenderer.LoadMaze(maze, null);
                    break;
                }
            }
        }

        private void RenderMask()
        {
            const int scaleFactor = 3;
            var mask = _maskCreationWindow.MaskData;
            var imageInfo = new SKImageInfo(_maskCreationWindow.ColumnCount * scaleFactor, _maskCreationWindow.RowCount * scaleFactor, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(imageInfo))
            {
                surface.Canvas.Clear(SKColors.Black);
                var white = new SKPaint {Color = SKColors.White};
                for (var x = 0; x < mask.Count; x++)
                {
                    if (!mask[x])
                    {
                        continue;
                    }
                    
                    var row = x / _maskCreationWindow.ColumnCount;
                    var column = x % _maskCreationWindow.ColumnCount;
                    surface.Canvas.DrawRect(column * scaleFactor, row * scaleFactor, scaleFactor, scaleFactor, white);
                }

                using (var image = surface.Snapshot())
                {
                    var texture2d = MonoGameUtils.RenderImageToTexture2D(image, _graphicsDevice);
                    _mazeConfigWindow.SetMaskTexture(texture2d);
                }
            }
        }
    }
}