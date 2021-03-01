using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    public static int PlayerScore=0;
    public static int currentLevelnum = 0;//number of dungeons completed +1


    public void LoadGame()
    {
        SceneManager.LoadScene("GameplayScene");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
