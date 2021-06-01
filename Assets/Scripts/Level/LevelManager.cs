using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    //store data between scenes
    public static int PlayerScore=0;
    public static int currentLevelnum = 0;//number of dungeons completed +1
    public static Player player;

    void Awake()//awake is called before start methods
    {

    }
    void Start()
    {
        if(player != null)
        {
            Level.CurrentLevel.currentPlayer = player;//give reference to non destroyed player 
        }
        
    }
    public static void LoadStaticGame()
    {
        SceneManager.LoadScene("GameplayScene");
    }
    public void LoadGame()
    {   //need a non static method for buttons (buttons will not see static methods) 
        SceneManager.LoadScene("GameplayScene");
    }
    public static void LoadShopScene()
    {
        Debug.Log("Loading ShopScene...");
        SceneManager.LoadScene("ShopScene");
    }

    public static void LoadStaticMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void LoadMainMenu()
    {   //need a non static method for buttons (buttons will not see static methods) 
        SceneManager.LoadScene("MainMenu");
    }
    public static void LoadEndScene()
    {
        Debug.Log("Loading end menu...");
        SceneManager.LoadScene("EndScene");
    }

    public static void QuitGame()
    {
        Application.Quit();
    }

    public static void SavePlayerData()
    {//save data before opening a new scene
        Debug.Log("saving player data...");
        if(player == null)
        {
            player = Level.CurrentLevel.currentPlayer;//creates copy 
            DontDestroyOnLoad(Level.CurrentLevel.currentPlayer);
        }
        
    }

}
