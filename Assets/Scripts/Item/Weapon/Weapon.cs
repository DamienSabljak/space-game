using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item {
    [Header("Projectile")]
    [SerializeField] GameObject projectile;
    [SerializeField] public int Damage = 100;//will override damage dealer of missle leaving weapon
    [SerializeField] float projSpeed = 10f;
    [Header("Weapon Parameters")]
    [SerializeField] public WeaponType weaponType = WeaponType.NORMALSHOT;
    [SerializeField] bool InfiniteAmmo = false;
    [SerializeField] float fireDelay = 0.5f;//minimum time between shots
    [SerializeField] float weaponSpreadAngle = 0.0f;//max angle in degrees from boresight weapon can shoot off from 
    [SerializeField] public int shotsPerBurst = 1;//how many shots to fire at once after shot time has been reached
    [SerializeField] public float timeBetweenBurstShots = 0.1f;//when burst firing multiple shots, how long between each shot should be 
    [Header("ChargeProjectileSpecific")]
    [SerializeField] GameObject aimingLaser;//laser prefab to be drawn to as laser guide
    [SerializeField] float chargeCoolDowntime = 1.0f;
    private bool CoolingDown = false;
    [Header("Audio")]
    [SerializeField] AudioClip fireSFX;
    [SerializeField] [Range(0, 1)] float fireSFXVol = 0.25f;
    //******** private global references **********
    [SerializeField] float shotCounter = 0;  //used to countdown time between shots 
    private float burstCounter;  //used to store timing of burst shots during burst fire 
    private float remainingBurstShots;//used to store remaining shots during burst fire
    [HideInInspector] public Player Parent; //refference to Player holding weapon
    private Inventory inventory;
    Coroutine firingCoroutine;
    Coroutine burstFireCoroutine;
    Laser laserInUse; //reference to laser prefab created during charge shots

    public enum WeaponType
    {
        NORMALSHOT, 
        CHARGESHOT,
        SHOTGUN,
        MELEE
    }

    // Use this for initialization
    void Start () {
        InitWeapon();
	}

    private void InitWeapon()
    {
        burstCounter = 0;
        shotCounter = 0;
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

    public void HandleFiring()
    {
        //Debug.Log("shotCouter" + shotCounter.ToString());
        bool usingUnity = false;
#if UNITY_EDITOR_WIN
        usingUnity = true;
#endif

        if ((usingUnity && Input.GetButton("Fire1") == true) || (!usingUnity && MobileControls.inputValues[MobileControls.RIGHT].magnitude >0))
        {   
            ShootAt();
        }
        else
        {
            StopShooting();
        }
    }
    public void ShootAt()
    {   //main method for firing, switches into other firing modes 
        switch (weaponType)
        {
            case (WeaponType.NORMALSHOT):
                CountAndShootAt( ((Vector3)Level.CurrentLevel.currentPlayer.pointingTowards) + Level.CurrentLevel.currentPlayer.transform.position);
                break;
            case (WeaponType.CHARGESHOT):
                ChargeAndShootAt(((Vector3)Level.CurrentLevel.currentPlayer.pointingTowards) + Level.CurrentLevel.currentPlayer.transform.position);
                break;
            case (WeaponType.MELEE):
                Debug.Log("WARNING: MELEE METHOD FOR SHOOTAT HAS NOT BEEN CREATED");
                break;
        }

    }

    public void StopShooting()
    {
        switch (weaponType)
        {
            case (WeaponType.NORMALSHOT):
                shotCounter -= Time.deltaTime;//continue to reduce time 
                burstCounter -= Time.deltaTime;
                remainingBurstShots = shotsPerBurst;
                if (shotCounter<0)
                {
                    shotCounter = 0;
                }
                if (burstCounter < 0)
                {
                    burstCounter = 0;
                }
                break;
            case (WeaponType.CHARGESHOT):
                CancelChargeShot();
                break;
            case (WeaponType.MELEE):
                Debug.Log("WARNING: MELEE METHOD FOR SHOOTAT HAS NOT BEEN CREATED");
                break;
        }
    }
    public void CountAndShootAt(Vector3 position)
    {
        //count down and shoot
        shotCounter -= Time.deltaTime;
        if (shotCounter <= 0f)
        {
            if (remainingBurstShots > 0)
            {   //call keeps catching here until burst complete
                BurstFire(position);
            }
            else
            {
                shotCounter = fireDelay;
                remainingBurstShots = shotsPerBurst;
            }

        }
    }

    public void ChargeAndShootAt(Vector3 position)
    {   //2 staged firing, 
        //charge and fire (laser is drawn to alert player)
        //cooldown (laser is removed) 
        //shotcounter is used for both cooling down and charging up timing 
        if (CoolingDown != true)
        {   //charge weapon and fire 
            //create laser
            if (laserInUse == null && aimingLaser !=null)
            {
                GameObject laser = Instantiate(aimingLaser);
                laserInUse = laser.GetComponent<Laser>();
            }
            laserInUse.GetComponent<Laser>().SetLaserLocation(transform.position, position);

            shotCounter -= Time.deltaTime;
            if (shotCounter <= 0f)
            {
                if (remainingBurstShots > 0)
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
            if (laserInUse != null) { Destroy(laserInUse.gameObject); }

            //countdown
            shotCounter -= Time.deltaTime;
            if (shotCounter <= 0f)
            {
                shotCounter = fireDelay;
                CoolingDown = false;
            }
        }
    }
    public void CancelChargeShot()
    {   //resets charge shot after loss of L.O.S or range 
        if (laserInUse != null)
        {
            Destroy(laserInUse.gameObject);//remove aiming laser 
        }

        shotCounter = fireDelay;
    }

    private void BurstFire(Vector3 target)
    {//used to countdown and burst fire 
        burstCounter -= Time.deltaTime;
        if (burstCounter <= 0)
        {
            Fire(target);
            remainingBurstShots--;
            burstCounter = timeBetweenBurstShots;
        }
    }
    private void Fire(Vector3 target)
    {   //instantiates and fires missile at target
        if(InfiniteAmmo || inventory.consumableArr[(int) Consumable.Type.AMMO] >0)
        {//missile can be fired
         //Remove Missle from inventory 
            if (!InfiniteAmmo)
            {
                inventory.consumableArr[(int)Consumable.Type.AMMO] -= 1;
            }
            //Missle Instantiation
            GameObject Missle = Instantiate(projectile, //IMPORTANT change position if refferencing weapon withi no transform
                   Parent.gameObject.transform.position, Quaternion.identity)
                   as GameObject;
            if (Missle.GetComponent<DamageDealer>())//projectile could be non damage dealing, such as grenade
            {
                Missle.GetComponent<DamageDealer>().Damage = Damage;
            }
            //Determine spread
            Vector2 boresight = Vector3.Normalize(target - transform.position);//unit vector from enemy to target 
            float angleFromBoresight = UnityEngine.Random.Range(-weaponSpreadAngle, weaponSpreadAngle);// [-phi,phi]
            Vector2 direction = Quaternion.AngleAxis(angleFromBoresight, Vector3.forward) * boresight;//multiply by quaternion to rotate 
            //launch missle
            Missle.GetComponent<Rigidbody2D>().transform.Translate(new Vector2(0, 0));
            Missle.GetComponent<Rigidbody2D>().velocity = direction * projSpeed;
            //play sound
            if (fireSFX != null)
            {
                AudioSource.PlayClipAtPoint(fireSFX, Camera.main.transform.position, fireSFXVol);
            }
        }
        
    }






}
