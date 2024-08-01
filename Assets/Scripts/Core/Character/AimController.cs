using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

/// <summary>
/// Abstract class for controlling the aim of a character.
/// Manages yaw (horizontal) and pitch (vertical) rotations.
/// </summary>
public abstract class AimController : MonoBehaviour, IAimController
{
    // Serialized fields for configuring the aim behavior
    [SerializeField, Tooltip("The transform to yaw when aiming. This should be a parent of the pitch transform.")]
    private Transform m_YawTransform = null;

    [SerializeField, Tooltip("This optional transform detaches the character direction from the aim direction.")]
    private Transform m_AimYawTransform = null;

    [SerializeField, Range(0f, 1f), Tooltip("The time taken to turn the character to the aim-yaw direction (if Aim Yaw Transform is set). 0 = call LerpYawToAim() manually, 1 = instant.")]
    private float m_SteeringRate = 0.5f;

    [SerializeField, Tooltip("The transform to pitch when aiming. This should be a child of the yaw transform.")]
    private Transform m_PitchTransform = null;

    [Header("Constraints")]

    [SerializeField, Range(0f, 89f), Tooltip("The maximum pitch angle (in degrees) from horizontal the aimer can rotate.")]
    private float m_MaxPitch = 89f;

    [SerializeField, Range(0f, 1f), Tooltip("The amount of damping applied when rotating the camera to match constraints.")]
    private float m_ConstraintsDamping = 0.5f;

    [SerializeField, Tooltip("Once the angle outside constraints goes below this value, the camera will snap to the constraints. Larger values will have a visible effect.")]
    private float m_ConstraintsTolerance = 0.25f;

    [SerializeField, Tooltip("An angle range from the yaw constraint limits where the input falls off. This gives the effect of softer constraint limits instead of hitting an invisible wall.")]
    private float m_YawConstraintsFalloff = 10f;

    // Constants to define the constraint match multiplier range
    private const float k_MaxConstraintsMatchMult = 20f;
    private const float k_MinConstraintsMatchMult = 1f;

    // Internal state variables
    private bool m_DisconnectAimFromYaw = false;
    private float m_ConstraintsMatchMult = 0f;
    private bool m_YawConstrained = false;
    private Vector3 m_YawConstraint = Vector3.zero;
    private float m_YawLimit = 0f;
    private bool m_HeadingConstrained = false;
    private Vector3 m_HeadingConstraint = Vector3.zero;
    private float m_HeadingLimit = 0f;
    private Quaternion m_YawLocalRotation = Quaternion.identity;
    private float m_PendingYaw = 0f;
    private float m_CurrentPitch = 0f;
    private float m_PitchLimitMin = -89f;
    private float m_PitchLimitMax = 89f;
    private float m_PendingPitch = 0f;

    // Property to check if the setup is valid
    protected bool isValid
    {
        get;
        private set;
    }

    // Public properties for external access to certain values
    public Quaternion rotation
    {
        get { return m_PitchTransform.rotation; }
        set { m_PitchTransform.rotation = value; }
    }

    public Vector3 aimHeading
    {
        get { return m_AimYawTransform.forward; }
    }

    public Vector3 heading
    {
        get { return m_YawTransform.forward; }
    }

    public Vector3 forward
    {
        get { return m_PitchTransform.forward; }
    }

    public Vector3 yawUp
    {
        get { return m_YawTransform.up; }
    }

    public float pitch
    {
        get
        {
            return -(Mathf.Repeat(m_PitchTransform.localRotation.eulerAngles.x + 180f, 360f) - 180f);
        }
    }

    public float aimYawDiff
    {
        get
        {
            if (m_DisconnectAimFromYaw)
                return m_YawLocalRotation.eulerAngles.y;
            else
                return 0f;
        }
    }

    public float constraintsSmoothing
    {
        get { return m_ConstraintsDamping; }
        set
        {
            m_ConstraintsDamping = Mathf.Clamp01(value);
            CalculateSmoothingMultiplier();
        }
    }

