using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryField : Field
{   //class for boundary objects to detect when player is out of bounds 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Determine type of character subclass field attatched to 
        Character character = other.GetComponent<Character>();
        if ( character!= null)
        {   //make character fall off field 
            character.isGrounded = false;//prevent walking 
            StartCoroutine(character.FallOffMap());
            
        }

    }
}
