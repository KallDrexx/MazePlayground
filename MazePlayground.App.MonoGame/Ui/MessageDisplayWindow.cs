using ImGuiNET;

namespace MazePlayground.App.MonoGame.Ui
{
    public class MessageDisplayWindow
    {
        private string _title, _content;
        private bool _isDisplayed;

        public void ShowMessage(string title, string content)
        {
            _title = title;
            _content = content;
            _isDisplayed = true;
        }

        public void Render()
        {
            if (!_isDisplayed)
            {
                return;
            }
            
            ImGui.OpenPopup(_title);

            if (ImGui.BeginPopupModal(_title, ref _isDisplayed, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.HorizontalScrollbar))
            {
                ImGui.Text(_content);

                ImGui.EndPopup();
            }
        }
    }
}