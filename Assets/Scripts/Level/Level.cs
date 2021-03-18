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
    [SerializeField] public Canvas vendorCanvas;
    [SerializeField] public Canvas InventoryCanvas;
    [SerializeField] Level  levelstart;
    [SerializeField] public Player currentPlayer;
    [SerializeField] public Vendor currentVendor;
    [SerializeField] public static Level CurrentLevel;//stores current level object (not level number)
    [SerializeField] public bool isShopLevel;//used to determine if this scene is a shop level
    public List<GameObject> remainingEnemies;//reference to enemies spawned this level (only counted if spawned from spawnManager)
    bool enemiesDefeated = false;//used as flag for text alert (prevent repetetive alerts) 
    Canvas menucanvas = null;

    void Start()
    {
        CurrentLevel = levelstart;
        ResumeGame();
        HandleShopLevelStart();
        
    }

    void Update()
    {
        HandlePause();
        HandleRemainingEnemies();
    }
    private void HandleShopLevelStart()
    {
        if (isShopLevel)
        {
            gameCanvas.gameObject.GetComponent<UI>().DisplayGameAlert("Shop - purchase items...");
        }
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
    private void HandleRemainingEnemies()
    {
        if(!isShopLevel && remainingEnemies.Count <= 0 && SpawnManager.EnemiesHaveBeenSpawned && !enemiesDefeated)
        {
            Debug.Log("Displaying enemies defeated alert..");
            gameCanvas.gameObject.GetComponent<UI>().DisplayGameAlert("All Enemies Defeated!\n escape before help arrives");
            enemiesDefeated = true;
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
        vendorCanvas.gameObject.SetActive(false);
        pause = false; 
    }

    public void OpenVendorMenu()
    {
        Time.timeScale = 0;
        CurrentLevel.vendorCanvas.gameObject.SetActive(true);
        //CurrentLevel.gameCanvas.gameObject.SetActive(false);
        //VendorCanvas.GetComponent<ShopMenu>().RefreshMenu();
        pause = true;
    }

    public void CloseVendorMenu()
    {
        Debug.Log("Closing the vendor menu...");
        Time.timeScale = 1;
        CurrentLevel.vendorCanvas.gameObject.SetActive(false);
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
