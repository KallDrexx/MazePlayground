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

        public LogicController(MazeConfigWindow mazeConfigWindow, MazeRenderer mazeRenderer)
        {
            _mazeConfigWindow = mazeConfigWindow;
            _mazeRenderer = mazeRenderer;
        }

        public void ExecuteLogic()
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

            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
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
                // No longer shifting
                _mousePositionLastFrame = null;
            }

            var scrollWheelChange = mouseState.ScrollWheelValue - _scrollWheelLastFrame;
            _scrollWheelLastFrame = mouseState.ScrollWheelValue;
            _mazeRenderer.ScaleRenderedMaze(scrollWheelChange);
        }
    }
}