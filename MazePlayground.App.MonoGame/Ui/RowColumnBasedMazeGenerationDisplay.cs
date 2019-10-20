using ImGuiNET;

namespace MazePlayground.App.MonoGame.Ui
{
    public class RowColumnBasedMazeGenerationDisplay
    {
        private int _rowCount = 29;
        private int _columnCount = 29;

        public int RowCount => _rowCount;
        public int ColumnCount => _columnCount;

        public void Render()
        {
            ImGui.InputInt("Rows", ref _rowCount);
            ImGui.InputInt("Columns", ref _columnCount);
        }
    }
}