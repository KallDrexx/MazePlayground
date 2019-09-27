using ImGuiNET;

namespace MazePlayground.App.MonoGame.Ui
{
    public class CircularMazeGenerationDisplay
    {
        private int _ringCount = 12;
        private int _scaleFactor = 6;
        private int _halveFactor = 3;

        public int RingCount => _ringCount;
        public int ScaleFactor => _scaleFactor;
        public int HalveFactor => _halveFactor;

        public void Render()
        {
            ImGui.InputInt("Rings", ref _ringCount);
            ImGui.InputInt("Scale Factor", ref _scaleFactor);
            ImGui.InputInt("Halve Factor", ref _halveFactor);
        }
    }
}