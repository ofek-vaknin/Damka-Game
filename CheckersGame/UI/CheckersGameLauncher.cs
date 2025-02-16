using CheckersGame.Logic;
using System.Windows.Forms;

namespace CheckersGame.UI
{
    public class CheckersGameLauncher
    {
        public void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            FormGameSettings settingsForm = new FormGameSettings();
            DialogResult result = settingsForm.ShowDialog();

            if(result == DialogResult.OK)
            {
                GameController gameController = new GameController(settingsForm.BoardSize, settingsForm.Player1Name,
                    settingsForm.Player2Name, settingsForm.IsAgainstComputer);
                FormCheckersGame gameForm = new FormCheckersGame(gameController);

                gameForm.ShowDialog();
            }
        }
    }
}