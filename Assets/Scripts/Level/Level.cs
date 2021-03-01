using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour {
    //This class is intended to be used for 
    //each individual level, holding data related to the current level
    static public bool pause = false;
    [SerializeField] public Canvas pauseCanvas;
    [SerializeField] public Canvas gameCanvas;
    [SerializeField] public Canvas VendorCanvas;
    [SerializeField] public Canvas InventoryCanvas;
    [SerializeField] Level  levelstart;
    [SerializeField] public Player currentPlayer;
    [SerializeField] public Vendor currentVendor;
    [SerializeField] public static Level CurrentLevel;//stores current level object (not level number)
    public int remainingEnemies;//reference to how many enemies are left on this level 
    Canvas menucanvas = null;

    void Start()
    {
        CurrentLevel = levelstart;
        ResumeGame();
        
    }

    void Update()
    {
        HandlePause();
    }

    private void HandlePause()
    {
        if (Input.GetButtonDown("Pause"))
        {
            if (pause == true)
                ResumeGame();
            else
                PauseGame();
        }
    }

    //****BUTTON METHODS****
    public void PauseGame()
    {
        Time.timeScale = 0;
        gameCanvas.gameObject.SetActive(false);
        pauseCanvas.gameObject.SetActive(true);
        pause = true;
    }
    public void ResumeGame()
    {
        Time.timeScale = 1; //normal time scale is 1
        gameCanvas.gameObject.SetActive(true);
        pauseCanvas.gameObject.SetActive(false);
        pause = false; 
    }

    public void OpenVendorMenu()
    {
        Time.timeScale = 0;
        CurrentLevel.VendorCanvas.gameObject.SetActive(true);
        //CurrentLevel.gameCanvas.gameObject.SetActive(false);
        VendorCanvas.GetComponent<VendorInventoryMenu>().RefreshMenu();
        pause = true;
    }

    public void CloseVendorMenu()
    {
        Debug.Log("Closing the vendor menu...");
        Time.timeScale = 1;
        CurrentLevel.VendorCanvas.gameObject.SetActive(false);
        CurrentLevel.gameCanvas.gameObject.SetActive(true);
        pause = false;
    }

    public void OpenInventoryMenu()
    {
        Debug.Log("Opening the Inventory menu...");
        Time.timeScale = 0;
        CurrentLevel.InventoryCanvas.gameObject.SetActive(true);
        InventoryCanvas.GetComponent<PlayerInventoryMenu>().RefreshMenu();
        pause = true;
        //CurrentLevel.gameCanvas.gameObject.SetActive(false);
    }

    public void CloseInventoryMenu()
    {
        Debug.Log("Closing the Inventory menu...");
        Time.timeScale = 1;
        CurrentLevel.InventoryCanvas.gameObject.SetActive(false);
        CurrentLevel.gameCanvas.gameObject.SetActive(true);
        pause = false;
    }

    public void LoadStartMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
