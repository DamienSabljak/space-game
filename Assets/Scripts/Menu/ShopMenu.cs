using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;//ADD LIBRARY WHEN NEEDED

public class ShopMenu : Menu
{
    [SerializeField] Text ShopCount;
    [SerializeField] List<GameObject> itempanels;//list referencing each item purchasing object
    [SerializeField] Text buyAmmoText;//points to COST text (number only)
    [SerializeField] int ammoPerPurchase;//determines amount of ammo to give per ammo purchase 
    [SerializeField] Text buyHealthText;//points to COST text (number only)
    [SerializeField] Player player;
    [SerializeField] Vendor vendor;//vendor attatched to this menu 
    bool debug_freeStuff = true;//used to make things cost nothing 
    private ShopPanel[] PanelArr;

    public static Vendor CurrentVendor;//pointer to current vendor for button and update purposes

    // Use this for initialization
    void Start()
    {
        ReferencePersistentPlayer();
        InitItemPanels();

    }

    private void ReferencePersistentPlayer()
    {
        if (LevelManager.player != null)
        {
            player = LevelManager.player;
        }
    }

    private void InitItemPanels()
    {
        int panelIndex = 0;
        foreach (GameObject panel in itempanels)
        {
            panel.GetComponent<ShopPanel>().itemName.text = vendor.availableItems[panelIndex].name;
            panel.GetComponent<ShopPanel>().itemCost.text = vendor.availableItems[panelIndex].GetComponent<Item>().baseCost.ToString();
            panel.GetComponent<ShopPanel>().itemImage.sprite = vendor.availableItems[panelIndex].GetComponent<SpriteRenderer>().sprite;
            panelIndex++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        buyAmmoText.text   = vendor.ConsumablesCostArr[(int)Consumable.Type.ammo].ToString();
        buyHealthText.text = vendor.ConsumablesCostArr[(int)Consumable.Type.health].ToString();
    }

    public void OnBuyItem(int itemPos)
    {   //handles the buy button on item panels
        Debug.Log("OnBuyItem method called");
        int itemCost = int.Parse(itempanels[itemPos].GetComponent<ShopPanel>().itemCost.text);//pull from UI to make sure value is always what player sees 
        if (itemCost <= vendor.CustomerInv.consumableArr[(int)Consumable.Type.money] || debug_freeStuff)
        {   //remove money and add item
            vendor.CustomerInv.consumableArr[(int)Consumable.Type.money] -= itemCost;
            ((PlayerInventory)vendor.CustomerInv).AddItem(Instantiate(itempanels[itemPos].GetComponent<ShopPanel>().item));//cast to override parent method 
            //disable button and change appearance of panel
            itempanels[itemPos].GetComponent<ShopPanel>().buybutton.interactable = false;
            itempanels[itemPos].GetComponent<ShopPanel>().itemCost.text = "-SOLD-";
        }
        else { Debug.Log("Not enough money to purchase that item!"); }

    }

    public void OnBuyConsumable(string consumable_string)
    {//handles the buy button on consumables 
        //convert string to enum
        Consumable.Type consumableType = Consumable.Type.money;//should never be money 
        if      (consumable_string == "ammo") {   consumableType = Consumable.Type.ammo; }
        else if (consumable_string == "health") { consumableType = Consumable.Type.health; }
        //determine cost
        Debug.Log("OnBuyConsumable method called");
        int itemCost = 0;
        if (consumableType == Consumable.Type.ammo)
        {
            itemCost = int.Parse(buyAmmoText.text);//pull from UI to make sure value is always what player sees 
        }
        else if(consumableType == Consumable.Type.health)
        {
            itemCost = int.Parse(buyHealthText.text);//pull from UI to make sure value is always what player sees 
        }
        else { Debug.Log("Warning: onBuyConsumable() couldnt determine item cost, setting it to free!");}

        if (itemCost <= vendor.CustomerInv.consumableArr[(int)Consumable.Type.money] || debug_freeStuff)
        {
            //remove money and add consumables 
            Debug.Log("adding consumable");
            Debug.Log(Consumable.numTypes);
            vendor.CustomerInv.consumableArr[(int)Consumable.Type.money] -= itemCost;
            if(consumableType == Consumable.Type.ammo) { vendor.CustomerInv.consumableArr[(int)consumableType] += ammoPerPurchase; }
            else if (consumableType == Consumable.Type.health) { ((Player) vendor.Customer).Health += 10; }
            else                                       { vendor.CustomerInv.consumableArr[(int)consumableType] += 1; }
            
        }
        else { Debug.Log("Not enough money to purchase that item!"); }

    }









}//end class
