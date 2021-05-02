using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
    [SerializeField] public WorldText overheadtext;
    [SerializeField] public float MoveSpeed = 10f;

    public enum direction { LEFT, RIGHT, UP, DOWN };
    public enum State { IDLE, CHASING, ATTACKING };//states used in state machines 
    public State currentState;
    public bool isGrounded = true;
    public direction facing = direction.RIGHT;
    [HideInInspector] public Interactable standingOn;//holds info on what field character is standing in

    // Use this for initialization
    void Start () {
        currentState = State.IDLE;
	}
	
	// Update is called once per frame
	void Update () {
        //face facing direction driven by astar pathfinder 
        SetScaleByFacing();
	}

    public void SetScaleByFacing()
    {   //sets the direction the char faces by changing the scale_x property 
        if (facing == direction.LEFT && transform.localScale.x > 0)
        {
            transform.localScale = new Vector3(-1 * transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        else if (facing == direction.RIGHT && transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(-1 * transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    //To Add - method arguments for different colours
    public IEnumerator FlashColour()
    {
        //Debug.Log("FLASH");
        SpriteRenderer sprite = this.gameObject.GetComponent<SpriteRenderer>();
        sprite.color = new Color(1f, 0.1f, 0.1f, 1);
        yield return new WaitForSeconds(0.1f);
        //Debug.Log("End yield");
        sprite.color = new Color(1, 1, 1, 1);

    }   

    public void SpawnItem(Item item)
    {
        GameObject itemdrop = Instantiate(item.gameObject,
        transform.position,
        Quaternion.identity)
        as GameObject;


    }

    public void SpawnObject(GameObject obj)
    {
        GameObject objectDrop = Instantiate(obj,
        transform.position,
        Quaternion.identity)
        as GameObject;


    }

    public void HandleField()
    {
        Debug.Log("character HandleField not implemented");
    }

    public void HandleFieldExit()
    {
        Debug.Log("character HandleFieldExit not implemented");
    }

    virtual public void HandleDamage(DamageDealer damageDealer)
    {
        Debug.Log("Handle Damage Method Not Implemented on Character Class!");
    }

    virtual public void HandleModifier(Modifier modifier)
    {
        Debug.Log("Handle Modifier Method Not Implemented on Character Class!");
    }

    public virtual void Die()
    {
        //abstract method  to be used in other children classes 
    }
    
    public IEnumerator FallOffMap()
    {
        this.GetComponent<Animator>().SetTrigger("offMap");
        yield return new WaitForSeconds(2.0f);
        this.Die();

        yield return null;
    }

}
