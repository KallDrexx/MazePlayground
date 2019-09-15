using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Num = System.Numerics;

namespace MazePlayground.App.MonoGame
{
    public class App : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private MazeConfigWindow _mazeConfigWindow;
        private MazeRenderer _mazeRenderer;
        private LogicController _logicController;
        private ImGuiRenderer _imGuiRenderer;

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
            
            _mazeConfigWindow = new MazeConfigWindow();
            _mazeRenderer = new MazeRenderer(_graphics.GraphicsDevice);
            _logicController = new LogicController(_mazeConfigWindow, _mazeRenderer);

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
	        _logicController.ExecuteLogic();
	        
	        base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            _mazeRenderer.Draw();
            
            _imGuiRenderer.BeforeLayout(gameTime);
            _mazeConfigWindow.Render();
            _imGuiRenderer.AfterLayout();

            base.Draw(gameTime);
        }
    }
}