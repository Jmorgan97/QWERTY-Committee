using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject itemCursor;
    [SerializeField] private GameObject nameCursor;

    [SerializeField] private GameObject slotHolder; //slots in inventory
    [SerializeField] private ItemClass itemToAdd;
    [SerializeField] private ItemClass itemToRemove;

    private SlotClass[] items; //array of items in inventory. Private to this class
    [SerializeField] private SlotClass[] startingItems; // starting items, which can be set in unity graphically. Or will just be empty.

    private GameObject[] slots;

    private SlotClass movingSlot;
    private SlotClass tempSlot;
    private SlotClass originalSlot;
    private bool isMovingItem;

    [SerializeField] private GameObject playerSpawnItem;

    private void Start()
    {
        slots = new GameObject[slotHolder.transform.childCount]; //sets slots to gameobjects of count childcount(number of prefab item slots in Content)
        items = new SlotClass[slots.Length];
        for (int i = 0; i < items.Length; i++)//populate items as slots
        {
            items[i] = new SlotClass();
        }
        for (int i = 0; i < startingItems.Length; i++)//add starting inventory to items[]
        {
            items[i] = startingItems[i];
        }

        //set slots
        for (int i = 0; i < slotHolder.transform.childCount; i++)//set slots 
        {
            slots[i] = slotHolder.transform.GetChild(i).gameObject;
        }

        RefreshUI();

        Add(itemToAdd, 1);
        Remove(itemToRemove);
    }


    private void Update()
    {
        nameCursor.SetActive(false);

        itemCursor.SetActive(isMovingItem);
        itemCursor.transform.position = Mouse.current.position.ReadValue();

        if (isMovingItem)
        {
            itemCursor.GetComponent<Image>().sprite = movingSlot.GetItem().itemIcon;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame) //if we clicked...
        {
            if(isMovingItem)
            {
                //end item move
                EndItemMove();
            }
            else
            {
                BeginItemMove();
            }
        }

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            BeginRemoveItem();
        }

        //tooltip hovering update
        if (GetClosestSlot() is SlotClass && GetClosestSlot().GetItem() is ItemClass)
        {
            IsHoveringOverItem();
        }
    }


    #region inventory management functions
    public void RefreshUI()
    {
        //looks through items in inventory and verifies item states and images
        for (int i = 0; i < slots.Length; i++)
        {
            try
            {
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = items[i].GetItem().itemIcon;
                slots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                if (items[i].GetQuantity() == 0 || items[i].GetQuantity() == 1)
                    slots[i].transform.GetChild(1).GetComponent<Text>().text = "";
                else
                    slots[i].transform.GetChild(1).GetComponent<Text>().text = items[i].GetQuantity() + "";
            }
            catch
            {
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                slots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                slots[i].transform.GetChild(1).GetComponent<Text>().text = "";
            }
        }
    }

    public bool Add(ItemClass item, int quantity)
    {

        //check if inventory contains item, then add item to existing slot in inventory with count +1. So items can stack
        SlotClass slot = CheckContains(item);
        if (slot != null && slot.GetItem().isStackable)
            slot.AddQuantity(1);
        else
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].GetItem() == null)
                {
                    items[i].AddItem(item, quantity);
                    break;
                }
            }
            /*if (items.Count < slots.Length) //if there is space in the inventory, add the item
                items.Add(new SlotClass(item, 1));
            else
                return false;*/ //if there is not space in the inventory, return false for failed add.
        }

        
        RefreshUI();
        return true;
    }

    //public bool Add(string itemClassName, int quantity)
    //{
    //    ItemClass item = new ItemClass()

    //    //check if inventory contains item, then add item to existing slot in inventory with count +1. So items can stack
    //    SlotClass slot = CheckContains(item);
    //    if (slot != null && slot.GetItem().isStackable)
    //        slot.AddQuantity(1);
    //    else
    //    {
    //        for (int i = 0; i < items.Length; i++)
    //        {
    //            if (items[i].GetItem() == null)
    //            {
    //                items[i].AddItem(item, quantity);
    //                break;
    //            }
    //        }
    //        /*if (items.Count < slots.Length) //if there is space in the inventory, add the item
    //            items.Add(new SlotClass(item, 1));
    //        else
    //            return false;*/ //if there is not space in the inventory, return false for failed add.
    //    }


    //    RefreshUI();
    //    return true;
    //}

    public bool Remove(ItemClass item)
    {

        SlotClass temp = CheckContains(item);
        if (temp != null)
        {
            if (temp.GetQuantity() > 1) //if there is already more than one on the stack in inventory, just remove 1 from count.
            {
                CreateWorldObject(temp.GetItem(), 1);
                temp.RemoveQuantity(1);
            }
            else //else, remove the object from inventory entirely.
            {
                SlotClass slotToRemove = new SlotClass();
                int indexToRemove = 0;
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].GetItem() == item)
                    {
                        indexToRemove = i;
                        break;
                    }
                }

                CreateWorldObject(items[indexToRemove].GetItem(), 1);
                items[indexToRemove].Clear();
            }
        }  
        else 
        {
            return false;
        }

        
        RefreshUI();
        return true;

    }

    public SlotClass CheckContains(ItemClass item)
    {
        /*        for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].GetItem() == item)
                        return items[i];
                }
                return null;*/
        foreach (SlotClass slot in items)
        {
            if (slot.GetItem() == item)
                return slot;
        }
        return null;
    }
    #endregion

    #region Dynamic Inventory, moving things around

    private bool BeginItemMove()
    {
        originalSlot = GetClosestSlot();
        if (originalSlot == null || originalSlot.GetItem() == null)
        {
            return false;
        }

        movingSlot = new SlotClass(originalSlot);
        originalSlot.Clear();
        isMovingItem = true;
        RefreshUI();
        return true;
    }

    private bool EndItemMove()
    {
        originalSlot = GetClosestSlot();
        if (originalSlot == null)
        {
            Add(movingSlot.GetItem(), movingSlot.GetQuantity());
            movingSlot.Clear();
        }
        else
        {
            if (originalSlot.GetItem() != null)
            {
                if (originalSlot.GetItem() == movingSlot.GetItem())//these are the same item, meaning they should stack methinks
                {
                    if (originalSlot.GetItem().isStackable)
                    {
                        originalSlot.AddQuantity(movingSlot.GetQuantity());
                        movingSlot.Clear();
                    }
                    else
                        return false;
                }
                else //if they are not the same item, swap spots
                {
                    tempSlot = new SlotClass(originalSlot); // first = second, for swapping inventory spots
                    originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity()); //second = third
                    movingSlot.AddItem(tempSlot.GetItem(), tempSlot.GetQuantity()); // third = first
                                                                                    //movingSlot.Clear();
                    RefreshUI();
                    return true;
                }
            }
            else //place item normally
            {
                //originalSlot = new SlotClass(movingSlot);
                originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
                movingSlot.Clear();
            }
        }
        isMovingItem = false;
        RefreshUI();
        return true;

    }
    private SlotClass GetClosestSlot()
    {
        //has to find the cursors position and return nearest slot
        for (int i = 0; i < slots.Length; i++) 
        {
            if (Vector2.Distance(slots[i].transform.position, Mouse.current.position.ReadValue()) <= 60)
            {
                return items[i];
            }
        }
        return null;
    }

    #endregion

    #region tooltips
    private void IsHoveringOverItem()
    {
        nameCursor.SetActive(true);
        //nameCursor.transform.position = Input.mousePosition;
        nameCursor.transform.GetChild(0).GetComponent<Text>().text = ItemTooltip();//null ref... what logic is wrong? FIXED
        //get the tooltip position with offset
        //Vector3 position = new Vector3(Input.mousePosition.x + rect.rect.width, Input.mousePosition.y, 0f);
        //clamp it to the screen size so it doesn't go outside
        //nameCursor.transform.position = new Vector3(Mathf.Clamp(position.x, min.x + rect.rect.width / 2, max.x - rect.rect.width / 2), Mathf.Clamp(position.y, min.y + rect.rect.height / 2, max.y - rect.rect.height / 2), transform.position.z);
    }

    private string ItemTooltip()
    {
        var item = GetClosestSlot().GetItem();
        string temp_tooltip;
        temp_tooltip = item.itemName + ": \n" + item.itemDescription;
        return temp_tooltip;
    }
    #endregion

    #region removing from inventory

    public void BeginRemoveItem()
    {
        //create tempSlot with GetClosestSlot(). This finds the closest slot to your click.
        tempSlot = GetClosestSlot();
        //get item from that slot... pass item into Remove(), which deletes 1 of the item from that inventory slot.
        //Remove also calls functionality to spawn world object
        itemToRemove = tempSlot.GetItem();
        Remove(itemToRemove);


    }

    public void CreateWorldObject(ItemClass removedItem, int quantity)
    {
        //creates world object
        //Instantiate, with position, n shit like that

        for (int i = 0; i < quantity; i++)//spawn this number of item type into the world near the player
        {
            Instantiate(removedItem.worldItem, playerSpawnItem.transform.position, Quaternion.identity);
        }

    }
    #endregion

}
