using System;
using System.Drawing;
using System.Windows.Forms;
using CheckersGame.Logic;
using CheckersGame.Properties;

namespace CheckersGame.UI
{
    public class FormCheckersGame : Form
    {
        private const int k_ButtonSize = 50;
        private Label m_LabelPlayer1;
        private Label m_LabelPlayer2;
        private Button[,] m_ButtonsBoard;
        private readonly GameController r_GameController;
        private bool m_IsPieceSelected = false;
        private Button m_SelectedButton = null;
        private readonly Timer r_ComputerMoveTimer;
        private bool m_IsComputerMoveInProgress = false;

        public FormCheckersGame(GameController i_GameController)
        {
            r_GameController = i_GameController;
            initializeComponents();
            initializeBoard();
            updateScoresLabels();
            r_ComputerMoveTimer = new Timer();
            r_ComputerMoveTimer.Interval = 1000;
            r_ComputerMoveTimer.Tick += r_ComputerMoveTimer_Tick;
        }

        private void initializeComponents()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Checkers Game";
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            m_LabelPlayer1 = new Label();
            m_LabelPlayer1.AutoSize = true;
            m_LabelPlayer1.Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);
            m_LabelPlayer2 = new Label();
            m_LabelPlayer2.AutoSize = true;
            m_LabelPlayer2.Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);
            this.Controls.Add(m_LabelPlayer1);
            this.Controls.Add(m_LabelPlayer2);
            updatePlayerLabelsPosition();
        }

        private void updatePlayerLabelsPosition()
        {
            int boardSize = r_GameController.Board.Size;
            int boardWidthInPixels = boardSize * k_ButtonSize;
            int formWidth = boardWidthInPixels + 40;

            this.ClientSize = new Size(formWidth, this.ClientSize.Height);
            m_LabelPlayer1.Text = $"{r_GameController.FirstPlayer.Name}: {r_GameController.FirstPlayer.Score}";
            m_LabelPlayer2.Text = $"{r_GameController.SecondPlayer.Name}: {r_GameController.SecondPlayer.Score}";
            int player1LabelX = (formWidth / 4) - (m_LabelPlayer1.Width / 2);
            int player2LabelX = (3 * formWidth / 4) - (m_LabelPlayer2.Width / 2);

            m_LabelPlayer1.Location = new Point(player1LabelX, 10);
            m_LabelPlayer2.Location = new Point(player2LabelX, 10);
        }

        private void initializeBoard()
        {
            const int k_TopOffset = 40;
            const int k_LeftOffset = 10;
            int boardSize = r_GameController.Board.Size;

            m_ButtonsBoard = new Button[boardSize, boardSize];
            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    Button button = new Button();

                    button.TabStop = false;
                    button.CausesValidation = false;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 0;
                    button.Size = new Size(k_ButtonSize, k_ButtonSize);
                    button.Location = new Point(k_LeftOffset + col * k_ButtonSize, k_TopOffset + row * k_ButtonSize);
                    button.Tag = new Point(row, col);
                    button.Click += button_Click;
                    if ((row + col) % 2 == 0)
                    {
                        button.BackColor = Color.FromArgb(220, 220, 220);
                        button.Enabled = false;
                    }
                    else
                    {
                        button.BackColor = Color.FromArgb(50, 50, 50);
                    }

                    this.Controls.Add(button);
                    m_ButtonsBoard[row, col] = button;
                }
            }

            int formWidth = boardSize * k_ButtonSize + 40;
            int formHeight = boardSize * k_ButtonSize + 80;

            this.ClientSize = new Size(formWidth, formHeight);
            updateBoardUI();
        }

        private static Image pieceToImage(eCellState i_State)
        {
            Image result;

            switch (i_State)
            {
                case eCellState.PlayerX:
                    result = Resources.blackpiece;
                    break;
                case eCellState.PlayerXKing:
                    result = Resources.blackpieceking;
                    break;
                case eCellState.PlayerO:
                    result = Resources.redpiece;
                    break;
                case eCellState.PlayerOKing:
                    result = Resources.redpieceking;
                    break;
                case eCellState.Empty:
                default:
                    result = null;
                    break;
            }

            return result;
        }

        private void updateBoardUI()
        {
            int boardSize = r_GameController.Board.Size;

            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    eCellState cellState = r_GameController.Board.GetCellState(row, col);
                    Image pieceImage = pieceToImage(cellState);

                    m_ButtonsBoard[row, col].BackgroundImageLayout = ImageLayout.Stretch;
                    m_ButtonsBoard[row, col].BackgroundImage = pieceImage;
                }
            }
        }

        private void updateScoresLabels()
        {
            m_LabelPlayer1.Text = $"{r_GameController.FirstPlayer.Name}: {r_GameController.FirstPlayer.Score}";
            m_LabelPlayer2.Text = $"{r_GameController.SecondPlayer.Name}: {r_GameController.SecondPlayer.Score}";
        }

        private void button_Click(object sender, EventArgs e)
        {
            Button theSender = sender as Button;

            handleButtonClick(theSender);
        }

        private void handleButtonClick(Button i_ClickedButton)
        {
            bool isCanContinue = i_ClickedButton != null;

            if (isCanContinue && r_GameController.IsComputerPlayer(r_GameController.CurrentPlayer))
            {
                isCanContinue = false;
            }

            if (isCanContinue)
            {
                handleValidButtonClick(i_ClickedButton);
            }
        }

        private void handleValidButtonClick(Button i_ClickedButton)
        {
            Point position = (Point)i_ClickedButton.Tag;
            int row = position.X;
            int col = position.Y;
            eCellState cellState = r_GameController.Board.GetCellState(row, col);

            if (!m_IsPieceSelected)
            {
                handleFirstClick(i_ClickedButton, cellState);
            }
            else
            {
                handleSecondClick(i_ClickedButton);
            }
        }

        private void handleFirstClick(Button i_ClickedButton, eCellState i_CellState)
        {
            if (isCellBelongToCurrentPlayer(i_CellState))
            {
                m_IsPieceSelected = true;
                m_SelectedButton = i_ClickedButton;
                i_ClickedButton.BackColor = Color.LightBlue;
                this.ActiveControl = null;
            }
        }

        private bool isCellBelongToCurrentPlayer(eCellState i_CellState)
        {
            Player current = r_GameController.CurrentPlayer;

            return (i_CellState == current.Symbol ||
                    (i_CellState == eCellState.PlayerXKing && current.Symbol == eCellState.PlayerX) ||
                    (i_CellState == eCellState.PlayerOKing && current.Symbol == eCellState.PlayerO));
        }

        private void handleSecondClick(Button i_ClickedButton)
        {
            Point toPos = (Point)i_ClickedButton.Tag;
            Point fromPos = (Point)m_SelectedButton.Tag;
            bool isSameCell = fromPos == toPos;

            if (!isSameCell)
            {
                string fromCellStr = convertRowColToCellString(fromPos.X, fromPos.Y);
                string toCellStr = convertRowColToCellString(toPos.X, toPos.Y);
                eMoveResult moveResult = r_GameController.MakeMove(fromCellStr, toCellStr);

                handleMoveResult(moveResult);
            }

            m_SelectedButton.BackColor = Color.FromArgb(50, 50, 50);
            m_IsPieceSelected = false;
            m_SelectedButton = null;
        }

        private static string convertRowColToCellString(int i_Row, int i_Col)
        {
            char rowChar = (char)('A' + i_Row);
            char colChar = (char)('a' + i_Col);

            return $"{rowChar}{colChar}";
        }

        private void handleMoveResult(eMoveResult i_Result)
        {
            switch (i_Result)
            {
                case eMoveResult.InvalidFormat:
                    MessageBox.Show("Invalid cell format!");
                    break;
                case eMoveResult.InvalidMove:
                    MessageBox.Show("Invalid move!");
                    break;
                case eMoveResult.MustCapture:
                    MessageBox.Show("You must capture!");
                    break;
                case eMoveResult.MustCaptureAgain:
                    MessageBox.Show("You have to continue your previous capture!");
                    break;
                case eMoveResult.AdditionalCaptureRequired:
                    updateBoardUI();
                    break;
                case eMoveResult.Success:
                    updateBoardUI();
                    checkGameOverAndHandleComputerTurn();
                    break;
            }
        }

        private void checkGameOverAndHandleComputerTurn()
        {
            if(r_GameController.IsGameOver(out Player winner))
            {
                displayGameOverMessage(winner);
            }
            else
            {
                if (r_GameController.IsComputerPlayer(r_GameController.CurrentPlayer))
                {
                    m_IsComputerMoveInProgress = true;
                    r_ComputerMoveTimer.Start();
                }
            }
        }

        private void r_ComputerMoveTimer_Tick(object sender, EventArgs e)
        {
            bool isStopTimer = !m_IsComputerMoveInProgress;

            if (!isStopTimer)
            {
                r_GameController.MakeComputerMove();
                updateBoardUI();
                if (r_GameController.IsGameOver(out Player winner) || !r_GameController.IsComputerPlayer(r_GameController.CurrentPlayer))
                {
                    isStopTimer = true;

                    m_IsComputerMoveInProgress = false;
                    if (r_GameController.IsGameOver(out winner))
                    {
                        displayGameOverMessage(winner);
                    }
                }
            }

            if (isStopTimer)
            {
                r_ComputerMoveTimer.Stop();
            }
        }

        private void displayGameOverMessage(Player i_Winner)
        {
            string message;

            if (i_Winner == null)
            {
                message = $"Tie!{Environment.NewLine}Another round?";
            }
            else
            {
                message = $"{i_Winner.Name} Won!{Environment.NewLine}Another round?";
            }

            DialogResult userChoice = MessageBox.Show(message, "Game Over", MessageBoxButtons.YesNo);

            handleEndGameDialogResult(userChoice);
        }

        private void handleEndGameDialogResult(DialogResult i_Result)
        {
            if (i_Result == DialogResult.Yes)
            {
                r_GameController.ResetGame(r_GameController.Board.Size);
                updateBoardUI();
                updateScoresLabels();
            }
            else
            {
                this.Close();
            }
        }
    }
}