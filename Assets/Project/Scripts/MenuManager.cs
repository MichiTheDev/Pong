using UnityEngine;
using UnityEngine.SceneManagement;

namespace Twode.Pong
{
    public enum GameMode
    {
        PvP = 0,
        PvE = 1
    }
    
    public sealed class MenuManager : MonoBehaviour
    {
        public void SelectGameMode(int gameMode)
        {
            GameManager.Instance!.SetGameMode((GameMode) gameMode);
            SceneManager.LoadScene("Pong");
        }
        
        public void QuitGame()
        {
            Application.Quit();
        }
    }
}