using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour {
    public GameObject Contents;//the Item this box contains
    [SerializeField] public GameObject MenuParent;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //will be called by button component on itembox when clicked
    public void SelectItem()
    {
        Debug.Log("ItemBox Clicked! Item is:" + Contents);

        //select item if none already selected
        if (Menu.SelectedItem == null)
        {
            Debug.Log("Item has been selected: " + Contents);
            Menu.SelectedItem = this.Contents;

            //Assign menu type
            if (MenuParent.GetComponent<Menu>().GetType() == typeof(PlayerInventoryMenu))
            {
                Menu.SelectedItemMenu = new PlayerInventoryMenu();
            }
            else if (MenuParent.GetComponent<Menu>().GetType() == typeof(VendorInventoryMenu))
            {
                Menu.SelectedItemMenu = new VendorInventoryMenu();
            }
        }

        else
            ViewportClick();

    }

    //will be called by event system when mouse enters area
    public void MouseHover()
    {
        //Debug.Log("Mouse is over ITEM Box!: "+ Contents);
    }

    //will be called by event system on viewport
    public void ViewportClick()
    {
        Debug.Log("ViewPortClicked!");

        if (Menu.SelectedItem != null)
        {
            //do actions based on previous and next menu
            //PlayerMenu -> VendorMenu - sell
            if (Menu.SelectedItemMenu.GetType() == typeof(PlayerInventoryMenu) && MenuParent.GetComponent<Menu>().GetType() == typeof(VendorInventoryMenu))
            {
                //sell item
                Debug.Log("selling item from UI interaction");
                Level.CurrentLevel.currentVendor.SellItem(Menu.SelectedItem.GetComponent<Item>(), 1);
                //refresh menus as change has occured
                Level.CurrentLevel.vendorCanvas.GetComponent<VendorInventoryMenu>().RefreshMenu();
                Level.CurrentLevel.InventoryCanvas.GetComponent<PlayerInventoryMenu>().RefreshMenu();

            }
            //VendorMenu -> PlayerMenu - buy
            else if (Menu.SelectedItemMenu.GetType() == typeof(VendorInventoryMenu) && MenuParent.GetComponent<Menu>().GetType() == typeof(PlayerInventoryMenu))
            {
                //buy item
                Debug.Log("buying item from UI interaction");
                Level.CurrentLevel.currentVendor.PurchaseItem(Menu.SelectedItem.GetComponent<Item>(), 1);
                //refresh menus as change has occured
                Level.CurrentLevel.vendorCanvas.GetComponent<VendorInventoryMenu>().RefreshMenu();
                Level.CurrentLevel.InventoryCanvas.GetComponent<PlayerInventoryMenu>().RefreshMenu();
            }
            Menu.SelectedItem = null;
            Menu.SelectedItemMenu = null;
        }//end if 
    }
   
}
