﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;////ADD LIBRARY WHEN NEEDED

public class UI : MonoBehaviour {
    [SerializeField] Text HealthUI;
    [SerializeField] Text ammoUI;
    [SerializeField] Player player;
    [SerializeField] ToolBar toolBarCanvas;
    [SerializeField] GameObject alertText;
    // Use this for initialization
    void Start () {
		if(LevelManager.player != null)//reference player from previous scene 
        {
            player = LevelManager.player;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        UpdateText();
        UpdateToolBar();
    }

    private void UpdateText()
    {
        Consumable.Type c = Consumable.Type.SCRAP;
        HealthUI.text = "Health:" + player.Health;
        c = Consumable.Type.AMMO;
        ammoUI.text = "Ammo:" + player.inventory.consumableArr[(int)c];
    }

    private void UpdateToolBar()
    {
        int i = 0;
        foreach (Item item in player.inventory.ToolBarList)
        {
            
            toolBarCanvas.UpdateItemBox(item, i);
            i++;
        }
    }

    public void DisplayGameAlert(string message)
    {//displays message and then fades away 
        alertText.GetComponent<Text>().text = message;
        alertText.GetComponent<Animator>().SetTrigger("at_fade");//starts fade animation
        Debug.Log("UI: displaying alert...");
        
    }
}
