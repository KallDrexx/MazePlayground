using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using MazePlayground.App.MonoGame.Config;
using MazePlayground.Common.Mazes;

namespace MazePlayground.App.MonoGame
{
    public class MazeConfigWindow
    {
        private readonly string[] _mazeGridTypes;
        private readonly string[] _gridAlgorithmNames;
        private int _selectedMazeTypeIndex;
        
        private int _rowCount = 20;
        private int _columnCount = 20;
        private int _selectedWallSetupAlgorithmIndex;
        
        public bool GenerateButtonPressed { get; private set; }
        public MazeType MazeType => GetMazeType();
        public GridMazeConfig GridMazeConfig => GetGridMazeConfig();

        public MazeConfigWindow()
        {
            _mazeGridTypes = Enum.GetValues(typeof(MazeType))
                .Cast<MazeType>()
                .Select(x => x.ToString())
                .ToArray();

            _gridAlgorithmNames = Enum.GetValues(typeof(GridMaze.WallSetupAlgorithm))
                .Cast<GridMaze.WallSetupAlgorithm>()
                .Select(x => x.ToString())
                .ToArray();
        }
        
        public void Render()
        {
            ImGui.Begin("Maze Configuration", ImGuiWindowFlags.AlwaysAutoResize);

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
            
            ImGui.End();
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
    }
}