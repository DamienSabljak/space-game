using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vendor : Character {
    [SerializeField] public List<GameObject> possibleItems;//holds a list containing all items which are possible to appear at a vendor for purchase 
    public List<GameObject> availableItems;//holds a list of all items for sale (should be 3 only)
    public Character Customer;
    [HideInInspector]  public Inventory CustomerInv;
    [HideInInspector]  public Inventory VendorInv;
    [SerializeField] public int[] ConsumablesCostArr;//array containing cost of consumables at enum index
    // Use this for initialization
    void Start () {
        InitVendor();
        //InitCosts();
	}

    public void InitVendor()
    {
        VendorInv = gameObject.GetComponent<Inventory>();
        //Debug.Log("Vendorinv should be added");
        availableItems.Add(Instantiate(possibleItems[UnityEngine.Random.Range(0, possibleItems.Count - 1)]));//randomly choose 3 items for vendor inventory 
        availableItems.Add(Instantiate(possibleItems[UnityEngine.Random.Range(0, possibleItems.Count - 1)]));
        availableItems.Add(Instantiate(possibleItems[UnityEngine.Random.Range(0, possibleItems.Count - 1)]));
    }

    public void InitCosts()
    {
        //set all consumables to cost of 5
        ConsumablesCostArr = new int[Consumable.numTypes];
        for (int i = 0; i < ConsumablesCostArr.Length; i++)
        {
            ConsumablesCostArr[i] = 5;//no need for int cast, discriminate to enum scheme
        }
    }

    // Update is called once per frame
    void Update () {
		
	}

    
    public void StartDialog(Character cus)//adds pointer to customer for future calls
    {
        Debug.Log("Greeting vendor...");
        //set customer
        Customer = cus;
        CustomerInv = Customer.gameObject.GetComponent<PlayerInventory>();

        Level.CurrentLevel.currentVendor = this;
        Level.CurrentLevel.OpenVendorMenu();//opens shopMenu 
        //Level.CurrentLevel.OpenInventoryMenu();//opens player's inventory (old)

        //used only for legacy menu (ShopMenu) 
        ShopMenu.CurrentVendor = this;
    }

    virtual public void PurchaseItem(Item item, int amount)//create a new item object if needed when calling 
    {
        //access customer money
        Consumable.Type m = Consumable.Type.money;
        int cusMoney = CustomerInv.consumableArr[(int)m];

        //consumable section
        if (item.GetType() == typeof(Consumable))
        {
            //Check money first
            Consumable consumable = (Consumable)item;//cast to consumable
            Consumable.Type c = consumable.consumabletype;
            int cost = ConsumablesCostArr[(int)c];

            if (cusMoney >= cost)
            {

               if (VendorInv.consumableArr[(int)c] >= amount)
               {
                   //PURCHASE IS SUCCESFUL
                   Debug.Log("Okay, here you go");
                   VendorInv.consumableArr[(int)c] -= amount;
                   CustomerInv.consumableArr[(int)c] += amount;
                   CustomerInv.consumableArr[(int)m] -= cost;
               }
               else
               {
                   Debug.Log("I dont have that many left, sorry");
               }
            }
            else
            {
                Debug.Log("Looks like you dont have enough money to buy that");
            }
        }
        //if not consumable attempt to add to item array
        else
        {
            VendorInv.RemoveItem(item);
            CustomerInv.gameObject.GetComponent<PlayerInventory>().AddItem(item);//must have weird cast to access playerinv version of method

            //***WIP*** Add methods/classes to dynamically assign costs
            CustomerInv.consumableArr[(int)m] -= 5;
        }
    }

    virtual public void SellItem(Item item, int amount)//create a new item object if needed when calling 
    {
        Consumable.Type m = Consumable.Type.money;
        int cusMoney = CustomerInv.consumableArr[(int)m];

        //consumable section
        if (item.GetType() == typeof(Consumable))
        {
            Consumable consumable = (Consumable)item;//cast to consumable
            Consumable.Type c = consumable.consumabletype;
            int cost = ConsumablesCostArr[(int)c];
            if (CustomerInv.consumableArr[(int)c] >= amount)
            {
                //SUCCESFULL SELL
                Debug.Log("Okay, here you go");
                CustomerInv.consumableArr[(int)c] -= amount;
                VendorInv.consumableArr[(int)c] += amount;
                CustomerInv.consumableArr[(int)m] += cost;
            }
            else
            {
                Debug.Log("I dont have that many!!");
            }
        }
        //if not consumable attempt to add to item array
        else
        {
            Debug.Log(VendorInv);
            VendorInv.AddItem(item);
            Debug.Log("Vendor sellItem method about to remove item from customer inv");
            CustomerInv.gameObject.GetComponent<PlayerInventory>().RemoveItem(item);//must have weird cast to access playerinv version of method
            CustomerInv.consumableArr[(int)m] += 5;
        }

    }







}
