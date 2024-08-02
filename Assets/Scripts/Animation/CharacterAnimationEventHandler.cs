using UnityEngine;

/// <summary>
/// Handles all the animation events that come from the character in the asset.
/// </summary>
public class CharacterAnimationEventHandler : MonoBehaviour
{
    #region FIELDS

    /// <summary>
    /// Character Component Reference.
    /// </summary>
    //private CharacterBehaviour playerCharacter;

    #endregion

    #region UNITY

    private void Awake()
    {
        Debug.Log("Awake method called");
    }

    #endregion

    #region ANIMATION

    /// <summary>
    /// Ejects a casing from the character's equipped weapon. This function is called from an Animation Event.
    /// </summary>
    private void OnEjectCasing()
    {
        Debug.Log("OnEjectCasing method called");
    }

    /// <summary>
    /// Fills the character's equipped weapon's ammunition by a certain amount, or fully if set to 0. This function is called
    /// from a Animation Event.
    /// </summary>
    private void OnAmmunitionFill(int amount = 0)
    {
        Debug.Log("OnAmmunitionFill method called with amount: " + amount);
    }

    /// <summary>
    /// Sets the character's knife active value. This function is called from an Animation Event.
    /// </summary>
    private void OnSetActiveKnife(int active)
    {
        Debug.Log("OnSetActiveKnife method called with active: " + active);
    }

    /// <summary>
    /// Spawns a grenade at the correct location. This function is called from an Animation Event.
    /// </summary>
    private void OnGrenade()
    {
        Debug.Log("OnGrenade method called");
    }

    /// <summary>
    /// Sets the equipped weapon's magazine to be active or inactive! This function is called from an Animation Event.
    /// </summary>
    private void OnSetActiveMagazine(int active)
    {
        Debug.Log("OnSetActiveMagazine method called with active: " + active);
    }

    /// <summary>
    /// Bolt Animation Ended. This function is called from an Animation Event.
    /// </summary>
    private void OnAnimationEndedBolt()
    {
        Debug.Log("OnAnimationEndedBolt method called");
    }

    /// <summary>
    /// Reload Animation Ended. This function is called from an Animation Event.
    /// </summary>
    private void OnAnimationEndedReload()
    {
        Debug.Log("OnAnimationEndedReload method called");
    }

    /// <summary>
    /// Grenade Throw Animation Ended. This function is called from an Animation Event.
    /// </summary>
    private void OnAnimationEndedGrenadeThrow()
    {
        Debug.Log("OnAnimationEndedGrenadeThrow method called");
    }

    /// <summary>
    /// Melee Animation Ended. This function is called from an Animation Event.
    /// </summary>
    private void OnAnimationEndedMelee()
    {
        Debug.Log("OnAnimationEndedMelee method called");
    }

    /// <summary>
    /// Inspect Animation Ended. This function is called from an Animation Event.
    /// </summary>
    private void OnAnimationEndedInspect()
    {
        Debug.Log("OnAnimationEndedInspect method called");
    }

    /// <summary>
    /// Holster Animation Ended. This function is called from an Animation Event.
    /// </summary>
    private void OnAnimationEndedHolster()
    {
        Debug.Log("OnAnimationEndedHolster method called");
    }

    /// <summary>
    /// Sets the character's equipped weapon's slide back pose. This function is called from an Animation Event.
    /// </summary>
    private void OnSlideBack(int back)
    {
        Debug.Log("OnSlideBack method called with back: " + back);
    }

    #endregion
}
