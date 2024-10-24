using System;
using JetBrains.Annotations;
using TwodeUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Twode.Pong
{
    public enum GameState
    {
        Idle, Playing, Paused
    }
    
    public sealed class GameManager : Singleton<GameManager>
    {
        private const float PADDLE_SPAWN_PADDING = 1.0f;
        
        public event Action OnGameStarted; 
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        public event Action<int, int> OnScoreUpdated;
        
        public GameMode GameMode { get; private set; }
        public GameState GameState { get; private set; }

        [NotNull] private GameInput _input = null!;

        private SpriteRenderer _middleLinePrefab;
        
        private Ball _ballPrefab;
        private Ball _ball;
        
        private Paddle _paddlePrefab;
        private Paddle _leftPaddle;
        private Paddle _rightPaddle;

        private Vector3 _leftSpawnPosition;
        private Vector3 _rightSpawnPosition;

        private int _leftScore;
        private int _rightScore;
        private int _lastWinner;
        
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            LoadResources();
            BindEvents();
            _input = new GameInput();
        }

        public void SetGameMode(GameMode gameMode)
        {
            GameMode = gameMode;
        }
        
        public void StartGame()
        {
            if(GameState != GameState.Idle) return;
            
            SetScore(0, 0);
            EnablePaddleInput();
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
        
        private void SetGameState(GameState newGameState)
        {
            GameState = newGameState;
        }
        
        private void RespawnObjects()
        {
            if(_leftPaddle is null || _rightPaddle is null || _ball is null) return;

            _ball.Reset();
            _leftPaddle.transform.position = _leftSpawnPosition;
            _rightPaddle.transform.position = _rightSpawnPosition;
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

        private void EnablePaddleInput()
        {
            _input.LeftPlayer.Enable();
            _input.RightPlayer.Enable();
        }

        private void DisablePaddleInput()
        {
            _input.LeftPlayer.Disable();
            _input.RightPlayer.Disable();
        }

        private void LoadResources()
        {
            _paddlePrefab = Resources.Load<Paddle>("Paddle");
            _ballPrefab = Resources.Load<Ball>("Ball");
            _middleLinePrefab = Resources.Load<SpriteRenderer>("Middle Line");
        }

        private void InitializeLevel()
        {
            Camera cam = Camera.main;
            if(cam is null)
            {
                Debug.LogError("No Camera detected.");
                return;
            }

            if(_paddlePrefab is null)
            {
                Debug.LogError("Paddle prefab is null. Not loaded correctly.");
                return;
            }

            float screenHeight = cam.orthographicSize * 2.0f;
            float screenWidth = screenHeight * cam.aspect;
            float halfScreenHeight = screenHeight / 2.0f;
            float halfScreenWidth = screenWidth / 2.0f;
            
            SpawnPaddles();
            ConfigurePaddles();
            SpawnBall();
            SpawnGoals();
            SpawnLevelBorders();
            SpawnMiddleLine();
            
            return;
            void SpawnPaddles()
            {
                _leftSpawnPosition = new Vector3(-halfScreenWidth + PADDLE_SPAWN_PADDING, 0);
                _rightSpawnPosition = new Vector3(halfScreenWidth - PADDLE_SPAWN_PADDING, 0);

                _leftPaddle = Instantiate(_paddlePrefab, _leftSpawnPosition, Quaternion.identity);
                _rightPaddle = Instantiate(_paddlePrefab, _rightSpawnPosition, Quaternion.identity)!;
            }

            void ConfigurePaddles()
            {
                _leftPaddle!.SetMovementInputActions(_input.LeftPlayer.Movement);
                if(GameMode == GameMode.PvP)
                {
                    _rightPaddle!.SetMovementInputActions(_input.RightPlayer.Movement);
                    return;
                }
                
                _rightPaddle!.EnableAI();
            }

            void SpawnBall()
            {
                _ball = Instantiate(_ballPrefab);
                _ball!.OnGoalTouched += GoalScored;
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
                
        private void GoalScored(int side)
        {
            _lastWinner = side;
            int leftScore = side < 0 ? _leftScore + 1 : _leftScore;
            int rightScore = side > 0 ? _rightScore + 1 : _rightScore;
            
            SetScore(leftScore, rightScore);
            SetGameState(GameState.Idle);
            DisablePaddleInput();
            RespawnObjects();
        }

        private void BindEvents()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            switch(scene.name)
            {
                case null: return;
                case "Pong":
                    InitializeLevel();
                    BindInput();
                    DisablePaddleInput();
                    break;
                default:
                    DisableInput();
                    break;
            }
        }

        private void BindInput()
        {
            _input.Enable();

            if(_input.Game.StartGame is null || _input.Game.PauseGame is null)
            {
                Debug.LogError("StartGame or PauseGame not initialized. Unitys fault tho.");
                return;
            }
            
            _input.Game.StartGame.started += StartGameInput;
            _input.Game.PauseGame.started += PauseGameInput;
        }

        private void DisableInput()
        {
            _input.Disable();
        }
        
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

        private void SetScore(int leftScore, int rightScore)
        {
            _leftScore = leftScore;
            _rightScore = rightScore;
            OnScoreUpdated?.Invoke(_leftScore, _rightScore);
        }

        private void FreezeObjects()
        {
            _ball?.Freeze();
            _leftPaddle?.Freeze();
            _rightPaddle?.Freeze();
        }

        private void UnfreezeObjects()
        {
            _ball?.Unfreeze();
            _leftPaddle?.Unfreeze();
            _rightPaddle?.Unfreeze();
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