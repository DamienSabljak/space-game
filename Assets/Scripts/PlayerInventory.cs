using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : Inventory {
    [SerializeField] int MaxItemSlots = 10;

    [HideInInspector] public List<Item> ToolBarList;//holds items seen within toolbar
    
    //TODO** add a way to limit itemlist size

    // Use this for initialization
    void Start () {
        InitToolBarList();
        InitConsumableArr();
        InitItemList();
    }

    private void InitToolBarList()
    {
        //leave blank spaces in list for accessing 
        ToolBarList.Add(null);
        ToolBarList.Add(null);
        ToolBarList.Add(null);
        ToolBarList.Add(null);
    }

    private void InitItemList()
    {
        for(int i=0; i < MaxItemSlots; i++)
        {
            ItemList.Add(null);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}

    //overwrite old method
    new public void AddItem(Item item)
    {//WARNING: MAKE SURE TO CAST AS PLAYERINV TO OVERRIDE EXISTING ITEM PARENT CLASS METHOD
     //adds item to inventory
     //does not create copy of item
        Debug.Log("PlayerInv: Add item method called");
        bool wasAdded = false;
        //Add to toolbar slot if theres space
        for (int i = 0; i < ToolBarList.Count; i++)
        {
            if (ToolBarList[i] == null)
            {
                ToolBarList[i] = item;
                break;
            }
        }
        //try to add to inventory
        for (int i = 0; i < MaxItemSlots; i++)
        {
            if (ItemList[i] == null)
            {
                ItemList[i] = item;
                wasAdded = true;
                break;
            }
        }
        

        if (wasAdded == true)
        {
            item.gameObject.SetActive(false);//dont destroy, keep refference for later
            item.gameObject.transform.SetParent(this.transform);
            item.transform.position = this.transform.position;//set at same pos as parent
        }

        else
        {
            Debug.Log("not enough space in toolbar left!");
        }
    }

    new public void RemoveItem(Item item)
    {
        Debug.Log("now entering PlayerInventory remove item method");
        bool wasfound = false;
        //search and destroy refference to item in array
        for (int i = 0; i < ItemList.Count; i++)
        {
            if (ItemList[i] == item)
            {
                ItemList[i] = null;
                wasfound = true;
                break;
            }
        }
        Debug.Log("PlayerInventory wasfound = "+wasfound);
        //remove from tool bar
        if (wasfound == true)
        {
            Debug.Log("item to be removed was found");
            for (int i = 0; i < ToolBarList.Count; i++)
            {
                if (ToolBarList[i] == item)
                {
                    Debug.Log("toolbarItemRemoved");
                    ToolBarList[i] = null;
                    break;
                }
            }
        }
    }
}
