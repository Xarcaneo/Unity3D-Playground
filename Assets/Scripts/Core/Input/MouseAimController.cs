using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the mouse-based aim functionality, handling input smoothing, acceleration, and rotation.
/// Inherits from the AimController class.
/// </summary>
public class MouseAimController : AimController
{
    [Header("Mouse Turn")]

    [SerializeField, Tooltip("Number of degrees for 1 unit of mouse movement if sensitivity is set to 0")]
    private float m_MouseTurnAngleMin = 0.25f;

    [SerializeField, Tooltip("Number of degrees for 1 unit of mouse movement if sensitivity is set to 1")]
    private float m_MouseTurnAngleMax = 5f;

    [SerializeField, Tooltip("The transform to calculate the input relative to. Use this to factor tilt into the yaw and pitch input")]
    public Transform m_RelativeTo = null;

    [Header("Mouse Smoothing")]

    [SerializeField, Delayed, Tooltip("The smoothing time used in damping the mouse input at minimum smoothing strength.")]
    private float m_MinSmoothingTime = 0.01f;
    [SerializeField, Delayed, Tooltip("The smoothing time used in damping the mouse input at maximum smoothing strength.")]
    private float m_MaxSmoothingTime = 0.075f;

    [Header("Mouse Acceleration")]

    [SerializeField, Tooltip("The base acceleration multiplier when acceleration is set to the minimum.")]
    private float m_MouseAccelSpeedMultiplyMin = 0.001f;

    [SerializeField, Tooltip("The base acceleration multiplier when acceleration is set to the maximum.")]
    private float m_MouseAccelSpeedMultiplyMax = 0.01f;

    [SerializeField, Tooltip("The maximum multiplier acceleration can apply to the mouse input (0 means no maximum)")]
    private float m_MouseAccelerationMax = 0f;

    /// <summary>
    /// Validates the input parameters and ensures they are within acceptable ranges.
    /// </summary>
    protected override void OnValidate()
    {
        base.OnValidate();

        // Clamp mouse turn angles to valid ranges
        m_MouseTurnAngleMin = Mathf.Clamp(m_MouseTurnAngleMin, 0.1f, 10f);
        m_MouseTurnAngleMax = Mathf.Clamp(m_MouseTurnAngleMax, 0.1f, 10f);

        // Clamp mouse acceleration multipliers
        m_MouseAccelSpeedMultiplyMin = Mathf.Clamp(m_MouseAccelSpeedMultiplyMin, 0.0001f, 0.1f);
        m_MouseAccelSpeedMultiplyMax = Mathf.Clamp(m_MouseAccelSpeedMultiplyMax, 0.0001f, 0.1f);
        m_MouseAccelerationMax = Mathf.Clamp(m_MouseAccelerationMax, 0f, 20f);

        // Clamp smoothing times to ensure valid interpolation between min and max
        m_MinSmoothingTime = Mathf.Clamp(m_MinSmoothingTime, 0.001f, m_MaxSmoothingTime);
        m_MaxSmoothingTime = Mathf.Clamp(m_MaxSmoothingTime, m_MinSmoothingTime, 0.25f);
    }

    // Internal state for smoothing and acceleration
    private Vector2 m_PreviousAimDelta = Vector2.zero;
    private Vector2 m_AimDeltaAcceleration = Vector2.zero;

    /// <summary>
    /// Gets the horizontal turn angle based on mouse sensitivity settings.
    /// </summary>
    public float mouseTurnAngleH
    {
        get { return Mathf.Lerp(m_MouseTurnAngleMin, m_MouseTurnAngleMax, Settings.input.horizontalMouseSensitivity); }
    }

    /// <summary>
    /// Gets the vertical turn angle based on mouse sensitivity settings.
    /// </summary>
    public float mouseTurnAngleV
    {
        get { return Mathf.Lerp(m_MouseTurnAngleMin, m_MouseTurnAngleMax, Settings.input.verticalMouseSensitivity); }
    }

    /// <summary>
    /// Registers to mouse settings change events when enabled.
    /// </summary>
    protected void OnEnable()
    {
        Settings.input.onMouseSettingsChanged += OnMouseSettingsChanged;
        OnMouseSettingsChanged();
    }

    /// <summary>
    /// Unregisters from mouse settings change events when disabled.
    /// </summary>
    protected void OnDisable()
    {
        Settings.input.onMouseSettingsChanged -= OnMouseSettingsChanged;
    }

    /// <summary>
    /// Resets smoothing when mouse settings are changed.
    /// </summary>
    void OnMouseSettingsChanged()
    {
        ResetSmoothing();
    }

    /// <summary>
    /// Handles mouse input for aiming, applying acceleration and smoothing if enabled.
    /// </summary>
    /// <param name="input">The raw mouse input as a Vector2.</param>
    public void HandleMouseInput(Vector2 input)
    {
        // Invert vertical mouse input if the setting is enabled
        if (!Settings.input.invertMouse)
            input.y *= -1f;

        // Apply mouse acceleration if enabled
        if (Settings.input.enableMouseAcceleration)
        {
            float acceleration = Settings.input.mouseAcceleration;
            input = GetAcceleratedMouseInput(input, acceleration);
        }

        // Apply mouse smoothing if enabled
        if (Settings.input.enableMouseSmoothing)
        {
            float smoothing = Settings.input.mouseSmoothing;
            input = GetSmoothedMouseInput(input, smoothing);
        }

        // Apply rotation input relative to a specified transform if available
        if (m_RelativeTo != null)
        {
            input.x *= mouseTurnAngleH;
            input.y *= mouseTurnAngleV;
            AddRotationInput(input, m_RelativeTo);
        }
        else
        {
            // Apply direct rotation input based on mouse movement
            AddRotation(input.x * mouseTurnAngleH, input.y * mouseTurnAngleV);
        }
    }

    /// <summary>
    /// Resets the mouse input smoothing state.
    /// </summary>
    void ResetSmoothing()
    {
        m_PreviousAimDelta = Vector2.zero;
        m_AimDeltaAcceleration = Vector2.zero;
    }

    /// <summary>
    /// Smooths mouse input using a damping function based on the smoothing strength.
    /// </summary>
    /// <param name="input">The raw mouse input.</param>
    /// <param name="strength">The smoothing strength (0-1).</param>
    /// <returns>The smoothed mouse input as a Vector2.</returns>
    Vector2 GetSmoothedMouseInput(Vector2 input, float strength)
    {
        m_PreviousAimDelta = Vector2.SmoothDamp(m_PreviousAimDelta, input, ref m_AimDeltaAcceleration, Mathf.Lerp(m_MinSmoothingTime, m_MaxSmoothingTime, strength));
        return m_PreviousAimDelta;
    }

    /// <summary>
    /// Accelerates mouse input based on the movement speed and strength of the acceleration.
    /// </summary>
    /// <param name="input">The raw mouse input.</param>
    /// <param name="strength">The acceleration strength (0-1).</param>
    /// <returns>The accelerated mouse input as a Vector2.</returns>
    Vector2 GetAcceleratedMouseInput(Vector2 input, float strength)
    {
        float speed = input.magnitude / Time.deltaTime;

        float speedMultiplier = Mathf.Lerp(m_MouseAccelSpeedMultiplyMin, m_MouseAccelSpeedMultiplyMax, strength);

        float multiplier = 1f + (speed * speedMultiplier);

        // Clamp the multiplier to the maximum allowed value
        if (m_MouseAccelerationMax > 1f && multiplier > m_MouseAccelerationMax)
            multiplier = m_MouseAccelerationMax;

        return input * multiplier;
    }
}
