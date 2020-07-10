using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldText : MonoBehaviour {
    //Class is used to update text attatched to in game objects
    [SerializeField] Character character;
    [SerializeField] Text text; 

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void dispStandingOn()
    {
        if (character.standingOn == null)
            text.text = "";
        else
        text.text = "standing on: " + character.standingOn.name;
        
    }

}
