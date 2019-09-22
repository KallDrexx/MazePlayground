using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ImGuiRenderer _imGuiRenderer;
        private readonly string[] _mazeGridTypes;
        private readonly string[] _gridAlgorithmNames;
        private int _selectedMazeTypeIndex;
        
        private int _rowCount = 29;
        private int _columnCount = 29;
        private int _selectedWallSetupAlgorithmIndex;

        private Texture2D _maskTexture;
        private IntPtr _maskTexturePointer;

        private bool _showDemoWindow;
        private bool _showMetricsWindow;

        private bool _highlightShortestPath;
        private bool _showDistances;
        private bool _shadeDistancesFromStart;

        private IReadOnlyList<KeyValuePair<string, string>> _mazeStats;

        public bool GenerateButtonPressed { get; private set; }
        public bool RenderingOptionsChanged { get; private set; }
        public bool WindowHasFocus { get; private set; }
        public bool ResetMazePositionPressed { get; private set; }
        public bool ShowMaskEditorButtonPressed { get; private set; }
        
        public MazeType MazeType => GetMazeType();
        public RectangularMazeConfig RectangularMazeConfig => GetRectangularMazeConfig();
        public RenderOptions RenderingOptions => GetRenderOptions();

        public MazeConfigWindow(GraphicsDevice graphicsDevice, ImGuiRenderer imGuiRenderer)
        {
            _graphicsDevice = graphicsDevice;
            _imGuiRenderer = imGuiRenderer;

            _mazeGridTypes = Enum.GetValues(typeof(MazeType))
                .Cast<MazeType>()
                .Select(x => x.ToString())
                .ToArray();

            _gridAlgorithmNames = Enum.GetValues(typeof(WallSetupAlgorithm))
                .Cast<WallSetupAlgorithm>()
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
            _mazeStats = stats;
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
            
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Spacing();

            if (_mazeStats != null && _mazeStats.Any())
            {
                if (ImGui.CollapsingHeader("Maze Stats", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Columns(2, "stats-columns", false);
                    foreach (var stat in _mazeStats)
                    {
                        ImGui.TextWrapped(stat.Key + ":");
                        ImGui.NextColumn();
                        ImGui.TextWrapped(stat.Value);
                        ImGui.NextColumn();
                    }
                }
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
            else if (_selectedMazeTypeIndex == 1)
            {
                ShowMaskEditorButtonPressed = ImGui.Button("Open Mask Editor");

                if (_maskTexture != null)
                {
                    var imageSize = new Vector2(_maskTexture.Width, _maskTexture.Height);
                    ImGui.Image(_maskTexturePointer, imageSize);
                }
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
            var originalShadeDistances = _shadeDistancesFromStart;
            
            ImGui.Checkbox("Highlight shortest path", ref _highlightShortestPath);
            ImGui.Checkbox("Show cell distances", ref _showDistances);
            ImGui.Checkbox("Shade distance from start", ref _shadeDistancesFromStart);
            ResetMazePositionPressed = ImGui.Button("Reset maze position");

            RenderingOptionsChanged = originalCellDistances != _showDistances ||
                                      originalHighlightShortestPath != _highlightShortestPath ||
                                      originalShadeDistances != _shadeDistancesFromStart;
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
                // Not a grid maze
                return null;
            }

            var algorithmType = Enum.GetValues(typeof(WallSetupAlgorithm))
                .Cast<WallSetupAlgorithm>()
                .Skip(_selectedWallSetupAlgorithmIndex)
                .First();
            
            return new RectangularMazeConfig(_rowCount, _columnCount, algorithmType);
        }

        private RenderOptions GetRenderOptions()
        {
            return new RenderOptions
            {
                HighlightShortestPath = _highlightShortestPath, 
                ShowAllDistances = _showDistances,
                ShowGradientOfDistanceFromStart = _shadeDistancesFromStart,
            };
        }
    }
}