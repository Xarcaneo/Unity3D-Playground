using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    #region FIELDS

    /// <summary>
    /// An array containing all the weapons. 
    /// The weapons are retrieved based on their order within the hierarchy as children of this object.
    /// </summary>
    private Weapon[] weaponArray;

    /// <summary>
    /// Currently equipped Weapon.
    /// </summary>
    private Weapon currentWeapon;

    /// <summary>
    /// Currently equipped index.
    /// </summary>
    private int currentWeaponIndex = -1;

    #endregion

    /// <summary>
    /// Initialize the weapon system. This function is triggered when the game begins. 
    /// We avoid using Awake or Start methods here to ensure the PlayerCharacter component can equip the correct weapon 
    /// based on the specified index.
    /// </summary>
    /// <param name="initialWeaponIndex">The inventory index of the weapon to equip when the game starts.</param>
    public void Initialize(int initialWeaponIndex = 0)
    {
        // Retrieve all weapon components. Note that weapons should be children of the object that this script is attached to.
        weaponArray = GetComponentsInChildren<Weapon>(true);

        // Deactivate all weapons initially. This allows us to easily activate the selected weapon only.
        foreach (Weapon weapon in weaponArray)
            weapon.gameObject.SetActive(false);

        // Equip the weapon based on the provided index.
        EquipWeapon(initialWeaponIndex);
    }

    public Weapon EquipWeapon(int index)
    {
        // If no weapons are available, there's nothing to equip.
        if (weaponArray == null)
            return currentWeapon;

        // Ensure the index is within the valid range of the array.
        if (index >= weaponArray.Length)
            return currentWeapon;

        // Avoid re-equipping the currently selected weapon.
        if (currentWeaponIndex == index)
            return currentWeapon;

        // Deactivate the weapon that is currently equipped, if any.
        if (currentWeapon != null)
            currentWeapon.gameObject.SetActive(false);

        // Update the current weapon index.
        currentWeaponIndex = index;
        // Assign the new weapon to the equipped slot.
        currentWeapon = weaponArray[currentWeaponIndex];
        // Activate the newly equipped weapon.
        currentWeapon.gameObject.SetActive(true);

        // Return the newly equipped weapon.
        return currentWeapon;
    }
    public  Weapon GetEquippedWeapon() => currentWeapon;
}
