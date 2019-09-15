namespace MazePlayground.Common.Mazes
{
    public interface IMaze
    {
        Cell StartingCell { get; }
        Cell FinishingCell { get; }
        Cell GetLinkedCell(Cell linkedFromCell, byte wallIndex);
    }
}