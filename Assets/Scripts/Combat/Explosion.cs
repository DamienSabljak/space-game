using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : DamageDealer {
    [SerializeField] public float BlastRadius = 1;
    [SerializeField] public float GrowthSpeed = 1;

	// Use this for initialization
	void Start () {
        DestroyOnHit = false;//wont be destroyed when touched (from DamageDealrer)
        StartCoroutine(Explode());
    }
	
	// Update is called once per frame
	void Update () {
        
	}

    public IEnumerator Explode()
    {
        Transform transform = this.gameObject.transform;
        Collider2D collider = this.gameObject.GetComponent<Collider2D>();
        //start explosion small
        transform.localScale = new Vector2(0, 0);

        //grow explosion
        while(collider.bounds.max.x - collider.bounds.center.x < BlastRadius)
        {
            //Debug.Log("Explosion GROW");
            transform.localScale = new Vector2(transform.localScale.x + GrowthSpeed*Time.deltaTime, transform.localScale.y + GrowthSpeed*Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        //Shrink explosion
        while (transform.localScale.x >= 0)
        {
            //Debug.Log("Explosion SHRINK");
            transform.localScale = new Vector2(transform.localScale.x - GrowthSpeed * Time.deltaTime, transform.localScale.y - GrowthSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        Destroy(this.gameObject);

    }
}
