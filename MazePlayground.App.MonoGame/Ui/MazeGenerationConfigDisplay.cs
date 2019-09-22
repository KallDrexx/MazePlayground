using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using MazePlayground.App.MonoGame.Config;
using MazePlayground.Common.WallSetup;
using Microsoft.Xna.Framework.Graphics;

namespace MazePlayground.App.MonoGame.Ui
{
    public class MazeGenerationConfigDisplay
    {
        private readonly RectangularMazeGenerationDisplay _rectangularMazeGenerationDisplay;
        private readonly MaskedMazeGenerationDisplay _maskedMazeGenerationDisplay;
        private readonly string[] _mazeGridTypes;
        private readonly string[] _gridAlgorithmNames;
        private int _selectedMazeTypeIndex;
        private int _selectedWallSetupAlgorithmIndex;
        
        public MazeType MazeType => GetMazeType();
        public RectangularMazeConfig RectangularMazeConfig => GetRectangularMazeConfig();
        public WallSetupAlgorithm SelectedWallSetupAlgorithm => GetSelectedWallSetupAlgorithm();
        public bool GenerateButtonPressed { get; private set; }
        public bool ShowMaskEditorButtonPressed => _maskedMazeGenerationDisplay.ShowMaskEditorButtonPressed;

        public MazeGenerationConfigDisplay(GraphicsDevice graphicsDevice, ImGuiRenderer imGuiRenderer)
        {
            _rectangularMazeGenerationDisplay = new RectangularMazeGenerationDisplay();
            _maskedMazeGenerationDisplay = new MaskedMazeGenerationDisplay(imGuiRenderer, graphicsDevice);
            _mazeGridTypes = Enum.GetValues(typeof(MazeType))
                .Cast<MazeType>()
                .Select(x => x.ToString())
                .ToArray();
            
            _gridAlgorithmNames = Enum.GetValues(typeof(WallSetupAlgorithm))
                .Cast<WallSetupAlgorithm>()
                .Select(x => x.ToString())
                .ToArray();
        }
        
        public void Render()
        {
            ImGui.Combo("Maze Type", ref _selectedMazeTypeIndex, _mazeGridTypes, _mazeGridTypes.Length);
            ImGui.Combo("Algorithm", ref _selectedWallSetupAlgorithmIndex, _gridAlgorithmNames, _gridAlgorithmNames.Length);
            
            switch (_selectedMazeTypeIndex)
            {
                case 0:
                    _rectangularMazeGenerationDisplay.Render();
                    break;
                
                case 1:
                    _maskedMazeGenerationDisplay.Render();
                    break;
            }
            
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Spacing();
            
            GenerateButtonPressed = ImGui.Button("Generate Maze");
        }
        
        public void SetMask(IReadOnlyList<bool> mask, int rowCount, int columnCount)
        {
            _maskedMazeGenerationDisplay.SetMask(mask, rowCount, columnCount);
        }

        private MazeType GetMazeType()
        {
            return Enum.GetValues(typeof(MazeType))
                .Cast<MazeType>()
                .Skip(_selectedMazeTypeIndex)
                .First();
        }

        private WallSetupAlgorithm GetSelectedWallSetupAlgorithm()
        {
            return Enum.GetValues(typeof(WallSetupAlgorithm))
                .Cast<WallSetupAlgorithm>()
                .Skip(_selectedWallSetupAlgorithmIndex)
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
                _rectangularMazeGenerationDisplay.ColumnCount);
        }
    }
}