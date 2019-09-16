using MazePlayground.App.MonoGame.Config;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MazePlayground.App.MonoGame
{
    public class LogicController
    {
        private readonly MazeConfigWindow _mazeConfigWindow;
        private readonly MazeRenderer _mazeRenderer;
        private Point? _mousePositionLastFrame;
        private int _scrollWheelLastFrame;
        private bool _f1WasDown, _f2WasDown;

        public LogicController(MazeConfigWindow mazeConfigWindow, MazeRenderer mazeRenderer)
        {
            _mazeConfigWindow = mazeConfigWindow;
            _mazeRenderer = mazeRenderer;
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
                switch (_mazeConfigWindow.MazeType)
                {
                    case MazeType.Grid:
                        _mazeRenderer.LoadMaze(_mazeConfigWindow.GridMazeConfig);
                        break;
                }
            }

            if (_mazeConfigWindow.RenderingOptionsChanged)
            {
                _mazeRenderer.SetRenderOptions(_mazeConfigWindow.RenderingOptions);
            }

            if (_mazeConfigWindow.ResetMazePositionPressed)
            {
                _mazeRenderer.ResetMazePositionAndScaling();
            }
        }

        private void HandleInput()
        {
            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed && !_mazeConfigWindow.WindowHasFocus)
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
    }
}