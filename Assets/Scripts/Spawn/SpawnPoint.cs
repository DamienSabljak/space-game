using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour {
    [SerializeField] GameObject spawn;//Character to spawn 
    [SerializeField] bool showSpawnPointOnRuntime = false;
    // Use this for initialization
    void Start()
    {
        SpawnGameObject();
        if (!showSpawnPointOnRuntime) {
            SpriteRenderer sprite = this.gameObject.GetComponent<SpriteRenderer>();
            sprite.color = new Color(1f, 0.1f, 0.1f, 0);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SpawnGameObject()
    {
        GameObject spawnedCharacter;

        if (spawn != null)
            spawnedCharacter = Instantiate(spawn.gameObject, this.transform.position, Quaternion.identity);
        else
            Debug.Log("warning: " + spawn + " is empty and so spawnpoint will not spawn anything");
    }


}
