using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character {

    [SerializeField] int health = 100;
    [Header("AI")]
    [SerializeField] float FiringFieldMargin=0.1f;//how much furhter into the field to go for effective fire 
    [Header("Projectile")]
    [SerializeField] GameObject projectile;
    [SerializeField] float projSpeed = 5f;
    [SerializeField] float shotCounter;
    [SerializeField] float minShotTime = 0.2f;
    [SerializeField] float maxShotTime = 3f;
    [SerializeField] AudioClip fireSFX;
    [SerializeField] [Range(0, 1)] float fireSFXVol = 0.25f;
    [Header("DeathFX")]
    [SerializeField] GameObject DeathVFX;
    [SerializeField] float deathTime=1f;
    [SerializeField] AudioClip deathSFX;
    [SerializeField] [Range(0,1)] float deathSFXVol = 0.7f;
    [SerializeField] GameObject ItemDrop;
    public bool isShooting = false;
    Transform currentTarget;
    Coroutine ChaseTargetCoroutine;


    void Start()
    {
        shotCounter = UnityEngine.Random.Range(minShotTime, maxShotTime);


    }

    void Update()
    {
        if (isShooting)
        {
            //make sure walls arent in the way
            var layerMask = (1 << 8) + (1 << 16);//only look at player and wall layers
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.Normalize(currentTarget.transform.position - transform.position), 10000, layerMask);
            //Debug.Log(hit.transform.position);
            if (hit.transform.gameObject.GetComponent<Player>() != null)
            {
                CountDownAndShoot();
            }
        }
    }
    //****COMBAT****

    public void CountDownAndShoot()
    {
        shotCounter -= Time.deltaTime;
        if(shotCounter <= 0f)
        {
            Fire(currentTarget);
            shotCounter = UnityEngine.Random.Range(minShotTime, maxShotTime);
        }
    }

    private void Fire()
    {
        //create missile
        GameObject Missle = Instantiate(
            projectile,
            transform.position, //must lower starting point tostop from killing itself
            Quaternion.identity)
            as GameObject;

        //determine direction
        float xspeed = 0, yspeed = 0;
        switch (facing)
        {
            case direction.LEFT:
                xspeed = -1 * projSpeed;
                yspeed = 0;
                break;
            case direction.RIGHT:
                xspeed = projSpeed;
                yspeed = 0;
                break;
            case direction.UP:
                xspeed = 0;
                yspeed = projSpeed;
                break;
            case direction.DOWN:
                xspeed = 0;
                yspeed = -1 * projSpeed;
                break;
        }

        Missle.GetComponent<Rigidbody2D>().transform.Translate(new Vector2(0,0));//set location
        Missle.GetComponent<Rigidbody2D>().velocity = new Vector2(xspeed, yspeed);//set velocity
        //Audio 
        AudioSource.PlayClipAtPoint(fireSFX, Camera.main.transform.position, fireSFXVol);

    }

    private void Fire(Transform target)//used to shoot at specific object
    {
        //create missile
        GameObject Missle = Instantiate(
            projectile,
            transform.position, //must lower starting point tostop from killing itself
            Quaternion.identity)
            as GameObject;

        //set on enemy projectile layer
        Missle.gameObject.layer = LayerMask.NameToLayer("Enemy projectile");

        //determine direction
        Vector2 direction = Vector3.Normalize(target.transform.position - this.transform.position);

        Missle.GetComponent<Rigidbody2D>().transform.Translate(new Vector2(0, 0));//set location
        Missle.GetComponent<Rigidbody2D>().velocity = new Vector2(direction.x*projSpeed, direction.y*projSpeed);//set velocity
        //Audio 
        AudioSource.PlayClipAtPoint(fireSFX, Camera.main.transform.position, fireSFXVol);
    }

    private bool withinRange(GameObject other, float range)//specify range value to use
    {
        if (Vector2.Distance(this.transform.position,other.transform.position) <= range)
        {
            return true;
        }
        return false;
    }

    //****COLLISIONS****
    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageDealer damageDealer = other.gameObject.GetComponent<DamageDealer>();
        if (damageDealer != null)
        {
            HandleDamage(damageDealer);
        }


    }

    //****HANDLERS****
    public void HandleFieldEnter(GameObject other, SensingField.Mode fieldType)
    {
        //WIP make raycast in enumerators so they work even if starting in field witout line of sight
            
            //checked if it was the player who entered the field
         if (other.gameObject.GetComponent<Player>() != null)
         {
                


                //Debug.Log("Robotic voice: 'Enemy Detected' ");


                currentTarget = other.transform;

                if (fieldType == SensingField.Mode.Firing)
                {
                    //Debug.Log("Target entered firing field");
                    //shoot at target
                    isShooting = true;
                    //stop moving if moving
                    if(ChaseTargetCoroutine != null)
                    StopCoroutine(ChaseTargetCoroutine);
                    GetComponent<AStarPathfinder>().MoveToTarget(transform.position + Vector3.Normalize(currentTarget.transform.position - this.transform.position) * FiringFieldMargin);//move in to firing margin
                }

                if (fieldType == SensingField.Mode.Detecting)
                {
                    //move towards target
                    //Debug.Log("Target entered detecting field");
                    ChaseTargetCoroutine = StartCoroutine(ChaseTarget(currentTarget));
                }

                
         }
    }   

    public void HandleFieldExit(GameObject other, SensingField.Mode fieldType)
    {
        if (other.gameObject.GetComponent<Player>() != null)
        {
            //Debug.Log("Enemy field Exit being hanlded");
            if (fieldType == SensingField.Mode.Firing)
            {
                //Debug.Log("Target exiting firing field");
                isShooting = false;
                if(currentTarget !=null)//may be null due to raycast
                ChaseTargetCoroutine = StartCoroutine(ChaseTarget(currentTarget));
            }
            
            if (fieldType == SensingField.Mode.Detecting)
            {
                //move towards target
                //Debug.Log("Target exiting detecting field");
                if(ChaseTargetCoroutine != null)//may be dull due to raycast
                StopCoroutine(ChaseTargetCoroutine);
                
            }

            if (other == currentTarget)
            {
                //de select target
                currentTarget = null;
            }
        }
    }

    override public void HandleDamage(DamageDealer damageDealer)
    {
        StartCoroutine(FlashColour());

        health -= damageDealer.getDamage();

        if (health <= 0)
        {
            Die();

        }
    }

    //Follow TargetUntil Ontop of them
    public IEnumerator ChaseTarget(Transform target)
    {
        //chase until on top or coroutine externally stopped
        while(target.transform.position != transform.position)
        {
            //check for line of sight
            //make sure walls arent in the way
            var layerMask = (1 << 8) + (1 << 16);//only look at player and wall layers
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.Normalize(target.transform.position - transform.position), 10000, layerMask);
            //Debug.Log(hit.transform.position);
            if (hit.transform.gameObject.GetComponent<Player>() != null)
            {
                GetComponent<AStarPathfinder>().MoveToTarget(target.position);
            }
            yield return new WaitForSeconds(1);
            
        }
    }

    private void Die()
    {
        //Destroy enemy
        Destroy(gameObject);
        //create death FX
        GameObject deathVfx = Instantiate(
        DeathVFX,
        transform.position, //must lower starting point tostop from killing itself
        Quaternion.identity)
        as GameObject;
        Destroy(deathVfx, deathTime);
        //play death sound
        AudioSource.PlayClipAtPoint(deathSFX, Camera.main.transform.position,deathSFXVol);
        //spawn item upon death
        if(ItemDrop != null)
        {
            SpawnItem(ItemDrop.GetComponent<Item>());
        }


    }



}
