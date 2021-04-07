using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//collapse text: ctrl+M+H
public class Player : Character {
    [Header("Characteristics")]
    [SerializeField] public float Health = 100f;
    
    [SerializeField] float padding =1f;//distance from camera allowed when camera static
    [Header("Projectile")]
    [SerializeField] GameObject CurrentToolBarItem;//MAKE SURE declared gameobject to use general prefab
    [SerializeField] GameObject projectile;
    [SerializeField] public bool InfiniteAmmo = false;
    [SerializeField] float projSpeed = 10f;
    [SerializeField] float fireRate = 0.5f;
    [SerializeField] float shotCounter=0;
    [Header("UI")]
    [SerializeField] GameObject playerUI;
    [SerializeField] bool ShiftCameraEnabled = true; 
    [SerializeField] [Range(0, 0.005f)] float CameraShiftScale = 0.01f;//determines magnitude of camera shift
    [Header("Audio")]
    [SerializeField] AudioClip fireSFX;
    [SerializeField] [Range(0, 1)] float fireSFXVol = 0.25f;
    [SerializeField] AudioClip deathSFX;
    [SerializeField] [Range(0, 1)] float deathSFXVol = 0.7f;

    public PlayerInventory inventory;

    Coroutine firingCoroutine; 
    float xMin, xMax, yMin, yMax;
    public List<Modifier> ModifiersList;//holds information on current modifiers affecting the player

    //****START/UPDATE**** 
    public void OnMouseDown()
    {
        Debug.Log("player Clicked!");

    }
    // Use this for initialization
    void Awake()//awake is called before start methods
    {
        
    }
    void Start () {
        //SetUpMoveBoundaries(); //use if boundaries for staic cameras needed
        if (LevelManager.player != null)
        {
            Debug.Log("sepeku..");
            LevelManager.player.gameObject.transform.position = this.gameObject.transform.position;
            Destroy(this.gameObject); //prevent duplicate player objects
        }
        UpdateCurrentToolBarItem();
        initInventory();
        

    }

    // Update is called once per frame
    void Update () {
        if (Level.pause != true)
        {
            Move();
            Fire();
            Interact();
            SelectToolBarItem();
            overheadtext.dispStandingOn();
            if(ShiftCameraEnabled && Time.timeScale != 0)
            ShiftCamera();
            UpdateModifiers();

            LevelManager.PlayerScore = this.inventory.consumableArr[(int) Consumable.Type.scrap] + this.inventory.consumableArr[(int)Consumable.Type.money];
        }
    }

    private void initInventory()
    {
        Debug.Log("Player: creating new inventory object");
        inventory = gameObject.GetComponent<PlayerInventory>();

        if (inventory == null)
            Debug.Log("WARNING: Player must have inventory script attatched");
    }

    private void SetUpMoveBoundaries()
    {
        Camera gameCamera = Camera.main;
        xMin = gameCamera.ViewportToWorldPoint(new Vector3(0,0,0)).x + padding;
        xMax = gameCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - padding;

        yMin = gameCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + padding;
        yMax = gameCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y - padding;
    }

    //****USER INPUTS****
    private void Move()
    {
        if(isGrounded)
        {
            Rigidbody2D body = gameObject.GetComponent<Rigidbody2D>();
            var deltaX = Input.GetAxis("Horizontal") * Time.deltaTime * MoveSpeed;
            var deltaY = Input.GetAxis("Vertical") * Time.deltaTime * MoveSpeed;

            //set new facing
            if (deltaY > 0)
                facing = direction.UP;
            else if (deltaY < 0)
                facing = direction.DOWN;
            if (deltaX > 0)
                facing = direction.RIGHT;
            else if (deltaX < 0)
                facing = direction.LEFT;

            // var newXPos = Mathf.Clamp(transform.position.x + deltaX,xMin,xMax);//use with setupboundaries
            // var newYPos = Mathf.Clamp(transform.position.y + deltaY,yMin,yMax);
            var newXPos = transform.position.x + deltaX;
            var newYPos = transform.position.y + deltaY;

            body.MovePosition(new Vector2(newXPos, newYPos));
        }
        
    }

