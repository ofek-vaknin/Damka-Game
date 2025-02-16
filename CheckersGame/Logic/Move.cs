namespace CheckersGame.Logic
{
    public class Move
    {
        public int FromRow { get; }
        public int FromCol { get; }
        public int ToRow { get; }
        public int ToCol { get; }

        public Move(int i_FromRow, int i_FromCol, int i_ToRow, int i_ToCol)
        {
            FromRow = i_FromRow;
            FromCol = i_FromCol;
            ToRow = i_ToRow;
            ToCol = i_ToCol;
        }
    }
}