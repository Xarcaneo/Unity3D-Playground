using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Tooltip("The number of rounds this weapon can fire per minute, determining its firing speed.")]
    [SerializeField]
    private int fireRatePerMinute = 250;

    public float GetFireRate() => fireRatePerMinute;
}