    private void Interact()
    {//used to handle action button calls 
        if (Input.GetButtonDown("Interact"))
        {
            Debug.Log("interact button pressed");
            if(standingOn != null)
            { 
                Debug.Log("handling interact:");
                HandleInteract(standingOn);
            }
        }


    }

    
    private void ShiftCamera()
    {//Shifts camera based on mouse posiion
        Transform cameraPos = transform.Find("Main Camera");
        cameraPos.localPosition = (Input.mousePosition - new Vector3(-1.6f,-1.6f))*CameraShiftScale;
    }

    private void SelectToolBarItem()
    {
        if (Input.GetButtonDown("Hotkey1"))
        {
            selecttoolbaritem(0);
        }
        else if (Input.GetButtonDown("Hotkey2"))
        {
            selecttoolbaritem(1);
        }
        else if (Input.GetButtonDown("Hotkey3"))
        {
            selecttoolbaritem(2);
        }
        else if (Input.GetButtonDown("Hotkey4"))
        {
            selecttoolbaritem(3);
        }
    }

    private void selecttoolbaritem(int index)
    {//helper method used in method above
        Debug.Log("hotkey " +((int) index+1)+ " selected");
        if (inventory.ToolBarList[index] == CurrentToolBarItem)
        {
            //intentionally do nothing
        }
        else if (inventory.ToolBarList[index] == null)
        {
            //deactivate old item
            if (CurrentToolBarItem != null)
            {
                Debug.Log("deactivate " + CurrentToolBarItem);
                CurrentToolBarItem.gameObject.SetActive(false);
            }
            CurrentToolBarItem = null;
        }
        else //replace with new item
        {
            //deactivate old item
            if (CurrentToolBarItem != null)
            {
                Debug.Log("deactivate " + CurrentToolBarItem);
                CurrentToolBarItem.gameObject.SetActive(false);
            }
            //activate new item
            CurrentToolBarItem = inventory.ToolBarList[index].gameObject;
            CurrentToolBarItem.gameObject.SetActive(true);
            UpdateCurrentToolBarItem();//set parent refference for weapon script
        }
    }

    private void UpdateCurrentToolBarItem()
    {//use to select weapon
        if (CurrentToolBarItem != null)
        {
            Debug.Log("updating current weapon");
            CurrentToolBarItem.GetComponent<Weapon>().UpdateParent(this);
        }
    }

    //****COMBAT****
    private void Fire()
    {
        if (CurrentToolBarItem != null)
            CurrentToolBarItem.GetComponent<Weapon>().Fire();
        else
            if (Input.GetButtonDown("Fire1"))
            Debug.Log("no weapon equipped!");
    }

    override public void HandleDamage(DamageDealer damageDealer)
    {
        StartCoroutine(FlashColour());
        Health -= damageDealer.getDamage();

        //check for modifiers
        if(damageDealer.Modifier != null)
        {
            GameObject mod = Instantiate(damageDealer.Modifier, this.transform.position, Quaternion.identity);
            HandleModifier(mod.GetComponent<Modifier>());
        }

        if (Health <= 0)
        {
            Die();

        }
    }

    public override void Die()
    {
        //Destroy(gameObject); //find a way for this to not cause errors 
        AudioSource.PlayClipAtPoint(deathSFX, Camera.main.transform.position, deathSFXVol);
        SceneManager.LoadScene("EndScene");
    }
    

//****COLLISIONS****

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("collision!");
        DamageDealer damageDealer = other.gameObject.GetComponent<DamageDealer>();
        if (damageDealer != null)
        {
            //Debug.Log("collided w damage dealer");
            HandleDamage(damageDealer);
           
        }
        if(other.gameObject.GetComponent<Item>() != null)
        {
            //Debug.Log("collided with Item");
            Item item = other.gameObject.GetComponent<Item>();
            HandleItem(item);
        }
        else if (other.gameObject.GetComponent<Interactable>() != null)
        {
            Interactable interact = other.gameObject.GetComponent<Interactable>();
            standingOn = interact;
            //Debug.Log("standing on interactable");
            HandleInteract(interact);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(standingOn == other.gameObject.GetComponent<Interactable>())
        {
            standingOn = null;
            //Debug.Log("Not standing on anymore");   
        }

    }

//****HANDLERS****

    private void HandleInteract(Interactable interact)
    {
        //Debug.Log("welcome to interact method!");
        Debug.Log(interact && interact.WaitForInput && Input.GetButtonDown("Interact"));
        //GameObject parent = interact.transform.parent.gameObject;//find out what the interactable is attatched to 
        if (interact.WaitForInput && Input.GetButtonDown("Interact"))//see if interaction requires user input
        {
            interact.HandleInteractAction(this.gameObject);
            //Debug.Log("INTERACTT");
        }
    }

