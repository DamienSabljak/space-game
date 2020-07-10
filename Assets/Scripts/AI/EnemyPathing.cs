using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathing : MonoBehaviour {

    WaveConfig waveConfig;
    List<Transform> waypoints;
    float moveSpeed;
    private int waypointIndex = 0;


	// Use this for initialization
	void Start () {
        waypoints = waveConfig.getWaypoints();
        transform.position = waypoints[waypointIndex].transform.position;
        moveSpeed = waveConfig.getMoveSpeed();
	}

    // Update is called once per frame
    void Update()
    {
        move();
    }

    public void setWaveConfig(WaveConfig waveConfig)
    {
        this.waveConfig = waveConfig;
    }

    private void move()
    {
        //move through waypoints
        if (waypointIndex <= waypoints.Count - 1)
        {
            var targetPosition = waypoints[waypointIndex].transform.position;
            var deltaMove = moveSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, deltaMove);

            if (transform.position == waypoints[waypointIndex].position)
            {
                waypointIndex++;
            }
        }

        //enemy is destroyed at the end of the waypoints 
        else
        {
            Destroy(gameObject);
        }

    
    }
}
