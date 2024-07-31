using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the first-person player character.
/// </summary>
public class FirstPersonController : MonoBehaviour
{
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

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 90.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -90.0f;

    // Cinemachine
    private float _cinemachineTargetPitch; // Current pitch of the Cinemachine camera.

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
    private GameObject _mainCamera; // Reference to the main camera.

    private const float _threshold = 0.01f; // Small threshold value for input checks.

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
        // Get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
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
    }

    /// <summary>
    /// Updates player movement, grounded state, and jump and gravity logic.
    /// </summary>
    private void Update()
    {
        JumpAndGravity();
        GroundedCheck();
        Move();
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
        // If there is an input
        if (_input.look.sqrMagnitude >= _threshold)
        {
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            // Adjust the pitch and yaw based on the input
            _cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
            _rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

            // Clamp the pitch rotation to avoid excessive rotation
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Apply the rotation to the camera target
            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
            // Rotate the player based on the input
            transform.Rotate(Vector3.up * _rotationVelocity);
        }
    }

    /// <summary>
    /// Handles player movement based on input.
    /// </summary>
    private void Move()
    {
        // Set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

        // A simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // Note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // If there is no input, set the target speed to 0
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        // A reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        // Accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // Creates curved result rather than a linear one giving a more organic speed change
            // Note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

            // Round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        // Normalize input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        // Note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // If there is a move input rotate player when the player is moving
        if (_input.move != Vector2.zero)
        {
            // Move
            inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
        }

        // Move the player
        _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
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
    /// Clamps the given angle between the specified minimum and maximum values.
    /// This ensures the angle remains within a 360-degree range and is clamped to the specified limits.
    /// </summary>
    /// <param name="lfAngle">The angle to clamp.</param>
    /// <param name="lfMin">The minimum value.</param>
    /// <param name="lfMax">The maximum value.</param>
    /// <returns>The clamped angle.</returns>
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        // Ensure the angle is within a 360-degree range.
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;

        // Clamp the angle to the specified minimum and maximum values.
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}