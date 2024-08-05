/// <summary>
/// Interface for game mode-related services.
/// </summary>
public interface IGameModeManager : IGameService
{
    /// <summary>
    /// Retrieves the player character instance.
    /// </summary>
    FirstPersonController GetPlayerCharacter();
}
