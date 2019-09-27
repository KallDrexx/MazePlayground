namespace MazePlayground.App.MonoGame.Config
{
    public class CircularMazeConfig
    {
        public int RingCount { get; }
        public int ScaleFactor { get; }
        public int HalveFactor { get; }
        
        public CircularMazeConfig(int ringCount, int scaleFactor, int halveFactor)
        {
            RingCount = ringCount;
            ScaleFactor = scaleFactor;
            HalveFactor = halveFactor;
        }
    }
}