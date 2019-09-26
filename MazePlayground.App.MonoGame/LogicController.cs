using System;
using System.Diagnostics;
using MazePlayground.App.MonoGame.Config;
using MazePlayground.App.MonoGame.Ui;
using MazePlayground.Common;
using MazePlayground.Common.Mazes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MazePlayground.App.MonoGame
{
    public class LogicController
    {
        private readonly MazeConfigWindow _mazeConfigWindow;
        private readonly MaskCreationWindow _maskCreationWindow;
        private readonly MazeRenderer _mazeRenderer;
        private readonly MessageDisplayWindow _messageDisplayWindow;
        private Point? _mousePositionLastFrame;
        private int _scrollWheelLastFrame;
        private bool _f1WasDown, _f2WasDown;

        public LogicController(MazeConfigWindow mazeConfigWindow, 
            MazeRenderer mazeRenderer, 
            MaskCreationWindow maskCreationWindow, 
            MessageDisplayWindow messageDisplayWindow)
        {
            _mazeConfigWindow = mazeConfigWindow;
            _mazeRenderer = mazeRenderer;
            _maskCreationWindow = maskCreationWindow;
            _messageDisplayWindow = messageDisplayWindow;
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
                try
                {
                    GenerateMaze();
                }
                catch (Exception ex)
                {
                    _messageDisplayWindow.ShowMessage("Error Generating Maze", ex.ToString());
                }
            }

            if (_mazeConfigWindow.RenderingOptionsChanged)
            {
                try
                {
                    _mazeRenderer.SetRenderOptions(_mazeConfigWindow.RenderingOptions);
                }
                catch (Exception ex)
                {
                    _messageDisplayWindow.ShowMessage("Error Updating Maze Rendering", ex.ToString());
                }
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
            var stopwatch = Stopwatch.StartNew();
            var maze = CreateMaze();
            stopwatch.Stop();
            
            var stats = new MazeStats(maze, _mazeConfigWindow.SelectedWallSetupAlgorithm);
            stats.AddCustomStat("Generation Time", $"{stopwatch.ElapsedMilliseconds}ms");
            
            _mazeRenderer.LoadMaze(maze, stats);
            _mazeConfigWindow.SetMazeStats(stats.Entries);
        }

        private IMaze CreateMaze()
        {
            switch (_mazeConfigWindow.MazeType)
            {
                case MazeType.Rectangular:
                    var rectangularMazeConfig = _mazeConfigWindow.RectangularMazeConfig;
                    return new RectangularMaze(rectangularMazeConfig.RowCount, 
                        rectangularMazeConfig.ColumnCount,
                        _mazeConfigWindow.SelectedWallSetupAlgorithm);
                
                case MazeType.Masked:
                    return new MaskedMaze(_maskCreationWindow.RowCount, 
                        _maskCreationWindow.ColumnCount, 
                        _maskCreationWindow.MaskData, 
                        _mazeConfigWindow.SelectedWallSetupAlgorithm);
                
                case MazeType.Circular:
                    return new CircularMaze(13, 12, 3);
                
                default:
                    throw new NotSupportedException($"No known way to create maze of type {_mazeConfigWindow.MazeType}");
            }
        }

        private void RenderMask()
        {
            var mask = _maskCreationWindow.MaskData;
            var rows = _maskCreationWindow.RowCount;
            var columns = _maskCreationWindow.ColumnCount;
            
            _mazeConfigWindow.SetMask(mask, rows, columns);
        }
    }
}