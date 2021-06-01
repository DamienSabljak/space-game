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
    [SerializeField] AttackType  attackType = AttackType.NORMALSHOT;
    [SerializeField] GameObject projectile;
    [SerializeField] float projSpeed = 5f;
    [SerializeField] float minShotTime = 0.2f;
    [SerializeField] float maxShotTime = 3f;
    [SerializeField] float weaponSpreadAngle = 0.0f;//max angle in degrees from boresight weapon can shoot off from 
    [SerializeField] public float timeBetweenBurstShots = 0.1f;//when burst firing multiple shots, how long between each shot should be 
    [SerializeField] public int shotsPerBurst = 1;//how many shots to fire at once after shot time has been reached 
    [SerializeField] AudioClip fireSFX;
    [SerializeField] [Range(0, 1)] float fireSFXVol = 0.25f;
    private float shotCounter;  //used to store timing of shots during ShootAt methods
    private float burstCounter;  //used to store timing of burst shots during burst fire 
    private float remainingBurstShots;//used to store remaining shots during burst fire
    [Header("ChargeProjectileSpecific")]
    [SerializeField] GameObject aimingLaser;//laser prefab to be drawn to warn player of charge shot 
    [SerializeField] float chargeCoolDowntime = 1.0f;
    private bool CoolingDown = false;   
    [Header("DeathFX")]
    [SerializeField] GameObject DeathVFX;
    [SerializeField] float deathTime=1f;
    [SerializeField] AudioClip deathSFX;
    [SerializeField] [Range(0,1)] float deathSFXVol = 0.7f;
    [SerializeField] GameObject ItemDrop;
    [Header("internal parameters")]
    public bool isShooting = false;
    public bool TargetWithinFiringField = false;
    public bool TargetWithinDetectingField = false;
    GameObject currentTarget;//defines enemy's current target to attack, rn always player
    Vector3  TargetLastKnownPosition;
    float moveUpdateTime = 0.0f;
    Laser laserInUse; //reference to laser prefab created during charge shots 

    


    void Start()
    {
        shotCounter = UnityEngine.Random.Range(minShotTime, maxShotTime);
        burstCounter = timeBetweenBurstShots;
        GetComponent<Animator>().SetBool("walking", false);
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
                ShootAt(currentTarget.transform);
            }
            else if (!HasLineOfSight(currentTarget.transform) || !TargetWithinFiringField)
            {
                StopShooting();
                currentState = State.CHASING;
            }
        }
    }
    //****COMBAT****

    public void ShootAt(Transform position)
    {   //main method for firing, switches into other firing modes 
        switch (attackType)
        {
            case (AttackType.NORMALSHOT):
                CountAndShootAt(position);
                break;
            case (AttackType.CHARGESHOT):
                ChargeAndShootAt(position);
                break;
            case (AttackType.MELEE):
                Debug.Log("WARNING: MELEE METHOD FOR SHOOTAT HAS NOT BEEN CREATED");
                break;
        }

    }
    public void StopShooting()
    {
        switch (attackType)
        {
            case (AttackType.NORMALSHOT):
                break;
            case (AttackType.CHARGESHOT):
                CancelChargeShot();
                break;
            case (AttackType.MELEE):
                Debug.Log("WARNING: MELEE METHOD FOR SHOOTAT HAS NOT BEEN CREATED");
                break;
        }
    }

    public void CountAndShootAt(Transform position)
    {
        //count down and shoot
        shotCounter -= Time.deltaTime;
        if(shotCounter <= 0f)
        {
            if (remainingBurstShots > 0)
            {   //call keeps catching here until burst complete
                BurstFire(position);
            }
            else
            {
                shotCounter = UnityEngine.Random.Range(minShotTime, maxShotTime);
                remainingBurstShots = shotsPerBurst;
            }
            
        }
    }

    public void ChargeAndShootAt(Transform position)
    {   //2 staged firing, 
        //charge and fire (laser is drawn to alert player)
        //cooldown (laser is removed) 
        //shotcounter is used for both cooling down and charging up timing 
        if(CoolingDown != true)
        {   //charge weapon and fire 
            //create laser
            if (laserInUse == null)
            {
                GameObject laser = Instantiate(aimingLaser);
                laserInUse = laser.GetComponent<Laser>();
            }
            laserInUse.GetComponent<Laser>().SetLaserLocation(transform.position, position.position);

            shotCounter -= Time.deltaTime;
            if (shotCounter <= 0f )
            {
                if(remainingBurstShots>0)
                {   //call keeps catching here until burst complete
                    BurstFire(position);
                }
                else
                {   //firing complete, reset
                    shotCounter = chargeCoolDowntime;
                    remainingBurstShots = shotsPerBurst;
                    CoolingDown = true;
                } 
            }
        }
        else
        {   //weapon is cooling down 
            //remove laser 
            if(laserInUse != null) { Destroy(laserInUse.gameObject); }

            //countdown
            shotCounter -= Time.deltaTime;
            if (shotCounter <= 0f)
            {
                shotCounter = UnityEngine.Random.Range(minShotTime, maxShotTime);
                CoolingDown = false;
            }
        }
    }

    public void CancelChargeShot()
    {   //resets charge shot after loss of L.O.S or range 
        if(laserInUse != null)
        {
            Destroy(laserInUse.gameObject);//remove aiming laser 
        }
        
        shotCounter = UnityEngine.Random.Range(minShotTime, maxShotTime);
    }

    private void BurstFire(Transform target)
    {//used to countdown and burst fire 
        burstCounter -= Time.deltaTime;
        if(burstCounter<= 0)
        {
            Fire(target);
            remainingBurstShots--;
            burstCounter = timeBetweenBurstShots;
        }
    }
    private void Fire(Transform target)//used to shoot at specific object
    {   //fire single shot, with random direction within maxAngleFromBoresight 
        //create missile
        GameObject Missle = Instantiate(
            projectile,
            transform.position, //must lower starting point tostop from killing itself
            Quaternion.identity)
            as GameObject;

        //set on enemy projectile layer
        Missle.gameObject.layer = LayerMask.NameToLayer("Enemy projectile");

        //determine direction
        //see https://forum.unity.com/threads/rotating-a-vector-by-an-eular-angle.18485/ for more 
        Vector2 boresight = Vector3.Normalize(target.transform.position - this.transform.position);//unit vector from enemy to target 
        float angleFromBoresight = UnityEngine.Random.Range(-weaponSpreadAngle, weaponSpreadAngle);// [-phi,phi]
        Vector2 direction = Quaternion.AngleAxis(angleFromBoresight, Vector3.forward) * boresight;//multiply by quaternion to rotate 
        //instantiate prefab 
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
        //destroy laser if it exists
        if(laserInUse != null) { Destroy(laserInUse.gameObject); }
        
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
