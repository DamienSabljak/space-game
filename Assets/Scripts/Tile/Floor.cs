using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/*The floor generation process is as follows: the laid out zone tiles are collected and sorted into lists in the InitFloor() method, start room generation is called, the number of rooms
set by numRoom is checked and docked per each call of RandomlyGenerateRoom(), any rooms that were collected as "connected zones" in the InitFloor() method AND arent currently connected 
are then connect by ConnectConnectedZones()

    */
public class Floor : MonoBehaviour {
    //used for other methods waiting for rooms to be generated
    public static bool RoomsHaveBeenGenerated = false;//set to true when generation complete

    public static Floor CurrentFloor;
    [SerializeField] SpawnManager spawnManager;
    [SerializeField] Floor FloorStart;
    [SerializeField] public Grid RoomGrid; //grid to hold all room tilemaps in 
    [SerializeField] public GameObject FirstRoom;
    [SerializeField] public GameObject TestRoom;//used for testing
    [SerializeField] public bool SpecialRoomsMustBeConnected;

    [Header("ZoneTiles")]  
    [SerializeField] public Tile NormalZoneTile;
    [SerializeField] public Tile StartingZoneTile;
    [SerializeField] public Tile ConnectingZoneTile;
    [SerializeField] public Tile EngineZoneTile;
    [SerializeField] public Tile BridgeZoneTile;
    //Steps to add a new zone: 1)make a zone tile 2)add zone tile to serialized fields above 3)add tile zones list
    //4)add if statement in initfloor() method 5) add if statmenet in Room class SetRoomLayout() Method 6) add zone enum in Room class Type enum...
    public int NumRooms = 3;//may be less due to rooms self-closing, will be affected by #of dungeons previously completed **IGNORED BY "CONNECTED ZONE" TILES**
    [HideInInspector] public Tilemap ZonesTilemap; //tilemap to hold zone tiles
    [SerializeField] public float Xoffset=0;//used to offset room tiles uniformly
    [SerializeField] public float Yoffset=0;
    [SerializeField] List<GameObject> NormalRoomTypes;

    [Header("Room Layouts")]
    [SerializeField] public GameObject RoomLayout;
    [SerializeField] public GameObject EngineRoomLayout;
    [SerializeField] public GameObject BridgeRoomLayout;

    //used for Room generation
    public TileBase[] tileArr;//holds tile type 
    public List<Vector3Int> tileCoordinates = new List<Vector3Int>();//holds tile position
    public List<Vector3Int> OccupiedCoordinates = new List<Vector3Int>();
    //zones lists contain position refferences to rooms of that zone
    public List<Vector3Int> ConnectedZones = new List<Vector3Int>();
    public List<Vector3Int> EngineZones = new List<Vector3Int>();
    public List<Vector3Int> BridgeZones = new List<Vector3Int>();
    private bool ReadyForNextConnectedZone = true;//used to space out corutines
    public Vector3Int StartZoneLocation;//holds location of starting zone tile if placed

    
    // Use this for initialization
    void Start ()
    {
        NumRooms = LevelManager.currentLevelnum;
        RoomsHaveBeenGenerated = false;//reset for new scene loads
        InitFloor();
        StartRoomGeneration();
        StartCoroutine(ConnectConnectedZones());
        //StartCoroutine(ConnectConnectedZone(ConnectedZones[0]));
        
    }

    

    private void InitFloor()
    {
        CurrentFloor = FloorStart;
        ZonesTilemap = gameObject.GetComponent<Tilemap>();

        //pull tilemap into array and list for later refferencing
        tileArr = ZonesTilemap.GetTilesBlock(ZonesTilemap.cellBounds);
        foreach (var position in ZonesTilemap.cellBounds.allPositionsWithin)
        {
            if (ZonesTilemap.HasTile(position))
                tileCoordinates.Add(position);
            //find and add starting zone if applicable
            if (ZonesTilemap.GetTile(position) == StartingZoneTile)
                StartZoneLocation = position;
            //find and add connecting zone if applicable
            if (ZonesTilemap.GetTile(position) == ConnectingZoneTile)
                ConnectedZones.Add(position);
            //find and add connecting zone if applicable
            if (ZonesTilemap.GetTile(position) == EngineZoneTile)
            {
                EngineZones.Add(position);
                if(SpecialRoomsMustBeConnected)
                    ConnectedZones.Add(position);//engine zones are needed 
            }
            if (ZonesTilemap.GetTile(position) == BridgeZoneTile)
            {
                BridgeZones.Add(position);
                if (SpecialRoomsMustBeConnected)
                    ConnectedZones.Add(position);//engine zones are needed 
            }
        }
    }

