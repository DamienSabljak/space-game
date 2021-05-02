using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character {

    [SerializeField] int health = 100;
    [Header("AI")]
    [SerializeField] float FiringFieldMargin=0.1f;//how much furhter into the field to go for effective fire 
    [SerializeField] GameObject armsGroup; //refference to arms GROUP for animation
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
    public bool TargetWithinFiringField = false;
    public bool TargetWithinDetectingField = false;
    GameObject currentTarget;//defines enemy's current target to attack, rn always player
    Vector3  TargetLastKnownPosition;
    float moveUpdateTime = 0.0f;


    void Start()
    {
        shotCounter = UnityEngine.Random.Range(minShotTime, maxShotTime);
        GetComponent<Animator>().SetBool("walking", false);
        currentTarget = Level.CurrentLevel.currentPlayer.gameObject;
        TargetLastKnownPosition = transform.position;
    }

    void Update()
    {
        currentTarget = Level.CurrentLevel.currentPlayer.gameObject;
        SetScaleByFacing();
        UpdateEngagementStateMachine();
    }

    //****STATE MACHINES****
    private void UpdateEngagementStateMachine()
    {
        //see state machine block diagram 
        if (currentState == State.IDLE)
        {
            StopMoving();
            //check transitions to other states 
            if (HasLineOfSight(currentTarget.transform))
            {
                if (TargetWithinDetectingField)
                {
                    TargetLastKnownPosition = currentTarget.transform.position;
                    currentState = State.CHASING;
                }
                else if (TargetWithinFiringField)
                {
                    TargetLastKnownPosition = currentTarget.transform.position;
                    currentState = State.ATTACKING;
                }
            }
        }
        else if (currentState == State.CHASING)
        {
            MoveTo(TargetLastKnownPosition);
            //check transitions to other states
            if (HasLineOfSight(currentTarget.transform))
            {
                RotateArms(currentTarget.transform.position);//rotate arms to point weapon at target
                //Debug.Log("new path being created...");
                if (TargetWithinFiringField)
                {
                    //transition to attacking 
                    TargetLastKnownPosition = currentTarget.transform.position;
                    currentState = State.ATTACKING;
                }
                else if (TargetWithinDetectingField)
                {
                    TargetLastKnownPosition = currentTarget.transform.position;
                }
            }
            if (Vector3.Magnitude(TargetLastKnownPosition - transform.position) < 2f)
            {//reached last known position
                currentState = State.IDLE;
            }
        }
        else if (currentState == State.ATTACKING)
        {
            StopMoving();
            if (HasLineOfSight(currentTarget.transform) && TargetWithinFiringField)
            {
                TargetLastKnownPosition = currentTarget.transform.position;
                RotateArms(currentTarget.transform.position);//rotate arms to point weapon at target
                CountAndShootAt(currentTarget.transform);
            }
            else if (!HasLineOfSight(currentTarget.transform) || !TargetWithinFiringField)
            {
                currentState = State.CHASING;
            }
        }
    }
    //****COMBAT****

    public void CountAndShootAt(Transform position)
    {
        //count down and shoot
        shotCounter -= Time.deltaTime;
        if(shotCounter <= 0f)
        {
            Fire(position);
            shotCounter = UnityEngine.Random.Range(minShotTime, maxShotTime);
        }
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

    private void RotateArms(Vector3 TargetPos)
    {   //rotate arms and weapon to face where mouse is looking 
        
        Vector3 WeaponDir = TargetPos - transform.position;//vector from this to target to point at 
        float angle = Vector3.Angle(new Vector3(1, 0, 0), WeaponDir);//take angle relative to positive x axis, only from 0-180 deg
        if (WeaponDir.y < 0)
        {
            angle *= -1; //compensate for lost information from "arctan" 
        }
        if (Mathf.Abs(angle) > 90)
        {
            facing = direction.LEFT;
            angle += 180;
        }
        else
        {
            facing = direction.RIGHT;
        }
        //arms.transform.Rotate(0, 0, angle);
        armsGroup.transform.eulerAngles = new Vector3(0.0f, 0.0f, angle);
    }
    private void ResetArmRotation()
    {   //not tested 
        if(facing == direction.RIGHT)
        {
            RotateArms(new Vector3(transform.position.x + 1, transform.position.y, transform.position.z));
        }
        if (facing == direction.RIGHT)
        {
            RotateArms(new Vector3(transform.position.x - 1, transform.position.y, transform.position.z));
        }
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
         //to fix: check if object is target, not player
         if (other.gameObject.GetComponent<Player>() != null)
         {
            if (fieldType == SensingField.Mode.Firing)    { TargetWithinFiringField = true; }
            if (fieldType == SensingField.Mode.Detecting) { TargetWithinDetectingField = true; }    
         }
    }   

    public void HandleFieldExit(GameObject other, SensingField.Mode fieldType)
    {
        //to fix: check if object is target, not player
        if (other.gameObject.GetComponent<Player>() != null)
        {
            if (fieldType == SensingField.Mode.Firing) { TargetWithinFiringField       = false; }
            if (fieldType == SensingField.Mode.Detecting) { TargetWithinDetectingField = false; }
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
    

    public void MoveTo(Vector3 position)
    {//move towards a specified position and animate
        //only update new waypoint periodically 
        moveUpdateTime -= Time.deltaTime;
        if (moveUpdateTime <= 0f)
        {
            if (isGrounded)
            {
                //use Astar to move
                GetComponent<AStarPathfinder>().MoveToTarget(position);//sets position to move to 
                                                                       //trigger animations
                GetComponent<Animator>().SetBool("walking", true);
            }
            moveUpdateTime = 1.0f;
        }
        
    }
    public void StopMoving()
    {//stay still and animate 
        GetComponent<AStarPathfinder>().StopMoving();
        GetComponent<Animator>().SetBool("walking", false);
    }
    
    public bool HasLineOfSight(Transform target)
    {//check for line of sight
        //make sure walls arent in the way
        var layerMask = (1 << 8) + (1 << 16);//only look at player and wall layers
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.Normalize(target.transform.position - transform.position), 10000, layerMask);
        //Debug.Log(hit.transform.position);
        return hit.transform.gameObject.GetComponent<Player>() != null;
    }


    public override void Die()
    {
        Level.CurrentLevel.remainingEnemies.Remove(this.gameObject);//remove from tracker 
        Destroy(gameObject);//Destroy enemy
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

    //******************* OLD METHODS *****************************
    /*
    private void Fire()
    {//old method, fire in cartesian direction
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

        Missle.GetComponent<Rigidbody2D>().transform.Translate(new Vector2(0, 0));//set location
        Missle.GetComponent<Rigidbody2D>().velocity = new Vector2(xspeed, yspeed);//set velocity
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

    public IEnumerator ChaseTarget(Transform target)
    {
        //chase until close to target or externally stopped 
        while(Vector3.Magnitude(target.transform.position - transform.position) > 0.05f)
        {
            //if player seen, set new target destination 
            if (HasLineOfSight(target))
            {
                    MoveTo(TargetLastKnownPosition.position);

                
            }
            yield return new WaitForSeconds(1);
            
        }
        //stop chasing, set idle animation 
        GetComponent<Animator>().SetBool("walking", false);
    }
    */
}
