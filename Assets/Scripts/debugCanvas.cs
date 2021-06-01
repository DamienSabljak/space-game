using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;//needed to access UI objects and components 

public class debugCanvas : MonoBehaviour
{
    [SerializeField] Level currentLevel;
    [SerializeField] GameObject LevelText; //displays debug info from level object 
    [SerializeField] GameObject touchArea_left;//left touch area
    [SerializeField] public static debugCanvas currentDebugCanvas;
    string debugLog = "";
    // Start is called before the first frame update
    void Start()
    {
        currentDebugCanvas = this;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("updte debugCanvas");
        DisplayLevelLog();
    }

    private void DisplayLevelLog()
    {
        //remaining enemies
        LevelText.GetComponent<Text>().text = "Level Log: \n" + "Remaining enemies: " + currentLevel.remainingEnemies.Count + "\n";

        //touch position
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            LevelText.GetComponent<Text>().text += "/n Touch Position : " + touch.position;
        }
        LevelText.GetComponent<Text>().text += System.Environment.NewLine + debugLog;

    }

    public void log(string txt)
    {   //method for other classes to call 
        debugLog += System.Environment.NewLine + txt;
    }
}
