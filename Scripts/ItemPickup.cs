using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPickup : MonoBehaviour
{
    public GameObject Inventory;
    public GameObject ItemPickupDisplay;
    public InventoryManager IManager;
    [SerializeField] public ItemClass Item;
    public StarterAssetsInputs _saa;
    public ThirdPersonShooterController _tpsc;
    public GameObject Player;
    public int Quantity;
    GameObject WeaponOnPlayer;

    void Start()
    {
        Player = GameObject.Find("PlayerArmature");
        Inventory = GameObject.Find("Inventory");
        IManager = Inventory.GetComponent<InventoryManager>();
        _saa = Player.GetComponent<StarterAssetsInputs>();
        _tpsc = Player.GetComponent<ThirdPersonShooterController>();
        ItemPickupDisplay = _tpsc.itemPickupDisplay;
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            ItemPickupDisplay.SetActive(true);
            ItemPickupDisplay.GetComponent<Text>().text = "Press 'E' to pick up " + Item.name;

            if (_saa.interact)
            {
                Player.GetComponent<AudioSource>().Play(); // play pickup sound
                this.gameObject.SetActive(false); // hide prop

                if (Item.name == "Rifle")
                    WeaponOnPlayer = _tpsc.weaponRifleObject;
                else if (Item.name == "Pistol")
                    WeaponOnPlayer = _tpsc.weaponPistolObject;
                else if (Item.name == "Machete")
                    WeaponOnPlayer = _tpsc.weaponMeleeObject;

                else
                    WeaponOnPlayer = null;

                if (WeaponOnPlayer != null)
                {
                    if (!_tpsc.starterAssetsInputs.aim)
                    {
                        _tpsc.SwitchOnPickup(WeaponOnPlayer.GetComponent<WeaponProperties>().weaponType);
                    }
                    _tpsc.weaponsEquipped += 1;
                    _tpsc.weaponHolstered.SetActive(false);
                    WeaponOnPlayer.GetComponent<WeaponProperties>().isPickedUp = true;
                }

                IManager.Add(Item, Quantity);

                ItemPickupDisplay.SetActive(false);
                _saa.interact = false;
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        ItemPickupDisplay.SetActive(false);
    }
}
