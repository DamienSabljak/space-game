using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour {
    [SerializeField] Text ShopCount;
    [SerializeField] Text CustomerCount;
    [SerializeField] Text Cost;
    [SerializeField] public Consumable.Type PanelItemType;//Stores the consumable type being displayed in this panel
    //****CURRENTLY ONLY SET UP TO ACCEPT CONSUMABLES****
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdatePanel(Vendor CurrentVendor)
    {
        
        Consumable.Type c = PanelItemType;
        int shopcount = CurrentVendor.VendorInv.consumableArr[(int)c];
        ShopCount.text = "" + shopcount;

        int customercount = CurrentVendor.CustomerInv.consumableArr[(int)c];
        CustomerCount.text = "" + customercount;

        Cost.text = "$" + CurrentVendor.ConsumablesCostArr[(int)c];
        
    }

    public void BUTPurchaseItem(int amount)//method for using buttons to purchase 
    {
        Consumable.Type type = PanelItemType;
        ShopMenu.CurrentVendor.PurchaseItem(new Consumable(type), amount);
    }
    public void BUTSellItem(int amount)//method for using buttons to purchase 
    {
        Consumable.Type type = PanelItemType;
        ShopMenu.CurrentVendor.SellItem(new Consumable(type), amount);
    }

}
