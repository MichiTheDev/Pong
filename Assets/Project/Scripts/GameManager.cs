using TwodeUtils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Twode.Pong
{
    public enum GameMode
    {
        PvP = 0,
        PvE = 1
    }
    
    public sealed class GameManager : Singleton<GameManager>
    {
        public GameMode GameMode { get; private set; }
        
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        // GameMode can only be set in menu
        public void SetGameMode(GameMode gameMode)
        {
            if(SceneManager.GetActiveScene().name != "Menu") return;
            
            GameMode = gameMode;
            SceneManager.LoadScene("Pong");
        }
    }
    
    public sealed class GameInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneload()
        {
            new GameObject("Game Manager").AddComponent<GameManager>();
        }
    }
}