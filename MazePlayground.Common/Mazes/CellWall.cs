namespace MazePlayground.Common.Mazes
{
    /// <summary>
    /// Represents a wall between two cells that may be passable or not.
    /// </summary>
    public class CellWall
    {
        public Cell First { get; }
        public Cell Second { get; }
        public bool IsPassable { get; set; }
        
        public CellWall(Cell first, Cell second)
        {
            First = first;
            Second = second;
        }

        public Cell GetOtherCell(Cell notThisCell)
        {
            return First == notThisCell ? Second : First;
        }
    }
}