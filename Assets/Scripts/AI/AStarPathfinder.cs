using UnityEngine;
using System.Collections;
//Note this line, if it is left out, the script won't know that the class 'Path' exists and it will throw compiler errors
//This line should always be present at the top of scripts which use pathfinding
using Pathfinding;
public class AStarPathfinder : MonoBehaviour
{
    //****NOTE****
    //call this method to return list of path transforms: seeker.StartPath(transform.position, targetPosition, OnPathComplete);
    [SerializeField] public GameObject StartingTarget;
    [SerializeField] bool EnablePathing = true;
    public Vector2 targetPosition;
    private Seeker seeker;
    public Path path;//The calculated path
    public float speed;
    public float nextWaypointDistance = 0.1f;//The max distance from the AI to a waypoint for it to continue to the next waypoint
    private int currentWaypoint = 0;


    public void Start()
    {
        if (EnablePathing)
        {
            MoveToTarget(StartingTarget.transform.position);
        }
    }

    public void MoveToTarget(Vector3 target)
    {
        seeker = GetComponent<Seeker>();
        targetPosition = target;//new code
        //Start a new path to the targetPosition, return the result to the OnPathComplete function
        seeker.StartPath(transform.position, targetPosition, OnPathComplete);
        speed = gameObject.GetComponent<Character>().MoveSpeed;
    }
    public void StopMoving()
    {
        path = null;
    }

    public void OnPathComplete(Path p)
    {
        //Debug.Log("Yay, we got a path back. Did it have an error? " + p.error);
        if (!p.error)
        {
            path = p;
            //Reset the waypoint counter
            currentWaypoint = 0;
        }
    }
    public void Update()
    {
        if (path == null)
        {
            //We have no path to move after yet
            return;
        }
        if (currentWaypoint >= path.vectorPath.Count)
        {
            //Debug.Log("End Of Path Reached");
            return;
        }
        //Direction to the next waypoint
        float deltaMove = speed * Time.deltaTime;
        float last_x = transform.position.x;
        transform.position = Vector2.MoveTowards(transform.position, path.vectorPath[currentWaypoint], deltaMove);
        float new_x = transform.position.x;
        if (new_x - last_x < 0)
        {
            GetComponent<Character>().facing = Character.direction.LEFT;
        }
        else if (new_x - last_x < 0)
        {
            GetComponent<Character>().facing = Character.direction.RIGHT;
        }
        if (transform.position == path.vectorPath[currentWaypoint])
        {
            currentWaypoint++;
            return;
        }
    }
}