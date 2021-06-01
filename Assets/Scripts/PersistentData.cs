using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentData : MonoBehaviour
{
    // Start is called before the first frame update
    private static string scrapKey = "scrap";
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static void IncrementScrapAmount(int deltaScrap)
    {   //changes and saves persistent scrap amount by deltaScrap
        if(PlayerPrefs.HasKey(scrapKey))
        {
            PlayerPrefs.SetInt(scrapKey, PlayerPrefs.GetInt(scrapKey) + deltaScrap);
        }
        else
        {
            SetScrapAmount(deltaScrap);
        }
        
    }
    public static void SetScrapAmount(int newScrap)
    {   //sets saved scrap amount to newScrap
        PlayerPrefs.SetInt(scrapKey, newScrap);
    }

    public static int GetScrapAmount()
    {
        if(!PlayerPrefs.HasKey(scrapKey))
        {
            Debug.Log("WARNING: SCRAPKEY HAS NOT PREVIOUSLY BEEN SET");
            return 0;
        }
        return PlayerPrefs.GetInt(scrapKey);
    }
}
