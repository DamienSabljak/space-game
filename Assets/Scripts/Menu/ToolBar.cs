using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolBar : MonoBehaviour {
    public List<GameObject> ItemBoxList;
    [SerializeField] GameObject player; //used for referencing players stats
    [SerializeField] GameObject scrapCount;
    [SerializeField] GameObject moneyCount;


    // Use this for initialization
    void Start () {
        InitItemBoxList();
        if(LevelManager.player != null)
        {
            Debug.Log("referencing persistent player");
            player = LevelManager.player.gameObject;
        }
	}

    private void InitItemBoxList()
    {
        //add each Item box to list for accessing
        foreach (Transform child in this.transform)
        {
            if (child.gameObject.name.Contains("ItemBox"))//***ItemBox MUST have name in editor***
            {
                ItemBoxList.Add(child.gameObject);
                //child.transform.GetChild(0).gameObject.SetActive(false); //set all item sprites empty if wanted
            }
        }
    }

    // Update is called once per frame
    void Update () {
        UpdateScrapCount();
        UpdateMoneyCount();

    }

    public void UpdateItemBox(Item item, int index)
    {
        Image boxImage = ItemBoxList[index].transform.GetChild(0).GetComponent<Image>();
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
    
    public void UpdateScrapCount()
    {
        scrapCount.GetComponent<Text>().text = player.GetComponent<PlayerInventory>().consumableArr[(int) Consumable.Type.scrap].ToString();
    }
    public void UpdateMoneyCount()
    {
        moneyCount.GetComponent<Text>().text = player.GetComponent<PlayerInventory>().consumableArr[(int)Consumable.Type.money].ToString();
    }


}
