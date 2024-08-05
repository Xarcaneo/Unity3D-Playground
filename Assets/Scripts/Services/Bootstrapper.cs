using GameServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bootstrapper class responsible for initializing essential game services.
/// </summary>
public static class Bootstrapper
{
    /// <summary>
    /// Method to perform initial setup before the first scene is loaded.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Setup()
    {
        // Initialize the default service locator.
        ServiceRegistry.Initialize();

        //Game Mode Service.
        ServiceRegistry.Instance.RegisterService<IGameModeManager>(new GameModeManager());
    }
}