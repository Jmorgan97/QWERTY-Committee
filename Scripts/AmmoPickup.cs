using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;

public class AmmoPickup : MonoBehaviour
{
    [SerializeField] public ItemClass Item;
    [SerializeField] private GameObject Player;
    public int Quantity;
    private ThirdPersonShooterController _tpsc;
    void Start()
    {
        Player = GameObject.Find("PlayerArmature");
        _tpsc = Player.GetComponent<ThirdPersonShooterController>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Player.GetComponent<AudioSource>().Play(); // play pickup sound
            this.gameObject.SetActive(false); // hide prop

            if (Item.name == "AmmoPistol")
                _tpsc.weaponPistolObject.GetComponent<WeaponProperties>().totalAmmo += Quantity;
            else if (Item.name == "AmmoRifle")
                _tpsc.weaponRifleObject.GetComponent<WeaponProperties>().totalAmmo += Quantity;
        }
    }
}