    // Update is called once per frame
    void Update () {
		
	}

    private void StartRoomGeneration()
    {
        Room.BranchType h1 = Room.BranchType.Hallway1;
        //if a starting tile is placed, start generation there
        if (StartZoneLocation != null)
            RandomlyGenerateRoom(StartZoneLocation,0, h1);
        //otherwise start at center of grid
        else
        RandomlyGenerateRoom(tileCoordinates[tileCoordinates.Count / 2], 0,h1);//choose starting point for room generation
    }

    public void RandomlyGenerateRoom(Vector3Int RoomPosition, int EnteringFrom, Room.BranchType EnteringType)
    {
        //wait for initial loading of rooms
        

        //Entering from will hold value of direction needed to be open from recursive call
        //0 == FIRSTROOM //1 = UP //2 = RIGHT //3 = DOWN //4 = LEFT
        if (NumRooms >= 0)
        {
            NumRooms -= 1;
            Debug.Log("NumRooms:" + NumRooms);
            OccupiedCoordinates.Add(RoomPosition);//remove position to avoid overites
            //Debug.Log("generating room, number of rooms left: " + NumberOfRooms);
            GameObject r;
            //Determine if this is first room or not 
            if (EnteringFrom == 0)
            {//Instantiate selected first room
                r = Instantiate(FirstRoom.gameObject, RoomGrid.transform)
                            as GameObject;
                Floor.CurrentFloor.OccupiedCoordinates.Add(RoomPosition);
                r.GetComponent<Room>().EnableGeneration = true;
            }
            else
            {//Instantate random room
                //make sure room is acceptable based on previous accessor
                bool acceptableRoomFound = false;
                int randomint = UnityEngine.Random.Range(0, NormalRoomTypes.Count-1);//pick random room
                for (int i=0;i<NormalRoomTypes.Count-1;i++)
                {
                    NormalRoomTypes[randomint].GetComponent<Room>().InitBranchEnterArr();//init arr to access it from prefab

                    if(NormalRoomTypes[randomint].GetComponent<Room>().BranchEnterArr[EnteringFrom-1] == EnteringType)//check if selected room is acceptable
                    {//room is acceptable
                        acceptableRoomFound = true;
                        break;
                    }
                    else
                    {//test next room in array
                        randomint += 1;
                        if (randomint >= NormalRoomTypes.Count)//wrap around array if needed
                            randomint = 0;
                    }
                }
                if (acceptableRoomFound)
                {
                    r = Instantiate(NormalRoomTypes[randomint].gameObject, RoomGrid.transform)
                                as GameObject;
                }
                else
                {
                    Debug.Log("ERROR: NO ACCEPTABLE ROOM FOUND: using starting room instead");
                    r = Instantiate(FirstRoom.gameObject, RoomGrid.transform)
                            as GameObject;
                }
            }
            //set proper position
            r.gameObject.transform.parent = RoomGrid.transform;//set parent to roomgrid for proper sizing
            r.transform.position = (Vector2)ZonesTilemap.GetCellCenterWorld(RoomPosition) + new Vector2(Xoffset, Yoffset);//set room position to zone tile position

            //update room class
            Room room = r.GetComponent<Room>();
            room.LocalPosition = RoomPosition;
            room.EnableGeneration = true;//continue propogation

            //Determine Room type and assign it
            if (EngineZones.Contains(RoomPosition))
            {
                room.Type = Room.RoomType.Engine;
            }

            else
                room.Type = Room.RoomType.Normal;

            //TESTING
            //Room.BranchType bt = Room.BranchType.Hallway1;
            //Room.BranchType cl = Room.BranchType.Closed;
            //room.ChangeRoom(bt,cl,bt,bt);
            //More rooms will be spawned automatically by start method of each new floor
        }
        else
        {
            //No more rooms left to make
        }
    }

