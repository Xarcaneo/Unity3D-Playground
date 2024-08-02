// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

/// <summary>
/// Handles all the animation events that come from the weapon in the asset.
/// </summary>
public class WeaponAnimationEventHandler : MonoBehaviour
{
    #region FIELDS

    /// <summary>
    /// Equipped Weapon.
    /// </summary>
    //private WeaponBehaviour weapon;

    #endregion

    #region UNITY

    private void Awake()
    {
        Debug.Log("Awake method called in WeaponAnimationEventHandler");
    }

    #endregion

    #region ANIMATION

    /// <summary>
    /// Ejects a casing from this weapon. This function is called from an Animation Event.
    /// </summary>
    private void OnEjectCasing()
    {
        Debug.Log("OnEjectCasing method called in WeaponAnimationEventHandler");
    }

    #endregion
}
