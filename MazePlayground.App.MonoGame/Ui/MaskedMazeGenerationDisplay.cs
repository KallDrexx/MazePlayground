using System;
using System.Numerics;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;

namespace MazePlayground.App.MonoGame.Ui
{
    public class MaskedMazeGenerationDisplay
    {
        private readonly ImGuiRenderer _imGuiRenderer;
        private Texture2D _maskTexture;
        private IntPtr _maskTexturePointer;

        public bool ShowMaskEditorButtonPressed { get; private set; }

        public MaskedMazeGenerationDisplay(ImGuiRenderer imGuiRenderer)
        {
            _imGuiRenderer = imGuiRenderer;
        }
        
        public void SetMaskTexture(Texture2D maskTexture)
        {
            if (_maskTexture != null)
            {
                _imGuiRenderer.UnbindTexture(_maskTexturePointer);
            }
            
            _maskTexture = maskTexture;
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
    }
}