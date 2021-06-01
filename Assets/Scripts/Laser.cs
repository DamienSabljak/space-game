using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetLaserLocation(Vector3 laserStart, Vector3 laserEnd)
    {   //sets start/end points for laser to be rendered 
        LineRenderer line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        Vector3[] newPos_arr = {laserStart, laserEnd};
        line.SetPositions(newPos_arr);
    }
}
