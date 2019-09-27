using System;
using System.Collections.Generic;
using System.Linq;
using MazePlayground.Common.Solvers;
using MazePlayground.Common.WallSetup;

namespace MazePlayground.Common.Mazes
{
    public class CircularMaze : IMaze
    {
        private readonly Random _random = new Random();
        private readonly Dictionary<Cell, CircularPosition> _cellPositionMap = new Dictionary<Cell, CircularPosition>();
        private readonly Dictionary<int, Cell[]> _cellsInRingMap = new Dictionary<int, Cell[]>();
        private readonly Cell[] _cells;
        
        public int RingCount { get; }
        public Cell StartingCell { get; private set; }
        public Cell FinishingCell { get; private set; }
        public IReadOnlyList<Cell> AllCells => _cells;

        public CircularMaze(int ringCount, int cellCountScaleFactor, int halveFactor, WallSetupAlgorithm algorithm)
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
            BuildWalls();
            SetupWalls(algorithm);
            SetStartingAndFinishingCells();
        }

        public CircularPosition GetPositionOfCell(Cell cell) => _cellPositionMap[cell];

        private void BuildWalls()
        {
            for (var currentRing = 1; currentRing < RingCount; currentRing++)
            {
                var cellsInRing = _cellsInRingMap[currentRing];
                var cellsInInnerRing = _cellsInRingMap[currentRing - 1];
                var cellsInOuterRing = currentRing < RingCount - 1 ? _cellsInRingMap[currentRing + 1] : new Cell[0];

                var cellsInOrder = cellsInRing.Select(x => new {cell = x, pos = _cellPositionMap[x]})
                    .OrderBy(x => x.pos.StartingDegree)
                    .Select(x => x.cell)
                    .ToArray();
                
                for (var index = 0; index < cellsInOrder.Length; index++)
                {
                    var cell = cellsInOrder[index];
                    var adjacentCells = new List<Cell>();
                    
                    // Find inner neighbor(s)
                    if (currentRing == 1)
                    {
                        // Auto bind it to center cell
                        adjacentCells.Add(_cells[0]);
                    }
                    else
                    {
                        adjacentCells.AddRange(cellsInInnerRing.Where(x => CheckIfCellsAlign(cell, x)));
                    }
                    
                    // outer neighbors
                    if (cellsInOuterRing.Any())
                    {
                        adjacentCells.AddRange(cellsInOuterRing.Where(x => CheckIfCellsAlign(cell, x)));
                    }
                    
                    // Find next cell to either side
                    adjacentCells.Add(index == 0 ? cellsInOrder[cellsInOrder.Length - 1] : cellsInOrder[index - 1]);
                    adjacentCells.Add(index == cellsInOrder.Length - 1 ? cellsInOrder[0] : cellsInOrder[index + 1]);
                    
                    // build walls
                    foreach (var adjacentCell in adjacentCells)
                    {
                        // Make sure this wall doesn't already exist
                        if (adjacentCell.CellWalls.All(x => x.GetOtherCell(adjacentCell) != cell))
                        {
                            var wall = new CellWall(cell, adjacentCell);
                            cell.CellWalls.Add(wall);
                            adjacentCell.CellWalls.Add(wall);
                        }
                    }
                }
            }
        }

        private bool CheckIfCellsAlign(Cell first, Cell second)
        {
            var firstPosition = _cellPositionMap[first];
            var secondPosition = _cellPositionMap[second];

            // Cells are aligned if they are only one ring apart and if at least one wall lines up
            if (Math.Abs(firstPosition.RingNumber - secondPosition.RingNumber) > 1)
            {
                return false;
            }

            if (Math.Abs(firstPosition.StartingDegree - secondPosition.StartingDegree) < 0.001)
            {
                return true;
            }

            if (Math.Abs(firstPosition.EndingDegree - secondPosition.EndingDegree) < 0.001)
            {
                return true;
            }

            return false;
        }

        private void SetupWalls(WallSetupAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case WallSetupAlgorithm.Wilson: 
                    new Wilson().Run(this);
                    break;
                
                case WallSetupAlgorithm.AldousBroder:
                    new AldousBroder().Run(this);
                    break;
                
                case WallSetupAlgorithm.HuntAndKill:
                    new HuntAndKill().Run(this);
                    break;
                
                case WallSetupAlgorithm.RecursiveBackTracker:
                    new RecursiveBackTracker().Run(this);
                    break;
                
                default:
                    throw new NotSupportedException($"Algorithm {algorithm} is not supported by circular mazes");
            }
        }

        private void SetStartingAndFinishingCells()
        {
            // Starting and finishing cells should be on the outer ring
            var cellsInRing = _cellsInRingMap[RingCount - 1];
            StartingCell = cellsInRing[_random.Next(0, cellsInRing.Length)];
            
            var distanceInfo = CellDistanceSolver.GetPassableDistancesFromCell(StartingCell);
            FinishingCell = cellsInRing.Select(x => new {cell = x, distance = distanceInfo.DistanceFromStartMap[x]})
                .OrderByDescending(x => x.distance)
                .Select(x => x.cell)
                .First();
        }

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
                return obj is CircularPosition pos && Equals(pos);
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
                return RingNumber == other.RingNumber && Math.Abs(StartingDegree - other.StartingDegree) < .001;
            }
        }
    }
}