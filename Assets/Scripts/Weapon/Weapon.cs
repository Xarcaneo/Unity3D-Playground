using GameServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Tooltip("The number of rounds this weapon can fire per minute, determining its firing speed.")]
    [SerializeField]
    private int fireRatePerMinute = 250;

    #region FIELDS

    /// <summary>
    /// Weapon Animator.
    /// </summary>
    private Animator animator;

    /// <summary>
    /// Reference to the game mode manager utilized in this game.
    /// </summary>
    private IGameModeManager gameModeManager;

    /// <summary>
    /// The primary component controlling player character actions.
    /// </summary>
    private FirstPersonController playerControl;

    /// <summary>
    /// Reference to the player's camera transform.
    /// </summary>
    private Transform cameraTransform;


    #endregion

    private void Awake()
    {
        //Get Animator.
        animator = GetComponent<Animator>();

        // Store a reference to the game mode manager. Although it's only needed here, we'll keep it cached for future use.
        gameModeManager = ServiceRegistry.Instance.GetService<IGameModeManager>();
        // Store a reference to the player character.
        playerControl = gameModeManager.GetPlayerCharacter();
        // Cache the main camera used for world interactions, useful for performing line traces.
        cameraTransform = playerControl.GetMainCamera().transform;
    }


    public float GetFireRate() => fireRatePerMinute;

    public void Fire()
    {
        //Play the firing animation.
        const string stateName = "Fire";
        animator.Play(stateName, 0, 0.0f);
    }

}
