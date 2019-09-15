using System;
using System.Runtime.InteropServices;
using MazePlayground.App.MonoGame.Config;
using MazePlayground.Common.Mazes;
using MazePlayground.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkiaSharp;

namespace MazePlayground.App.MonoGame
{
    public class MazeRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private Texture2D _currentMazeTexture = null;

        public MazeRenderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);
        }

        public void Draw()
        {
            if (_currentMazeTexture != null)
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(_currentMazeTexture, Vector2.Zero, Color.White);
                _spriteBatch.End();
            }
        }

        public void LoadMaze(GridMazeConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var maze = new GridMaze(config.RowCount, config.ColumnCount, config.WallSetupAlgorithm);
            using (var image = SkiaMazeRenderer.Render(maze))
            {
                RenderImageToTexture2D(image);
            }
        }

        private void RenderImageToTexture2D(SKImage image)
        {
            var pixelMap = image.PeekPixels();
            var pointer = pixelMap.GetPixels();
            var pixels = new byte[image.Height * pixelMap.RowBytes];

            Marshal.Copy(pointer, pixels, 0, pixels.Length);
            _currentMazeTexture = new Texture2D(_graphicsDevice, image.Width, image.Height);
            _currentMazeTexture.SetData(pixels);
        }
    }
}