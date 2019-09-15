using MazePlayground.App.MonoGame.Config;

namespace MazePlayground.App.MonoGame
{
    public class LogicController
    {
        private readonly MazeConfigWindow _mazeConfigWindow;
        private readonly MazeRenderer _mazeRenderer;

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
        }
    }
}