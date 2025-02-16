namespace CheckersGame.Logic
{
    public class Board
    {
        private readonly eCellState[,] r_Board;
        private readonly int r_UpperRegionEnd;
        private readonly int r_LowerRegionStart;
        private static readonly int[] sr_ValidBoardSizes = { 6, 8, 10 };

        public int Size { get; private set; }

        public eCellState[,] GetBoardState()
        {
            return r_Board;
        }

        public Board(int i_Size)
        {
            if (IsValidBoardSize(i_Size))
            {
                Size = i_Size;
                r_UpperRegionEnd = (Size / 2) - 1;
                r_LowerRegionStart = (Size / 2) + 1;
                r_Board = new eCellState[Size, Size];
                initializeBoard();
            }
            else
            {
                Size = 0;
                r_Board = null;
            }
        }

        public static bool IsValidBoardSize(int i_Size)
        {
            bool isValid = false;

            foreach (int size in sr_ValidBoardSizes)
            {
                if (size == i_Size)
                {
                    isValid = true;
                    break;
                }
            }

            return isValid;
        }

        private void initializeBoard()
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if ((row + col) % 2 != 0)
                    {
                        if (row < r_UpperRegionEnd)
                        {
                            r_Board[row, col] = eCellState.PlayerO;
                        }
                        else if (row >= r_LowerRegionStart)
                        {
                            r_Board[row, col] = eCellState.PlayerX;
                        }
                        else
                        {
                            r_Board[row, col] = eCellState.Empty;
                        }
                    }
                    else
                    {
                        r_Board[row, col] = eCellState.Empty;
                    }
                }
            }
        }

        public eCellState GetCellState(int i_Row, int i_Col)
        {
            return ValidatePosition(i_Row, i_Col) ? r_Board[i_Row, i_Col] : eCellState.Empty;
        }

        public void SetCellState(int i_Row, int i_Col, eCellState i_State)
        {
            if (ValidatePosition(i_Row, i_Col))
            {
                r_Board[i_Row, i_Col] = i_State;
            }
        }

        public bool ValidatePosition(int i_Row, int i_Col)
        {
            bool isValid = i_Row >= 0 && i_Row < Size && i_Col >= 0 && i_Col < Size;

            return isValid;
        }

        public char GetCharForCell(eCellState i_Cell)
        {
            char result;

            switch (i_Cell)
            {
                case eCellState.PlayerX:
                    result = 'X';
                    break;
                case eCellState.PlayerO:
                    result = 'O';
                    break;
                case eCellState.PlayerXKing:
                    result = 'K';
                    break;
                case eCellState.PlayerOKing:
                    result = 'U';
                    break;
                default:
                    result = ' ';
                    break;
            }

            return result;
        }

        public int[] CountPieces()
        {
            int[] counts = new int[4]; // [0]: PlayerXKings, [1]: PlayerXPlayer, [2]: PlayerOKings, [3]: PlayerOPlayer

            foreach (eCellState cell in r_Board)
            {
                switch (cell)
                {
                    case eCellState.PlayerXKing:
                        counts[0]++;
                        break;
                    case eCellState.PlayerX:
                        counts[1]++;
                        break;
                    case eCellState.PlayerOKing:
                        counts[2]++;
                        break;
                    case eCellState.PlayerO:
                        counts[3]++;
                        break;
                }
            }

            return counts;
        }
    }
}