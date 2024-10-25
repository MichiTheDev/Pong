using System;
using JetBrains.Annotations;
using TwodeUtils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Twode.Pong
{
    public enum GameState
    {
        Idle, Playing, Paused
    }
    
    public sealed class LevelManager : Singleton<LevelManager>
    {
        private const float PADDLE_SPAWN_PADDING = 1.0f;
     
        public event Action OnGameStarted; 
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        public event Action<int, int> OnScoreUpdated;

        public GameState GameState { get; private set; } = GameState.Idle;
        
        [SerializeField] private Ball _ballPrefab;
        [SerializeField] private Paddle _paddlePrefab;
        [SerializeField] private SpriteRenderer _middleLinePrefab;

        [NotNull] private GameInput _input = null!;
        
        private Paddle _leftPaddle;
        private Paddle _rightPaddle;
        private Ball _ball;
        
        private int _leftScore;
        private int _rightScore;
        private int _lastWinner;
        
        protected override void Awake()
        {
            base.Awake();
            InitializeLevel();
        }

        #region Game Logic

        public void StartGame()
        {
            if(GameState != GameState.Idle) return;
            
            SetPaddleInputEnabled(true);
            LaunchBall();
            SetGameState(GameState.Playing);
            OnGameStarted?.Invoke();
        }

        public void PauseGame()
        {
            switch(GameState)
            {
                case GameState.Idle: break;
                case GameState.Playing: break;
                case GameState.Paused:
                    ResumeGame();
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            FreezeObjects();
            OnGamePaused?.Invoke();
        }

        public void ResumeGame()
        {
            switch(GameState)
            {
                case GameState.Idle: return;
                case GameState.Playing: return;
                case GameState.Paused: break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            UnfreezeObjects();
            OnGameResumed?.Invoke();
        }
        
        private void LaunchBall()
        {
            if(_ball is null)
            {
                Debug.LogError("Ball is null.");
                return;
            }
            _ball.Launch(_lastWinner);
        }

        private void ResetObjects()
        {
            _leftPaddle?.ResetObject();
            _rightPaddle?.ResetObject();
            _ball?.ResetObject();
        }

        private void FreezeObjects()
        {
            _leftPaddle?.Freeze();
            _rightPaddle?.Freeze();
            _ball?.Freeze();
        }

        private void UnfreezeObjects()
        {
            _leftPaddle?.Unfreeze();
            _rightPaddle?.Unfreeze();
            _ball?.Unfreeze();
        }

        #endregion

        #region Events

        private void StartGameInput(InputAction.CallbackContext context)
        {
            if(!context.started) return;
            StartGame();
        }
        
        private void PauseGameInput(InputAction.CallbackContext context)
        {
            if(!context.started) return;
            PauseGame();
        }
        
        private void GoalTouched(int side)
        {
            _lastWinner = side;
            int leftScore = side < 0 ? _leftScore + 1 : _leftScore;
            int rightScore = side > 0 ? _rightScore + 1 : _rightScore;
            
            SetScore(leftScore, rightScore);
            SetGameState(GameState.Idle);
            SetPaddleInputEnabled(false);
            ResetObjects();
        }

        #endregion

        #region Initialization

        private void InitializeLevel()
        {
            if(_ballPrefab is null || _paddlePrefab is null)
            {
                Debug.LogError("Ball or Paddle was not set in editor. Level cannot be initialized.");
                return;
            }
            
            SpawnLevel();
            ConfigureInput();
        }

        private void SpawnLevel()
        {
            Camera cam = Camera.main;
            if(cam is null)
            {
                Debug.LogError("No Main Camera set.");
                return;
            }
            
            float screenHeight = cam.orthographicSize * 2.0f;
            float screenWidth = screenHeight * cam.aspect;
            float halfScreenHeight = screenHeight / 2.0f;
            float halfScreenWidth = screenWidth / 2.0f;

            SpawnPaddles();
            SpawnBall();
            SpawnGoals();
            SpawnLevelBorders();
            SpawnMiddleLine();
            
            return;
            void SpawnPaddles()
            {
                 Vector3 leftSpawnPosition = new Vector3(-halfScreenWidth + PADDLE_SPAWN_PADDING, 0);
                 Vector3 rightSpawnPosition = new Vector3(halfScreenWidth - PADDLE_SPAWN_PADDING, 0);

                _leftPaddle = Instantiate(_paddlePrefab, leftSpawnPosition, Quaternion.identity)!;
                _rightPaddle = Instantiate(_paddlePrefab, rightSpawnPosition, Quaternion.identity)!;
            }
            
            void SpawnBall()
            {
                _ball = Instantiate(_ballPrefab);
                _ball!.OnGoalTouched += GoalTouched;
            }

            void SpawnGoals()
            {
                GameObject leftGoal = new GameObject("Left Goal");
                leftGoal.transform.position = new Vector3(-halfScreenWidth, 0f);
                leftGoal.tag = "Goal";
                
                GameObject rightGoal = new GameObject("Right Goal");
                rightGoal.transform.position = new Vector3(halfScreenWidth, 0f);
                rightGoal.tag = "Goal";
                
                BoxCollider2D leftCollider = leftGoal.AddComponent<BoxCollider2D>()!;
                leftCollider.size = new Vector2(1f, screenHeight);
                leftCollider.isTrigger = true;
                
                BoxCollider2D rightCollider = rightGoal.AddComponent<BoxCollider2D>()!;
                rightCollider.size = new Vector2(1f, screenHeight);
                rightCollider.isTrigger = true;
            }

            void SpawnLevelBorders()
            {
                GameObject topBorder = new GameObject("Top Border");
                topBorder.transform.position = new Vector3(0f, halfScreenHeight + 0.5f);
                
                GameObject bottomBorder = new GameObject("Bottom Border");
                bottomBorder.transform.position = new Vector3(0f, -halfScreenHeight - 0.5f);
                
                BoxCollider2D topCollider = topBorder.AddComponent<BoxCollider2D>()!;
                topCollider.size = new Vector2(screenWidth, 1f);

                BoxCollider2D bottomCollider = bottomBorder.AddComponent<BoxCollider2D>()!;
                bottomCollider.size = new Vector2(screenWidth, 1f);
            }

            void SpawnMiddleLine()
            {
                SpriteRenderer middleLine = Instantiate(_middleLinePrefab)!;
                middleLine.size = new Vector2(0.125f, screenHeight);
            }
        }

        private void ConfigureInput()
        {
            _input = new GameInput();
            _input.Enable();
            
            ConfigurePaddleInput();
            SetPaddleInputEnabled(false);
            ConfigureGameInput();

            return;
            void ConfigurePaddleInput()
            {
                _leftPaddle!.SetMovementInputActions(_input.LeftPlayer.Movement);
                
                if(GameManager.Instance!.GameMode == GameMode.PvP)
                {
                    _rightPaddle!.SetMovementInputActions(_input.RightPlayer.Movement);
                    return;
                }
                
                _rightPaddle!.EnableAI();
            }

            void ConfigureGameInput()
            {
                InputAction startGameAction = _input.Game.StartGame;
                InputAction pauseGameAction = _input.Game.PauseGame;
                
                if(startGameAction is null || pauseGameAction is null)
                {
                    Debug.LogError("StartGame or PauseGame not found or initialized.");
                    return;
                }
            
                startGameAction.started += StartGameInput;
                pauseGameAction.started += PauseGameInput;
            }
        }

        #endregion
        
        private void SetGameState(GameState newGameState)
        {
            GameState = newGameState;
        }

        private void SetScore(int leftScore, int rightScore)
        {
            _leftScore = leftScore;
            _rightScore = rightScore;
            OnScoreUpdated?.Invoke(_leftScore, _rightScore);
        }

        private void SetPaddleInputEnabled(bool inputEnabled)
        {
            _leftPaddle?.SetInputEnabled(inputEnabled);
            _rightPaddle?.SetInputEnabled(inputEnabled);
        }
    }
}