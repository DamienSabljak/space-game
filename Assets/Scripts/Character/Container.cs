using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : Vendor {

	// Use this for initialization
	void Start () {
        InitVendor();
        InitCosts();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    override public void PurchaseItem(Item item, int amount)//create a new item object if needed when calling 
    {

        //consumable section
        if (item.GetType() == typeof(Consumable))
        {
            //Dont check money
            Consumable consumable = (Consumable)item;//cast to consumable
            Consumable.Type c = consumable.consumabletype;

               if (VendorInv.consumableArr[(int)c] >= amount)
               {
                   //PURCHASE IS SUCCESFUL
                   Debug.Log("Okay, here you go");
                   VendorInv.consumableArr[(int)c] -= amount;
                   CustomerInv.consumableArr[(int)c] += amount;
               }
               else
               {
                   Debug.Log("I dont have that many left, sorry");
               }

        }
        //if not consumable attempt to add to item array
        else
        {
            VendorInv.RemoveItem(item);
            CustomerInv.gameObject.GetComponent<PlayerInventory>().AddItem(item);//must have weird cast to access playerinv version of method

        }
    }

    override public void SellItem(Item item, int amount)//create a new item object if needed when calling 
    {
        //consumable section
        if (item.GetType() == typeof(Consumable))
        {
            Consumable consumable = (Consumable)item;//cast to consumable
            Consumable.Type c = consumable.consumabletype;
            if (CustomerInv.consumableArr[(int)c] >= amount)
            {
                //SUCCESFULL SELL
                Debug.Log("Okay, here you go");
                CustomerInv.consumableArr[(int)c] -= amount;
                VendorInv.consumableArr[(int)c] += amount;
            }
            else
            {
                Debug.Log("I dont have that many!!");
            }
        }
        //if not consumable attempt to add to item array
        else
        {
            VendorInv.AddItem(item);
            Debug.Log("Vendor sellItem method about to remove item from customer inv");
            CustomerInv.gameObject.GetComponent<PlayerInventory>().RemoveItem(item);//must have weird cast to access playerinv version of method
        } 
    }


}
