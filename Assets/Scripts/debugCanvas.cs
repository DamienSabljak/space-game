using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;//needed to access UI objects and components 

public class debugCanvas : MonoBehaviour
{
    [SerializeField] Level currentLevel;
    [SerializeField] GameObject LevelText; //displays debug info from level object 
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("updte debugCanvas");
        DisplayLevelLog();
    }

    private void DisplayLevelLog()
    {
        LevelText.GetComponent<Text>().text = "Level Log: \n" + "Remaining enemies: " + currentLevel.remainingEnemies.Count + "\n";
        //LevelText.GetComponent<Text>().text = "poop";
    }
}
