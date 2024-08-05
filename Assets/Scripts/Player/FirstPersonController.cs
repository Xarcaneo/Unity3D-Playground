using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the first-person player character.
/// </summary>
public class FirstPersonController : MonoBehaviour
{
    [Header("Mouse Aim")]
    [SerializeField, Tooltip("The mouse aim sensitivity.")]
    private float m_MouseAimMultiplier = 5f;

    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 4.0f;
    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 6.0f;
    [Tooltip("Rotation speed of the character")]
    public float RotationSpeed = 1.0f;
    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.1f;
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.5f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Tooltip("Inventory.")]
    [SerializeField]
    private Inventory inventory;

    [Header("Animation Procedural")]
    [Tooltip("Character Animator.")]
    [SerializeField] private Animator characterAnimator;

    [Header("Animation")]
    [Tooltip("Determines how smooth the locomotion blendspace is.")]
    [SerializeField]
    private float dampTimeLocomotion = 0.15f;

    /// <summary>
    /// Overlay Layer Index. Useful for playing things like firing animations.
    /// </summary>
    private int layerOverlay;
    /// <summary>
    /// Holster Layer Index. Used to play holster animations.
    /// </summary>
    private int layerHolster;
    /// <summary>
    /// Actions Layer Index. Used to play actions like reloading.
    /// </summary>
    private int layerActions;

    private LayerMask m_LayerMask;  
    private MouseAimController m_Aimer = null;

    // Player
    private float _speed; // Current speed of the player.
    private float _rotationVelocity; // Current rotation velocity of the player.
    private float _verticalVelocity; // Current vertical velocity of the player.
    private float _terminalVelocity = 53.0f; // Terminal velocity of the player.

    // Timeout deltatime
    private float _jumpTimeoutDelta; // Time remaining before the player can jump again.
    private float _fallTimeoutDelta; // Time remaining before the player enters the fall state.

    private PlayerInput _playerInput; // Reference to the PlayerInput component.
    private CharacterController _controller; // Reference to the CharacterController component.
    private AssetsInputs _input; // Reference to the AssetsInputs component.

    #region CONSTANTS

    /// <summary>
    /// Aiming Alpha Value.
    /// </summary>
    private static readonly int HashAimingAlpha = Animator.StringToHash("Aiming");

    /// <summary>
    /// Hashed "Movement".
    /// </summary>
    private static readonly int HashMovement = Animator.StringToHash("Movement");

    #endregion

    #region FIELDS

    /// <summary>
    /// True if the character is sprinting.
    /// </summary>
    private bool sprinting;

    /// <summary>
    /// The currently equipped weapon.
    /// </summary>
    private Weapon equippedWeapon;

    #endregion

    /// <summary>
    /// Last Time.time at which we shot.
    /// </summary>
    private float lastShotTime;

    /// <summary>
    /// Gets whether the current input device is a mouse.
    /// </summary>
    private bool IsCurrentDeviceMouse
    {
        get
        {
            return _playerInput.currentControlScheme == "KeyboardMouse";
        }
    }

    /// <summary>
    /// Initializes references to main camera and input components.
    /// </summary>
    private void Awake()
    {
        m_Aimer = GetComponent<MouseAimController>();

        //Initialize Inventory.
        inventory.Initialize();

        //Load weapon
        WeaponSetup();
    }

    /// <summary>
    /// Initializes components and resets timeouts on start.
    /// </summary>
    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<AssetsInputs>();
        _playerInput = GetComponent<PlayerInput>();

        // Reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;

        //Cache a reference to the holster layer's index.
        layerHolster = characterAnimator.GetLayerIndex("Layer Holster");
        //Cache a reference to the action layer's index.
        layerActions = characterAnimator.GetLayerIndex("Layer Actions");
        //Cache a reference to the overlay layer's index.
        layerOverlay = characterAnimator.GetLayerIndex("Layer Overlay");
    }

    /// <summary>
    /// Updates player movement, grounded state, and jump and gravity logic.
    /// </summary>
    private void Update()
    {
        JumpAndGravity();
        GroundedCheck();
        Move();

        if(_input.fire)
        {
            //Has fire rate passed.
            if (Time.time - lastShotTime > 60.0f / equippedWeapon.GetFireRate())
            {
                Fire();
            }
        }

        UpdateAnimator();
    }

    /// <summary>
    /// Updates the camera rotation based on player input.
    /// </summary>
    private void LateUpdate()
    {
        CameraRotation();
    }

    /// <summary>
    /// Checks if the player is grounded.
    /// </summary>
    private void GroundedCheck()
    {
        // Set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }

    /// <summary>
    /// Handles camera rotation based on player input.
    /// </summary>
    private void CameraRotation()
    {
        m_Aimer.HandleMouseInput(new Vector2(
            Input.GetAxis("Mouse X") * m_MouseAimMultiplier,
            Input.GetAxis("Mouse Y") * m_MouseAimMultiplier
        ));
    }

    /// <summary>
    /// Handles player movement based on input.
    /// </summary>
    private void Move()
    {
        //Match Sprint.
        sprinting = _input.sprint && CanSprint();

        // Set target speed based on move speed, sprint speed, and if sprint is pressed
        float targetSpeed = sprinting ? SprintSpeed : MoveSpeed;

        // If there is no input, set the target speed to 0
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        // A reference to the player's current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        // Accelerate or decelerate to the target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // Smooth acceleration/deceleration
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f; // Round speed to 3 decimal places
        }
        else
        {
            _speed = targetSpeed;
        }

        // Calculate input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        // Rotate player to face the direction of movement relative to the camera
        if (_input.move != Vector2.zero)
        {
            // Get the forward direction of the camera
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0; // Keep the rotation on the horizontal plane
            cameraForward.Normalize();

            // Calculate the target direction relative to the camera
            Vector3 moveDirection = cameraForward * _input.move.y + Camera.main.transform.right * _input.move.x;

            // Use the calculated movement direction
            inputDirection = moveDirection.normalized;
        }
        
        // Move the player
        _controller.Move(inputDirection * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }

    private void Fire()
    {
        //Save the shot time, so we can calculate the fire rate correctly.
        lastShotTime = Time.time;
        //Play firing animation.
        const string stateName = "Fire";
        characterAnimator.CrossFade(stateName, 0.05f, layerOverlay, 0);
    }


    /// <summary>
    /// Handles jumping and gravity.
    /// </summary>
    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // Reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // Stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // The square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }

            // Jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // Reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // Fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }

            // If we are not grounded, do not jump
            _input.jump = false;

            // Check for ceiling collision
            if ((_controller.collisionFlags & CollisionFlags.Above) != 0)
            {
                // Stop upward movement if hitting a ceiling
                _verticalVelocity = -2f; // Ensure downward force is applied immediately after hitting the ceiling
            }
        }

        // Apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    /// <summary>
    /// Updates all the animator properties for this frame.
    /// </summary>
    private void UpdateAnimator()
    {
        //Movement Value. This value affects absolute movement. Aiming movement uses this, as opposed to per-axis movement.
        characterAnimator.SetFloat(HashMovement, Mathf.Clamp01(Mathf.Abs(_input.move.x) + Mathf.Abs(_input.move.y)), dampTimeLocomotion, Time.deltaTime);

        //Update Animator Running.
        const string boolNameRun = "Running";
        characterAnimator.SetBool(boolNameRun, sprinting);
    }

    /// <summary>
    /// Returns true if the character can run.
    /// </summary>
    /// <returns></returns>
    private bool CanSprint()
    {
        //While we attempt fire, we dont allow to sprint
        if(_input.fire) //Later also add check for ammunition quantity, if 0 we dont return false 
            return false;

        return true;
    }

    /// <summary>
    /// Setup weapon animations and all weapon elements.
    /// </summary>
    private void WeaponSetup()
    {
        //Assign weapon to variable and check if its not NULL.
        if ((equippedWeapon = inventory.GetEquippedWeapon()) == null)
            return;
    }
}