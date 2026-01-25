/*Copyright © Spoiled Unknown*/
/*2024*/
using System.Collections;
using UnityEngine;
using XtremeFPS.InputHandling;
using Cinemachine;
using UnityEngine.UI;
using XtremeFPS.WeaponSystem.Pickup;
using XtremeFPS.Interfaces;

namespace CluckStorm.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(FPSInputManager))]
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("CluckStorm/Chicken Controller")]
    public class ChickenController : MonoBehaviour
    {
        #region Variables
        // Player
        public float transitionSpeed;
        public float walkSpeed = 2f; // Intentionally slow
        public float walkSoundSpeed;

        public Rigidbody Rigidbody { get; private set; }
        private CapsuleCollider capsuleCollider;
        private FPSInputManager inputManager;
        public PlayerMovementState MovementState { get; private set; }
        public enum PlayerMovementState
        {
            Sprinting,
            Crouching,
            Walking,
            Sliding,
            Default
        }
        public float targetSpeed;
        private float transitionDelta;

        //sprinting
        public bool canPlayerSprint;
        public bool unlimitedSprinting;
        public bool isSprintHold;
        public float sprintSpeed = 4f; // Intentionally slow
        public float sprintDuration = 8f;
        public float sprintCooldown = 8f;
        public Slider staminaBar;
        public float sprintSoundSpeed;

        private bool isSprinting;
        private readonly float sprintCooldownReset;
        private float sprintRemaining;


        // Gravity and Jumping
        public bool canJump;
        public float jumpHeight = 1f;
        public bool IsGrounded { get; private set; }
        public float groundCheckDistance = 0.2f;
        public LayerMask groundLayer;


        // Crouching
        public bool canPlayerCrouch;
        public bool isCrouchHold;
        public float crouchedHeight = 1f;
        public float crouchedSpeed = 1f;
        public float crouchSoundPlayTime;

        private bool isCrouching;
        private float newHeight;
        private float initialHeight;
        private Vector3 initialCameraPosition;

        //Sliding
        public float slidingSpeed;
        public float slidingDuration;

        private bool canSlide;
        private float slidingTime;
        private bool isOnSlope;
        private readonly float slopeCheckInterval = 0.2f;
        private float nextSlopeCheckTime;
        private RaycastHit slopeHit;

        // Camera
        public bool isCursorLocked;
        public Transform cameraFollow;
        public CinemachineVirtualCamera playerVirtualCamera;
        public float mouseSensitivity;
        public float maximumClamp;
        public float minimumClamp;
        public float sprintFOV;
        public float FOV;

        private float rotationY;
        float mouseDirectionX;
        float mouseDirectionY;

        //Zooming
        public bool enableZoom;
        public bool isZoomingHold;
        public float zoomFOV = 30f;

        private bool isZoomed;


        //Head Bobbing effect
        public bool canHeadBob;
        public float headBobAmplitude = 0.01f;
        public float headBobFrequency = 18.5f;

        private Vector3 headBobStartPosition;

        //Sound System
        public string SurfaceType { get; private set; }
        public string grassTag;
        public AudioClip[] soundGrass;

        public string waterTag;
        public AudioClip[] soundWater;

        public string metalTag;
        public AudioClip[] soundMetal;

        public string concreteTag;
        public AudioClip[] soundConcrete;

        public string gravelTag;
        public AudioClip[] soundGravel;

        public string woodTag;
        public AudioClip[] soundWood;

        public AudioClip landingAudioClip;
        public AudioClip jumpingAudioClip;
        public AudioClip slidingAudioClip;
        public float footstepSensitivity;

        private AudioSource audioSource;
        private float AudioEffectSpeed;
        private bool isMoving = false;


        // Handling Physics
        public bool canPush;
        public float pushStrength = 1.1f;

        private float hRecoil = 0f;
        private float vRecoil = 0f;

        //Interactions
        public int interactionLayersID;
        public float interactionRange;

        private IPickup closestPickup = null;
        #endregion

        #region MonoBehaviour Callbacks
        private void Start()
        {
            inputManager = FPSInputManager.Instance;
            audioSource = GetComponent<AudioSource>();
            Rigidbody = GetComponent<Rigidbody>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            
            Rigidbody.freezeRotation = true;

            playerVirtualCamera.m_Lens.FieldOfView = FOV;
            AudioEffectSpeed = walkSoundSpeed;
            headBobStartPosition = cameraFollow.localPosition;

            Cursor.lockState = isCursorLocked ? CursorLockMode.Locked : CursorLockMode.None;

            StartCoroutine(PlayFootstepSounds());

            if (!canPlayerCrouch) return;
            initialHeight = capsuleCollider.height;
            initialCameraPosition = cameraFollow.transform.localPosition;
        }

        private void Update()
        {
            transitionDelta = Time.deltaTime * transitionSpeed;

            Vector3 localVelocity = transform.InverseTransformDirection(Rigidbody.linearVelocity);
            isMoving = Mathf.Abs(localVelocity.z) > footstepSensitivity || Mathf.Abs(localVelocity.x) > footstepSensitivity;

            PlayerInputs();
            HandleZoom();
            HandleSprintCooldown();
            HandleStateMachine();
            DetectSurfaceAndMovement();
            InteractionHandling();
            if (MovementState == PlayerMovementState.Sliding) HanldeSliding();

            if (!canHeadBob || MovementState == PlayerMovementState.Sliding) return;
            CheckMotion();
            ResetPosition();
            cameraFollow.LookAt(FocusTarget());
        }

        private void FixedUpdate()
        {
            GroundCheck();
            
            Vector3 horizontalMovement = (inputManager.moveDirection.x * transform.right + inputManager.moveDirection.y * transform.forward).normalized * targetSpeed;
            Rigidbody.linearVelocity = new Vector3(horizontalMovement.x, Rigidbody.linearVelocity.y, horizontalMovement.z);
            
            HandleJumping();
        }

        private void LateUpdate()
        {
            rotationY -= mouseDirectionY;
            rotationY = Mathf.Clamp(rotationY, minimumClamp, maximumClamp);

            transform.Rotate(mouseDirectionX * transform.up);
            cameraFollow.localRotation = Quaternion.Euler(rotationY, 0f, 0f);
            inputManager.mouseDirection = Vector2.zero;
        }
        
        #endregion

        #region Private Methods

        private void GroundCheck()
        {
            bool wasPreviouslyGrounded = IsGrounded;
            IsGrounded = Physics.CheckSphere(transform.position - new Vector3(0, capsuleCollider.height / 2, 0), groundCheckDistance, groundLayer, QueryTriggerInteraction.Ignore);
            if (!wasPreviouslyGrounded && IsGrounded)
            {
                audioSource.PlayOneShot(landingAudioClip);
            }
        }

        private void HandleJumping()
        {
             if (IsGrounded && inputManager.haveJumped && canJump && MovementState != PlayerMovementState.Crouching && MovementState != PlayerMovementState.Sliding)
            {
                Rigidbody.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y), ForceMode.VelocityChange);
                audioSource.PlayOneShot(jumpingAudioClip);
            }
        }

        public void AddRecoilForce(Vector3 force)
        {
            Rigidbody.AddForce(force, ForceMode.Impulse);
        }

        private void PlayerInputs()
        {
            mouseDirectionX = inputManager.mouseDirection.x * mouseSensitivity * Time.deltaTime + hRecoil;
            mouseDirectionY = inputManager.mouseDirection.y * mouseSensitivity * Time.deltaTime + vRecoil;

            if (isSprintHold) isSprinting = inputManager.isSprintingHold && !(isZoomed && enableZoom);
            else isSprinting = inputManager.isSprintingTapped && !(isZoomed && enableZoom);

            if (isCrouchHold) isCrouching = inputManager.isCrouchingHold;
            else isCrouching = inputManager.isCrouchingTapped;

            if (isZoomingHold) isZoomed = inputManager.isZoomingHold;
            else isZoomed = inputManager.isZoomingTapped;

            canSlide = isCrouching && isSprinting && canPlayerCrouch;
        }
        #region Camera
        public void AddRecoil(float hRecoil, float vRecoil)
        {
            this.hRecoil = hRecoil;
            this.vRecoil = vRecoil;
        }

        private void HandleZoom()
        {
            if (!enableZoom) return;

            if (isZoomed) 
                playerVirtualCamera.m_Lens.FieldOfView = Mathf.Lerp(playerVirtualCamera.m_Lens.FieldOfView, zoomFOV, transitionDelta);
            else if (!isZoomed && !isSprinting) 
                playerVirtualCamera.m_Lens.FieldOfView = Mathf.Lerp(playerVirtualCamera.m_Lens.FieldOfView, FOV, transitionDelta);
        }

        private void AdjustFOVSettings(float targetFOV)
        {
            if (isZoomed && enableZoom) return;
            if (!isMoving) targetFOV = FOV;

            float currentFOV = playerVirtualCamera.m_Lens.FieldOfView;
            float newFOV = Mathf.Lerp(currentFOV, targetFOV, transitionDelta);
            playerVirtualCamera.m_Lens.FieldOfView = newFOV;
        }
        #region Head Bobbing
        private Vector3 FootStepMotion()
        {
            Vector3 pos = Vector3.zero;
            pos.y += Mathf.Sin(Time.time * headBobFrequency) * headBobAmplitude;
            pos.x += Mathf.Cos(Time.time * headBobFrequency / 2) * headBobAmplitude * 2;
            return pos;
        }

        private void CheckMotion()
        {
            if (!isMoving || !IsGrounded)
            {
                return;
            }
            PlayMotion(FootStepMotion());
        }

        private void PlayMotion(Vector3 motion)
        {
            cameraFollow.localPosition += motion;
        }

        private Vector3 FocusTarget()
        {
            Vector3 pos = new Vector3(transform.position.x, transform.position.y + cameraFollow.localPosition.y, transform.position.z);
            pos += cameraFollow.forward * 15.0f;
            return pos;
        }

        private void ResetPosition()
        {
            if (cameraFollow.localPosition != headBobStartPosition)
            {
                cameraFollow.localPosition = Vector3.Lerp(cameraFollow.localPosition, headBobStartPosition, 1f * Time.deltaTime);
            }
        }
        #endregion
        #endregion

        private void HandleSprintCooldown()
        {
            if (unlimitedSprinting) return;

            if (MovementState == PlayerMovementState.Sprinting &&
                Rigidbody.linearVelocity.magnitude > 0)
            {
                sprintRemaining -= 1 * Time.deltaTime;
                if (sprintRemaining <= 0)
                {
                    inputManager.isSprintingTapped = false;
                    inputManager.isSprintingHold = false;
                    sprintCooldown -= 1 * Time.deltaTime;
                }
                else sprintCooldown = sprintCooldownReset;
            }
            else sprintRemaining = Mathf.Clamp(sprintRemaining += 1 * Time.deltaTime, 0, sprintDuration);

            if (staminaBar != null)
            {
                float sprintRemainingPercent = sprintRemaining / sprintDuration;
                staminaBar.value = sprintRemainingPercent;
            }
        }

        private void AdjustCrouchHeight(float targetHeight, bool isTryingToUncrouch)
        {
            if (isTryingToUncrouch)
            {
                Vector3 castOrigin = transform.position + new Vector3(0f, newHeight / 2, 0f);
                if (Physics.Raycast(castOrigin, Vector3.up, out RaycastHit hit, 0.2f))
                {
                    float distanceToCeiling = hit.point.y - castOrigin.y;
                    targetHeight = Mathf.Max(newHeight + distanceToCeiling - 0.1f, crouchedHeight);
                }
            }

            newHeight = Mathf.Lerp(capsuleCollider.height, targetHeight, transitionDelta);
            capsuleCollider.height = newHeight;
            capsuleCollider.center = new Vector3(0, newHeight/2, 0);


            // Adjust the camera position based on the new height
            Vector3 halfHeightDifference = new Vector3(0, (initialHeight - newHeight) / 2, 0);
            Vector3 newCameraHeight = initialCameraPosition - halfHeightDifference;
            cameraFollow.localPosition = newCameraHeight;
        }

        #region Sliding
        private void HanldeSliding()
        {
            if (Time.time >= nextSlopeCheckTime)
            {
                nextSlopeCheckTime = Time.time + slopeCheckInterval;
                isOnSlope = CheckIfOnSlope();
            }
            if (!isOnSlope && IsGrounded) slidingTime -= Time.deltaTime;
            if (slidingTime <= 0)
            {
                inputManager.isSprintingHold = false;
                inputManager.isSprintingTapped = false;
                MovementState = PlayerMovementState.Crouching;
            }
        }

        private bool CheckIfOnSlope()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, capsuleCollider.height * 0.5f + 0.3f))
            {
                float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                if (angle > Rigidbody.GetComponent<Collider>().material.staticFriction || angle == 0) return false;
                Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized;
                Vector3 movementDirection = new Vector3(Rigidbody.linearVelocity.x, 0, Rigidbody.linearVelocity.z).normalized;
                float dotProduct = Vector3.Dot(movementDirection, slopeDirection);
                return dotProduct > 0;
            }
            return false;
        }
        #endregion
        private void HandleStateMachine()
        {
            if (canSlide && (targetSpeed > (sprintSpeed * 0.5f + 1.0f)) && MovementState != PlayerMovementState.Sliding)
            {
                slidingTime = slidingDuration;
                MovementState = PlayerMovementState.Sliding;
            }
            else if (canPlayerSprint && isSprinting && !isCrouching) MovementState = PlayerMovementState.Sprinting;
            else if (canPlayerCrouch && isCrouching && !isSprinting) MovementState = PlayerMovementState.Crouching;
            else if (!isSprinting && !isCrouching) MovementState = PlayerMovementState.Walking;

            SwitchMoveState(MovementState);
        }

        private void SwitchMoveState(PlayerMovementState movementState)
        {
            switch (movementState)
            {
                case PlayerMovementState.Sprinting:
                    targetSpeed = Mathf.Lerp(targetSpeed, sprintSpeed, transitionDelta);
                    AudioEffectSpeed = sprintSoundSpeed;
                    AdjustCrouchHeight(initialHeight, true);
                    AdjustFOVSettings(sprintFOV);
                    break;

                case PlayerMovementState.Crouching:
                    targetSpeed = Mathf.Lerp(targetSpeed, crouchedSpeed, transitionDelta);
                    AudioEffectSpeed = crouchSoundPlayTime;
                    AdjustCrouchHeight(crouchedHeight, false);
                    break;

                case PlayerMovementState.Walking:
                    targetSpeed = Mathf.Lerp(targetSpeed, walkSpeed, transitionDelta);
                    AudioEffectSpeed = walkSoundSpeed;
                    AdjustFOVSettings(FOV);
                    AdjustCrouchHeight(initialHeight, true);
                    break;

                case PlayerMovementState.Sliding:
                    targetSpeed = Mathf.Lerp(targetSpeed, slidingSpeed, transitionDelta);
                    AdjustCrouchHeight(crouchedHeight, false);
                    if (!audioSource.isPlaying && IsGrounded) audioSource.PlayOneShot(slidingAudioClip);
                    else if (!IsGrounded) audioSource.Stop();
                    canSlide = false;
                    break;

                case PlayerMovementState.Default:
                    break;
            }
        }

        #region Sound Management
        private void DetectSurfaceAndMovement()
        {
            if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 5f)) return;
            SurfaceType = hit.collider.tag.ToLower() switch
            {
                "grass" => "grass",
                "metals" => "metal",
                "gravel" => "gravel",
                "water" => "water",
                "concrete" => "concrete",
                "wood" => "wood",
                _ => "Unknown",
            };
        }

        private IEnumerator PlayFootstepSounds()
        {
            while (true)
            {
                if (!IsGrounded || !isMoving || MovementState == PlayerMovementState.Sliding)
                {
                    yield return null;
                    continue;
                }

                switch (SurfaceType)
                {
                    case "grass":
                        audioSource.clip = soundGrass[Random.Range(0, soundGrass.Length)];
                        break;
                    case "gravel":
                        audioSource.clip = soundGravel[Random.Range(0, soundGravel.Length)];
                        break;
                    case "water":
                        audioSource.clip = soundWater[Random.Range(0, soundWater.Length)];
                        break;
                    case "metal":
                        audioSource.clip = soundMetal[Random.Range(0, soundMetal.Length)];
                        break;
                    case "concrete":
                        audioSource.clip = soundConcrete[Random.Range(0, soundConcrete.Length)];
                        break;
                    case "wood":
                        audioSource.clip = soundWood[Random.Range(0, soundWood.Length)];
                        break;
                    default:
                        yield return null;
                        break;
                }

                if (audioSource.clip != null)
                {
                    audioSource.PlayOneShot(audioSource.clip);
                    yield return new WaitForSeconds(AudioEffectSpeed);
                }
                else yield return null;
            }
        }
        #endregion

        private void InteractionHandling()
        {
            if (!inputManager.isTryingToInteract) return;
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange, (1 << interactionLayersID));

            foreach (Collider collider in colliders)
            {
                if (collider.TryGetComponent(out IPickup pickup) && !isZoomed)
                {
                    closestPickup ??= pickup;
                    if (Vector3.Distance(transform.position, collider.transform.position) <
                        Vector3.Distance(transform.position, closestPickup.GetTransform().position)) closestPickup = pickup;
                    if (!closestPickup.IsEquiped() && !WeaponPickup.IsWeaponEquipped) closestPickup.PickUp();
                    else closestPickup.Drop();
                    break;
                }
            }
        }
        
        void OnCollisionEnter(Collision collision)
        {
            if (!canPush) return;

            Rigidbody body = collision.collider.attachedRigidbody;
            if (body == null || body.isKinematic) return;

            Vector3 pushDir = new Vector3(collision.contacts[0].normal.x, 0, collision.contacts[0].normal.z);
            body.linearVelocity = pushDir * pushStrength;
        }


        #endregion
    }
}
