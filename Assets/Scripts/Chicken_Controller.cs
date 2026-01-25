using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Chicken_Controller.cs
/// 
/// Core controller for the player chicken. Handles:
/// - Input processing (look, movement, firing, jumping)
/// - Physics-based movement (walking and airborne)
/// - Recoil mechanic (core gameplay)
/// - Jumping mechanic
/// - Camera control (via CameraPivot)
/// 
/// Architecture: MonoBehaviour (FishNet networking to be integrated later)
/// </summary>
public class Chicken_Controller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private CameraPivot cameraPivot;

    [Header("Look")]
    [SerializeField] private float lookSpeed = 1f;
    private float cameraPitch = 0f;

    [Header("Movement - Walking")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float groundDrag = 5f;
    
    [Header("Movement - Air")]
    [SerializeField] private float airControl = 0.5f;
    [SerializeField] private float airDrag = 0.1f;
    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 0.5f;
    
    [Header("Recoil")]
    [SerializeField] private float recoilForce = 10f;
    
    // Input System
    private InputAction lookAction;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction fireAction;
    
    // State tracking
    private bool isGrounded = false;
    private bool canJump = true;
    private float jumpCooldownTimer = 0f;
    
    // Movement direction cache
    private Vector3 moveDirection = Vector3.zero;

    private void OnEnable()
    {
        SetupInputActions();
    }

    private void OnDisable()
    {
        DisableInputActions();
    }

    private void Awake()
    {
        // Get Rigidbody component if not assigned
        if (playerRigidbody == null)
            playerRigidbody = GetComponent<Rigidbody>();
        
        // Get CameraPivot if not assigned
        if (cameraPivot == null)
            cameraPivot = GetComponentInChildren<CameraPivot>();
        
        // Validate rigidbody setup
        if (playerRigidbody != null)
        {
            playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            playerRigidbody.useGravity = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetupInputActions()
    {
        // Get input actions from Input System
        InputActionAsset inputAsset = null;
        
        // Try to find InputActionAssets in the project
        InputActionAsset[] allAssets = Resources.FindObjectsOfTypeAll<InputActionAsset>();
        if (allAssets.Length > 0)
        {
            inputAsset = allAssets[0];
        }
        
        // Try loading from Resources folder if not found
        if (inputAsset == null)
        {
            inputAsset = Resources.Load<InputActionAsset>("InputActions");
        }
        
        if (inputAsset != null)
        {
            lookAction = inputAsset.FindAction("Look");
            moveAction = inputAsset.FindAction("Move");
            jumpAction = inputAsset.FindAction("Jump");
            fireAction = inputAsset.FindAction("Fire");
            
            if (lookAction != null) lookAction.Enable();
            if (moveAction != null) moveAction.Enable();
            if (jumpAction != null) jumpAction.Enable();
            if (fireAction != null) fireAction.Enable();
        }
        else
        {
            Debug.LogWarning("InputActionAsset not found. Please create Input Actions or assign via Inspector.");
        }
    }

    private void DisableInputActions()
    {
        if (lookAction != null) lookAction.Disable();
        if (moveAction != null) moveAction.Disable();
        if (jumpAction != null) jumpAction.Disable();
        if (fireAction != null) fireAction.Disable();
    }

    private void Update()
    {
        HandleLook();
        HandleInput();
        HandleJumpCooldown();
    }

    private void FixedUpdate()
    {
        if (!IsLocalPlayer()) return;

        HandleGroundDetection();
        HandleMovement();
    }

     private void HandleLook()
    {
        if (lookAction == null || !IsLocalPlayer()) return;

        Vector2 lookInput = lookAction.ReadValue<Vector2>() * lookSpeed * Time.deltaTime;

        // Player body rotation (Yaw)
        transform.Rotate(Vector3.up, lookInput.x);

        // Camera pivot rotation (Pitch)
        cameraPitch -= lookInput.y;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
        cameraPivot.transform.localEulerAngles = new Vector3(cameraPitch, 0, 0);
    }


    /// <summary>
    /// Processes all input from the Input System
    /// </summary>
    private void HandleInput()
    {
        if (!IsLocalPlayer()) return;

        // Movement input
        if (moveAction != null)
        {
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        }
        
        // Jump input
        if (jumpAction != null && jumpAction.triggered && canJump && isGrounded)
        {
            Jump();
        }
        
        // Fire input
        if (fireAction != null && fireAction.triggered)
        {
            Fire();
        }
    }

    /// <summary>
    /// Detects if the player is grounded using raycasts
    /// </summary>
    private void HandleGroundDetection()
    {
        // Raycast down from player position
        float groundDistance = 0.1f;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundDistance);
    }

    /// <summary>
    /// Handles movement: walking when grounded, air control when airborne
    /// </summary>
    private void HandleMovement()
    {
        if (playerRigidbody == null) return;

        if (isGrounded)
        {
            HandleWalking();
        }
        else
        {
            HandleAirControl();
        }
    }

    /// <summary>
    /// Walking movement: WASD-based, slow and grounded
    /// </summary>
    private void HandleWalking()
    {
        // Get camera forward and right directions
        Vector3 cameraForward = cameraPivot.transform.forward;
        Vector3 cameraRight = cameraPivot.transform.right;
        
        // Flatten camera forward to prevent vertical tilt from affecting movement
        cameraForward.y = 0;
        cameraForward.Normalize();
        cameraRight.y = 0;
        cameraRight.Normalize();

        // Calculate desired movement direction relative to camera
        Vector3 desiredDirection = (cameraForward * moveDirection.z + cameraRight * moveDirection.x).normalized;
        
        // Apply movement speed
        Vector3 horizontalVel = playerRigidbody.linearVelocity;
        horizontalVel.y = 0; // Preserve vertical velocity
        
        if (desiredDirection.magnitude > 0)
        {
            horizontalVel = desiredDirection * walkSpeed;
        }
        else
        {
            // Slow down with drag
            horizontalVel = Vector3.Lerp(horizontalVel, Vector3.zero, groundDrag * Time.fixedDeltaTime);
        }
        
        // Reapply velocity
        playerRigidbody.linearVelocity = new Vector3(horizontalVel.x, playerRigidbody.linearVelocity.y, horizontalVel.z);
    }

    /// <summary>
    /// Air control: weak input-based correction while airborne
    /// </summary>
    private void HandleAirControl()
    {
        if (moveDirection.magnitude == 0) return;

        Vector3 cameraForward = cameraPivot.transform.forward;
        Vector3 cameraRight = cameraPivot.transform.right;
        
        cameraForward.y = 0;
        cameraForward.Normalize();
        cameraRight.y = 0;
        cameraRight.Normalize();

        Vector3 desiredDirection = (cameraForward * moveDirection.z + cameraRight * moveDirection.x).normalized;
        
        // Apply small air impulse
        playerRigidbody.linearVelocity += desiredDirection * airControl;
        
        // Apply air drag
        playerRigidbody.linearDamping = airDrag;
    }

    /// <summary>
    /// Jump mechanic: applies upward force and starts cooldown
    /// </summary>
    private void Jump()
    {
        if (playerRigidbody == null) return;

        // Reset vertical velocity to ensure consistent jump height
        Vector3 vel = playerRigidbody.linearVelocity;
        vel.y = 0;
        playerRigidbody.linearVelocity = vel;

        // Apply jump force
        playerRigidbody.linearVelocity += Vector3.up * jumpForce;
        
        // Start cooldown
        canJump = false;
        jumpCooldownTimer = jumpCooldown;
    }

    /// <summary>
    /// Handles jump cooldown timer
    /// </summary>
    private void HandleJumpCooldown()
    {
        if (!canJump)
        {
            jumpCooldownTimer -= Time.deltaTime;
            if (jumpCooldownTimer <= 0)
            {
                canJump = true;
            }
        }
    }

    /// <summary>
    /// Fire mechanic: applies recoil force to player
    /// Should be called by weapon system. Placeholder implementation.
    /// </summary>
    public void Fire()
    {
        // Get camera aim direction
        Vector3 aimDirection = cameraPivot.transform.forward;
        
        // Apply recoil force opposite to aim
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity += -aimDirection * recoilForce;
        }
        
        // TODO: Integrate with actual weapon system
        // This will call weapon.Fire() and handle weapon-specific recoil
    }

    /// <summary>
    /// Public method to apply external recoil force
    /// Will be networked later
    /// </summary>
    public void ApplyRecoilForce(Vector3 forceDirection, float force)
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity += forceDirection * force;
        }
    }

    /// <summary>
    /// Gets the player's current velocity (for networking synchronization)
    /// </summary>
    public Vector3 GetVelocity()
    {
        return playerRigidbody != null ? playerRigidbody.linearVelocity : Vector3.zero;
    }

    /// <summary>
    /// Gets grounded state
    /// </summary>
    public bool IsGrounded()
    {
        return isGrounded;
    }

    /// <summary>
    /// Checks if this is the local player (for single-player testing)
    /// Will be replaced with FishNet's IsOwner check later
    /// </summary>
    private bool IsLocalPlayer()
    {
        // For now, only one player in the scene is the local player
        // This will be replaced with FishNet networking later
        return true;
    }
}
