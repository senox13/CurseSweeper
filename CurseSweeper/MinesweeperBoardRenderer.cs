using System.Drawing;
using System.Text;
using Sharpie;
using Sharpie.Abstractions;

namespace CurseSweeper{
    public sealed class MinesweeperBoardRenderer : IDrawable{
        private static readonly Rune RUNE_COVERED = new('#');
        private static readonly Rune RUNE_FLAG = new('^');
        private static readonly Rune RUNE_MINE = new('*');
        private static readonly Rune[] NUM_RUNES = new Rune[]{
            new(' '),
            new('1'),
            new('2'),
            new('3'),
            new('4'),
            new('5'),
            new('6'),
            new('7'),
            new('8')
        };
        private readonly Style flagStyle;
        private readonly Style mineStyle;
        private readonly Style[] numStyles;
        private readonly MinesweeperBoard board;

        public Size Size => board.Size;

        public MinesweeperBoardRenderer(IColorManager colors, MinesweeperBoard boardIn){
            board = boardIn;
            StandardColor bgColor = StandardColor.Black;
            flagStyle = new(){
                ColorMixture = colors.MixColors(StandardColor.Red, bgColor),
                Attributes = VideoAttribute.Bold
            };
            mineStyle = new(){
                ColorMixture = colors.MixColors(StandardColor.White, bgColor),
                Attributes = VideoAttribute.Bold
            };
            numStyles = new Style[]{
                Style.Default,
                new(){ColorMixture = colors.MixColors(StandardColor.Blue, bgColor)}, //1
                new(){ColorMixture = colors.MixColors(StandardColor.Green, bgColor)}, //2
                new(){ColorMixture = colors.MixColors(StandardColor.Red, bgColor)}, //3
                new(){ColorMixture = colors.MixColors(StandardColor.Magenta, bgColor)}, //4
                new(){ColorMixture = colors.MixColors(StandardColor.Magenta, bgColor)}, //5
                new(){ColorMixture = colors.MixColors(StandardColor.Magenta, bgColor)}, //6
                new(){ColorMixture = colors.MixColors(StandardColor.Magenta, bgColor)}, //7
                new(){ColorMixture = colors.MixColors(StandardColor.Magenta, bgColor)} //8
            };
        }

        private (Rune, Style) GetRune(Point tilePos, TileState tile){
            switch(tile){
                case TileState.COVERED:
                    return (RUNE_COVERED, Style.Default);
                case TileState.FLAG:
                    return (RUNE_FLAG, flagStyle);
                case TileState.MINE:
                    return (RUNE_MINE, mineStyle);
                case TileState.EMPTY:
                    int adjCount = board.GetAdjacentCount(tilePos);
                    return (NUM_RUNES[adjCount], numStyles[adjCount]);
                default:
                    throw new ArgumentException($"Invalid tile state: {tile}", nameof(tile));
            }
        }

        public void DrawOnto(IDrawSurface destination, Rectangle srcArea, Point destLocation){
            //TODO: Use srcArea
            for(int x=0; x<srcArea.Width; x++){
                for(int y=0; y<srcArea.Height; y++){
                    Point tilePos = new(x, y);
                    TileState tileState = board.GetTile(tilePos);
                    Point destTilePos = new(x, y);
                    destTilePos.Offset(destLocation);
                    (Rune rune, Style style) = GetRune(tilePos, tileState);
                    destination.DrawCell(destTilePos, rune, style);
                }
            }
        }
    }
}
