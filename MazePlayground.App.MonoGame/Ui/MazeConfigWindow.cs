using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using MazePlayground.App.MonoGame.Config;
using MazePlayground.Common.Rendering;
using MazePlayground.Common.WallSetup;
using Microsoft.Xna.Framework.Graphics;

namespace MazePlayground.App.MonoGame.Ui
{
    public class MazeConfigWindow
    {
        public const int WindowWidth = 250;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly MazeStatsDisplay _mazeStatsDisplay;
        private readonly RenderOptionsDisplay _renderOptionsDisplay;
        private readonly MazeGenerationConfigDisplay _mazeGenerationConfigDisplay;
        
        private bool _showDemoWindow;
        private bool _showMetricsWindow;

        public bool GenerateButtonPressed => _mazeGenerationConfigDisplay.GenerateButtonPressed;
        public bool RenderingOptionsChanged => _renderOptionsDisplay.RenderingOptionsChanged;
        public bool WindowHasFocus { get; private set; }
        public bool ResetMazePositionPressed => _renderOptionsDisplay.ResetMazePositionPressed;
        public bool ShowMaskEditorButtonPressed => _mazeGenerationConfigDisplay.ShowMaskEditorButtonPressed;

        public MazeType MazeType => _mazeGenerationConfigDisplay.MazeType;
        public RowColumnMazeConfig RowColumnMazeConfig => _mazeGenerationConfigDisplay.RowColumnMazeConfig;
        public CircularMazeConfig CircularMazeConfig => _mazeGenerationConfigDisplay.CircularMazeConfig;
        public WallSetupAlgorithm SelectedWallSetupAlgorithm => _mazeGenerationConfigDisplay.SelectedWallSetupAlgorithm;
        public RenderOptions RenderingOptions => GetRenderOptions();

        public MazeConfigWindow(GraphicsDevice graphicsDevice, ImGuiRenderer imGuiRenderer)
        {
            _graphicsDevice = graphicsDevice;
            _mazeStatsDisplay = new MazeStatsDisplay();
            _mazeGenerationConfigDisplay = new MazeGenerationConfigDisplay(graphicsDevice, imGuiRenderer);
            _renderOptionsDisplay = new RenderOptionsDisplay();
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

        public void SetMask(IReadOnlyList<bool> mask, int rowCount, int columnCount)
        {
            _mazeGenerationConfigDisplay.SetMask(mask, rowCount, columnCount);
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
                _mazeGenerationConfigDisplay.Render();
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