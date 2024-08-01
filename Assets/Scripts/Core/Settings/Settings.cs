using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

/// <summary>
/// Manages game settings, specifically input settings, and ensures they persist across scenes.
/// </summary>
public class Settings : MonoBehaviour
{
    /// <summary>
    /// Static instance of InputSettings to manage input-related settings.
    /// </summary>
    public static InputSettings input { get; private set; }

    // Reference to the runtime settings GameObject
    private static GameObject s_RuntimeSettingsObject = null;

    /// <summary>
    /// Gets or creates a persistent GameObject to hold runtime settings.
    /// </summary>
    public static GameObject runtimeSettingsObject
    {
        get
        {
            if (s_RuntimeSettingsObject == null)
            {
                // Create a new GameObject for runtime settings if it doesn't exist
                s_RuntimeSettingsObject = new GameObject("SettingsRuntime");
                UnityEngine.Object.DontDestroyOnLoad(s_RuntimeSettingsObject);
            }
            return s_RuntimeSettingsObject;
        }
    }

    /// <summary>
    /// Initializes the input settings before any scene is loaded.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RuntimeInitializeOnLoad()
    {
        // Get or create an instance of InputSettings with the specified key
        input = InputSettings.GetInstance("Settings_Input");
    }

    /// <summary>
    /// Loads the input settings after a scene has been loaded.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void RuntimeInitializeOnLoadPost()
    {
        // Load the input settings
        input.Load();
    }

    /// <summary>
    /// Saves the current input settings.
    /// </summary>
    public static void Save()
    {
        input.Save();
    }
}
