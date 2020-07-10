using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Modifier : MonoBehaviour {
    //modifier functions are called given the proper stat name(ToggleModifier()) using master-slave relationship with modified class
    [SerializeField] public Stat EffectedStat;// eg "Health"
    [SerializeField] public string Name;// eg "Smoker's Lung"
    [SerializeField] public bool Stackable=false;//determines if multiple modifiers of this name can be stacked
    [SerializeField] public string Description;// eg "months of smoking cyber darts has impacted your health"
    [SerializeField] public float Factor;//eg 0.2
    [SerializeField] public float Rate;//eg -15/x seconds
    [SerializeField] public float RatePeriod;// eg -x every 2 seconds
    [HideInInspector]public float RatePeriodCounter;
    [SerializeField] public bool HasDuration;
    [SerializeField] public float Duration = 10;//eg 10 seconds
    [SerializeField] public Type ModifierType;

    [HideInInspector] public float RemainingTime;
    [HideInInspector] public bool Applied;

    public enum Stat
    {
        Health, Speed
    }

    public enum Type
    {
        Factor, Constant, Rate
    }
	// Use this for initialization
	void Start () {
        InitModifier();
	}

    private void InitModifier()
    {
        RemainingTime = Duration;
        Applied = false;
        RatePeriodCounter = RatePeriod;
    }

    // Update is called once per frame
    void Update () {
		
	}

    //applies modifier to requested stat
    public void ToggleModifier(ref float stat)
    { 
        Debug.Log("toggle modifier to requested stat");
        if (ModifierType == Modifier.Type.Factor)
        {
            if (!Applied)
                stat *= Factor;

            else
                stat /= Factor;
        }
        Debug.Log("stat: " + stat);

        if(ModifierType == Modifier.Type.Rate)
        {
            Debug.Log("WARNING: RATE MODIFIER NOT FULLY IMPLEMENTED YET");
        }
    }

    public void ApplyRate(ref float stat)
    {
        stat -= Rate; 
    }


}
