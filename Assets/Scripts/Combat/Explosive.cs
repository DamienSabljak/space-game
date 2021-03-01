using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : Character {
    [SerializeField] int Health = 100;
    [SerializeField] GameObject Explosion;
    [SerializeField] GameObject ObjectDrop;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.GetComponent<DamageDealer>() != null)
        {
            Health -= other.GetComponent<DamageDealer>().getDamage();
            if (Health <= 0)
                Die();
        }
    }

    private void Die()
    {
        //Instantiate object
        GameObject explosion = Instantiate(
        Explosion,
        transform.position,
        Quaternion.identity)
        as GameObject;
        Destroy(explosion.GetComponent<Rigidbody2D>());

        //Drop object
        if (ObjectDrop != null)
        {
            SpawnObject(ObjectDrop);
        }

        Destroy(this.gameObject);  
    }
}
