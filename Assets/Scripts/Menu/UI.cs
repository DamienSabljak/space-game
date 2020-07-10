using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;////ADD LIBRARY WHEN NEEDED

public class UI : MonoBehaviour {
    [SerializeField] Text HealthUI;
    [SerializeField] Text AmmoUI;
    [SerializeField] Player Player;
    [SerializeField] ToolBar toolBarCanvas;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        UpdateText();
        UpdateToolBar();
    }

    private void UpdateText()
    {
        Consumable.Type c = Consumable.Type.scrap;
        HealthUI.text = "Health:" + Player.Health;
        c = Consumable.Type.ammo;
        AmmoUI.text = "Ammo:" + Player.inventory.consumableArr[(int)c];
    }

    private void UpdateToolBar()
    {
        int i = 0;
        foreach (Item item in Player.inventory.ToolBarList)
        {
            
            toolBarCanvas.UpdateItemBox(item, i);
            i++;
        }
    }

}
