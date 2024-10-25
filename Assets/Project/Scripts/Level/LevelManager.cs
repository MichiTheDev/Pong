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
        [SerializeField] private Paddle paddlePrefab;

        [NotNull] private GameInput _input = null!;
        
        private IPaddleController _leftPaddleController;
        private IPaddleController _rightPaddleController;
        private Ball _ball;
        
        private int _leftScore;
        private int _rightScore;
        private int _lastWinner = -1;
        
        protected override void Awake()
        {
            base.Awake();
            InitializeLevel();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _input.Dispose();
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
                case GameState.Idle: return;
                case GameState.Playing: break;
                case GameState.Paused:
                    ResumeGame();
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            FreezeObjects();
            SetGameState(GameState.Paused);
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
            SetGameState(GameState.Playing);
            OnGameResumed?.Invoke();
        }
        
        private void LaunchBall()
        {
            if(_ball is null)
            {
                Debug.LogError("Ball is null.");
                return;
            }
            _ball.Launch(_lastWinner < 0 ? 1 : -1);
        }

        private void ResetObjects()
        {
            _leftPaddleController?.Paddle?.ResetObject();
            _rightPaddleController?.Paddle?.ResetObject();
            _ball?.ResetObject();
        }

        private void FreezeObjects()
        {
            _leftPaddleController?.Paddle?.Freeze();
            _rightPaddleController?.Paddle?.Freeze();
            _ball?.Freeze();
        }

        private void UnfreezeObjects()
        {
            _leftPaddleController?.Paddle?.Unfreeze();
            _rightPaddleController?.Paddle?.Unfreeze();
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
            _lastWinner = side < 0 ? 1 : -1;
            int leftScore = side < 0 ? _leftScore : _leftScore + 1;
            int rightScore = side > 0 ? _rightScore : _rightScore  + 1;
            
            SetScore(leftScore, rightScore);
            SetGameState(GameState.Idle);
            SetPaddleInputEnabled(false);
            ResetObjects();
        }

        #endregion

        #region Initialization

        private void InitializeLevel()
        {
            if(_ballPrefab is null || paddlePrefab is null)
            {
                Debug.LogError("Ball or Paddle was not set in editor. Level cannot be initialized.");
                return;
            }
            
            SpawnLevelAndConfigureInput();
        }

        private void SpawnLevelAndConfigureInput()
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
            
            _input = new GameInput();
            _input.Enable();
            
            SpawnBall();
            SpawnGoals();
            SpawnLevelBorders();
            SpawnAndConfigurePaddles();
            ConfigureGameInput();
            
            return;
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
                topBorder.tag = "Border";
                
                GameObject bottomBorder = new GameObject("Bottom Border");
                bottomBorder.transform.position = new Vector3(0f, -halfScreenHeight - 0.5f);
                bottomBorder.tag = "Border";
                
                BoxCollider2D topCollider = topBorder.AddComponent<BoxCollider2D>()!;
                topCollider.size = new Vector2(screenWidth, 1f);

                BoxCollider2D bottomCollider = bottomBorder.AddComponent<BoxCollider2D>()!;
                bottomCollider.size = new Vector2(screenWidth, 1f);
            }
            
            void SpawnAndConfigurePaddles()
            {
                Vector3 leftSpawnPosition = new Vector3(-halfScreenWidth + PADDLE_SPAWN_PADDING, 0);
                Vector3 rightSpawnPosition = new Vector3(halfScreenWidth - PADDLE_SPAWN_PADDING, 0);

                Paddle leftPaddle = Instantiate(paddlePrefab, leftSpawnPosition, Quaternion.identity)!;
                Paddle rightPaddle = Instantiate(paddlePrefab, rightSpawnPosition, Quaternion.identity)!;

                PlayerPaddleController leftPaddleController = leftPaddle.gameObject.AddComponent<PlayerPaddleController>()!;
                _leftPaddleController = leftPaddleController;
                leftPaddleController.SetMovementInputActions(_input.LeftPlayer.Movement);
                _leftPaddleController.Paddle = leftPaddle;
                
                if(GameManager.Instance!.GameMode == GameMode.PvP)
                {
                    PlayerPaddleController rightPaddleController = rightPaddle.gameObject.AddComponent<PlayerPaddleController>()!;
                    _rightPaddleController = rightPaddleController;
                    rightPaddleController.SetMovementInputActions(_input.RightPlayer.Movement);
                }
                else
                {
                    AIPaddleController rightAIPaddleController = rightPaddle.gameObject.AddComponent<AIPaddleController>();
                    rightAIPaddleController?.SetTarget(_ball!.transform);
                    _rightPaddleController = rightAIPaddleController;
                }
                _rightPaddleController!.Paddle = rightPaddle;
                
                _leftPaddleController.SetInputEnabled(false);
                _rightPaddleController.SetInputEnabled(false);
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

            switch(GameState)
            {
                case GameState.Idle:
                    Cursor.visible = true;
                    break;
                case GameState.Playing:
                    Cursor.visible = false;
                    break;
                case GameState.Paused:
                    Cursor.visible = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetScore(int leftScore, int rightScore)
        {
            _leftScore = leftScore;
            _rightScore = rightScore;
            OnScoreUpdated?.Invoke(_leftScore, _rightScore);
        }

        private void SetPaddleInputEnabled(bool inputEnabled)
        {
            _leftPaddleController?.SetInputEnabled(inputEnabled);
            _rightPaddleController?.SetInputEnabled(inputEnabled);
        }
    }
}