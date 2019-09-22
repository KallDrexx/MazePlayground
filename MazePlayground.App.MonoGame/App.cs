using MazePlayground.App.MonoGame.Ui;
using Microsoft.Xna.Framework;
using Num = System.Numerics;

namespace MazePlayground.App.MonoGame
{
    public class App : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private MazeConfigWindow _mazeConfigWindow;
        private MaskCreationWindow _maskCreationWindow;
        private MazeRenderer _mazeRenderer;
        private LogicController _logicController;
        private ImGuiRenderer _imGuiRenderer;
        private MessageDisplayWindow _messageDisplayWindow;

        public App()
        {
	        _graphics = new GraphicsDeviceManager(this)
	        {
		        PreferredBackBufferWidth = 1024, 
		        PreferredBackBufferHeight = 768,
		        PreferMultiSampling = true
	        };

	        IsMouseVisible = true;
        }
        
        protected override void Initialize()
        {
            _imGuiRenderer = new ImGuiRenderer(this);
            _imGuiRenderer.RebuildFontAtlas();
            
            _mazeConfigWindow = new MazeConfigWindow(_graphics.GraphicsDevice, _imGuiRenderer);
            _maskCreationWindow = new MaskCreationWindow(_graphics.GraphicsDevice);
            _mazeRenderer = new MazeRenderer(_graphics.GraphicsDevice);
            _messageDisplayWindow = new MessageDisplayWindow();
            _logicController = new LogicController(_mazeConfigWindow, _mazeRenderer, _maskCreationWindow,
	            _graphics.GraphicsDevice, _messageDisplayWindow);

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
	        _logicController.ExecuteLogic();
	        
	        base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            _mazeRenderer.Draw();
            
            _imGuiRenderer.BeforeLayout(gameTime);
            _mazeConfigWindow.Render();
            _maskCreationWindow.Render();
            _messageDisplayWindow.Render();
            _imGuiRenderer.AfterLayout();

            base.Draw(gameTime);
        }
    }
}