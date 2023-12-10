using System.Collections;
using System.Drawing;

namespace CurseSweeper{
    public sealed class MinesweeperBoard{
        private readonly BitArray coveredTiles;
        private readonly HashSet<int> mines;
        private readonly HashSet<int> flags;
        private readonly int[] adjacentCounts;

        public bool GameStarted{get;private set;}
        public bool GameFinished => GameLost || GameWon;
        public bool GameLost{get;private set;}
        public bool GameWon{get;private set;}
        public Size Size{get;}
        public int MineCount{get;}
        private int TileCount => Size.Width * Size.Height;

        public MinesweeperBoard(Size size, int mineCount){
            Size = size;
            MineCount = mineCount;
            coveredTiles = new BitArray(TileCount, true);
            mines = GenerateMines();
            flags = new();
            adjacentCounts = UpdateAdjacentCounts();
        }

        private HashSet<int> GenerateMines(){
            //TODO: Check that enough mines can actually be generated in the space given
            //TODO: Store an anyRevealed bool and use it to reroll mine positions if the first revealed is a mine
            Random rand = new();
            HashSet<int> result = new(MineCount);
            while(result.Count < MineCount){
                result.Add(rand.Next(TileCount));
            }
            return result;
        }

        private int[] UpdateAdjacentCounts(){
            //DEBUG
            int[] result = new int[TileCount];
            for(int x=0; x<Size.Width; x++){
                for(int y=0; y<Size.Height; y++){
                    Point tilePos = new(x, y);
                    int count = 0;
                    for(int xOff=-1; xOff<=1; xOff++){
                        for(int yOff=-1; yOff<=1; yOff++){
                            Point adjPos = new(x + xOff, y + yOff);
                            if(!ValidateTilePos(adjPos))
                                continue;
                            if(mines.Contains(TileIndex(adjPos)))
                                count++;
                        }
                    }
                    result[TileIndex(tilePos)] = count;
                }
            }
            return result;
        }

        private int TileIndex(Point tilePos){
            return tilePos.Y * Size.Width + tilePos.X;
        }

        private bool ValidateTilePos(Point tilePos){
            if(tilePos.X < 0 || tilePos.X >= Size.Width ||
                tilePos.Y < 0 || tilePos.Y >= Size.Height){
                return false;
            }
            return true;
        }

        private void UncoverAllMines(){
            foreach(int mineIndex in mines){
                coveredTiles[mineIndex] = false;
            }
        }

        public TileState GetTile(Point tilePos){
            if(!ValidateTilePos(tilePos)){
                throw new ArgumentException($"Tile position {tilePos} is out of bounds for board size {Size}", nameof(tilePos));
            }
            int tileIndex = TileIndex(tilePos);
            if(flags.Contains(tileIndex)){
                return TileState.FLAG;
            }
            if(coveredTiles[tileIndex]){
                return TileState.COVERED;
            }
            if(mines.Contains(tileIndex)){
                return TileState.MINE;
            }
            return TileState.EMPTY;
        }

        public int GetAdjacentCount(Point tilePos){
            if(!ValidateTilePos(tilePos)){
                throw new ArgumentException($"Tile position {tilePos} is out of bounds for board size {Size}", nameof(tilePos));
            }
            return adjacentCounts[TileIndex(tilePos)];
        }

        public void ToggleFlag(Point tilePos){
            int tileIndex = TileIndex(tilePos);
            if(!coveredTiles[tileIndex])
                return;
            if(flags.Contains(tileIndex)){
                flags.Remove(tileIndex);
            }else{
                if(flags.Count < MineCount){
                    flags.Add(tileIndex);
                }
            }
        }

        public void UncoverTile(Point tilePos){
            if(!ValidateTilePos(tilePos)){
                throw new ArgumentException($"Tile position {tilePos} is out of bounds for board size {Size}", nameof(tilePos));
            }
            if(GameFinished){
                return;
            }
            int tileIndex = TileIndex(tilePos);
            if(!coveredTiles[tileIndex]){
                //Tile already uncovered
                return;
            }
            if(flags.Contains(tileIndex)){
                return;
            }
            coveredTiles[tileIndex] = false;
            if(GetAdjacentCount(tilePos)==0){
                Queue<Point> queue = new();
                HashSet<Point> checkedTiles = new();
                HashSet<Point> connectedZeroAdjacent = new();
                queue.Enqueue(tilePos);
                while(queue.Count > 0){
                    Point p = queue.Dequeue();
                    for(int x=-1; x<=1; x++){
                        for(int y=-1; y<=1; y++){
                            Point candPos = new(p.X + x, p.Y + y);
                            if(!ValidateTilePos(candPos))
                                continue;
                            if(checkedTiles.Contains(candPos))
                                continue;
                            if(GetAdjacentCount(candPos)==0)
                                connectedZeroAdjacent.Add(candPos);
                        }
                    }
                }
                foreach(Point p in connectedZeroAdjacent){
                    for(int x=-1; x<=1; x++){
                        for(int y=-1; y<=1; y++){
                        Point offPos = new(p.X + x, p.Y + y);
                            if(!ValidateTilePos(offPos))
                                continue;
                            UncoverTile(offPos);
                        }
                    }
                }
            }
            if(mines.Contains(tileIndex)){
                //If this tile is a mine, lose the game
                GameLost = true;
                UncoverAllMines();
            }else if(coveredTiles.Cast<bool>().Count(b => b) == MineCount){
                //If only mines remain covered, win the game
                GameWon = true;
            }
        }
    }
}
