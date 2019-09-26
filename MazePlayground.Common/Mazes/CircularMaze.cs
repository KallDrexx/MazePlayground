using System;
using System.Collections.Generic;

namespace MazePlayground.Common.Mazes
{
    public class CircularMaze : IMaze
    {
        private readonly Dictionary<Cell, CircularPosition> _cellPositionMap = new Dictionary<Cell, CircularPosition>();
        private readonly Dictionary<int, Cell[]> _cellsInRingMap = new Dictionary<int, Cell[]>();
        private readonly Cell[] _cells;
        
        public enum Direction { Clockwise, CounterClockwise, Inner, OuterClockwise, OuterCounterClockwise }
        
        public int RingCount { get; }
        public Cell StartingCell { get; }
        public Cell FinishingCell { get; }
        public IReadOnlyList<Cell> AllCells => _cells;

        public CircularMaze(int ringCount, int cellCountScaleFactor, int halveFactor)
        {
            RingCount = ringCount;

            var cells = new List<Cell>();
            for (var currentRing = 0; currentRing < ringCount; currentRing++)
            {
                var cellCountInRing = currentRing == 0 ? 1 : (int)Math.Pow(2, currentRing / halveFactor) * cellCountScaleFactor;
                var degreesPerCell = 360f / cellCountInRing;
                var currentDegree = 0f;

                var cellsInRing = new List<Cell>();
                for (var cellIndex = 0; cellIndex < cellCountInRing; cellIndex++)
                {
                    var cell = new Cell();
                    var position = new CircularPosition(currentRing, currentDegree, currentDegree + degreesPerCell);
                    cellsInRing.Add(cell);
                    cells.Add(cell);
                    _cellPositionMap[cell] = position;

                    currentDegree += degreesPerCell;
                }

                _cellsInRingMap[currentRing] = cellsInRing.ToArray();
            }

            _cells = cells.ToArray();
        }
        
        public IReadOnlyList<CellWall> GetWallsForCell(Cell cell)
        {
            throw new NotImplementedException();
        }

        public byte GetOppositeLinkId(byte linkId)
        {
            throw new NotImplementedException();
        }

        public CircularPosition GetPositionOfCell(Cell cell) => _cellPositionMap[cell];

        public struct CircularPosition : IEquatable<CircularPosition>
        {
            public readonly int RingNumber;
            public readonly float StartingDegree;
            public readonly float EndingDegree;

            public CircularPosition(int ringNumber, float startingDegree, float endingDegree)
            {
                RingNumber = ringNumber;
                StartingDegree = startingDegree;
                EndingDegree = endingDegree;
            }

            public override bool Equals(object obj)
            {
                return obj is CircularPosition pos && 
                       pos.RingNumber == RingNumber &&
                       pos.StartingDegree == StartingDegree;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (RingNumber * 397) ^ StartingDegree.GetHashCode();
                }
            }

            public bool Equals(CircularPosition other)
            {
                return RingNumber == other.RingNumber && StartingDegree == other.StartingDegree;
            }
        }
    }
}