using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {
    [SerializeField] public BranchType UPBranch;
    [SerializeField] public BranchType RIGHTBranch;
    [SerializeField] public BranchType DOWNBranch;
    [SerializeField] public BranchType LEFTBranch;
    [SerializeField] public RoomType   Type;
    public Vector3Int LocalPosition = new Vector3Int(99, 99, 99);//holds room's local position in zones tilemap, 99's so it doesnt mess with generation code
    [SerializeField] public bool EnableGeneration = false;
    //Static information
    [HideInInspector] public BranchType[] BranchEnterArr;//array about requested entrance information
    public static float[] RoomLocalBounds = {-86.32f,-60.32f, -36.4f, 15.6f};//Gives minX,maxX, minY,maxY coordinates of room for random spawning purposes (with some margin)
    public enum BranchType
    {
        Hallway1, Hallway3, Open, Closed,
    }

    public enum RoomType
    {
        Normal, Engine, Bridge
    }


    // Use this for initialization
    void Start()
    {
        InitBranchEnterArr();

        
        
        
        //old method
        /*
        if (EnableGeneration)
        {
            //Debug.Log("Random Generation Started...");
            StartCoroutine(CreateSurroundingRooms());
        }
        */
        StartCoroutine(SetRoomLayout());
    }

    public void InitBranchEnterArr()
    {
        //contains opposites of entering code in floor class
        BranchEnterArr = new BranchType[4];
        BranchEnterArr[0] = DOWNBranch;
        BranchEnterArr[1] = LEFTBranch;
        BranchEnterArr[2] = UPBranch;
        BranchEnterArr[3] = RIGHTBranch;
    }


    // Update is called once per frame
    void Update() {

    }

    //**ROOM LAYOUT**
    public IEnumerator SetRoomLayout()
    {
        while (Floor.CurrentFloor == null)
            yield return new WaitForEndOfFrame();
        //Debug.Log(Floor.CurrentFloor.RoomLayout);
        GameObject roomLayout;

        //******SET ROOM TYPE******
        var random = new System.Random();//used to pull random layout from list of possible layouts 
        if (Floor.CurrentFloor.EngineZones.Contains(LocalPosition))
        {
            this.Type = RoomType.Engine;
            roomLayout = Instantiate(Floor.CurrentFloor.EngineRoomLayout, this.transform.position, Quaternion.identity);
            roomLayout.transform.parent = transform;
            roomLayout.transform.localPosition = new Vector2(-84f, -12f); //found by moving layout in editor while paused 
        }

        else if (Floor.CurrentFloor.BridgeZones.Contains(LocalPosition))
        {
            this.Type = RoomType.Bridge;
            roomLayout = Instantiate(Floor.CurrentFloor.BridgeRoomLayout, this.transform.position, Quaternion.identity);
            roomLayout.transform.parent = transform;
            roomLayout.transform.localPosition = new Vector2(-84f, -12f); //found by moving layout in editor while paused 
        }
        else
        {
            this.GetComponent<Room>().Type = RoomType.Normal;
            int randomIndex = random.Next(Floor.CurrentFloor.NormalRoomLayouts.Count);
            roomLayout = Instantiate(Floor.CurrentFloor.NormalRoomLayouts[randomIndex], this.transform.position, Quaternion.identity);
            roomLayout.transform.parent = transform;
            roomLayout.transform.localPosition = new Vector2(-88.0f, -8.0f); //found by moving layout in editor while paused 
        }
         

        
    }

    //**ROOM GENERATION AND CHANGES**
    private IEnumerator CreateSurroundingRooms()
    {
        if (Floor.CurrentFloor == null)
            yield return new WaitForSeconds(1);//wait for current level to load

        //UP
        Vector3Int newPosition = new Vector3Int(LocalPosition.x, LocalPosition.y + 1, LocalPosition.z);
        //check if current room has opening //and if the next spot is within grid //and if a room isnt already made
        if (UPBranch == Room.BranchType.Hallway1 && Floor.CurrentFloor.tileCoordinates.Contains(newPosition) && !Floor.CurrentFloor.OccupiedCoordinates.Contains(newPosition))
        {           
            //Debug.Log("UP expand ");
            Floor.CurrentFloor.RandomlyGenerateRoom(newPosition, 1,UPBranch);
        }
        //RIGHT
        newPosition = new Vector3Int(LocalPosition.x + 1, LocalPosition.y, LocalPosition.z);
        if (RIGHTBranch == Room.BranchType.Hallway1 && Floor.CurrentFloor.tileCoordinates.Contains(newPosition) && !Floor.CurrentFloor.OccupiedCoordinates.Contains(newPosition))
        {
            //Debug.Log("RIGHT expand");
            Floor.CurrentFloor.RandomlyGenerateRoom(newPosition, 2, RIGHTBranch);
        }
        //DOWN
        newPosition = new Vector3Int(LocalPosition.x, LocalPosition.y - 1, LocalPosition.z);
        if (DOWNBranch == Room.BranchType.Hallway1 && Floor.CurrentFloor.tileCoordinates.Contains(newPosition) && !Floor.CurrentFloor.OccupiedCoordinates.Contains(newPosition))
        {
            //Debug.Log("DOWN expand");
            Floor.CurrentFloor.RandomlyGenerateRoom(newPosition, 3,DOWNBranch);
        }
        //LEFT
        newPosition = new Vector3Int(LocalPosition.x - 1, LocalPosition.y, LocalPosition.z);
        if (LEFTBranch == Room.BranchType.Hallway1 && Floor.CurrentFloor.tileCoordinates.Contains(newPosition) && !Floor.CurrentFloor.OccupiedCoordinates.Contains(newPosition))
        {
            //Debug.Log("LEFT expand");
            Floor.CurrentFloor.RandomlyGenerateRoom(newPosition, 4,LEFTBranch);
        }
    }

    //method to instantiate a new room to avoid errors such as no localposition set
    public static GameObject CreateRoom(BranchType U, BranchType R, BranchType D, BranchType L,Vector3Int newPosition)
    {
        //Find and load requested room
        String roomname = "Room";
        roomname += returnstringcase(U, 0);
        roomname += returnstringcase(R, 1);
        roomname += returnstringcase(D, 2);
        roomname += returnstringcase(L, 3);

        //Debug.Log("roomname: " + roomname);
        GameObject newRoom = (GameObject)Resources.Load(roomname);
        if (newRoom == null)
            Debug.Log("WARNING: no floor with this name exists in the room resources folder, check spelling or create new room with requested name");

        //instantiate room
        GameObject r = Instantiate(newRoom, Floor.CurrentFloor.transform)
                            as GameObject;
        r.gameObject.transform.parent = Floor.CurrentFloor.RoomGrid.transform;//set parent to roomgrid for proper sizing
        r.GetComponent<Room>().LocalPosition = newPosition;
        r.GetComponent<Room>().EnableGeneration = false;//prevent spontaneous genaration
        r.transform.position = (Vector2)Floor.CurrentFloor.ZonesTilemap.GetCellCenterWorld(newPosition) + new Vector2(Floor.CurrentFloor.Xoffset, Floor.CurrentFloor.Yoffset);//set room position to zone tile position

        return r;
    }

    //Changes selected room into desired room type
    public void ChangeRoom(BranchType U, BranchType R, BranchType D, BranchType L)
    {
        //Find and load requested room prefab
        String roomname = "Room";
        roomname += returnstringcase(U, 0);
        roomname += returnstringcase(R, 1);
        roomname += returnstringcase(D, 2);
        roomname += returnstringcase(L, 3);

        //Debug.Log("roomname: " + roomname);
        GameObject newRoom = (GameObject)Resources.Load(roomname);
        if (newRoom == null)
            Debug.Log("WARNING: no floor with this name exists in the room resources folder, check spelling or create new room with requested name");

        //replace this room with new room
        GameObject r = Instantiate(newRoom, this.transform)
                            as GameObject;
        r.gameObject.transform.parent = Floor.CurrentFloor.RoomGrid.transform;//set parent to roomgrid for proper sizing
        r.GetComponent<Room>().LocalPosition = LocalPosition;//pass on room position
        r.GetComponent<Room>().EnableGeneration = false;//prevent spontaneous genaration
        r.transform.position = (Vector2)Floor.CurrentFloor.ZonesTilemap.GetCellCenterWorld(LocalPosition) + new Vector2(Floor.CurrentFloor.Xoffset, Floor.CurrentFloor.Yoffset);//set room position to zone tile position
        Destroy(gameObject);
    }
    //internal method used in change room branch
    private static string returnstringcase(BranchType branch, int direction)
    {
        string s="";
        //check direction of branch 
        //0-up, 1-right, 2-down, 4-left
        switch (direction)
        {
            case 0:
                s = "UP";
                break;
            case 1:
                s = "RIGHT";
                break;
            case 2:
                s = "DOWN";
                break;
            case 3:
                s = "LEFT";
                break;

        }

        //check type of branch
        switch (branch) {
            case BranchType.Closed:
                return "";//if a closed path no text will be appended
            case BranchType.Open:
                s = s+ "O";
                break;
            case BranchType.Hallway3:
                s = s + "3";
                break;
            default:
                break; 
        }
        return s;

    }

    public void ChangeBranch(int branch, BranchType bt)
    {
      switch (branch)//analogous to enteringfromArr
        {
            case 1:
                
                ChangeRoom(bt, RIGHTBranch, DOWNBranch, LEFTBranch);
                break;
            case 2:
                ChangeRoom(UPBranch, bt, DOWNBranch, LEFTBranch);
                break;
            case 3:
                ChangeRoom(UPBranch, RIGHTBranch, bt, LEFTBranch);
                break;
            case 4:
                ChangeRoom(UPBranch, RIGHTBranch, DOWNBranch, bt); 
                break;
            default:
                Debug.Log("WARNING: enteringFrom not properly assigned");
                break;
        }


    }


}
