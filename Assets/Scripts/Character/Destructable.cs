using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : Character {
    [SerializeField] private float Health = 100;
    [SerializeField] GameObject ItemDrop;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Destructable collided w something");
        DamageDealer damageDealer = other.gameObject.GetComponent<DamageDealer>();
        if (damageDealer != null)
        {
            Debug.Log("Destructable dealing w damage");
            HandleDamage(damageDealer);
        }


    }

    public void HandleDamage(DamageDealer damageDealer)
    {
        StartCoroutine(FlashColour());
        //Debug.Log("shouldve flashed");

        Health -= damageDealer.Damage;

        if (Health <= 0)
            Die();
    }

    private void Die()
    {
        //Destroy enemy
        Destroy(gameObject);
        
        if (ItemDrop != null)
        {
            SpawnItem(ItemDrop.GetComponent<Item>());
        }

    }
}
