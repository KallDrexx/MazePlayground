using System.Collections.Generic;
using System.Linq;

namespace MazePlayground.Common.Mazes
{
    public abstract class RowColumnBasedMaze : IMaze
    {
        protected readonly Dictionary<Cell, int> CellIndexMap = new Dictionary<Cell, int>();
        protected Cell[] Cells { get; set; }
        
        public int RowCount { get; protected set; }
        public int ColumnCount { get; protected set; }
        public Cell StartingCell { get; protected set; }
        public Cell FinishingCell { get; protected set; }
        public virtual IReadOnlyList<Cell> AllCells => Cells.ToArray();
        
        public (int row, int column) GetPositionOfCell(Cell cell)
        {
            var index = CellIndexMap[cell];
            return GetPositionFromIndex(index);
        }
        
        protected Cell GetCell(int row, int column)
        {
            if (row < 0 || column < 0 || row >= RowCount || column >= ColumnCount)
            {
                return null;
            }
            
            var index = (row * ColumnCount) + column;

            return index >= 0 && index < Cells.Length
                ? Cells[index]
                : null;
        }

        protected static void LinkCellsIfNotAlreadyLinked(Cell current, Cell neighbor)
        {
            if (neighbor == null)
            {
                return;
            }

            if (neighbor.CellWalls.Any(x => x.GetOtherCell(neighbor) == current))
            {
                return;
            }
            
            var wall = new CellWall(current, neighbor);
            current.CellWalls.Add(wall);
            neighbor.CellWalls.Add(wall);
        }

        private (int row, int column) GetPositionFromIndex(int index)
        {
            var row = index / ColumnCount;
            var column = index % ColumnCount;

            return (row, column);
        }
    }
}