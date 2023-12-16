using System.CommandLine;
using System.CommandLine.Parsing;
using Sharpie;
using Sharpie.Backend;

namespace CurseSweeper{
    internal class Program{
        private const string ROOT_DESC = "A curses-driven CLI minesweeper clone";
        private const string DIFF_DESC = "Specifies a preset difficulty. Must be one of the following: {0}";
        private const string WIDTH_DESC = "Specifies a custom width for the game board";
        private const string HEIGHT_DESC = "Specifies a custom height for the game board";
        private const string MINES_DESC = "Specifies a custom number of mines to place on the game board";
        private const string ERR_DIFF_EXCLUSION = "Difficulty cannot be specified along with any of width, height, or mines";
        private const string ERR_PARTIAL_ARGS = "Width, height, and mines must be specified together";
        private const string ERR_BAD_DIFF = "Got unrecognized difficulty name: ";

        internal static int Main(string[] args){
            Option<string?> difficultyOpt = new(
                aliases: new[]{"-d", "--difficulty"},
                description: string.Format(DIFF_DESC, string.Join(", ", MinesweeperGame.Difficulty.BUILTINS.Keys))
            );
            Option<int?> widthOpt = new(
                aliases: new[]{"-w", "--width"},
                description: WIDTH_DESC
            );
            Option<int?> heightOpt = new(
                aliases: new[]{"-h", "--height"},
                description: HEIGHT_DESC
            );
            Option<int?> minesOpt = new(
                aliases: new[]{"-m", "--mines"},
                description: MINES_DESC
            );
            RootCommand rootCmd = new(ROOT_DESC);
            rootCmd.AddOption(difficultyOpt);
            rootCmd.AddOption(widthOpt);
            rootCmd.AddOption(heightOpt);
            rootCmd.AddOption(minesOpt);
            rootCmd.AddValidator(result => {
                OptionResult? diffResult = result.FindResultFor(difficultyOpt);
                OptionResult? widthResult = result.FindResultFor(widthOpt);
                OptionResult? heightResult = result.FindResultFor(heightOpt);
                OptionResult? minesResult = result.FindResultFor(minesOpt);
                bool numericArgProvided = widthResult != null || heightResult != null || minesResult != null;
                bool allNumericArgsProvided = widthResult != null && heightResult != null && minesResult != null;
                if(diffResult == null && widthResult == null && heightResult == null && minesResult == null){
                    return; //No args specified, will fall back to defaults
                }else if(diffResult != null && numericArgProvided){
                    result.ErrorMessage = ERR_DIFF_EXCLUSION;
                }else if(diffResult == null && numericArgProvided && !allNumericArgsProvided){
                    result.ErrorMessage = ERR_PARTIAL_ARGS;
                }else if(diffResult != null){
                    string diffName = diffResult.GetValueForOption(difficultyOpt)!.ToLower();
                    if(!MinesweeperGame.Difficulty.BUILTINS.ContainsKey(diffName)){
                        result.ErrorMessage = ERR_BAD_DIFF + diffName;
                    }
                }
            });
            rootCmd.SetHandler((difficulty, width, height, mines) => {
                MinesweeperGame.Difficulty resultDifficulty = MinesweeperGame.Difficulty.BEGINNER;
                if(difficulty != null){
                    MinesweeperGame.Difficulty.BUILTINS.TryGetValue(difficulty, out resultDifficulty);
                }else if(width.HasValue && height.HasValue && mines.HasValue){
                    resultDifficulty = new MinesweeperGame.Difficulty(width.Value, height.Value, mines.Value);
                }
                RunGame(resultDifficulty);
            }, difficultyOpt, widthOpt, heightOpt, minesOpt);
            return rootCmd.Invoke(args);
        }

        private static void RunGame(MinesweeperGame.Difficulty difficulty){
            Terminal terminal = null!;
            try{
                #pragma warning disable CA1416 // Validate platform compatibility
                terminal = new(CursesBackend.Load(), MinesweeperGame.GetTerminalOptions());
                #pragma warning restore CA1416 // Validate platform compatibility
                MinesweeperGame game = new(terminal.Colors, difficulty);
                terminal.Repeat(game.Redraw, 100);
                terminal.Run(game.ProcessEvent);
            }catch(CursesInitializationException){
                Console.WriteLine("No curses backend available");
                Environment.Exit(-1);
            }finally{
                terminal?.Dispose();
            }
        }
    }
}