    private void HandleItem(Item item)
    {
        //Debug.Log("ITEMMM");
        if(item.isConsumable())
        {
            Consumable consumable = (Consumable)item;
            inventory.AddConsumable(consumable.consumabletype, consumable.amount);
            if(consumable.pickupSFX != null)
            {
                AudioSource.PlayClipAtPoint(consumable.pickupSFX, Camera.main.transform.position, consumable.pickupSFXVol);//play opening sound
            }
            Destroy(item.gameObject);
        }
        else
        {
            inventory.AddItem(item);
        }
    }

    

    override public void HandleModifier(Modifier modifier)
    {
        Debug.Log("attempting to add modifier to player");
        bool wasmodified=false;//flag to see successful modification

        //check if stat is existing
        bool AlreadyExists=false;
        foreach (Modifier mod in ModifiersList)
        {
            //if mod is stackable no need to check rest of list
            if (modifier.Stackable)
            {
                AlreadyExists = false;
                break;
            }

            else if(mod.name == modifier.name)
            {
                AlreadyExists = true;
                Debug.Log("modifier already exists");
                mod.RemainingTime = mod.Duration;//restart duration
                Destroy(modifier.gameObject);
                break;
            }
        }

        if (!AlreadyExists)
        {
            //determine stat to modify
            if (modifier.EffectedStat == Modifier.Stat.Speed)
            {
                modifier.Applied = false;
                modifier.ToggleModifier(ref MoveSpeed);//passes desired stat refference to modifier method for mutation
                
                wasmodified = true;
            }

            else if (modifier.EffectedStat == Modifier.Stat.Health)
            {
                modifier.Applied = false;
                modifier.ToggleModifier(ref Health);//passes desired stat refference to modifier method for mutation

                wasmodified = true;
            }

            else
            {
                Debug.Log("could not match modifier to stat in HandleModifier()");
            }
        }

        if (wasmodified)
        {
            Debug.Log("adding modifier to list");
            modifier.transform.parent = this.transform;
            modifier.GetComponent<SpriteRenderer>().enabled = false;
            ModifiersList.Add(modifier);
        }
        else
        {
            Debug.Log("modifier was not successfully applied");
        }
    }

    private void UpdateModifiers()
    {
        for (int i=0;i<ModifiersList.Count;i++)
        {
            //Debug.Log(ModifiersList[i] + "is in modifier list");
            if (ModifiersList[i].HasDuration) {
                //Debug.Log("counting down modifier time");

                //Check Rate counter
                if (ModifiersList[i].ModifierType == Modifier.Type.Rate)
                {   //Debug.Log("ratemod detected in uupdate");
                    ModifiersList[i].RatePeriodCounter -= Time.deltaTime;

                    //check if counter is zero -> apply rate
                    if (ModifiersList[i].RatePeriodCounter <= 0)
                    {
                        Debug.Log("modifier countdown complete");
                        //determine affected stat
                        if (ModifiersList[i].EffectedStat == Modifier.Stat.Speed)
                        {
                            ModifiersList[i].ApplyRate(ref MoveSpeed);
                        }
                        if (ModifiersList[i].EffectedStat == Modifier.Stat.Health)
                        {
                            Debug.Log("ratemod affecting health");
                            DamageDealer dealer = new DamageDealer();
                            dealer.Damage = (int)ModifiersList[i].Rate;
                            HandleDamage(dealer);
                        }

                        ModifiersList[i].RatePeriodCounter = ModifiersList[i].RatePeriod;
                    }
                }

                //Check Remaining Time
                ModifiersList[i].RemainingTime -= Time.deltaTime;
                if (ModifiersList[i].RemainingTime <= 0)
                {
                    //Debug.Log("modifier being removed due to timeout");
                    ModifiersList[i].Applied = true;//set so that modifier gets removed 
                    ModifiersList[i].ToggleModifier(ref MoveSpeed);
                    ModifiersList[i] = null;
                    ModifiersList.Remove(ModifiersList[i]);
                }

            }
        }
    }



    /*
     * ********old methods *******
     * 
     * private void HandleVendor(Vendor vendor)
    {
        vendor.StartDialog(this);
    }
    */
}//end class
