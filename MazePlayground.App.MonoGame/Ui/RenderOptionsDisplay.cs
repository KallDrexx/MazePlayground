using ImGuiNET;

namespace MazePlayground.App.MonoGame.Ui
{
    public class RenderOptionsDisplay
    {
        private bool _highlightShortestPath;
        private bool _showDistances;
        private bool _shadeDistancesFromStart;
        
        public bool ResetMazePositionPressed { get; private set; }
        public bool RenderingOptionsChanged { get; private set; }
        public bool HighlightShortestPath => _highlightShortestPath;
        public bool ShowDistances => _showDistances;
        public bool ShadeDistancesFromStart => _shadeDistancesFromStart;

        public void Render()
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
    }
}