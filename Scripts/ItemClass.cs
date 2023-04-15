using System.Collections;
using UnityEngine;

public abstract class ItemClass : ScriptableObject
{
    [Header("Item")] //data shared across every single item
    public string itemName;
    public Sprite itemIcon;
    public bool isStackable = true;
    public string itemDescription;
    public GameObject worldItem;
    public abstract ItemClass GetItem();
    public abstract ToolClass GetTool();
    public abstract MiscClass GetMisc();
    public abstract ConsumableClass GetConsumable();

}
