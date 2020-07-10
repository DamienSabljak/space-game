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
        SceneManager.LoadScene("GameplayScene");
    }

    private void LoadShopScene()
    {
        Debug.Log("Loading ShopScene...");
        SceneManager.LoadScene("ShopScene");
    }

    public void LoadMainMenu()
    {
        Debug.Log("Loading main menu...");
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadEndScene()
    {
        Debug.Log("Loading end menu...");
        SceneManager.LoadScene("EndScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
