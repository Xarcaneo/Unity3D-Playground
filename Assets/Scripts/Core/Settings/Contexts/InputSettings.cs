using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages input-related settings, including mouse sensitivity, smoothing, and acceleration.
/// Inherits from SettingsContext to allow saving and loading of settings data.
/// </summary>
[CreateAssetMenu(fileName = "Input", menuName = "Settings/Input")]
public class InputSettings : SettingsContext<InputSettings>
{
    /// <summary>
    /// The context name used for identifying this settings context.
    /// </summary>
    protected override string contextName { get { return "Input"; } }

    /// <summary>
    /// The display title for this settings context, used in the UI.
    /// </summary>
    public override string displayTitle { get { return "Input Settings"; } }

    /// <summary>
    /// The name used in the table of contents for this settings context.
    /// </summary>
    public override string tocName { get { return "Input Settings"; } }

    /// <summary>
    /// The unique identifier used in the table of contents for this settings context.
    /// </summary>
    public override string tocID { get { return "settings_input"; } }

    [Header("Mouse")]

    [SerializeField, Tooltip("The horizontal mouse look sensitivity.")]
    private float m_MouseSensitivityH = 0.5f;

    [SerializeField, Tooltip("The vertical mouse look sensitivity.")]
    private float m_MouseSensitivityV = 0.5f;

    [SerializeField, Tooltip("Invert the mouse vertical aim.")]
    private bool m_InvertMouse = false;

    [SerializeField, Tooltip("Mouse smoothing takes a weighted average of the mouse movement over time for a smoother effect.")]
    private bool m_EnableMouseSmoothing = false;

    [SerializeField, Tooltip("The amount of mouse smoothing to add.")]
    private float m_MouseSmoothing = 0.5f;

    [SerializeField, Tooltip("Mouse acceleration amplifies faster mouse movements.")]
    private bool m_EnableMouseAcceleration = false;

    [SerializeField, Tooltip("The amount of mouse acceleration to add.")]
    private float m_MouseAcceleration = 0.5f;

    /// <summary>
    /// Event triggered when mouse settings are changed.
    /// </summary>
    public event UnityAction onMouseSettingsChanged;

    // Constants defining the minimum and maximum sensitivity values
    const float k_MinSensitivity = 0.01f;
    const float k_MaxSensitivity = 1f;

    /// <summary>
    /// Checks if this settings context is the current active one.
    /// </summary>
    /// <returns>True if this is the current input settings context; otherwise, false.</returns>
    protected override bool CheckIfCurrent()
    {
        return Settings.input == this;
    }

    /// <summary>
    /// Validates and clamps the mouse sensitivity, acceleration, and smoothing values.
    /// </summary>
    protected void OnValidate()
    {
        m_MouseSensitivityH = Mathf.Clamp(m_MouseSensitivityH, k_MinSensitivity, k_MaxSensitivity);
        m_MouseSensitivityV = Mathf.Clamp(m_MouseSensitivityV, k_MinSensitivity, k_MaxSensitivity);
        m_MouseAcceleration = Mathf.Clamp01(m_MouseAcceleration);
        m_MouseSmoothing = Mathf.Clamp01(m_MouseSmoothing);
    }

    /// <summary>
    /// Gets or sets the horizontal mouse sensitivity.
    /// </summary>
    public float horizontalMouseSensitivity
    {
        get { return m_MouseSensitivityH; }
        set
        {
            SetValue(ref m_MouseSensitivityH, Mathf.Clamp(value, k_MinSensitivity, k_MaxSensitivity));
            onMouseSettingsChanged?.Invoke();
        }
    }

    /// <summary>
    /// Gets or sets the vertical mouse sensitivity.
    /// </summary>
    public float verticalMouseSensitivity
    {
        get { return m_MouseSensitivityV; }
        set
        {
            SetValue(ref m_MouseSensitivityV, Mathf.Clamp(value, k_MinSensitivity, k_MaxSensitivity));
            onMouseSettingsChanged?.Invoke();
        }
    }

    /// <summary>
    /// Gets or sets whether the vertical mouse axis is inverted.
    /// </summary>
    public bool invertMouse
    {
        get { return m_InvertMouse; }
        set
        {
            SetValue(ref m_InvertMouse, value);
            onMouseSettingsChanged?.Invoke();
        }
    }

    /// <summary>
    /// Gets or sets whether mouse smoothing is enabled.
    /// </summary>
    public bool enableMouseSmoothing
    {
        get { return m_EnableMouseSmoothing; }
        set
        {
            SetValue(ref m_EnableMouseSmoothing, value);
            onMouseSettingsChanged?.Invoke();
        }
    }

    /// <summary>
    /// Gets or sets the amount of mouse smoothing.
    /// </summary>
    public float mouseSmoothing
    {
        get { return m_MouseSmoothing; }
        set
        {
            SetValue(ref m_MouseSmoothing, Mathf.Clamp01(value));
            onMouseSettingsChanged?.Invoke();
        }
    }

    /// <summary>
    /// Gets or sets whether mouse acceleration is enabled.
    /// </summary>
    public bool enableMouseAcceleration
    {
        get { return m_EnableMouseAcceleration; }
        set
        {
            SetValue(ref m_EnableMouseAcceleration, value);
            onMouseSettingsChanged?.Invoke();
        }
    }

    /// <summary>
    /// Gets or sets the amount of mouse acceleration.
    /// </summary>
    public float mouseAcceleration
    {
        get { return m_MouseAcceleration; }
        set
        {
            SetValue(ref m_MouseAcceleration, Mathf.Clamp01(value));
            onMouseSettingsChanged?.Invoke();
        }
    }
}
