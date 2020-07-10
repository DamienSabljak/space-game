using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;//USE WHEN NEEDED

public class EndSceneHandler : MonoBehaviour {
    [SerializeField] private Text YourScore;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        YourScore.text = "Your Score:" + LevelManager.PlayerScore;
		
	}
}
