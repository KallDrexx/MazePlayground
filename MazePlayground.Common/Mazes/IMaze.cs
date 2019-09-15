namespace MazePlayground.Common.Mazes
{
    public interface IMaze
    {
        Cell StartingCell { get; }
        Cell FinishingCell { get; }
    }
}