    private IEnumerator ConnectConnectedZones()
    {
        yield return new WaitForSeconds(2);//wait for room loading

        foreach (Vector3Int connectedzone in ConnectedZones)
        {
            StartCoroutine(ConnectConnectedZone(connectedzone));
            //wait for zone to be connected
            while (!ReadyForNextConnectedZone)
                yield return null;
            yield return new WaitForSeconds(1);//wait for room loading
        }

        AstarPath.active.Scan();//updateNavMesh;
        //*****ONLY START PLACING SPAWN CLUSTERS AFTER ROOMS ARE GENERATED******
        spawnManager.PlaceScrapSpawnClusters();
        RoomsHaveBeenGenerated = true;
    }

    //connect generated floor to Connected zones (must be connected)
    public IEnumerator ConnectConnectedZone(Vector3Int connectedZone)
    {
        ReadyForNextConnectedZone = false;
        //wait for room loading
        //yield return new WaitForSeconds(1);

        //Check if already connected
        if (!OccupiedCoordinates.Contains(connectedZone))
        {
            //find closest occupied zones
            Vector3Int closestCoordinate = OccupiedCoordinates[0];
            if (closestCoordinate == null)
                Debug.Log("ERROR: OCCUPIED COORDINATES LIST EMPTY");
            foreach (Vector3Int coordinate in OccupiedCoordinates)
            {
                //if the next tile is closer, replace it 
                if (Mathf.Abs(coordinate.x - connectedZone.x) + Mathf.Abs(coordinate.y - connectedZone.y) < Mathf.Abs(closestCoordinate.x - connectedZone.x) + Mathf.Abs(closestCoordinate.y - connectedZone.y))
                {
                    //Debug.Log("new closest room found");
                    closestCoordinate = coordinate;
                }
            }
            //TESTING 
            //Debug.Log(closestCoordinate);
            //FindRoomAtZone(closestCoordinate).ChangeRoom(Room.BranchType.Open, Room.BranchType.Open, Room.BranchType.Open, Room.BranchType.Open);
            
            //start connecting the two rooms together
            //while not at connected zone yet
            GameObject r; bool wasmoveX=false, wasmoveY=false;
            Room.BranchType newBT = Room.BranchType.Hallway1;
            Vector3Int XMoveRoom = closestCoordinate;//refference to be  used later to open rooms
            Vector3Int YMoveRoom = closestCoordinate;//refference to be  used later to open rooms
            Vector3Int LastMoveRoom = closestCoordinate;//refference to be  used later to open rooms
            int EnteringFromX, EnteringFromY;//determine needed entrance for X and Y expansions
            int i = 0;//used for timeout
            while (closestCoordinate.x != connectedZone.x || closestCoordinate.y != connectedZone.y || i==100)
            {
                i++;
                yield return new WaitForEndOfFrame();//Required to allow time to update

                LastMoveRoom = closestCoordinate;
                //move towards room, open up or instantiate
                //**move towards in x direction**
                if (closestCoordinate.x < connectedZone.x)
                {
                    closestCoordinate.x += 1;
                    wasmoveX = true;
                    EnteringFromX = 4;
                }
                else if (closestCoordinate.x > connectedZone.x)
                {
                    closestCoordinate.x -= 1;
                    wasmoveX = true;
                    EnteringFromX = 2;
                }
                else
                {
                    wasmoveX = false;
                    EnteringFromX = 5;//used for error message
                }
                //check if room exists
                //Debug.Log("Xmove: " + wasmoveX);
                if (wasmoveX)
                {
                    //replace this room with new room
                    int j = 0;//used for infinite loop timeout
                    while (FindRoomAtZone(closestCoordinate) != null && j < 20)
                    {
                        Destroy(FindRoomAtZone(closestCoordinate));
                        j++;
                    }
                    if(j>=20)
                        Debug.Log("Warning: while loop has timed out:");
                    j = 0;

                    //WARNING: THIS IS USING UPROOM AS START WHICH MAYCAUSE UNDERSIRED EFFECTS, SAME FOR y PART
                    r = Room.CreateRoom(Room.BranchType.Hallway1, Room.BranchType.Closed, Room.BranchType.Closed, Room.BranchType.Closed, closestCoordinate);
                    XMoveRoom = closestCoordinate;//hold for branch changes at end
                    Room Xroom = FindRoomAtZone(XMoveRoom);
                    Xroom.ChangeBranch(EnteringFromX, newBT);//connect new room to old room using hallway1 entrance

                    yield return new WaitForEndOfFrame();//Required to allow time to update

                    Room Lastroom = FindRoomAtZone(LastMoveRoom);//connect old room to new room as well
                    if (EnteringFromX == 2)
                        Lastroom.ChangeBranch(4, newBT); 
                    else if(EnteringFromX == 4)
                        Lastroom.ChangeBranch(2, newBT);//connect old room to new room as well
                    
                }
                LastMoveRoom = closestCoordinate;

                //**do same for y direction**
                if (closestCoordinate.y < connectedZone.y)
                {
                    closestCoordinate.y += 1;
                    wasmoveY = true;
                    EnteringFromY = 3;
                }
                else if (closestCoordinate.y > connectedZone.y)
                {
                    closestCoordinate.y -= 1;
                    wasmoveY = true;
                    EnteringFromY = 1;
                }
                else
                {
                    wasmoveY = false;
                    EnteringFromY = 9;
                }
                //Debug.Log("Ymove: " + wasmoveY);
                if (wasmoveY)
                {
                    //replace this room with new room
                    int j = 0;//used for infinite loop timeout
                    while (FindRoomAtZone(closestCoordinate) != null && j<100)
                    {
                        Destroy(FindRoomAtZone(closestCoordinate));
                        j++;
                    }
                    if (j >= 20)
                        Debug.Log("Warning: while loop has timed out:");
                    j = 0;

                    //WARNING: THIS IS USING UPROOM AS START WHICH MAYCAUSE UNDERSIRED EFFECTS, SAME FOR X PART
                    r = Room.CreateRoom(Room.BranchType.Hallway1, Room.BranchType.Closed, Room.BranchType.Closed, Room.BranchType.Closed, closestCoordinate);
                    YMoveRoom = closestCoordinate;
                    Room Yroom = FindRoomAtZone(YMoveRoom);
                    Yroom.ChangeBranch(EnteringFromY, newBT);//connect new room to old room using hallway1 entrance

                    yield return new WaitForEndOfFrame();//Required to allow time to update

                    Room LastRoom = FindRoomAtZone(XMoveRoom);//connect old room to new room as well
                    if (EnteringFromY == 1)
                        LastRoom.ChangeBranch(3, newBT);
                    else if (EnteringFromY == 3)
                        LastRoom.ChangeBranch(1, newBT);//connect old room to new room as well
                }

                //yield return new WaitForSeconds(2);//Delay for debugging
                //Continue until connectedZone is reached
                
            }//end While
            if (i == 100)
                Debug.Log("while loop has timed out");
        }//end if(contained in occupied zone)
        ReadyForNextConnectedZone = true;
    }

