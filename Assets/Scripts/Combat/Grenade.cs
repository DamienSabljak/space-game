using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour {
    [SerializeField] float FuseTime;
    [SerializeField] float BlastRadius;
    [SerializeField] GameObject Explosion;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //count down fuse
        FuseTime -= Time.deltaTime;
        if (FuseTime <= 0)
        {
            Explode();
        }

	}

    private void Explode()
    {
        GameObject explosion = Instantiate(Explosion, this.transform.position, Quaternion.identity);
        explosion.GetComponent<Explosion>().BlastRadius = BlastRadius;

        //set layer to this layer
        explosion.gameObject.layer = this.gameObject.layer;

        Destroy(this.gameObject);
    }
}
