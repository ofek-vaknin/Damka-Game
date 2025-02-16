using System;
using System.Collections.Generic;

namespace CheckersGame.Logic
{
    public class GameController
    {
        private Board m_Board;
        private readonly Player r_FirstPlayer;
        private readonly Player r_SecondPlayer;
        private Player m_CurrentPlayer;
        private readonly bool r_IsVsComputer;
        private bool m_IsForfeit;
        private Move m_LastMove;
        private Move m_PreviousMove;
        private readonly Random r_Random = new Random();

        public GameController(int i_BoardSize, string i_Player1Name, string i_Player2Name, bool i_IsVsComputer)
        {
            m_Board = new Board(i_BoardSize);
            r_FirstPlayer = new Player(i_Player1Name, eCellState.PlayerX);
            r_SecondPlayer = new Player(i_Player2Name, eCellState.PlayerO);
            m_CurrentPlayer = r_FirstPlayer;
            r_IsVsComputer = i_IsVsComputer;
        }

        public Board Board
        {
            get { return m_Board; }
        }

        public Player CurrentPlayer
        {
            get { return m_CurrentPlayer; }
        }

        public bool IsForfeit
        {
            get { return m_IsForfeit; }
        }

        public Player FirstPlayer
        {
            get { return r_FirstPlayer; }
        }

        public Player SecondPlayer
        {
            get { return r_SecondPlayer; }
        }

        public bool IsComputerPlayer(Player i_Player)
        {
            return r_IsVsComputer && i_Player == r_SecondPlayer;
        }

        public bool TryParseMove(string i_Input, out string o_FromCell, out string o_ToCell)
        {
            o_FromCell = null;
            o_ToCell = null;
            bool isValid = false;

            if (!string.IsNullOrWhiteSpace(i_Input))
            {
                string[] parts = i_Input.Split('>');
                if (parts.Length == 2 && parts[0].Length == 2 && parts[1].Length == 2)
                {
                    o_FromCell = parts[0];
                    o_ToCell = parts[1];
                    isValid = true;
                }
            }

            return isValid;
        }

        public static bool ValidatePlayerName(string i_Name)
        {
            return !string.IsNullOrWhiteSpace(i_Name) && i_Name.Length <= 20 && !i_Name.Contains(" ");
        }

