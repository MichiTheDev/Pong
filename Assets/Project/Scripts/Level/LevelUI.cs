using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Twode.Pong
{
    public sealed class LevelUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _leftScoreText;
        [SerializeField] private TMP_Text _rightScoreText;
        [SerializeField] private TMP_Text _startGameHintText;
        [SerializeField] private TMP_Text _pausedText;
        [SerializeField] private GameObject _backButton;
        
        private void Start()
        {
            LevelManager.Instance!.OnScoreUpdated += ScoreUpdated;
            LevelManager.Instance.OnGameStarted += GameStarted;
            LevelManager.Instance.OnGamePaused += GamePaused;
            LevelManager.Instance.OnGameResumed += GameResumed;

            if(_pausedText is null)
            {
                Debug.LogError("Paused Text is not set in Editor.");
                return;
            }
            _pausedText.enabled = false;
        }
        
        private void GameStarted()
        {
            _backButton?.SetActive(false);

            if(_startGameHintText is not null)
            {
                _startGameHintText.enabled = false;
            }
            
            if(_pausedText is null) return;
            _pausedText.enabled = false;
        }
        
        private void ScoreUpdated(int leftScore, int rightScore)
        {
            _backButton?.SetActive(true);

            if(_leftScoreText is not null)
            {
                _leftScoreText.text = leftScore.ToString();
            }

            if(_rightScoreText is not null)
            {
                _rightScoreText.text = rightScore.ToString();
            }
            
            if(_startGameHintText is null) return;
            _startGameHintText.enabled = true;
        }
        
        private void GamePaused()
        {
            _backButton?.SetActive(true);
            
            if(_pausedText is not null)
            {
                _pausedText.enabled = true;
            }
        }

        private void GameResumed()
        {
            _backButton?.SetActive(false);
            
            if(_pausedText is not null)
            {
                _pausedText.enabled = false;
            }
        }

        public void BackToMenu()
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
