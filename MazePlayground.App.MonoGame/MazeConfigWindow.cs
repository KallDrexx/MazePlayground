using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using MazePlayground.App.MonoGame.Config;
using MazePlayground.Common.Mazes;
using MazePlayground.Common.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace MazePlayground.App.MonoGame
{
    public class MazeConfigWindow
    {
        public const int WindowWidth = 250;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly string[] _mazeGridTypes;
        private readonly string[] _gridAlgorithmNames;
        private int _selectedMazeTypeIndex;
        
        private int _rowCount = 20;
        private int _columnCount = 20;
        private int _selectedWallSetupAlgorithmIndex;

        private bool _showDemoWindow;
        private bool _showMetricsWindow;

        private bool _highlightShortestPath;
        private bool _showDistances;

        public bool GenerateButtonPressed { get; private set; }
        public bool RenderingOptionsChanged { get; private set; }
        public bool WindowHasFocus { get; private set; }
        public bool ResetMazePositionPressed { get; private set; }
        
        public MazeType MazeType => GetMazeType();
        public GridMazeConfig GridMazeConfig => GetGridMazeConfig();
        public RenderOptions RenderingOptions => GetRenderOptions();

        public MazeConfigWindow(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            
            _mazeGridTypes = Enum.GetValues(typeof(MazeType))
                .Cast<MazeType>()
                .Select(x => x.ToString())
                .ToArray();

            _gridAlgorithmNames = Enum.GetValues(typeof(GridMaze.WallSetupAlgorithm))
                .Cast<GridMaze.WallSetupAlgorithm>()
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
        
        public void Render()
        {
            if (_showDemoWindow) ImGui.ShowDemoWindow();
            if (_showMetricsWindow) ImGui.ShowMetricsWindow();
            
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
                RenderRenderingOptionsTab();
            }
            
            WindowHasFocus = ImGui.IsWindowFocused();
            
            ImGui.End();
        }

        private void RenderGenerationTab()
        {
            ImGui.Combo("Maze Type", ref _selectedMazeTypeIndex, _mazeGridTypes, _mazeGridTypes.Length);
            
            if (_selectedMazeTypeIndex == 0)
            {
                ImGui.InputInt("Rows", ref _rowCount);
                ImGui.InputInt("Columns", ref _columnCount);
                ImGui.Combo("Algorithm", ref _selectedWallSetupAlgorithmIndex, _gridAlgorithmNames, _gridAlgorithmNames.Length);
            }
            
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Spacing();
            
            GenerateButtonPressed = ImGui.Button("Generate Maze");
        }

        private void RenderRenderingOptionsTab()
        {
            var originalHighlightShortestPath = _highlightShortestPath;
            var originalCellDistances = _showDistances;
            
            ImGui.Checkbox("Highlight shortest path", ref _highlightShortestPath);
            ImGui.Checkbox("Show cell distances", ref _showDistances);
            ResetMazePositionPressed = ImGui.Button("Reset maze position");

            RenderingOptionsChanged = originalCellDistances != _showDistances ||
                                      originalHighlightShortestPath != _highlightShortestPath;
        }

        private MazeType GetMazeType()
        {
            return Enum.GetValues(typeof(MazeType))
                .Cast<MazeType>()
                .Skip(_selectedMazeTypeIndex)
                .First();
        }

        private GridMazeConfig GetGridMazeConfig()
        {
            if (_selectedMazeTypeIndex != 0)
            {
                // Not a grid maze
                return null;
            }

            var algorithmType = Enum.GetValues(typeof(GridMaze.WallSetupAlgorithm))
                .Cast<GridMaze.WallSetupAlgorithm>()
                .Skip(_selectedWallSetupAlgorithmIndex)
                .First();
            
            return new GridMazeConfig(_rowCount, _columnCount, algorithmType);
        }

        private RenderOptions GetRenderOptions()
        {
            return new RenderOptions
            {
                HighlightShortestPath = _highlightShortestPath, 
                ShowAllDistances = _showDistances,
            };
        }
    }
}