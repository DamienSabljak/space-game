using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour {
    //Abstract class used to detect presence and then make calls to parent object
    public GameObject Parent;

    // Use this for initialization
    void Start () {
        InitField();
    }

    private void InitField()
    {
        Parent = this.gameObject.transform.parent.gameObject;//find out what the interactable is attatched to 
    }

    // Update is called once per frame
    void Update () {
		
	}
}
