using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour {
    [SerializeField] public Text itemCost;
    [SerializeField] public Text itemName;
    [SerializeField] public Image itemImage;
    [SerializeField] public Button buybutton;
    [SerializeField] public Item item;//reference to prefab this panel represents 

    public bool hasBeenSold = false;
    //****CURRENTLY ONLY SET UP TO ACCEPT CONSUMABLES****
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


}
