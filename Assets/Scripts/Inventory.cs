using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {
    //****Consumables****
    [Header("Starting inventory")]
    [SerializeField] int Scrap = 0;
    [SerializeField] int Money = 0;
    [SerializeField] int Ammo  = 0;


    [HideInInspector] public int[] consumableArr = new int[Consumable.numTypes];//creates array sizeof number of types of consumables 
    [HideInInspector] public List<Item> ItemList;//holds items seen within toolbar

    // Use this for initialization
    void Start () {
        InitConsumableArr();
        
	}

    public void InitConsumableArr()//add starting inventories
    {
        //scrap
        Consumable.Type s = Consumable.Type.scrap;
        consumableArr[(int)s] = Scrap;
        //money
        Consumable.Type m = Consumable.Type.money;
        consumableArr[(int)m] = Money;
        //Ammo
        Consumable.Type a = Consumable.Type.ammo;
        consumableArr[(int)a] = Ammo;
    }

    

    // Update is called once per frame
    void Update () {
		
	}

    public void AddConsumable(Consumable.Type consumableType, int amount)
    {
        consumableArr[(int)consumableType] += amount;
    }

    public void AddItem(Item item)
    {
        bool wasAdded = false;

        //try to add to existing slot
        for (int i = 0; i < ItemList.Count; i++)
        {
            if (ItemList[i] == null)
            {
                ItemList[i] = item;
                wasAdded = true;
                break;
            }
        }
        //otherwise append to end (always add to inventory)
        if (wasAdded == false)
        {
            ItemList.Add(item);
            wasAdded = true;
        }

        if (wasAdded == true)
        {
            item.gameObject.SetActive(false);//dont destroy, keep refference for later
            item.gameObject.transform.SetParent(this.transform);
            item.transform.position = this.transform.position;//set at same pos as parent
        }
            
        else
        {
            Debug.Log("not enough space in inventory to add item!");
        }
    }

    public void RemoveItem(Item item)
    {
        Debug.Log("inventory removeItemClass started");
        //search and destroy refference to item in array
        for(int i=0;i<ItemList.Count;i++)
        {
            if(ItemList[i] == item)
            {
                ItemList[i] = null;
                break;
            }
        }
    }



}
