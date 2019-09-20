namespace MazePlayground.Common.Mazes
{
    public struct CellWall
    {
        public readonly byte LinkId;
        public readonly Cell CellOnOtherSide;

        public CellWall(byte linkId, Cell cellOnOtherSide)
        {
            LinkId = linkId;
            CellOnOtherSide = cellOnOtherSide;
        }
    }
}