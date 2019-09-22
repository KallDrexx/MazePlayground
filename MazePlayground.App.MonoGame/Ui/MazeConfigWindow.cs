using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using MazePlayground.App.MonoGame.Config;
using MazePlayground.Common.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace MazePlayground.App.MonoGame.Ui
{
    public class MazeConfigWindow
    {
        public const int WindowWidth = 250;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly MazeStatsDisplay _mazeStatsDisplay;
        private readonly RectangularMazeGenerationDisplay _rectangularMazeGenerationDisplay;
        private readonly MaskedMazeGenerationDisplay _maskedMazeGenerationDisplay;
        private readonly RenderOptionsDisplay _renderOptionsDisplay;
        
        private readonly string[] _mazeGridTypes;
        private int _selectedMazeTypeIndex;
        
        private bool _showDemoWindow;
        private bool _showMetricsWindow;

        public bool GenerateButtonPressed { get; private set; }
        public bool RenderingOptionsChanged => _renderOptionsDisplay.RenderingOptionsChanged;
        public bool WindowHasFocus { get; private set; }
        public bool ResetMazePositionPressed => _renderOptionsDisplay.ResetMazePositionPressed;
        public bool ShowMaskEditorButtonPressed => _maskedMazeGenerationDisplay.ShowMaskEditorButtonPressed;
        
        public MazeType MazeType => GetMazeType();
        public RectangularMazeConfig RectangularMazeConfig => GetRectangularMazeConfig();
        public RenderOptions RenderingOptions => GetRenderOptions();

        public MazeConfigWindow(GraphicsDevice graphicsDevice, ImGuiRenderer imGuiRenderer)
        {
            _graphicsDevice = graphicsDevice;
            _mazeStatsDisplay = new MazeStatsDisplay();
            _rectangularMazeGenerationDisplay = new RectangularMazeGenerationDisplay();
            _maskedMazeGenerationDisplay = new MaskedMazeGenerationDisplay(imGuiRenderer);
            _renderOptionsDisplay = new RenderOptionsDisplay();

            _mazeGridTypes = Enum.GetValues(typeof(MazeType))
                .Cast<MazeType>()
                .Select(x => x.ToString())
                .ToArray();
        }

        public void ToggleDemoWindow()
        {
            _showDemoWindow = !_showDemoWindow;
        }

        public void ToggleMetricsWindow()
        {
            _showMetricsWindow = !_showMetricsWindow;
        }

        public void SetMazeStats(IReadOnlyList<KeyValuePair<string, string>> stats)
        {
            _mazeStatsDisplay.SetMazeStats(stats);
        }

        public void SetMaskTexture(Texture2D maskTexture)
        {
            _maskedMazeGenerationDisplay.SetMaskTexture(maskTexture);
        }
        
        public void Render()
        {
            WindowHasFocus = ImGui.IsWindowFocused();
            
            var windowOptions = ImGuiWindowFlags.NoMove | 
                                ImGuiWindowFlags.NoResize | 
                                ImGuiWindowFlags.NoCollapse;
            
            ImGui.SetNextWindowPos(new Vector2(0), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(WindowWidth, _graphicsDevice.Viewport.Height));
            ImGui.Begin("Maze Configuration", windowOptions);

            if (ImGui.CollapsingHeader("Generation", ImGuiTreeNodeFlags.DefaultOpen))
            {
                RenderGenerationTab();
            }
            
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Spacing();

            if (ImGui.CollapsingHeader("Rendering", ImGuiTreeNodeFlags.DefaultOpen))
            {
                _renderOptionsDisplay.Render();
            }
            
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Spacing();

            _mazeStatsDisplay.Render();
            
            ImGui.End();
            
            if (_showDemoWindow) ImGui.ShowDemoWindow();
            if (_showMetricsWindow) ImGui.ShowMetricsWindow();
        }

        private void RenderGenerationTab()
        {
            ImGui.Combo("Maze Type", ref _selectedMazeTypeIndex, _mazeGridTypes, _mazeGridTypes.Length);
            
            if (_selectedMazeTypeIndex == 0)
            {
                _rectangularMazeGenerationDisplay.Render();
            }
            else if (_selectedMazeTypeIndex == 1)
            {
                _maskedMazeGenerationDisplay.Render();
            }
            
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Spacing();
            
            GenerateButtonPressed = ImGui.Button("Generate Maze");
        }

        private MazeType GetMazeType()
        {
            return Enum.GetValues(typeof(MazeType))
                .Cast<MazeType>()
                .Skip(_selectedMazeTypeIndex)
                .First();
        }

        private RectangularMazeConfig GetRectangularMazeConfig()
        {
            if (_selectedMazeTypeIndex != 0)
            {
                // Not a rectangular maze
                return null;
            }
            
            return new RectangularMazeConfig(_rectangularMazeGenerationDisplay.RowCount, 
                _rectangularMazeGenerationDisplay.ColumnCount, 
                _rectangularMazeGenerationDisplay.WallSetupAlgorithm);
        }

        private RenderOptions GetRenderOptions()
        {
            return new RenderOptions
            {
                HighlightShortestPath = _renderOptionsDisplay.HighlightShortestPath, 
                ShowAllDistances = _renderOptionsDisplay.ShowDistances,
                ShowGradientOfDistanceFromStart = _renderOptionsDisplay.ShadeDistancesFromStart,
            };
        }
    }
}