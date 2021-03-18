using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {
    [SerializeField] public int amount;
    [SerializeField] public int baseCost;//cost base to be used in transactions, affected by multipliers

    private void OnTriggerEnter2D(Collider2D other)
    {
        
    }

    public bool isConsumable()
    {
        if (this.GetType() == typeof(Consumable))
        {
            return true;
        }
        else
            return false;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
