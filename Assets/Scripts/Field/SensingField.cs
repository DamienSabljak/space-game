using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensingField : Field {
    [SerializeField] public Mode FieldType;

    public enum Mode
    {
        Detecting, Firing,
    }
    // Use this for initialization
    void Start () {
	}



    // Update is called once per frame
    void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Determine type of character subclass field attatched to 
        Character character1;
        if (Parent.GetComponent<Enemy>() != null)
        {
            
            character1 = Parent.GetComponent<Enemy>();
            Enemy enemy = (Enemy)character1;
            enemy.HandleFieldEnter(other.gameObject, FieldType);
        }
        else
        {
            character1 = Parent.GetComponent<Character>();
            Character character = character1;
            character.HandleField();
        }




    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //Determine type of character subclass field attatched to 
        Character character1;
        if (Parent.GetComponent<Enemy>() != null)
        {
            character1 = Parent.GetComponent<Enemy>();
            Enemy enemy = (Enemy)character1;
            enemy.HandleFieldExit(other.gameObject, FieldType);
        }
        else
        {
            character1 = Parent.GetComponent<Character>();
            Character character = character1;
            character.HandleFieldExit();
        }
    }



}
