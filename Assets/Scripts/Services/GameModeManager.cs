/// <summary>
/// Manages game mode-specific functionality.
/// </summary>
public class GameModeManager : IGameModeManager
{
    #region Fields

    /// <summary>
    /// Reference to the player character instance.
    /// </summary>
    private FirstPersonController playerCharacter;

    #endregion

    #region Methods

    public FirstPersonController GetPlayerCharacter()
    {
        // Ensure the player character reference is set.
        if (playerCharacter == null)
        {
            playerCharacter = UnityEngine.Object.FindObjectOfType<FirstPersonController>();
        }

        // Return the player character instance.
        return playerCharacter;
    }

    #endregion
}
