using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A generic abstract class for managing settings contexts.
/// Inherits from SettingsContextBase and provides a singleton-like pattern for accessing settings instances.
/// </summary>
/// <typeparam name="T">The type of the settings context, which must inherit from SettingsContext<T>.</typeparam>
public abstract class SettingsContext<T> : SettingsContextBase where T : SettingsContext<T>
{
    /// <summary>
    /// Singleton-like instance of the settings context.
    /// </summary>
    protected static T instance { get; private set; }

    /// <summary>
    /// Retrieves the singleton instance of the settings context, loading it from resources if necessary.
    /// </summary>
    /// <param name="filename">The name of the file to load the settings from, located in the Resources folder.</param>
    /// <returns>The singleton instance of the settings context.</returns>
    public static T GetInstance(string filename)
    {
        if (instance == null)
        {
            // Attempt to load the settings context from the Resources folder
            var loaded = Resources.Load<T>(filename);

            if (loaded == null)
            {
                // If not found, create a new instance of the settings context
                instance = CreateInstance<T>();
            }
            else
            {
                // If found, instantiate a copy to prevent changes during runtime from affecting the editor version
                instance = Instantiate(loaded);
            }
        }
        return instance;
    }
}