        private bool parseBoardPosition(string i_Position, out int o_Row, out int o_Col)
        {
            o_Row = o_Col = -1;
            bool isValid = true;

            if (string.IsNullOrEmpty(i_Position) || i_Position.Length != 2)
            {
                isValid = false;
            }
            else
            {
                o_Row = (i_Position[0]) - 'A';
                o_Col = (i_Position[1]) - 'a';

                if (!m_Board.ValidatePosition(o_Row, o_Col))
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        private static eCellState convertToKingSymbol(eCellState i_PlayerSymbol)
        {
            eCellState kingSymbol = eCellState.Empty;

            if (i_PlayerSymbol == eCellState.PlayerX)
            {
                kingSymbol = eCellState.PlayerXKing;
            }
            else if (i_PlayerSymbol == eCellState.PlayerO)
            {
                kingSymbol = eCellState.PlayerOKing;
            }

            return kingSymbol;
        }

        private static bool isOpponentPiece(eCellState i_MidPiece, eCellState i_CurrentPlayerPiece)
        {
            bool isMidPieceOpponent = false;

            if (i_MidPiece != eCellState.Empty)
            {
                bool isCurrentPlayerX = i_CurrentPlayerPiece == eCellState.PlayerX || i_CurrentPlayerPiece == eCellState.PlayerXKing;
                bool isMidPieceO = i_MidPiece == eCellState.PlayerO || i_MidPiece == eCellState.PlayerOKing;
                bool isCurrentPlayerO = i_CurrentPlayerPiece == eCellState.PlayerO || i_CurrentPlayerPiece == eCellState.PlayerOKing;
                bool isMidPieceX = i_MidPiece == eCellState.PlayerX || i_MidPiece == eCellState.PlayerXKing;
                isMidPieceOpponent  = (isCurrentPlayerX && isMidPieceO) || (isCurrentPlayerO && isMidPieceX);
            }

            return isMidPieceOpponent ;
        }

        private bool isValidJump(int i_FromRow, int i_FromCol, int i_MidRow, int i_MidCol, int i_ToRow, int i_ToCol)
        {
            bool isJumpValid = false;

            if (m_Board.ValidatePosition(i_MidRow, i_MidCol) && m_Board.ValidatePosition(i_ToRow, i_ToCol))
            {
                eCellState movingPiece = m_Board.GetCellState(i_FromRow, i_FromCol);
                eCellState midPiece = m_Board.GetCellState(i_MidRow, i_MidCol);
                eCellState targetPiece = m_Board.GetCellState(i_ToRow, i_ToCol);
                int rowDiff = Math.Abs(i_FromRow - i_ToRow);
                int colDiff = Math.Abs(i_FromCol - i_ToCol);

                if (isOpponentPiece(midPiece, movingPiece) && targetPiece == eCellState.Empty && rowDiff == 2 && colDiff == 2)
                {
                    if ((movingPiece == eCellState.PlayerXKing || movingPiece == eCellState.PlayerOKing) ||
                        (movingPiece == eCellState.PlayerX && i_ToRow < i_FromRow) ||
                        (movingPiece == eCellState.PlayerO && i_ToRow > i_FromRow))
                    {
                        isJumpValid = true;
                    }
                }
            }

            return isJumpValid;
        }

        private List<Move> getCaptureMovesFromPiece(int i_Row, int i_Col)
        {
            List<Move> moves = new List<Move>();
            int[] rowOffsets = { -2, -2, 2, 2 };
            int[] colOffsets = { -2, 2, -2, 2 };

            for (int i = 0; i < rowOffsets.Length; i++)
            {
                int midRow = i_Row + rowOffsets[i] / 2;
                int midCol = i_Col + colOffsets[i] / 2;
                int toRow = i_Row + rowOffsets[i];
                int toCol = i_Col + colOffsets[i];

                if (isValidJump(i_Row, i_Col, midRow, midCol, toRow, toCol))
                {
                    moves.Add(new Move(i_Row, i_Col, toRow, toCol));
                }
            }

            return moves;
        }

        private List<Move> getCaptureMovesForPlayer(Player i_Player)
        {
            List<Move> captureMoves = new List<Move>();

            for (int row = 0; row < m_Board.Size; row++)
            {
                for (int col = 0; col < m_Board.Size; col++)
                {
                    if (m_Board.GetCellState(row, col) == i_Player.Symbol ||
                        m_Board.GetCellState(row, col) == convertToKingSymbol(i_Player.Symbol))
                    {
                        captureMoves.AddRange(getCaptureMovesFromPiece(row, col));
                    }
                }
            }

            return captureMoves;
        }

        private bool isValidMove(int i_FromRow, int i_FromCol, int i_ToRow, int i_ToCol)
        {
            bool isValid = false;

            if (m_Board.ValidatePosition(i_ToRow, i_ToCol) && m_Board.GetCellState(i_ToRow, i_ToCol) == eCellState.Empty)
            {
                eCellState movingPiece = m_Board.GetCellState(i_FromRow, i_FromCol);
                int rowDifference = Math.Abs(i_FromRow - i_ToRow);
                int colDifference = Math.Abs(i_FromCol - i_ToCol);

                if (movingPiece == eCellState.PlayerXKing || movingPiece == eCellState.PlayerOKing)
                {
                    isValid = rowDifference == 1 && colDifference == 1;
                }
                else if (movingPiece == eCellState.PlayerX)
                {
                    isValid = i_ToRow < i_FromRow && rowDifference == 1 && colDifference == 1;
                }
                else if (movingPiece == eCellState.PlayerO)
                {
                    isValid = i_ToRow > i_FromRow && rowDifference == 1 && colDifference == 1;
                }
            }

            return isValid;
        }

        private void addValidRegularMoves(int i_Row, int i_Col, List<Move> i_ValidMoves)
        {
            int[] rowOffsets = { -1, -1, 1, 1 };
            int[] colOffsets = { -1, 1, -1, 1 };

            for (int i = 0; i < rowOffsets.Length; i++)
            {
                int newRow = i_Row + rowOffsets[i];
                int newCol = i_Col + colOffsets[i];

                if (isValidMove(i_Row, i_Col, newRow, newCol))
                {
                    i_ValidMoves.Add(new Move(i_Row, i_Col, newRow, newCol));
                }
            }
        }

        private List<Move> getValidMovesForPlayer(Player i_Player)
        {
            List<Move> validMoves = new List<Move>();
            List<Move> captureMoves = getCaptureMovesForPlayer(i_Player);

            if (captureMoves.Count > 0)
            {
                validMoves = captureMoves;
            }
            else
            {
                for (int row = 0; row < m_Board.Size; row++)
                {
                    for (int col = 0; col < m_Board.Size; col++)
                    {
                        if (m_Board.GetCellState(row, col) == i_Player.Symbol ||
                            m_Board.GetCellState(row, col) == convertToKingSymbol(i_Player.Symbol))
                        {
                            addValidRegularMoves(row, col, validMoves);
                        }
                    }
                }
            }

            return validMoves;
        }

        private void switchTurns()
        {
            m_CurrentPlayer = m_CurrentPlayer == r_FirstPlayer ? r_SecondPlayer : r_FirstPlayer;
        }

        private void promoteToKingIfNeeded(int i_Row, int i_Col)
        {
            eCellState piece = m_Board.GetCellState(i_Row, i_Col);

            if (piece == eCellState.PlayerX && i_Row == 0)
            {
                m_Board.SetCellState(i_Row, i_Col, eCellState.PlayerXKing);
            }
            else if (piece == eCellState.PlayerO && i_Row == m_Board.Size - 1)
            {
                m_Board.SetCellState(i_Row, i_Col, eCellState.PlayerOKing);
            }
        }

        private void executeMove(int i_FromRow, int i_FromCol, int i_ToRow, int i_ToCol)
        {
            eCellState movingPiece = m_Board.GetCellState(i_FromRow, i_FromCol);

            m_Board.SetCellState(i_ToRow, i_ToCol, movingPiece);
            m_Board.SetCellState(i_FromRow, i_FromCol, eCellState.Empty);
            if (Math.Abs(i_FromRow - i_ToRow) == 2)
            {
                int middleRow = (i_FromRow + i_ToRow) / 2;
                int middleCol = (i_FromCol + i_ToCol) / 2;

                m_Board.SetCellState(middleRow, middleCol, eCellState.Empty);
            }

            promoteToKingIfNeeded(i_ToRow, i_ToCol);
        }

        public bool PlayerMustCapture(Player i_Player)
        {
            return getCaptureMovesForPlayer(i_Player).Count > 0;
        }

        public bool HasMoreJumps(int i_FromRow, int i_FromCol)
        {
            return getCaptureMovesFromPiece(i_FromRow, i_FromCol).Count > 0;
        }

        public eMoveResult MakeMove(string i_From, string i_To)
        {
            eMoveResult result = eMoveResult.InvalidFormat;

            if (parseBoardPosition(i_From, out int fromRow, out int fromCol) && parseBoardPosition(i_To, out int toRow, out int toCol))
            {
                result = eMoveResult.InvalidMove;
                bool isInChain = false;

                if (m_LastMove != null)
                {
                    int lastRowDiff = Math.Abs(m_LastMove.FromRow - m_LastMove.ToRow);

                    if (lastRowDiff == 2)
                    {
                        isInChain = true;
                    }
                }

                bool isPickedDifferentPiece = false;

                if (isInChain)
                {
                    isPickedDifferentPiece = (m_LastMove.ToRow != fromRow || m_LastMove.ToCol != fromCol);
                }

                bool isSkipFurtherChecks = false;
                bool isValidRegular = isValidMove(fromRow, fromCol, toRow, toCol);
                bool isJumpValid = isValidJump(fromRow, fromCol, (fromRow + toRow) / 2, (fromCol + toCol) / 2, toRow, toCol);
                bool isLegal = (isValidRegular || isJumpValid);

                if (isInChain && isPickedDifferentPiece) 
                {
                    result = !isLegal ? eMoveResult.InvalidMove : eMoveResult.MustCaptureAgain;
                    isSkipFurtherChecks = true;
                }

                if (!isSkipFurtherChecks) 
                {
                    result = validateAndExecutionMove(isLegal, isInChain, fromRow, fromCol, toRow, toCol);
                }
            }

            return result;
        }

        private eMoveResult validateAndExecutionMove(bool i_IsLegal, bool i_IsInChain, int i_FromRow, int i_FromCol, int i_ToRow, int i_ToCol)
        {
            eMoveResult result;

            if (!i_IsLegal)
            {
                result = eMoveResult.InvalidMove;
            }
            else
            {
                bool isMustCapture = PlayerMustCapture(m_CurrentPlayer);
                int rowDiff = Math.Abs(i_FromRow - i_ToRow);

                if (isMustCapture && rowDiff != 2)
                {
                    result = i_IsInChain ? eMoveResult.MustCaptureAgain : eMoveResult.MustCapture;
                }
                else
                {
                    executeMove(i_FromRow, i_FromCol, i_ToRow, i_ToCol);
                    m_LastMove = new Move(i_FromRow, i_FromCol, i_ToRow, i_ToCol);
                    if (rowDiff == 2 && HasMoreJumps(i_ToRow, i_ToCol))
                    {
                        result = eMoveResult.AdditionalCaptureRequired;
                    }
                    else
                    {
                        m_PreviousMove = m_LastMove;
                        m_LastMove = null;
                        switchTurns();
                        result = eMoveResult.Success;
                    }
                }
            }

            return result;
        }

        public eMoveResult MakeComputerMove()
        {
            eMoveResult result = eMoveResult.InvalidMove; 
            List<Move> captureMoves = getCaptureMovesForPlayer(m_CurrentPlayer);
            List<Move> validMoves = (captureMoves.Count > 0) ? captureMoves : getValidMovesForPlayer(m_CurrentPlayer);

            if (validMoves.Count > 0)
            {
                Move move = validMoves[r_Random.Next(validMoves.Count)];
                string fromPosition = $"{(char)('A' + move.FromRow)}{(char)('a' + move.FromCol)}";
                string toPosition = $"{(char)('A' + move.ToRow)}{(char)('a' + move.ToCol)}";

                result = MakeMove(fromPosition, toPosition);
            }

            return result;
        }

        public int CalculatePlayerScore(Player i_Player)
        {
            int score = 0;

            for (int row = 0; row < m_Board.Size; row++)
            {
                for (int col = 0; col < m_Board.Size; col++)
                {
                    eCellState cell = m_Board.GetCellState(row, col);

                    if (cell == i_Player.Symbol)
                    {
                        score += 1; 
                    }
                    else if (cell == convertToKingSymbol(i_Player.Symbol))
                    {
                        score += 4; 
                    }
                }
            }

            return score;
        }

        private void calculateAndAddScore(Player i_Winner, Player i_Loser)
        {
            int winnerScore = CalculatePlayerScore(i_Winner);
            int loserScore = CalculatePlayerScore(i_Loser);
            int scoreDifference = (winnerScore - loserScore);

            i_Winner.IncreaseScoreBy(scoreDifference);
        }

        private bool IsKingVsKingOnly()
        {
            int[] counts = m_Board.CountPieces();
            int playerXKings = counts[0];
            int playerXPieces = counts[1];
            int playerOKings = counts[2];
            int playerOPieces = counts[3];

            return playerXKings == 1 && playerOKings == 1 && playerXPieces == 0 && playerOPieces == 0;
        }

        public bool IsGameOver(out Player o_Winner)
        {
            bool isGameOver = false;

            o_Winner = null;
            if (IsKingVsKingOnly()) 
            {
                isGameOver = true;
            }
            else
            {
                List<Move> player1Moves = getValidMovesForPlayer(r_FirstPlayer);
                List<Move> player2Moves = getValidMovesForPlayer(r_SecondPlayer);

                if (player1Moves.Count == 0 && player2Moves.Count == 0)
                {
                    isGameOver = true; 
                }
                else if (player1Moves.Count == 0) 
                {
                    if (!PlayerMustCapture(r_SecondPlayer)) 
                    {
                        o_Winner = r_SecondPlayer;
                        calculateAndAddScore(r_SecondPlayer, r_FirstPlayer);
                        isGameOver = true;
                    }
                }
                else if (player2Moves.Count == 0) 
                {
                    if (!PlayerMustCapture(r_FirstPlayer)) 
                    {
                        o_Winner = r_FirstPlayer;
                        calculateAndAddScore(r_FirstPlayer, r_SecondPlayer);
                        isGameOver = true;
                    }
                }
            }

            return isGameOver; 
        }

        public void ResetGame(int i_BoardSize)
        {
            m_Board = new Board(i_BoardSize);
            m_CurrentPlayer = r_FirstPlayer;
            m_IsForfeit = false;
            m_LastMove = null;
            m_PreviousMove = null;
        }

        public void HandleForfeit(Player i_ForfeitingPlayer, out Player o_Winner)
        {
            Player opponent = i_ForfeitingPlayer == r_FirstPlayer ? r_SecondPlayer : r_FirstPlayer;
            int forfeitingPlayerScore = CalculatePlayerScore(i_ForfeitingPlayer);
            int opponentScore = CalculatePlayerScore(opponent);
            int scoreDifference = Math.Abs(opponentScore - forfeitingPlayerScore);

            opponent.IncreaseScoreBy(scoreDifference);
            o_Winner = opponent;
            m_IsForfeit = true;
        }

        public string GetLastMoveDetails()
        {
            if (m_PreviousMove == null)
            {
                return null;
            }

            Player lastPlayer = CurrentPlayer == FirstPlayer ? SecondPlayer : FirstPlayer;
            char fromRowChar = (char)('A' + m_PreviousMove.FromRow);
            char fromColChar = (char)('a' + m_PreviousMove.FromCol);
            char toRowChar = (char)('A' + m_PreviousMove.ToRow);
            char toColChar = (char)('a' + m_PreviousMove.ToCol);

            return $"{lastPlayer.Name}'s move was ({lastPlayer.Symbol}): {fromRowChar}{fromColChar}>{toRowChar}{toColChar}";
        }
    }
}