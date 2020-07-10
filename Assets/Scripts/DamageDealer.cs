using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour {

    [SerializeField] public int Damage = 100;//sets default damage if weapon does not assign one
    [SerializeField] public bool DestroyOnHit = true;
    [SerializeField] public bool DamageOverTime = false; //apply periodic damage to objects within borders
    [SerializeField] public float DamagePeriod = 1; //apply periodic damage to objects within borders
    [SerializeField] bool Explosive;
    [SerializeField] GameObject explosion;
    [SerializeField] public GameObject Modifier;

    List<GameObject> ObjectsInBoundsList = new List<GameObject>();
    List<Coroutine> DamagingCoroutinesList = new List<Coroutine>();

    //****CONSTRUCTORS****

    void Start()
    {
        
    }

    void Update()
    {
        
        
    }

    //individually tracks the time each object spends in field and damages periodically
    public IEnumerator CountDownAndDamage(GameObject target)
    {
        while (true)
        {
            float DamagePeriodCounter = DamagePeriod;

            while (DamagePeriodCounter > 0)
            {
                DamagePeriodCounter -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            target.GetComponent<Character>().HandleDamage(this);//damage target
        }

    }

    public int getDamage()
    {
        return Damage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (DamageOverTime)
        {
            //log object and associated damaging coroutine
            Debug.Log(other);
            ObjectsInBoundsList.Add(other.gameObject);
            //start damaging coroutine
            DamagingCoroutinesList.Add(StartCoroutine(CountDownAndDamage(other.gameObject)));

        }

        if (Explosive)
        {
            GameObject Missle = Instantiate(
            explosion,
            transform.position, 
            Quaternion.identity)
            as GameObject;

            //set layer to this layer
            explosion.gameObject.layer = this.gameObject.layer;
            //pass on damage value
            Missle.GetComponent<DamageDealer>().Damage = Damage;

        }

        if (DestroyOnHit)
        {
            //stop any damaging coroutines
            if (DamageOverTime)
            {
                foreach (Coroutine coroutine in DamagingCoroutinesList)
                {
                    StopCoroutine(coroutine);
                }
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("object exiting damagedealer field: " + other.gameObject);
        //check for damaging coroutine and stop it 
        if (DamageOverTime)
        {
            Debug.Log("damage over time is true:" +ObjectsInBoundsList.Count);
            for(int i = 0; i < ObjectsInBoundsList.Count; i++)
            {
                Debug.Log(ObjectsInBoundsList[i]);
                if (ObjectsInBoundsList[i] == other.gameObject)
                {
                    Debug.Log("Object found in damage dealer list!");
                    //stop coroutine and remove refference
                    StopCoroutine(DamagingCoroutinesList[i]);
                    ObjectsInBoundsList.Remove(ObjectsInBoundsList[i]);
                    DamagingCoroutinesList.Remove(DamagingCoroutinesList[i]);
                }
            }
        }
    }
}