    public Room FindRoomAtZone(Vector3Int zone)
    {
        foreach(Transform room in RoomGrid.transform)
        {

            if(room.gameObject.GetComponent<Room>() !=null && room.gameObject.GetComponent<Room>().LocalPosition == zone)
            {
                //return room 
                return room.GetComponent<Room>();
            }
        }
        //if no room is there return null
        //Debug.Log("WARNING: FindRoomAtZone() found no results"); //this is okay when method called in while loop to destroy all existing rooms
        return null;
    }


    //*************************************************
    //**OLD** method, fills all rooms with starting room
    public void PopulateRooms()
    {
        //pull tilemap into array and list
        TileBase[] tileArr = ZonesTilemap.GetTilesBlock(ZonesTilemap.cellBounds);//holds tile type
        var tileCoordinates = new List<Vector3Int>();//holds tile position
        Debug.Log(tileArr.Length);
        Debug.Log("Tile coords count: " + tileCoordinates.Count);

        foreach (var position in ZonesTilemap.cellBounds.allPositionsWithin)
        {
            if (ZonesTilemap.HasTile(position))
                tileCoordinates.Add(position);

        }

        //instantiate rooms
        for (int i = 0; i < tileArr.Length; i++)
        {
            if (tileArr[i] == EngineZoneTile)
            {
                Debug.Log("engine tile detected");
            }
            else if (tileArr[i] == NormalZoneTile)
            {
                Debug.Log("normal tile detected");

                Tile tile = (Tile)tileArr[i];


                GameObject room = Instantiate(FirstRoom.gameObject, RoomGrid.transform)
                    as GameObject;
                room.gameObject.transform.parent = RoomGrid.transform;//set parent to roomgrid for proper sizing
                room.transform.position = (Vector2)ZonesTilemap.GetCellCenterWorld(tileCoordinates[i]) + new Vector2(Xoffset, Yoffset);//set room position to zone tile position

            }
        }
    }


}
