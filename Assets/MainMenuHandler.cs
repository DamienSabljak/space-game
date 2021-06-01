using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] Text ScrapReading;//reference to black market scrap reading    
    // Start is called before the first frame update
    void Start()
    {
        ScrapReading.text = PersistentData.GetScrapAmount().ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