    public Quaternion yawLocalRotation
    {
        get
        {
            if (m_DisconnectAimFromYaw)
                return m_YawTransform.localRotation;
            else
                return m_YawLocalRotation;
        }
    }
    public Quaternion pitchLocalRotation
    {
        get;
        private set;
    }

    private float currentPitch
    {
        get { return m_CurrentPitch; }
        set
        {
            m_CurrentPitch = value;
            if (m_CurrentPitch > 180f)
                m_CurrentPitch -= 360f;
        }
    }

    public float steeringRate
    {
        get { return m_SteeringRate; }
        set { m_SteeringRate = Mathf.Clamp01(value); }
    }

    private float m_TurnRateMultiplier = 1f;
    public float turnRateMultiplier
    {
        get { return m_TurnRateMultiplier; }
        set { m_TurnRateMultiplier = Mathf.Clamp01(value); }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Validates the transform hierarchy and ensures proper setup.
    /// Called when the script is loaded or a value changes in the Inspector.
    /// </summary>
    protected virtual void OnValidate()
    {
        // Clamp values to valid ranges
        m_MaxPitch = Mathf.Clamp(m_MaxPitch, 0f, 90f);
        m_ConstraintsTolerance = Mathf.Clamp(m_ConstraintsTolerance, 0f, 90f);
        m_YawConstraintsFalloff = Mathf.Clamp(m_YawConstraintsFalloff, 0f, 45f);
        CalculateSmoothingMultiplier();

        // Validate transform hierarchy
        if (m_AimYawTransform == null)
        {
            if (m_YawTransform != null && m_PitchTransform != null && !IsChildOf(m_PitchTransform, m_YawTransform))
            {
                m_PitchTransform = null;
                Debug.LogError("Pitch transform must be a child of the yaw transform.");
            }
        }
        else
        {
            if (m_YawTransform != null && m_AimYawTransform != null && !IsChildOf(m_AimYawTransform, m_YawTransform))
            {
                Debug.LogError("Aim-yaw transform transform must be a child of the yaw transform.");
            }
            if (m_AimYawTransform != null && m_PitchTransform != null && !IsChildOf(m_PitchTransform, m_AimYawTransform))
            {
                m_PitchTransform = null;
                Debug.LogError("Pitch transform transform must be a child of the aim-yaw transform.");
            }
        }
    }

    /// <summary>
    /// Helper method to check if one transform is a child of another.
    /// </summary>
    /// <param name="c">The child transform.</param>
    /// <param name="p">The potential parent transform.</param>
    /// <returns>True if the child is a descendant of the parent, false otherwise.</returns>
    bool IsChildOf(Transform c, Transform p)
    {
        Transform t = c.parent;
        while (t != null)
        {
            if (t == p)
                return true;
            t = t.parent;
        }
        return false;
    }
#endif

    /// <summary>
    /// Calculates the smoothing multiplier based on the constraints damping.
    /// </summary>
    void CalculateSmoothingMultiplier()
    {
        float lerp = 1f - m_ConstraintsDamping;
        lerp *= lerp;
        m_ConstraintsMatchMult = Mathf.Lerp(k_MinConstraintsMatchMult, k_MaxConstraintsMatchMult, lerp);
    }

    /// <summary>
    /// Initializes the controller and validates the setup.
    /// </summary>
    protected virtual void Awake()
    {
        ResetYawConstraints();
        ResetPitchConstraints();

        // Validate the transform references
        if (m_YawTransform == null || m_PitchTransform == null)
        {
            isValid = false;
#if UNITY_EDITOR
            Debug.LogError("AimController has invalid yaw and pitch transforms. Pitch transform should be a child of the yaw transform.");
#endif
            m_YawLocalRotation = Quaternion.identity;
            pitchLocalRotation = Quaternion.identity;
        }
        else
        {
            m_YawLocalRotation = m_YawTransform.localRotation;
            pitchLocalRotation = m_PitchTransform.localRotation;
            isValid = true;
        }
    }

    /// <summary>
    /// Sets up the aim controller when the script is started.
    /// </summary>
    protected virtual void Start()
    {
        if (!isValid)
            return;

        // Determine if the aim direction is detached from the character direction
        if (m_AimYawTransform != null && m_AimYawTransform != m_YawTransform)
            m_DisconnectAimFromYaw = true;
        else
            m_AimYawTransform = m_YawTransform;

        // Initialize pitch rotation
        currentPitch = m_PitchTransform.localEulerAngles.x;
        CalculateSmoothingMultiplier();
    }

    /// <summary>
    /// Updates the aim controller every frame.
    /// </summary>
    protected virtual void Update()
    {
        if (!isValid || Time.deltaTime < 0.0001f)
            return;

        UpdateAimInput();
        UpdateYaw();
        UpdatePitch();

        // Handle steering if aim direction is detached
        if (m_DisconnectAimFromYaw)
        {
            if (m_SteeringRate < 0.0001f)
            {
                // Lerp of 0 means handle heading constraints only
                LerpYawToAim(0f);
            }
            else
            {
                if (m_SteeringRate >= 0.999f)
                {
                    // Lerp of 1 is instant, but uses heading constraints
                    LerpYawToAim(1f);
                }
                else
                {
                    // Lerp over time, using heading constraints
                    float lerp = Mathf.Lerp(0.0025f, 0.25f, m_SteeringRate);
                    LerpYawToAim(lerp);
                }
            }
        }
    }

    /// <summary>
    /// Updates the yaw (horizontal rotation) based on constraints and input.
    /// </summary>
    void UpdateYaw()
    {
        if (m_YawConstrained)
        {
            Vector3 target = Vector3.ProjectOnPlane(m_YawConstraint, m_AimYawTransform.up).normalized;
            if (target != Vector3.zero)
            {
                // Get the signed yaw angle from the constraint target
                float angle = Vector3.SignedAngle(target, m_AimYawTransform.forward, m_AimYawTransform.up);

                // Get the min and max turn
                float minTurn = -m_YawLimit - angle;
                float maxTurn = m_YawLimit - angle;

                // Check if outside bounds
                bool outsideBounds = false;
                if (minTurn > 0f)
                {
                    // Get damped rotation towards constraints
                    float y = minTurn;
                    if (minTurn > m_ConstraintsTolerance)
                        y *= Mathf.Clamp01(Time.deltaTime * m_ConstraintsMatchMult);

                    // Set pending yaw if above reaches constraints faster
                    if (m_PendingYaw < y)
                        m_PendingYaw = y;

                    // Prevent overshoot
                    if (m_PendingYaw > maxTurn)
                        m_PendingYaw = maxTurn;

                    outsideBounds = true;
                }
                if (maxTurn < 0f)
                {
                    // Get damped rotation towards constraints
                    float y = maxTurn;
                    if (maxTurn < -m_ConstraintsTolerance)
                        y *= Mathf.Clamp01(Time.deltaTime * m_ConstraintsMatchMult);

                    // Set pending yaw if above reaches constraints faster
                    if (m_PendingYaw > y)
                        m_PendingYaw = y;

                    // Prevent overshoot
                    if (m_PendingYaw < minTurn)
                        m_PendingYaw = minTurn;

                    outsideBounds = true;
                }

                if (!outsideBounds)
                {
                    // Apply falloff
                    if (m_YawConstraintsFalloff > 0.0001f)
                    {
                        if (m_PendingYaw >= 0f)
                            m_PendingYaw *= Mathf.Clamp01(maxTurn / m_YawConstraintsFalloff);
                        else
                            m_PendingYaw *= Mathf.Clamp01(-minTurn / m_YawConstraintsFalloff);
                    }

                    // Clamp the rotation
                    m_PendingYaw = Mathf.Clamp(m_PendingYaw, minTurn, maxTurn);
                }
            }
        }

        // Apply yaw rotation
        m_YawLocalRotation *= Quaternion.Euler(0f, m_PendingYaw, 0f);
        m_AimYawTransform.localRotation = m_YawLocalRotation;

        // Reset pending yaw
        m_PendingYaw = 0f;
    }

    /// <summary>
    /// Updates the pitch (vertical rotation) based on constraints and input.
    /// </summary>
    void UpdatePitch()
    {
        // Check if outside bounds already
        bool outsideBounds = false;
        if (currentPitch > m_PitchLimitMax)
        {
            // Get damped rotation towards constraints
            float p = (m_PitchLimitMax - currentPitch) * Mathf.Clamp01(Time.deltaTime * m_ConstraintsMatchMult);

            // Set pending pitch if above reaches constraints faster
            if (m_PendingPitch > p)
                m_PendingPitch = p;

            // Assign & prevent overshoot
            currentPitch += m_PendingPitch;
            if (currentPitch < m_PitchLimitMin)
                currentPitch = m_PitchLimitMin;

            outsideBounds = true;
        }
        if (currentPitch < m_PitchLimitMin)
        {
            // Get damped rotation towards constraints
            float p = (m_PitchLimitMin - currentPitch) * Mathf.Clamp01(Time.deltaTime * m_ConstraintsMatchMult);

            // Set pending pitch if above reaches constraints faster
            if (m_PendingPitch < p)
                m_PendingPitch = p;

            // Assign & prevent overshoot
            currentPitch += m_PendingPitch;
            if (currentPitch > m_PitchLimitMax)
                currentPitch = m_PitchLimitMax;

            outsideBounds = true;
        }

        // Clamp the rotation
        if (!outsideBounds)
            currentPitch = Mathf.Clamp(currentPitch + m_PendingPitch, m_PitchLimitMin, m_PitchLimitMax);

        // Apply the pitch
        pitchLocalRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        m_PitchTransform.localRotation = pitchLocalRotation;

        // Reset pending pitch
        m_PendingPitch = 0f;
    }

    /// <summary>
    /// Virtual method to be overridden for custom aim input handling.
    /// </summary>
    public virtual void UpdateAimInput()
    {
        // Override if using custom. Sample aimer uses events instead
    }

    /// <summary>
    /// Adds yaw (horizontal rotation) input.
    /// </summary>
    /// <param name="y">Yaw amount to add.</param>
    public void AddYaw(float y)
    {
        if (enabled)
            m_PendingYaw += y * m_TurnRateMultiplier;
    }

    /// <summary>
    /// Resets the yaw rotation locally.
    /// </summary>
    public void ResetYawLocal()
    {
        if (isValid)
        {
            if (!m_DisconnectAimFromYaw)
                m_YawLocalRotation = Quaternion.identity;
            m_YawTransform.localRotation = Quaternion.identity;
        }
    }

    /// <summary>
    /// Resets the yaw rotation locally with an offset.
    /// </summary>
    /// <param name="offset">The offset to apply to the yaw rotation.</param>
    public void ResetYawLocal(float offset)
    {
        if (isValid)
        {
            var rotation = Quaternion.Euler(0f, offset, 0f);
            if (!m_DisconnectAimFromYaw)
                m_YawLocalRotation = Quaternion.identity;
            m_YawTransform.localRotation = rotation;
        }
    }

    /// <summary>
    /// Smoothly interpolates yaw to aim direction.
    /// </summary>
    /// <param name="amount">The interpolation amount, ranging from 0 to 1.</param>
    void LerpYawToAim(float amount)
    {
        if (!m_DisconnectAimFromYaw)
            return;

        if (m_HeadingConstrained)
        {
            Vector3 target = Vector3.ProjectOnPlane(m_HeadingConstraint, m_YawTransform.up).normalized;
            if (target != Vector3.zero)
            {
                // Get the signed yaw angle from the constraint target
                float angle = Vector3.SignedAngle(target, m_YawTransform.forward, m_YawTransform.up);

                // Get the min and max turn
                float minTurn = -m_HeadingLimit - angle;
                float maxTurn = m_HeadingLimit - angle;

                // Get pending heading
                float pending = 0f;
                if (amount > 0f)
                {
                    pending = m_YawLocalRotation.eulerAngles.y;
                    pending = Mathf.Repeat(pending + 180f, 360f) - 180f;
                    pending = Mathf.Lerp(0f, pending, amount);
                }

                // Check if outside bounds
                bool outsideBounds = false;
                if (minTurn > 0f)
                {
                    // Get damped rotation towards constraints
                    float y = minTurn;
                    if (minTurn > m_ConstraintsTolerance)
                        y *= Mathf.Clamp01(Time.deltaTime * m_ConstraintsMatchMult);

                    // Set pending yaw if above reaches constraints faster
                    if (pending < y)
                        pending = y;

                    // Prevent overshoot
                    if (pending > maxTurn)
                        pending = maxTurn;

                    outsideBounds = true;
                }
                if (maxTurn < 0f)
                {
                    // Get damped rotation towards constraints
                    float y = maxTurn;
                    if (maxTurn < -m_ConstraintsTolerance)
                        y *= Mathf.Clamp01(Time.deltaTime * m_ConstraintsMatchMult);

                    // Set pending yaw if above reaches constraints faster
                    if (pending > y)
                        pending = y;

                    // Prevent overshoot
                    if (pending < minTurn)
                        pending = minTurn;

                    outsideBounds = true;
                }

                if (!outsideBounds)
                {
                    // Apply falloff
                    if (m_YawConstraintsFalloff > 0.0001f)
                    {
                        if (pending >= 0f)
                            pending *= Mathf.Clamp01(maxTurn / m_YawConstraintsFalloff);
                        else
                            pending *= Mathf.Clamp01(-minTurn / m_YawConstraintsFalloff);
                    }

                    // Clamp the rotation
                    pending = Mathf.Clamp(pending, minTurn, maxTurn);
                }

                // Apply heading rotation
                var rotation = Quaternion.Euler(0f, pending, 0f);
                m_YawTransform.localRotation *= rotation;
                m_YawLocalRotation *= Quaternion.Inverse(rotation);
                m_AimYawTransform.localRotation = m_YawLocalRotation;
            }
        }
        else
        {
            if (amount > 0f)
            {
                if (amount >= 1f)
                {
                    m_YawTransform.localRotation *= m_YawLocalRotation;
                    m_YawLocalRotation = Quaternion.identity;
                    m_AimYawTransform.localRotation = m_YawLocalRotation;
                }
                else
                {
                    var lerped = Quaternion.Lerp(Quaternion.identity, m_YawLocalRotation, amount);
                    m_YawTransform.localRotation *= lerped;

                    m_YawLocalRotation *= Quaternion.Inverse(lerped);
                    m_AimYawTransform.localRotation = m_YawLocalRotation;
                }
            }
        }
    }

    /// <summary>
    /// Adds pitch (vertical rotation) input.
    /// </summary>
    /// <param name="p">Pitch amount to add.</param>
    public void AddPitch(float p)
    {
        if (enabled)
            m_PendingPitch += p * m_TurnRateMultiplier;
    }

    /// <summary>
    /// Resets the pitch rotation locally.
    /// </summary>
    public void ResetPitchLocal()
    {
        if (isValid)
        {
            currentPitch = Mathf.Clamp(0f, m_PitchLimitMin, m_PitchLimitMax);
            pitchLocalRotation = Quaternion.Euler(-currentPitch, 0f, 0f);
            m_PitchTransform.localRotation = pitchLocalRotation;
        }
    }

    /// <summary>
    /// Adds rotation input for both yaw and pitch.
    /// </summary>
    /// <param name="y">Yaw amount to add.</param>
    /// <param name="p">Pitch amount to add.</param>
    public void AddRotation(float y, float p)
    {
        AddYaw(y);
        AddPitch(p);
    }

    /// <summary>
    /// Adds rotation input relative to a specified transform.
    /// </summary>
    /// <param name="input">The rotation input as a Vector2.</param>
    /// <param name="relativeTo">The transform relative to which the rotation is applied.</param>
    public void AddRotationInput(Vector2 input, Transform relativeTo)
    {
        if (enabled)
        {
            // Get the corrected rotation
            Quaternion inputRotation = relativeTo.localRotation * Quaternion.Euler(input.y, input.x, 0f);
            Vector3 euler = inputRotation.eulerAngles;

            // Get the modified pitch & yaw (wrapped)
            float modifiedYaw = euler.y;
            if (modifiedYaw > 180f)
                modifiedYaw -= 360f;
            float modifiedPitch = euler.x;
            if (modifiedPitch > 180f)
                modifiedPitch -= 360f;

            // Get the vertical amount of the aimer (tilt has less effect closer to the vertical)
            float vertical = Mathf.Abs(Vector3.Dot(m_PitchTransform.forward, m_YawTransform.up));

            // Lerp between modified rotation and standard as it gets closer to vertical
            AddYaw(Mathf.Lerp(modifiedYaw, input.x, vertical));
            AddPitch(Mathf.Lerp(modifiedPitch, input.y, vertical));
        }
    }

    /// <summary>
    /// Sets constraints on yaw (horizontal) rotation.
    /// </summary>
    /// <param name="center">The center of the yaw constraint.</param>
    /// <param name="range">The range of the yaw constraint.</param>
    public void SetYawConstraints(Vector3 center, float range)
    {
        if (range >= 360f)
        {
            ResetYawConstraints();
            return;
        }

        // Clamp the yaw limit
        m_YawLimit = Mathf.Clamp(range * 0.5f, 0f, 180f);

        m_YawConstraint = center;

        m_YawConstrained = true;
    }

    /// <summary>
    /// Sets constraints on heading direction.
    /// </summary>
    /// <param name="center">The center of the heading constraint.</param>
    /// <param name="range">The range of the heading constraint.</param>
    public void SetHeadingConstraints(Vector3 center, float range)
    {
        // Assert steering is enabled
        if (m_DisconnectAimFromYaw)
        {
            if (range >= 360f)
            {
                ResetHeadingConstraints();
                return;
            }

            // Clamp the heading limit
            m_HeadingLimit = Mathf.Clamp(range * 0.5f, 0f, 180f);

            m_HeadingConstraint = center;

            m_HeadingConstrained = true;
        }
        else
            Debug.LogError("Aim controller must have separate yaw and aim yaw transforms to constrain heading");
    }

    /// <summary>
    /// Sets constraints on pitch (vertical) rotation.
    /// </summary>
    /// <param name="min">The minimum pitch constraint.</param>
    /// <param name="max">The maximum pitch constraint.</param>
    public void SetPitchConstraints(float min, float max)
    {
        m_PitchLimitMin = Mathf.Clamp(-max, -m_MaxPitch, m_MaxPitch);
        m_PitchLimitMax = Mathf.Clamp(-min, -m_MaxPitch, m_MaxPitch);
    }

    /// <summary>
    /// Resets yaw (horizontal) constraints.
    /// </summary>
    public void ResetYawConstraints()
    {
        m_YawConstrained = false;
    }

    /// <summary>
    /// Resets heading direction constraints.
    /// </summary>
    public void ResetHeadingConstraints()
    {
        m_HeadingConstrained = false;
    }

    /// <summary>
    /// Resets pitch (vertical) constraints.
    /// </summary>
    public void ResetPitchConstraints()
    {
        m_PitchLimitMin = -m_MaxPitch;
        m_PitchLimitMax = m_MaxPitch;
    }
}
