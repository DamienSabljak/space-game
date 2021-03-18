using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumable : Item {
    //****class Statics****
    public enum Type
    {//list of consumables
        scrap,
        ammo,
        money, 
        health
    };
    public static int numTypes = System.Enum.GetNames(typeof(Type)).Length;
  
    //****object attributes****
    public Type consumabletype;
    
    
    //****constructors****
    public Consumable(int amt, Type type)
    {
        amount = amt;
        consumabletype = type;
    }

    public Consumable(Type type)
    {
        consumabletype = type;
    }


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
