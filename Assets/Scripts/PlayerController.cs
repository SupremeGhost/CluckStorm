using UnityEngine;
using Cluckstorm.Input;
using Cluckstorm.Characters;

namespace Cluckstorm.Player
{
    /// <summary>
    /// Core first-person player controller for Cluckstorm.
    /// Handles movement, jumping, camera control, and recoil-based physics.
    /// Local gameplay - all physics handled locally.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        #region Movement Settings
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float airControl = 0.3f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float groundDrag = 10f;
        [SerializeField] private float airDrag = 2f;
        [SerializeField] private float jumpCooldown = 0.1f;

        private float lastJumpTime = 0f;
        #endregion

        #region Physics References
        private Rigidbody rb;
        private Character character;
        private InputManager input;
        #endregion

        #region Movement State
        public enum MovementState
        {
            Idle,
            Walking,
            Airborne
        }

        private MovementState currentState = MovementState.Idle;
        private bool isGrounded = true;
        private Vector3 velocity = Vector3.zero;
        #endregion

        #region Camera
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float maxLookAngle = 90f;
        private float lookRotationX = 0f;
        #endregion

        #region Recoil (Movement from Shooting)
        public Vector3 RecoilForce { get; private set; } = Vector3.zero;
        #endregion

        #region MonoBehaviour Callbacks
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            character = GetComponent<Character>();
            input = InputManager.Instance;

            // Setup rigidbody for network physics
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        private void Update()
        {
            HandleCameraLook();
            HandleJump();
            UpdateMovementState();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
            ApplyGravity();
            ApplyDrag();
        }
        #endregion

        #region Camera Control
        private void HandleCameraLook()
        {
            Vector2 lookInput = input.LookInput;

            // Horizontal rotation (body)
            transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

            // Vertical rotation (camera)
            lookRotationX -= lookInput.y * mouseSensitivity;
            lookRotationX = Mathf.Clamp(lookRotationX, -maxLookAngle, maxLookAngle);
            cameraTransform.localRotation = Quaternion.Euler(lookRotationX, 0f, 0f);
        }
        #endregion

        #region Movement
        private void HandleJump()
        {
            if (!input.JumpPressed || !isGrounded) return;
            if (Time.time - lastJumpTime < jumpCooldown) return;

            lastJumpTime = Time.time;
            Jump();
        }

        private void Jump()
        {
            isGrounded = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        private void ApplyMovement()
        {
            Vector2 moveInput = input.MoveInput;
            Vector3 moveDirection = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;

            float moveSpeed = isGrounded ? walkSpeed : walkSpeed * airControl;

            // Apply physics-based movement (no teleporting)
            Vector3 targetVelocity = moveDirection * moveSpeed;

            // Only apply horizontal movement; preserve vertical velocity
            rb.linearVelocity = new Vector3(
                targetVelocity.x,
                rb.linearVelocity.y,
                targetVelocity.z
            );
        }

        private void ApplyGravity()
        {
            // Gravity is handled by rigidbody, but we can customize it here
            // Standard gravity is 9.81 m/s^2
        }

        private void ApplyDrag()
        {
            rb.linearDamping = isGrounded ? groundDrag : airDrag;
        }

        private void UpdateMovementState()
        {
            // Check if grounded
            RaycastHit hit;
            isGrounded = Physics.Raycast(
                transform.position,
                Vector3.down,
                out hit,
                0.5f,
                LayerMask.GetMask("Ground")
            );

            if (isGrounded && rb.linearVelocity.y <= 0)
            {
                currentState = input.MoveInput.magnitude > 0 ? MovementState.Walking : MovementState.Idle;
            }
            else
            {
                currentState = MovementState.Airborne;
            }
        }
        #endregion

        #region Recoil (Shooting Force)
        /// <summary>
        /// Applies recoil force to the player from weapon fire.
        /// Called by the weapon system when a shot is fired.
        /// </summary>
        public void ApplyRecoil(Vector3 recoilForce)
        {
            // Apply character physics modifier
            float modifier = character.PhysicsModifier.RecoilForceMultiplier;
            rb.AddForce(recoilForce * modifier, ForceMode.Impulse);
        }
        #endregion

        #region Getters
        public bool IsGrounded => isGrounded;
        public MovementState CurrentMovementState => currentState;
        public Vector3 CurrentVelocity => rb.linearVelocity;
        #endregion
    }
}
