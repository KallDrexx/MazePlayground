using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using SkiaSharp;

namespace MazePlayground.App.MonoGame
{
    public static class MonoGameUtils
    {
        public static Texture2D RenderImageToTexture2D(SKImage image, GraphicsDevice graphicsDevice)
        {
            var pixelMap = image.PeekPixels();
            var pointer = pixelMap.GetPixels();
            var pixels = new byte[image.Height * pixelMap.RowBytes];

            Marshal.Copy(pointer, pixels, 0, pixels.Length);
            var texture = new Texture2D(graphicsDevice, image.Width, image.Height);
            texture.SetData(pixels);

            return texture;
        }
    }
}