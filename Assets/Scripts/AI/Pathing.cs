using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathing : MonoBehaviour {
    [SerializeField] GameObject Path;
    List<Transform> Waypoints;
    float moveSpeed;
    private int waypointIndex = 0;
    public Character parent;
    [SerializeField] public bool PathingEnabled=true;


    // Use this for initialization
    void Start()
    {
        if (PathingEnabled)
        {
            InitPathing();
        }
    }

    private void InitPathing()
    {
        //Init waypoints
        Waypoints = new List<Transform>();
        foreach (Transform child in Path.transform.GetComponentInChildren<Transform>())
        {Waypoints.Add(child); }

        //Init Parent
        parent = this.gameObject.GetComponent<Character>();
        //Get movespeed from parent character
        moveSpeed = parent.MoveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if(PathingEnabled)
            move();
    }

    private void move()
    {
        //move through waypoints
        if (waypointIndex <= Waypoints.Count - 1)
        {
            var targetPosition = Waypoints[waypointIndex].transform.position;
            var deltaMove = moveSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, deltaMove);
            //Debug.Log(transform.position);
            //Debug.Log(Waypoints[waypointIndex].position);

            //WARNING, SCRIPT WILL NOT TRIGGER IF Z POSITIONS ARE OFF
            if (transform.position == Waypoints[waypointIndex].position)
            {
                //Debug.Log("next waypoint " +Waypoints.Count);
                waypointIndex++;
            }
        }

        //restart path cycle
        else
        {
            waypointIndex = 0;
        }


    }

}
