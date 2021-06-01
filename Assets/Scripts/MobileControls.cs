using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileControls : MonoBehaviour
{
    [SerializeField] private List<GameObject> InputZones;//prefabs reffering to inputZones
    [SerializeField] public GameObject UseButton;
    public static List<Vector2> inputValues = new List<Vector2>();//values of touch readings from input zones 
    // Start is called before the first frame update
    public static int LEFT = 0;
    public static int RIGHT = 1;
    void Start()
    {
        //init values 
        inputValues.Add(new Vector2(0, 0));
        inputValues.Add(new Vector2(0, 0));
    }

    // Update is called once per frame
    void Update()
    {
        HandleTouch();
    }

    public void HandleTouch()
    {
        if (Input.touchCount > 0)
        {   //some input is being given 
            for (int index = 0; index < InputZones.Count; index++)
            {
                bool touchWithinZone = false;
                for (int touchId = 0; touchId < Input.touchCount; touchId++)
                {
                    if(TouchWithinZone(InputZones[index], touchId))
                    {   //zone is being touched 
                        touchWithinZone = true;
                        inputValues[index] = VectorFromCenterOfZone(InputZones[index], touchId);
                    }
                }
                if(!touchWithinZone)
                {   //set value to zero if nothing inside zone 
                    inputValues[index] = new Vector2(0, 0);
                }
            }
        }
        else
        {   //no input is being given 
            for (int index = 0; index < InputZones.Count; index++)
            {
                inputValues[index] = new Vector2(0,0);
            } 
        }
    }

    public bool TouchWithinZone(GameObject touchZone, int touchId)
    {   //see if a touch is within touch area 

        //switch touch to Input.mousePosition for testing on computer 
        Touch touch = Input.GetTouch(touchId);
        Vector2 localMousePosition = touchZone.GetComponent<RectTransform>().InverseTransformPoint(touch.position);
        if (touchZone.GetComponent<RectTransform>().rect.Contains(localMousePosition))
        {
            return true;
        }

        return false;
    }

    public Vector2 VectorFromCenterOfZone(GameObject touchZone, int touchId)
    {
        Vector2 zoneCenter = touchZone.GetComponent<RectTransform>().rect.center;//center zone in local space
        Touch touch = Input.GetTouch(touchId);
        Vector2 localMousePosition = touchZone.GetComponent<RectTransform>().InverseTransformPoint(touch.position);//get local coordinate of touch 
        return (localMousePosition - zoneCenter);
    }

    public void InvokeInteractKey()
    {   //invokes interact key on virtual keyboard,
#if UNITY_ANDROID
        if (Level.CurrentLevel.currentPlayer.standingOn != null)
        {
            Debug.Log("handling interact:");
            Level.CurrentLevel.currentPlayer.HandleInteract(Level.CurrentLevel.currentPlayer.standingOn, true);
        }
#endif
    }
}
