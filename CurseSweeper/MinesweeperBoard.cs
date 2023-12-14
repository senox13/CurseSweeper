using System.Collections;
using System.Drawing;

namespace CurseSweeper{
    public sealed class MinesweeperBoard{
        private readonly BitArray coveredTiles;
        private readonly HashSet<int> mines;
        private readonly HashSet<int> flags;
        private readonly int[] adjacentCounts;

        public MinesweeperGame.Difficulty Difficulty{get;}
        public bool GameStarted{get;private set;}
        public bool GameFinished => GameLost || GameWon;
        public bool GameLost{get;private set;}
        public bool GameWon{get;private set;}
        public Size Size => Difficulty.BoardSize;
        public int MineCount => Difficulty.MineCount;
        private int TileCount => Size.Width * Size.Height;
        public DateTime? StartTime;
        public DateTime? FinishTime;

        public MinesweeperBoard(MinesweeperGame.Difficulty difficulty){
            difficulty.Validate();
            Difficulty = difficulty;
            coveredTiles = new BitArray(TileCount, true);
            mines = GenerateMines();
            flags = new();
            adjacentCounts = UpdateAdjacentCounts();
        }

        private HashSet<int> GenerateMines(){
            //TODO: Reroll mine positions if the first revealed is a mine (Would also like to include a game seed, need to reconcile these two things)
            Random rand = new();
            HashSet<int> result = new(MineCount);
            while(result.Count < MineCount){
                result.Add(rand.Next(TileCount));
            }
            return result;
        }

        private int[] UpdateAdjacentCounts(){
            int[] result = new int[TileCount];
            for(int x=0; x<Size.Width; x++){
                for(int y=0; y<Size.Height; y++){
                    Point tilePos = new(x, y);
                    int count = 0;
                    foreach(Point adjPos in GetAdjacent(tilePos)){
                        if(mines.Contains(TileIndex(adjPos)))
                            count++;
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
        
        private IEnumerable<Point> GetAdjacent(Point centerPos){
            for(int x=-1; x<=1; x++){
                for(int y=-1; y<=1; y++){
                    Point candPos = new(centerPos.X + x, centerPos.Y + y);
                    if(!ValidateTilePos(candPos))
                        continue;
                    if(candPos == centerPos)
                        continue;
                    yield return candPos;
                }
            }
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
        
        public int GetAdjacentFlagCount(Point tilePos){
            int result = 0;
            foreach(Point adjPos in GetAdjacent(tilePos)){
                int adjIndex = TileIndex(adjPos);
                if(flags.Contains(adjIndex))
                    result++;
            }
            return result;
        }
        
        public void ToggleFlag(Point tilePos){
            if(GameFinished)
                return;
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
                //Tile already uncovered, check if adjacent mine count is equal to adjacent flag count
                if(GetAdjacentCount(tilePos) > GetAdjacentFlagCount(tilePos))
                    return;
                foreach(Point adjPos in GetAdjacent(tilePos)){
                    int adjIndex = TileIndex(adjPos);
                    if(flags.Contains(adjIndex))
                        continue;
                    if(coveredTiles[adjIndex])
                        UncoverTile(adjPos);
                }
                return;
            }
            if(flags.Contains(tileIndex)){
                return;
            }
            coveredTiles[tileIndex] = false;
            if(!GameStarted){
                GameStarted = true;
                StartTime = DateTime.Now;
            }
            if(GetAdjacentCount(tilePos)==0){
                Queue<Point> queue = new();
                HashSet<Point> checkedTiles = new();
                HashSet<Point> connectedZeroAdjacent = new(){tilePos};
                queue.Enqueue(tilePos);
                while(queue.Count > 0){
                    Point p = queue.Dequeue();
                    foreach(Point adjPos in GetAdjacent(p)){
                        if(checkedTiles.Contains(adjPos))
                            continue;
                        if(GetAdjacentCount(adjPos)==0)
                            connectedZeroAdjacent.Add(adjPos);
                    }
                }
                foreach(Point p in connectedZeroAdjacent){
                    foreach(Point adjPos in GetAdjacent(p)){
                        UncoverTile(adjPos);
                    }
                }
            }
            if(mines.Contains(tileIndex)){
                //If this tile is a mine, lose the game
                GameLost = true;
                FinishTime = DateTime.Now;
                UncoverAllMines();
            }else if(coveredTiles.Cast<bool>().Count(b => b) == MineCount){
                //If only mines remain covered, win the game
                GameWon = true;
                FinishTime = DateTime.Now;
                //Flag all mines
                foreach(int mineIndex in mines){
                    flags.Add(mineIndex);
                }
            }
        }
    }
}
