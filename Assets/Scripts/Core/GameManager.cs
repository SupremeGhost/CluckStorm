using UnityEngine;

namespace Cluckstorm.Core
{
    /// <summary>
    /// Main game manager for Cluckstorm.
    /// Handles game state and lifecycle management.
    /// Local gameplay - no networking required.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState
        {
            Menu,
            Playing,
            Paused,
            GameOver
        }

        private GameState currentState = GameState.Menu;

        #region MonoBehaviour Callbacks
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
        #endregion

        #region Game State Management
        public void SetGameState(GameState newState)
        {
            GameState oldState = currentState;
            currentState = newState;

            Debug.Log($"Game state changed: {oldState} -> {newState}");

            switch (newState)
            {
                case GameState.Menu:
                    OnMenuState();
                    break;
                case GameState.Playing:
                    OnPlayingState();
                    break;
                case GameState.Paused:
                    OnPausedState();
                    break;
                case GameState.GameOver:
                    OnGameOverState();
                    break;
            }
        }

        private void OnMenuState()
        {
            Time.timeScale = 0f;
            Debug.Log("Main menu state");
        }

        private void OnPlayingState()
        {
            Time.timeScale = 1f;
            Debug.Log("Game started!");
        }

        private void OnPausedState()
        {
            Time.timeScale = 0f;
            Debug.Log("Game paused");
        }

        private void OnGameOverState()
        {
            Time.timeScale = 0f;
            Debug.Log("Game over");
        }
        #endregion

        #region Game Commands
        public void StartGame()
        {
            SetGameState(GameState.Playing);
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing)
                SetGameState(GameState.Paused);
            else if (currentState == GameState.Paused)
                SetGameState(GameState.Playing);
        }

        public void EndGame()
        {
            SetGameState(GameState.GameOver);
        }
        #endregion

        #region Getters
        public GameState CurrentState => currentState;
        #endregion
    }
}
