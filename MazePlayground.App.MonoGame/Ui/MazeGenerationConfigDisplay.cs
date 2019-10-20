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
        private readonly RowColumnBasedMazeGenerationDisplay _rowColumnBasedMazeGenerationDisplay;
        private readonly MaskedMazeGenerationDisplay _maskedMazeGenerationDisplay;
        private readonly CircularMazeGenerationDisplay _circularMazeGenerationDisplay;
        private readonly string[] _mazeGridTypes;
        private readonly string[] _gridAlgorithmNames;
        private int _selectedMazeTypeIndex;
        private int _selectedWallSetupAlgorithmIndex;
        
        public MazeType MazeType => GetMazeType();
        public RowColumnMazeConfig RowColumnMazeConfig => GetRectangularMazeConfig();
        public CircularMazeConfig CircularMazeConfig => GetCircularMazeConfig();
        public WallSetupAlgorithm SelectedWallSetupAlgorithm => GetSelectedWallSetupAlgorithm();
        public bool GenerateButtonPressed { get; private set; }
        public bool ShowMaskEditorButtonPressed => _maskedMazeGenerationDisplay.ShowMaskEditorButtonPressed;

        public MazeGenerationConfigDisplay(GraphicsDevice graphicsDevice, ImGuiRenderer imGuiRenderer)
        {
            _rowColumnBasedMazeGenerationDisplay = new RowColumnBasedMazeGenerationDisplay();
            _maskedMazeGenerationDisplay = new MaskedMazeGenerationDisplay(imGuiRenderer, graphicsDevice);
            _circularMazeGenerationDisplay = new CircularMazeGenerationDisplay();
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
                case (int) MazeType.Rectangular:
                case (int) MazeType.Triangle:
                case (int) MazeType.Hex:
                    _rowColumnBasedMazeGenerationDisplay.Render();
                    break;
                
                case (int) MazeType.Masked:
                    _maskedMazeGenerationDisplay.Render();
                    break;
                
                case (int) MazeType.Circular:
                    _circularMazeGenerationDisplay.Render();
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

        private RowColumnMazeConfig GetRectangularMazeConfig()
        {
            var validIndexes = new[] {(int) MazeType.Rectangular, (int) MazeType.Triangle, (int) MazeType.Hex};
            if (!validIndexes.Contains(_selectedMazeTypeIndex))
            {
                // Not a rectangular maze
                return null;
            }
            
            return new RowColumnMazeConfig(_rowColumnBasedMazeGenerationDisplay.RowCount, 
                _rowColumnBasedMazeGenerationDisplay.ColumnCount);
        }

        private CircularMazeConfig GetCircularMazeConfig()
        {
            if (_selectedMazeTypeIndex != 2)
            {
                return null;
            }

            return new CircularMazeConfig(_circularMazeGenerationDisplay.RingCount,
                _circularMazeGenerationDisplay.ScaleFactor,
                _circularMazeGenerationDisplay.HalveFactor);
        }
    }
}