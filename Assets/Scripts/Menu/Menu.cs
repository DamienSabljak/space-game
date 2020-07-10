using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour {
    public static GameObject SelectedItem;//paceholder for item currently selected in any menu
    public static Menu SelectedItemMenu;//holds information about what menu the selected item came from


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void RefreshMenu()
    {
        Debug.Log("refresh for a general method class not implemented");
    }
}
