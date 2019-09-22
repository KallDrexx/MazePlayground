using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using SkiaSharp;

namespace MazePlayground.App.MonoGame.Ui
{
    public class MaskedMazeGenerationDisplay
    {
        private readonly ImGuiRenderer _imGuiRenderer;
        private readonly GraphicsDevice _graphicsDevice;
        private Texture2D _maskTexture;
        private IntPtr _maskTexturePointer;

        public bool ShowMaskEditorButtonPressed { get; private set; }

        public MaskedMazeGenerationDisplay(ImGuiRenderer imGuiRenderer, GraphicsDevice graphicsDevice)
        {
            _imGuiRenderer = imGuiRenderer;
            _graphicsDevice = graphicsDevice;
        }
        
        public void SetMask(IReadOnlyList<bool> mask, int rowCount, int columnCount)
        {
            if (_maskTexture != null)
            {
                _imGuiRenderer.UnbindTexture(_maskTexturePointer);
            }
            
            _maskTexture = GenerateTextureForMask(mask, rowCount, columnCount);
            _maskTexturePointer = _imGuiRenderer.BindTexture(_maskTexture);
        }

        public void Render()
        {

            ShowMaskEditorButtonPressed = ImGui.Button("Open Mask Editor");

            if (_maskTexture != null)
            {
                var imageSize = new Vector2(_maskTexture.Width, _maskTexture.Height);
                ImGui.Image(_maskTexturePointer, imageSize);
            }
        }

        private Texture2D GenerateTextureForMask(IReadOnlyList<bool> mask, int columnCount, int rowCount)
        {
            const int scaleFactor = 3;
            var imageInfo = new SKImageInfo(columnCount * scaleFactor, rowCount * scaleFactor, SKColorType.Rgba8888, SKAlphaType.Premul);
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
                    
                    var row = x / columnCount;
                    var column = x % columnCount;
                    surface.Canvas.DrawRect(column * scaleFactor, row * scaleFactor, scaleFactor, scaleFactor, white);
                }

                using (var image = surface.Snapshot())
                {
                    return MonoGameUtils.RenderImageToTexture2D(image, _graphicsDevice);
                }
            }
        }
    }
}