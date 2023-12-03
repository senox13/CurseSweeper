using Sharpie;
using Sharpie.Backend;

namespace CurseSweeper{
    internal class Program{
        private static readonly TerminalOptions options = new(
            ManagedWindows: true,
            UseMouse: false,
            CaretMode: CaretMode.Invisible
        );

        internal static void Main(string[] args){
            Terminal terminal = null!;
            try{
                #pragma warning disable CA1416 // Validate platform compatibility
                terminal = new(CursesBackend.Load(), options);
                #pragma warning restore CA1416 // Validate platform compatibility
                MinesweeperGame game = new();
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