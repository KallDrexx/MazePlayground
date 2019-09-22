using System;
using System.Linq;
using ImGuiNET;
using MazePlayground.Common.WallSetup;

namespace MazePlayground.App.MonoGame.Ui
{
    public class RectangularMazeGenerationDisplay
    {
        private readonly string[] _gridAlgorithmNames;
        private int _rowCount = 29;
        private int _columnCount = 29;
        private int _selectedWallSetupAlgorithmIndex;

        public int RowCount => _rowCount;
        public int ColumnCount => _columnCount;
        public WallSetupAlgorithm WallSetupAlgorithm => GetSelectedWallSetupAlgorithm();

        public RectangularMazeGenerationDisplay()
        {
            _gridAlgorithmNames = Enum.GetValues(typeof(WallSetupAlgorithm))
                .Cast<WallSetupAlgorithm>()
                .Select(x => x.ToString())
                .ToArray();
        }

        public void Render()
        {
            ImGui.InputInt("Rows", ref _rowCount);
            ImGui.InputInt("Columns", ref _columnCount);
            ImGui.Combo("Algorithm", ref _selectedWallSetupAlgorithmIndex, _gridAlgorithmNames, _gridAlgorithmNames.Length);
        }

        private WallSetupAlgorithm GetSelectedWallSetupAlgorithm()
        {
            return Enum.GetValues(typeof(WallSetupAlgorithm))
                .Cast<WallSetupAlgorithm>()
                .Skip(_selectedWallSetupAlgorithmIndex)
                .First();
        }
    }
}