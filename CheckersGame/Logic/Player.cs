namespace CheckersGame.Logic
{
    public class Player
    {
        public string Name { get; private set; }
        public eCellState Symbol { get; private set; }
        public int Score { get; private set; }

        public void IncreaseScoreBy(int i_Points)
        {
            Score += i_Points;
        }

        public Player(string i_Name, eCellState i_Symbol)
        {
            Name = string.IsNullOrWhiteSpace(i_Name) ? "Unknown" : i_Name;
            Symbol = i_Symbol;
            Score = 0;
        }
    }
}