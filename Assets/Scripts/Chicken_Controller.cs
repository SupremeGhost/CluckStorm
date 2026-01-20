using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Chicken_Controller : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Look")]
    [SerializeField] private Transform cameraPitchPivot; // child used only for pitch (up/down)
    [SerializeField] private float lookSensitivity = 0.02f; // multiplier for mouse delta
    [SerializeField] private float pitchMin = -70f;
    [SerializeField] private float pitchMax = 70f;

    [Header("Physics")]
    [SerializeField] private float recoilMultiplier = 1f;

    private Rigidbody _rb;
    private float _pitch = 0f; // current pitch angle
    private bool _cursorLocked = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // Freeze rotation so physics doesn't knock us over, but we still rotate via code
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        // Cursor lock/unlock via Input System
        if (!_cursorLocked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SetCursorLock(true);
        }
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetCursorLock(false);
        }

        // Handle look (mouse delta) when cursor locked
        if (_cursorLocked && Mouse.current != null)
        {
            Vector2 md = Mouse.current.delta.ReadValue();
            float yaw = md.x * lookSensitivity;
            float pitchDelta = -md.y * lookSensitivity;

            // apply yaw to player
            transform.Rotate(0f, yaw, 0f, Space.Self);

            // FIX: Forcefully keep the player upright (X and Z rotation = 0)
            // This prevents the "diagonal" look caused by the player leaning/rolling
            Vector3 currentRot = transform.localEulerAngles;
            transform.localEulerAngles = new Vector3(0, currentRot.y, 0);

            // apply pitch to pivot
            if (cameraPitchPivot != null)
            {
                _pitch = Mathf.Clamp(_pitch + pitchDelta, pitchMin, pitchMax);
                // Ensure Z is 0 so the camera doesn't roll
                cameraPitchPivot.localEulerAngles = new Vector3(_pitch, 0f, 0f); 
            }
        }
    }

    private void FixedUpdate()
    {
        // Movement via Keyboard (Input System)
        Vector2 move = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) move.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) move.x += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) move.y -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) move.y += 1f;
        }

        // Build desired velocity in world space
        Vector3 desired = (transform.right * move.x + transform.forward * move.y) * moveSpeed;
        
        // Use linearVelocity for Unity 6+ / new Physics package
        // If using older Unity, change .linearVelocity to .velocity
        Vector3 v = _rb.linearVelocity;
        v.x = desired.x;
        v.z = desired.z;
        _rb.linearVelocity = v;

        // Jump
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryJump();
        }
    }

    private void TryJump()
    {
        if (groundCheck == null) return;
        bool grounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);
        if (!grounded) return;
        
        Vector3 v = _rb.linearVelocity;
        v.y = 0f;
        _rb.linearVelocity = v;
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    public void ApplyRecoil(Vector3 impulse)
    {
        if (_rb == null) return;
        _rb.AddForce(impulse * recoilMultiplier, ForceMode.Impulse);
    }

    private void SetCursorLock(bool locked)
    {
        _cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}