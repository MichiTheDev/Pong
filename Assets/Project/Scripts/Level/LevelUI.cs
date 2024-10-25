using TMPro;
using UnityEngine;

namespace Twode.Pong
{
    public sealed class LevelUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _leftScoreText;
        [SerializeField] private TMP_Text _rightScoreText;
        [SerializeField] private TMP_Text _startGameHintText;

        private void Start()
        {
            LevelManager.Instance!.OnScoreUpdated += ScoreUpdated;
            LevelManager.Instance.OnGameStarted += GameStarted;
        }
        
        private void GameStarted()
        {
            if(_startGameHintText is null) return;
            
            _startGameHintText.enabled = false;
        }
        
        private void ScoreUpdated(int leftScore, int rightScore)
        {
            if(_leftScoreText is null || _rightScoreText is null) return;

            _leftScoreText.text = leftScore.ToString();
            _rightScoreText.text = rightScore.ToString();
            
            if(_startGameHintText is null) return;
            _startGameHintText.enabled = true;
        }
    }
}
