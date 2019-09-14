using System;

namespace MazePlayground.App.MonoGame
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new App())
            {
                game.Run();
            }
        }
    }
}