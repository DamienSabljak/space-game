using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item {
    [HideInInspector] public  Player Parent; //refference to Player holding weapon
    private Inventory inventory;
    [SerializeField] GameObject projectile;
    [SerializeField] bool InfiniteAmmo = false;
    [SerializeField] public int Damage = 100;//will override damage dealer of missle leaving weapon
    [SerializeField] float projSpeed = 10f;
    [SerializeField] float fireRate = 0.5f;
    [SerializeField] float shotCounter = 0;
    [SerializeField] AudioClip fireSFX;
    [SerializeField] [Range(0, 1)] float fireSFXVol = 0.25f;
    Coroutine firingCoroutine;

    // Use this for initialization
    void Start () {
        InitWeapon();
	}

    private void InitWeapon()//WIP need to choose storage for weapon object
    {
        
    }

    // Update is called once per frame
    void Update () {
		
	}

    //used for player/inventory calls to update current weapon
    public void UpdateParent(Player player)
    {
        Parent = player;
        inventory = Parent.gameObject.GetComponent<Inventory>();
    }

    public void Fire()
    {
        if (shotCounter == 0)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                firingCoroutine = StartCoroutine(FireContinously());
                shotCounter = fireRate;
            }

        }
        if (Input.GetButtonUp("Fire1"))
        {
            StopCoroutine(firingCoroutine);

        }
        shotCounter -= Time.deltaTime;
        if (shotCounter < 0)
            shotCounter = 0;
    }

    private IEnumerator FireContinously()
    {
        while (true)
        {
            Consumable.Type a = Consumable.Type.ammo;
            //Debug.Log(Parent);
            //Debug.Log(inventory);
            if (Parent.InfiniteAmmo || inventory.consumableArr[(int)a] > 0)
            {
                //Remove Missle from inventory 
                if (!InfiniteAmmo)
                {
                    inventory.consumableArr[(int)a] -= 1;
                }
                //Missle Instantiation
                GameObject Missle = Instantiate(projectile, //IMPORTANT change position if refferencing weapon withi no transform
                       Parent.gameObject.transform.position, Quaternion.identity)
                       as GameObject;
                //set new damage value
                if(Missle.GetComponent<DamageDealer>())//projectile could be non damage dealing, such as grenade
                Missle.GetComponent<DamageDealer>().Damage = Damage;

                //determine direction
                float xspeed = 0, yspeed = 0;
                switch (Parent.facing)
                {
                    case Character.direction.LEFT:
                        xspeed = -1 * projSpeed;
                        yspeed = 0;
                        break;
                    case Character.direction.RIGHT:
                        xspeed = projSpeed;
                        yspeed = 0;
                        break;
                    case Character.direction.UP:
                        xspeed = 0;
                        yspeed = projSpeed;
                        break;
                    case Character.direction.DOWN:
                        xspeed = 0;
                        yspeed = -1 * projSpeed;
                        break;
                }
                //launch missle
                Missle.GetComponent<Rigidbody2D>().transform.Translate(new Vector2(0,0));//set location
                //Missle.GetComponent<Rigidbody2D>().velocity = new Vector2(xspeed, yspeed);//set velocity //uncomment for cartesian firing mode
                Missle.GetComponent<Rigidbody2D>().velocity = Vector3.Normalize(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position) * projSpeed;

                if (fireSFX !=null)
                AudioSource.PlayClipAtPoint(fireSFX, Camera.main.transform.position, fireSFXVol);

                yield return new WaitForSeconds(fireRate);
            }
            else
            {
                Debug.Log("Not enough ammo!");
                yield return new WaitForSeconds(fireRate);
            }
        }

    }



}
