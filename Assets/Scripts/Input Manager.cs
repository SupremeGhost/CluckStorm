using UnityEngine;
using UnityEngine.InputSystem;

namespace Cluckstorm.Input
{
    /// <summary>
    /// Central input manager for Cluckstorm.
    /// Handles keyboard, mouse, and gamepad input across the game.
    /// Designed for networked gameplay using FishNet.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        #region Input State
        // Movement
        [HideInInspector] public Vector2 MoveInput { get; private set; }
        [HideInInspector] public Vector2 LookInput { get; private set; }

        // Actions
        [HideInInspector] public bool JumpPressed { get; private set; }
        [HideInInspector] public bool JumpHeld { get; private set; }
        
        [HideInInspector] public bool FirePressed { get; private set; }
        [HideInInspector] public bool FireHeld { get; private set; }
        
        [HideInInspector] public bool ReloadPressed { get; private set; }
        [HideInInspector] public bool InteractPressed { get; private set; }

        // Aiming (optional feature)
        [HideInInspector] public bool AimHeld { get; private set; }
        #endregion

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
            // Movement input
            MoveInput = new Vector2(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical")
            );

            // Look input
            LookInput = new Vector2(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y")
            );

            // Jump
            JumpPressed = Input.GetKeyDown(KeyCode.Space);
            JumpHeld = Input.GetKey(KeyCode.Space);

            // Fire
            FirePressed = Input.GetMouseButtonDown(0);
            FireHeld = Input.GetMouseButton(0);

            // Reload
            ReloadPressed = Input.GetKeyDown(KeyCode.R);

            // Interact
            InteractPressed = Input.GetKeyDown(KeyCode.E);

            // Aiming
            AimHeld = Input.GetMouseButton(1);
        }
        #endregion
    }
}