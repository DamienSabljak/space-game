using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour {

    [SerializeField] float scrollSpeed = 0.5f;
    Material material;
    Vector2 offset;


	// Use this for initialization
	void Start () {
        material = GetComponent<Renderer>().material;
        offset = new Vector2(0f, scrollSpeed);
		
	}
	
	// Update is called once per frame
	void Update () {
        material.mainTextureOffset += offset * Time.deltaTime;
	}
}
