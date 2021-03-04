using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GateWay : MonoBehaviour {
    //Gateway is intended to be like Buttons to access scenes/ LevelManager, but with inputs from collision
    [Header("Destination")]
    [SerializeField] bool Level1;
    [SerializeField] bool ShopScene;
    [SerializeField] bool MainMenu;
    [SerializeField] bool EndScene;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Level1)
            LoadGame();
        if (ShopScene)
            LoadShopScene();
        if (MainMenu)
            LoadMainMenu();
        if (EndScene)
            LoadEndScene();

    }

    public void LoadGame()
    {
        Debug.Log("Loading level 1...");
        LevelManager.SaveData();//persist info to next scene
        LevelManager.LoadGame();
    }

    private void LoadShopScene()
    {
        LevelManager.SaveData();//persist info to next scene
        Debug.Log(LevelManager.player);
        LevelManager.LoadShopScene();
    }

    public void LoadMainMenu()
    {
        LevelManager.LoadMainMenu();
    }

    public void LoadEndScene()
    {
        LevelManager.LoadEndScene();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
