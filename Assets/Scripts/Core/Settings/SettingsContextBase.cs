using UnityEngine;
using System.IO;

/// <summary>
/// Base class for managing settings contexts, which are saved and loaded from disk.
/// Provides functionality for handling dirty state, file paths, and serialization.
/// </summary>
public abstract class SettingsContextBase : ScriptableObject
{
    /// <summary>
    /// The name of the context, used for generating file paths and other identifiers.
    /// Must be overridden in derived classes.
    /// </summary>
    protected abstract string contextName { get; }

    /// <summary>
    /// The display title of the settings context, used in the UI.
    /// Must be overridden in derived classes.
    /// </summary>
    public abstract string displayTitle { get; }

    /// <summary>
    /// The name used in the table of contents for the settings context.
    /// Must be overridden in derived classes.
    /// </summary>
    public abstract string tocName { get; }

    /// <summary>
    /// The identifier used in the table of contents for the settings context.
    /// Must be overridden in derived classes.
    /// </summary>
    public abstract string tocID { get; }

    /// <summary>
    /// Indicates whether the settings context has unsaved changes.
    /// </summary>
    public bool dirty { get; protected set; }

    /// <summary>
    /// Checks if the current settings context is active or relevant.
    /// Must be overridden in derived classes.
    /// </summary>
    /// <returns>True if the context is current; otherwise, false.</returns>
    protected abstract bool CheckIfCurrent();

    // Stores the file path for saving and loading the settings context
    private string m_Filepath = string.Empty;

    /// <summary>
    /// Gets the file path for the settings context, generating it if necessary.
    /// The path is based on the persistent data path and the context name.
    /// </summary>
    protected string filepath
    {
        get
        {
            if (m_Filepath == string.Empty)
            {
                // Generate the file path based on the context name
                m_Filepath = Path.Combine(Application.persistentDataPath, contextName + ".settings");
            }
            return m_Filepath;
        }
    }

    /// <summary>
    /// Deletes the save file associated with this settings context.
    /// </summary>
    public void DeleteSaveFile()
    {
        if (File.Exists(filepath))
            File.Delete(filepath);
    }

    /// <summary>
    /// Sets a value and marks the context as dirty.
    /// </summary>
    /// <typeparam name="V">The type of the value being set.</typeparam>
    /// <param name="target">The reference to the value being updated.</param>
    /// <param name="to">The new value to assign.</param>
    protected void SetValue<V>(ref V target, V to)
    {
        target = to;
        dirty = true;
    }

    /// <summary>
    /// Loads the settings context from disk. If the file does not exist, it creates a new one with default settings.
    /// </summary>
    public virtual void Load()
    {
        if (File.Exists(filepath))
        {
            // Load settings from the file
            string json = File.ReadAllText(filepath);
            JsonUtility.FromJsonOverwrite(json, this);
        }
        else
        {
            // Create a new file with default settings
            File.WriteAllText(filepath, JsonUtility.ToJson(this, true));
        }
        dirty = false;
        OnLoad();
    }

    /// <summary>
    /// Saves the settings context to disk if there are unsaved changes (dirty state).
    /// </summary>
    public virtual void Save()
    {
        if (dirty)
        {
            // Save settings to the file
            File.WriteAllText(filepath, JsonUtility.ToJson(this, true));
            dirty = false;
        }
        OnSave();
    }

    /// <summary>
    /// Called after loading the settings context. Can be overridden by derived classes for custom behavior.
    /// </summary>
    public virtual void OnLoad() { }

    /// <summary>
    /// Called after saving the settings context. Can be overridden by derived classes for custom behavior.
    /// </summary>
    public virtual void OnSave() { }
}
