using System;
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
        private Texture2D _xnaTexture;
        private IntPtr _imGuiTexture;

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

        protected override void LoadContent()
        {
            // First, load the texture as a Texture2D (can also be done using the XNA/FNA content pipeline)
            _xnaTexture = CreateTexture(GraphicsDevice, 300, 150, pixel =>
            {
                var red = (pixel % 300) / 2;
                return new Color(red, 1, 1);
            });

            // Then, bind it to an ImGui-friendly pointer, that we can use during regular ImGui.** calls (see below)
            _imGuiTexture = _imGuiRenderer.BindTexture(_xnaTexture);
            base.LoadContent();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            _logicController.ExecuteLogic();
            
            _mazeRenderer.Draw();
            
            _imGuiRenderer.BeforeLayout(gameTime);
            _mazeConfigWindow.Render();
            _imGuiRenderer.AfterLayout();

            base.Draw(gameTime);
        }
        
        private static Texture2D CreateTexture(GraphicsDevice device, int width, int height, Func<int, Color> paint)
		{
			//initialize a texture
			var texture = new Texture2D(device, width, height);

			//the array holds the color for each pixel in the texture
			Color[] data = new Color[width * height];
			for(var pixel = 0; pixel < data.Length; pixel++)
			{
				//the function applies the color according to the specified pixel
				data[pixel] = paint( pixel );
			}

			//set the color
			texture.SetData( data );

			return texture;
		}
    }
}