using Sharpie;
using Sharpie.Abstractions;
using System.Drawing;

namespace CurseSweeper{
    public sealed class MinesweeperGame{
        private const string cursorStr = "X";
        private readonly MinesweeperBoard board;
        private readonly MinesweeperBoardRenderer renderer;
        private StyledText cursorRune = new(cursorStr, Style.Default); //TODO: Cursor will need to blink, can use system time maybe?
        private Point cursorPos = new(0, 0);
        private IWindow boardWindow = null!;

        public MinesweeperGame(IColorManager colors){
            //TODO: Add game options struct?
            board = new MinesweeperBoard(new(10, 10), 10);
            renderer = new MinesweeperBoardRenderer(colors, board);
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
            term.Screen.Clear();
            term.Screen.DrawBorder();
            
            //TODO: Should probably draw to a subwindow, then add back border, with top/bottom bar with timer
            boardWindow.Draw(new Point(0, 0), renderer);

            //Draw cursor
            boardWindow.CaretLocation = cursorPos;
            boardWindow.WriteText(cursorRune);

            term.Screen.Refresh();
            boardWindow.Refresh();
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
                //case KeyEvent{Key: Key.Character} keyEvt: //DEBUG
                    //lastKeyPress = keyEvt.Char;
                    //break;
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
                case KeyEvent{Key: Key.Character, Char.IsAscii: true, Char.Value: 'M', Modifiers: ModifierKey.Ctrl}:
                    board.ToggleFlag(cursorPos);
                    break;
                case KeyEvent{Key: Key.Character, Char.IsAscii: true, Char.Value: ' ', Modifiers: ModifierKey.None}:
                    board.UncoverTile(cursorPos);
                    break;
            }
            return Task.CompletedTask;
        }
    }
}
