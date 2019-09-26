using System;
using System.Collections.Generic;
using System.Linq;
using MazePlayground.Common.Mazes;
using MazePlayground.Common.WallSetup;

namespace MazePlayground.Common
{
    public class MazeStats
    {
        private readonly List<KeyValuePair<string, string>> _entries = new List<KeyValuePair<string, string>>();

        public IReadOnlyList<KeyValuePair<string, string>> Entries => _entries;

        public MazeStats(IMaze maze, WallSetupAlgorithm wallSetupAlgorithm)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));
            
            AddStat("Maze Type", maze.GetType().Name.AddSpacesBetweenUpperCaseLetters());
            AddStat("Algorithm", wallSetupAlgorithm.ToString().AddSpacesBetweenUpperCaseLetters());
            AddMazeSpecificStats(maze);
            AddStat("Total Cells", maze.AllCells.Count.ToString());
            AddDeadCellCount(maze);
        }

        public void AddCustomStat(string name, string value)
        {
            AddStat(name, value);
        }

        private void AddMazeSpecificStats(IMaze maze)
        {
            switch (maze)
            {
                case RectangularMaze rectangularMaze:
                    AddStat("Rows", rectangularMaze.RowCount.ToString());
                    AddStat("Columns", rectangularMaze.ColumnCount.ToString());
                    break;
            }
        }

        private void AddDeadCellCount(IMaze maze)
        {
            var deadEndCount = 0;
            var allCells = maze.AllCells;
            foreach (var cell in allCells)
            {
                if (cell.CellWalls.Count == 1)
                {
                    deadEndCount++;
                }
            }
            
            var percentage = (int) ((deadEndCount / (decimal) allCells.Count) * 100);
            AddStat("Dead Ends", $"{deadEndCount} ({percentage}%%)");
        }

        private void AddStat(string name, string value)
        {
            var entryIndex = _entries.FindIndex(x => x.Key == name);
            if (entryIndex >= 0)
            {
                _entries[entryIndex] = new KeyValuePair<string, string>(name, value);
            }
            else
            {
                _entries.Add(new KeyValuePair<string, string>(name, value));
            }
        }
    }
}