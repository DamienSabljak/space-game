using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {
    //****Consumables****
    [Header("Starting inventory")]
    [SerializeField] public int startingScrap = 0;
    [SerializeField] public int startingMoney = 0;
    [SerializeField] public int startingAmmo  = 0;

                     public int[] consumableArr;//creates array sizeof number of types of consumables 
    [HideInInspector] public List<Item> ItemList;//holds items seen within toolbar

    // Use this for initialization
    void Start () {
        InitConsumableArr();
        
	}

    public void InitConsumableArr()//add starting inventories
    {
        consumableArr = new int[Consumable.numTypes];
        //scrap
        Consumable.Type s = Consumable.Type.SCRAP;
        consumableArr[(int)s] = startingScrap;
        //money
        Consumable.Type m = Consumable.Type.MONEY;
        consumableArr[(int)m] = startingMoney;
        //Ammo
        Consumable.Type a = Consumable.Type.AMMO;
        consumableArr[(int)a] = startingAmmo;
    } 

    // Update is called once per frame
    void Update () {
        //Debug.Log(consumableArr[0]);
        //Debug.Log(consumableArr[1]);
        //Debug.Log(consumableArr[2]);
    }

    public void AddConsumable(Consumable.Type consumableType, int amount)
    {
        //Debug.Log("consumable added: ");
        consumableArr[(int)consumableType] += amount;
        //Debug.Log(consumableArr[(int)consumableType]);
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
