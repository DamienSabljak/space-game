using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interactable : Field {
    [SerializeField] public  bool WaitForInput=true;
    [SerializeField] string IntName= "interactable object";
    [SerializeField] GameObject helpText; //this help text will appear when the player is nearby, prompting them 
    
    

    // Use this for initialization
    void Start () {
       
	}

    // Update is called once per frame
    void Update () {
		
	}
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleSteppedOn(other);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        HandleSteppedOff(other);
    }
    void HandleSteppedOn(Collider2D other)
    {
        if(other.gameObject.GetComponent<Player>() != null)
        {
            if (helpText.GetComponent<Text>() != null)
            {
                helpText.GetComponent<Text>().text = "Open Door";
            }
            
        }
    }
    void HandleSteppedOff(Collider2D other)
    {
        if (other.gameObject.GetComponent<Player>() != null)
        {
            if (helpText.GetComponent<Text>() != null)
            {
                helpText.GetComponent<Text>().text = "";
            }

        }
    }

    public void HandleInteractAction(GameObject player)
    {
        if (Parent.gameObject.GetComponent<Terminal>() != null) { Parent.gameObject.GetComponent<Terminal>().HandleInteract(); }
        else if (Parent.gameObject.GetComponent<Vendor>() != null) { Parent.gameObject.GetComponent<Vendor>().StartDialog(player.GetComponent<Player>()); }
    }

}
