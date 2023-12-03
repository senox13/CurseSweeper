using Sharpie;
using Sharpie.Abstractions;
using System.Drawing;
using System.Text;

namespace CurseSweeper{
    public sealed class MinesweeperGame{
        //TODO: MinesweeperBoard instance
        //TODO: MinesweeperBoardRenderer instance, can probably implement IDrawable

        private const string cursorStr = "X";
        private StyledText cursorRune = new(cursorStr, Style.Default);
        private Point cursorPos = new(0, 0);
        private Rune lastKeyPress;

        private void ClampCursorPos(){
            cursorPos = new(
                Math.Clamp(cursorPos.X, 0, int.MaxValue), //TODO: Clamp to board size
                Math.Clamp(cursorPos.Y, 0, int.MaxValue)
            );
        }

        public Task Redraw(Terminal term){
            term.Screen.Clear();
            term.Screen.DrawBorder();
            
            //Draw last char
            term.Screen.CaretLocation = new(1, 1);
            term.Screen.WriteText(lastKeyPress.ToString());
            //Draw cursor
            term.Screen.CaretLocation = cursorPos;
            term.Screen.WriteText(cursorRune);

            term.Screen.Refresh();
            return Task.CompletedTask;
        }

        public Task ProcessEvent(ITerminal term, Event evt){
            switch(evt){
                case StartEvent:
                    cursorRune = new(cursorStr, new Style(){
                        ColorMixture = term.Colors.MixColors(StandardColor.Yellow, StandardColor.Black)
                    });
                    break;
                //TODO: Should handle resize event but it never seems to be fired
                case KeyEvent{Key: Key.Character} keyEvt:
                    lastKeyPress = keyEvt.Char;
                    break;
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
            }
            return Task.CompletedTask;
        }
    }
}
