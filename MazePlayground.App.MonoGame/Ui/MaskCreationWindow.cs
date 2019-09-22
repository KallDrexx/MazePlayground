using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;

namespace MazePlayground.App.MonoGame.Ui
{
    public class MaskCreationWindow
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly List<bool> _selectedItems = new List<bool>();
        private int _rowCount, _selectedRowCount;
        private int _columnCount, _selectedColumnCount;
        private bool _showWindow;
        
        public bool WindowHasFocus { get; private set; }
        public bool UpdateMaskButtonPressed { get; private set; }
        public IReadOnlyList<bool> MaskData => _selectedItems;
        public int RowCount => _rowCount;
        public int ColumnCount => _columnCount;

        public MaskCreationWindow(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;

            _rowCount = _selectedRowCount = 29;
            _columnCount = _selectedColumnCount = 29;
            _selectedItems.AddRange(Enumerable.Range(0, _rowCount * _columnCount).Select(x => true));
        }

        public void Render()
        {
            if (!_showWindow)
            {
                return;
            }
            
            var windowOptions = ImGuiWindowFlags.NoMove | 
                                ImGuiWindowFlags.NoResize | 
                                ImGuiWindowFlags.NoCollapse;
            
            ImGui.SetNextWindowPos(new Vector2(0), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height));
            ImGui.Begin("Mask Creation", windowOptions);

            ImGui.InputInt("Rows", ref _selectedRowCount);
            ImGui.InputInt("Columns", ref _selectedColumnCount);
            ImGui.Spacing(); ImGui.Spacing(); ImGui.Spacing();
            if (ImGui.Button("Resize Mask"))
            {
                _selectedItems.Clear();
                _selectedItems.AddRange(Enumerable.Range(0, _selectedRowCount * _selectedColumnCount).Select(x => true));
                _rowCount = _selectedRowCount;
                _columnCount = _selectedColumnCount;
            }

            ImGui.Spacing(); ImGui.Spacing(); ImGui.Spacing();
            for (var x = 0; x < _selectedItems.Count; x++)
            {
                ImGui.PushID(x);
                if (ImGui.Selectable("", _selectedItems[x], ImGuiSelectableFlags.None, new Vector2(10)))
                {
                    _selectedItems[x] = !_selectedItems[x];
                }
                
                if (x % _columnCount < _columnCount - 1)
                {
                    ImGui.SameLine();
                }
            }

            ImGui.Spacing(); ImGui.Spacing(); ImGui.Spacing();
            UpdateMaskButtonPressed = ImGui.Button("Save Mask");

            WindowHasFocus = ImGui.IsWindowFocused();
            ImGui.End();
        }

        public void ToggleWindow()
        {
            _showWindow = !_showWindow;
            UpdateMaskButtonPressed = false;
        }
    }
}