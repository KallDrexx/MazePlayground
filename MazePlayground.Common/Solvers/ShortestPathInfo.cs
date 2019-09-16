using System;
using System.Collections.Generic;
using MazePlayground.Common.Mazes;

namespace MazePlayground.Common.Solvers
{
    public class ShortestPathInfo
    {
        private readonly HashSet<Cell> _cellsInPath;
        public Cell[] OrderedPath { get; }

        public ShortestPathInfo(Cell[] cellsInPath)
        {
            OrderedPath = cellsInPath ?? throw new ArgumentNullException(nameof(cellsInPath));
            _cellsInPath = new HashSet<Cell>(cellsInPath);
        }

        public bool IsCellInPath(Cell cell) => _cellsInPath.Contains(cell);
    }
}