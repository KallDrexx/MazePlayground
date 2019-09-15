using System.Collections.Generic;

namespace MazePlayground.Common.Mazes
{
    /// <summary>
    /// Represents a single area of a maze with optional links to other cells.  Each cell is linked to another cell
    /// with a generic link id in numeric form.  By itself the link id value does not have any meaning as each type of
    /// maze that the cell belongs to uses different id values to represent different types of links.  For example,
    /// one type of maze may use specific numbers to denote cardinal directions while another maze has numbers
    /// represent 
    /// </summary>
    public class Cell
    {
        private readonly Dictionary<byte, Cell> _linkIdToCellMap = new Dictionary<byte, Cell>();

        public Dictionary<byte, Cell> LinkIdToCellMap => _linkIdToCellMap;

        public void LinkToOtherCell(Cell other, byte linkId)
        {
            _linkIdToCellMap[linkId] = other;
        }
    }
}