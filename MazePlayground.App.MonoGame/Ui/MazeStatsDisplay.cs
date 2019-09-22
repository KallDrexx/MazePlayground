using System.Collections.Generic;
using System.Linq;
using ImGuiNET;

namespace MazePlayground.App.MonoGame.Ui
{
    public class MazeStatsDisplay
    {
        private IReadOnlyList<KeyValuePair<string, string>> _mazeStats;
        
        public void SetMazeStats(IReadOnlyList<KeyValuePair<string, string>> stats)
        {
            _mazeStats = stats;
        }

        public void Render()
        {
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
        }
    }
}