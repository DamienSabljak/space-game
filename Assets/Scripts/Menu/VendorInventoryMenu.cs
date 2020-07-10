using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VendorInventoryMenu : Menu {
    Vendor Vendor;
    Inventory VendorInventory;
    [SerializeField] GameObject ItemBox;//template itembox to be used 
    [SerializeField] GameObject FirstItemBox;//first box placed, used as position refference
    [SerializeField] GameObject ViewportContent;
    [SerializeField] float ItemBoxWidth;//used to set offsets of next itembox's
    [SerializeField] float ItemBoxHeight;
    [SerializeField] int BoxesPerRow = 11;//used to tell when a new itembox row is needed
    public List<GameObject> ItemBoxList = new List<GameObject>();//holds refferences to all item boxes


    // Use this for initialization
    void Start()
    {
        RefreshMenu();
    }

    new public void RefreshMenu()
    {
        Vendor = Level.CurrentLevel.currentVendor;
        VendorInventory = Vendor.GetComponent<Inventory>();
        LoadItems();
    }

    private void LoadItems()
    {
        int col = 0, row = 0; //pointers to grid locations
        foreach (Item item in VendorInventory.ItemList)
        {
            //create new item box
            GameObject itemBox = Instantiate(ItemBox, this.transform.position, Quaternion.identity);//fix this later
            if (item != null)
                itemBox.GetComponent<ItemBox>().Contents = item.gameObject;
            itemBox.GetComponent<ItemBox>().MenuParent = this.gameObject;

            //set parent
            itemBox.transform.parent = ViewportContent.transform;
            itemBox.transform.localScale = new Vector3(1, 1, 1);//fix scaling issue caused by parent swap 

            //set position of box
            itemBox.transform.localPosition = FirstItemBox.transform.localPosition + new Vector3(ItemBoxWidth, 0, 0) * col - new Vector3(0, ItemBoxHeight, 0) * row;
            if (col + 1 >= BoxesPerRow)
            {
                col = 0;
                row++;
            }
            else
            {
                col++;
            }

            //add to list for tracking
            ItemBoxList.Add(itemBox);
            //update picture (if not an empty spot)
            if (item != null)
            {
                UpdateItemBox(item, itemBox);
            }
        }
    }


    // Update is called once per frame
    void Update()
    {

    }

    private void UpdateItemBox(Item item, GameObject ItemBox)
    {

        Image boxImage = ItemBox.transform.GetChild(0).GetComponent<Image>();
        //clear box if null sent 
        if (item == null)
        {
            boxImage.gameObject.SetActive(false);
        }
        //access image inside item box representing item
        else
        {
            boxImage.sprite = item.GetComponent<SpriteRenderer>().sprite;
            boxImage.gameObject.SetActive(true);
        }
    }

    public void OnMouseDown()
    {
        Debug.Log("menu Clicked!");

    }
}
