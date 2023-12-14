using System.Drawing;
using Sharpie;
using Sharpie.Abstractions;

namespace CurseSweeper{
    public sealed class MinesweeperGame{
        private const string HELP_TEXT = "Space to uncover tiles, enter to flag mines, r to reset";
        private const int CURSOR_BLINK_MS = 500;
        private const string cursorStr = "X";
        private MinesweeperBoard board;
        private MinesweeperBoardRenderer renderer;
        private StyledText cursorRune = new(cursorStr, Style.Default);
        private Point cursorPos = new(0, 0);
        private IWindow boardWindow = null!;

        public MinesweeperGame(IColorManager colors){
            board = new MinesweeperBoard(Difficulty.EXPERT);
            renderer = new MinesweeperBoardRenderer(colors, board);
        }

        public static TerminalOptions GetTerminalOptions(){
            return new(
                ManagedWindows: true,
                UseColors: true,
                UseMouse: false,
                CaretMode: CaretMode.Invisible,
                AllocateFooter: true
            );
        }

        private Rectangle GetBoardRect(){
            return new(){
                Location = new Point(1, 1),
                Size = new(board.Size.Width + 1, board.Size.Height + 1)
            };
        }

        private void ClampCursorPos(){
            cursorPos = new(
                Math.Clamp(cursorPos.X, 0, board.Size.Width - 1),
                Math.Clamp(cursorPos.Y, 0, board.Size.Height - 1)
            );
        }

        public Task Redraw(Terminal term){
            //Clear screen
            term.Screen.Clear();
            term.Screen.DrawBorder();
            term.Footer!.Clear();
            
            //Draw board
            boardWindow.Draw(new Point(0, 0), renderer);

            //Draw cursor
            if(!board.GameFinished && DateTime.Now.Millisecond % CURSOR_BLINK_MS > CURSOR_BLINK_MS / 2){
                boardWindow.CaretLocation = cursorPos;
                boardWindow.WriteText(cursorRune);
            }

            //Update footer
            string footerText;
            if(!board.GameStarted){
                footerText = HELP_TEXT;
            }else if(board.GameFinished){
                footerText = $"Time: {(board.FinishTime! - board.StartTime!).Value.Seconds} {HELP_TEXT}";
            }else{
                footerText = $"Time: {(DateTime.Now - board.StartTime!).Value.Seconds} {HELP_TEXT}";
            }
            term.Footer!.WriteText(footerText);

            //Refresh screen sections
            term.Screen.Refresh();
            boardWindow.Refresh();
            term.Footer!.Refresh();
            return Task.CompletedTask;
        }

        public Task ProcessEvent(ITerminal term, Event evt){
            switch(evt){
                case StartEvent:
                    cursorRune = new(cursorStr, new Style(){
                        ColorMixture = term.Colors.MixColors(StandardColor.Yellow, StandardColor.Black)
                    });
                    boardWindow = term.Screen.Window(GetBoardRect());
                    break;
                //TODO: Should handle resize event but it never seems to be fired
                case KeyEvent{Key: Key.KeypadUp}:
                    cursorPos.Offset(0, -1);
                    ClampCursorPos();
                    break;
                case KeyEvent{Key: Key.KeypadDown}:
                    cursorPos.Offset(0, 1);
                    ClampCursorPos();
                    break;
                case KeyEvent{Key: Key.KeypadLeft}:
                    cursorPos.Offset(-1, 0);
                    ClampCursorPos();
                    break;
                case KeyEvent{Key: Key.KeypadRight}:
                    cursorPos.Offset(1, 0);
                    ClampCursorPos();
                    break;
                case KeyEvent{Key: Key.Character, Char.IsAscii: true, Char.Value: 'r', Modifiers: ModifierKey.None}:
                    Difficulty prevDifficulty = board.Difficulty;
                    board = new MinesweeperBoard(prevDifficulty);
                    renderer = new MinesweeperBoardRenderer(term.Colors, board);
                    cursorPos = new(0, 0);
                    break;
                case KeyEvent{Key: Key.Character, Char.IsAscii: true, Char.Value: 'M', Modifiers: ModifierKey.Ctrl}:
                    board.ToggleFlag(cursorPos);
                    break;
                case KeyEvent{Key: Key.Character, Char.IsAscii: true, Char.Value: ' ', Modifiers: ModifierKey.None}:
                    board.UncoverTile(cursorPos);
                    break;
            }
            return Task.CompletedTask;
        }

        public readonly struct Difficulty{
            public static readonly Difficulty BEGINNER = new(9, 9, 10);
            public static readonly Difficulty INTERMEDIATE = new(16, 16, 40);
            public static readonly Difficulty EXPERT = new(30, 16, 99);

            public readonly Size BoardSize{get;init;}
            public readonly int MineCount{get;init;}

            public Difficulty(int width, int height, int mines){
                BoardSize = new(width, height);
                MineCount = mines;
            }

            public void Validate(){
                if(BoardSize.Width < 0 || BoardSize.Height < 0)
                    throw new ArgumentException("Board size must be positive on both axes", nameof(BoardSize));
                if(MineCount >= BoardSize.Width * BoardSize.Height)
                    throw new ArgumentException("Mine count must be less than board area", nameof(MineCount));
            }
        }
    }
}
