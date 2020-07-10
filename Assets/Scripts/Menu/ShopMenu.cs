using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;//ADD LIBRARY WHEN NEEDED

public class ShopMenu : Menu {
    [SerializeField] Text ShopCount;
    [SerializeField] Text CustomerCount;
    [SerializeField] Text Cost;
    [SerializeField] Text CustomerMoney;
    [SerializeField] GameObject PanelsParent;
    private ShopPanel[] PanelArr;

    public static Vendor CurrentVendor;//pointer to current vendor for button and update purposes

    // Use this for initialization
    void Start ()
    {
        InitPanelArr();
    }

    private void InitPanelArr()
    {
        PanelArr = new ShopPanel[PanelsParent.transform.childCount];
        for (int i = 0; i < PanelsParent.transform.childCount; i++)
        {
            PanelArr[i] = PanelsParent.transform.GetChild(i).GetComponent<ShopPanel>();
        }
    }

    // Update is called once per frame
    void Update () {
        //**MAY cause lag in future**
        if (CurrentVendor != null)//IMPORTANT remove refference after youre done talking to vendor
        {
            //show customer credits
            Consumable.Type m = Consumable.Type.money;
            int customermoney = CurrentVendor.CustomerInv.consumableArr[(int)m];
            CustomerMoney.text = "Credits: " + customermoney;

            //update panels
            for(int i = 0; i< PanelArr.Length; i++)
            {
                PanelArr[i].UpdatePanel(CurrentVendor);
            }

        }
    }

}
