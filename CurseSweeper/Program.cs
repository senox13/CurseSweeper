using Sharpie;
using Sharpie.Backend;

namespace CurseSweeper{
    internal class Program{
        internal static void Main(string[] args){
            Terminal terminal = null!;
            try{
                #pragma warning disable CA1416 // Validate platform compatibility
                terminal = new(CursesBackend.Load(), MinesweeperGame.GetTerminalOptions());
                #pragma warning restore CA1416 // Validate platform compatibility
                MinesweeperGame game = new(terminal.Colors);
                terminal.Repeat(game.Redraw, 100);
                terminal.Run(game.ProcessEvent);
            }
            catch(CursesInitializationException){
                Console.WriteLine("No curses backend available");
                Environment.Exit(-1);
            }finally{
                terminal?.Dispose();
            }
        }
    }
}
