using UnityEngine;

namespace Twode.Pong
{
    public sealed class MenuManager : MonoBehaviour
    {
        public void SelectGameMode(int gameMode)
        {
            GameManager.Instance!.SetGameMode((GameMode) gameMode);
        }
        
        public void QuitGame()
        {
            Application.Quit();
        }
    }
}