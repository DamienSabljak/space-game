using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terminal : MonoBehaviour
{
    [SerializeField] GameObject door;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleInteract()
    {//currently switches parameter on animation controller, opening door 
        door.gameObject.GetComponent<Animator>().SetBool("open", true);
    }
}
