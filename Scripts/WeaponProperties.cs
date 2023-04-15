using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponProperties : MonoBehaviour
{
    [SerializeField] public AudioSource switchSound; // Sound that plays when you switch to that wepaon
    [SerializeField] public AudioSource reloadSound;
    [SerializeField] public AudioSource attackSound; // Shoot/swing/whatever weapon
    [SerializeField] public AudioSource emptySound; // Sound that plays when attacking with no ammo
    [SerializeField] public GameObject muzzleFlash; // Object target for location left hand rig tries to move to grip
    [SerializeField] public int weaponType;
    [SerializeField] public bool isPickedUp;
    [SerializeField] public int magSize;
    [SerializeField] public int currentAmmo;
    [SerializeField] public int totalAmmo;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
