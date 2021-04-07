using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terminal : MonoBehaviour
{
    [SerializeField] GameObject door;
    [SerializeField] AudioClip openingSFX;//sound to be played when door opens 
    [SerializeField] [Range(0, 1)] float openingSFXVol = 0.7f;
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
        door.gameObject.GetComponent<Animator>().SetBool("open", true);//start opening animation
        AudioSource.PlayClipAtPoint(openingSFX, Camera.main.transform.position, openingSFXVol);//play opening sound
    }
}
