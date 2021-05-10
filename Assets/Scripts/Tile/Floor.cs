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
    [Header("ZoneTiles")]  //determines which tiles in zone tilemap correspond to which zones 
    [SerializeField] public Tile NormalZoneTile;
    [SerializeField] public Tile StartingZoneTile;
    [SerializeField] public Tile ConnectingZoneTile;
    [SerializeField] public Tile EngineZoneTile;
    [SerializeField] public Tile BridgeZoneTile;
    //Steps to add a new zone: 1)make a zone tile 2)add zone tile to serialized fields above 3)add tile zones list
    //4)add if statement in initfloor() method 5) add if statmenet in Room class SetRoomLayout() Method 6) add zone enum in Room class Type enum...
    public int NumRooms = 100;//max number of rooms to be generated
    [HideInInspector] public Tilemap ZonesTilemap; //tilemap to hold zone tiles
    [SerializeField] public float xoffset=0;//used to offset room tiles uniformly
    [SerializeField] public float yoffset=0;
    [SerializeField] List<GameObject> NormalRoomTypes;
    [SerializeField] GameObject RoomPrefab;//reference to room prefab to be used to generate all rooms 

    [Header("Room Layouts")]//prefabs to room layouts 
    [SerializeField] public List<GameObject> NormalRoomLayouts;
    [SerializeField] public GameObject EngineRoomLayout;
    [SerializeField] public GameObject BridgeRoomLayout;

    //used for Room generation
    public TileBase[] tileArr;//holds tile type 
    public List<Vector3Int> tileCoordinates = new List<Vector3Int>();//holds tile position
    public List<Vector3Int> OccupiedCoordinates = new List<Vector3Int>();//use 3D vector for easy transform assignment 
    //zones lists contain position refferences to rooms of that zone
    public List<Vector3Int> ConnectedZones = new List<Vector3Int>();
    public List<Vector3Int> EngineZones = new List<Vector3Int>();
    public List<Vector3Int> BridgeZones = new List<Vector3Int>();
    private bool ReadyForNextConnectedZone = true;//used to space out corutines
    public Vector3Int firstRoom;//room adjacent to entrance, updated by TranslateFloor() method 
    public BSPTree tree;
    
    // Use this for initialization
    void Start ()
    {
        //NumRooms = LevelManager.currentLevelnum;//create 1 extra floor per level 
        RoomsHaveBeenGenerated = false;//reset for new scene loads
        ZonesTilemap = gameObject.GetComponent<Tilemap>();
        int floorWidth = 20;//coordinate values will be from ([0,floorWidth],[0,floorheight])
        int floorHeight = 10;
        GenerateFloor(floorWidth, floorHeight);
        DecorateFloor(floorWidth, floorHeight);
        //OLD CODE
        //InitFloor();
        //StartRoomGeneration();
        //StartCoroutine(ConnectConnectedZones());
        
    }

    // Update is called once per frame
    void Update () {
		
	}
    public Room FindRoomAtZone(Vector3Int coord)
    {
        foreach(Transform room in RoomGrid.transform)
        {   //cycle all rooms in RoomGrid Object 

            if(room.gameObject.GetComponent<Room>() !=null && room.gameObject.GetComponent<Room>().RoomWasGenerated && room.gameObject.GetComponent<Room>().LocalPosition == coord)
            {
                //return room 
                return room.GetComponent<Room>();
            }
        }
        //if no room is there return null
        //Debug.Log("WARNING: FindRoomAtZone() found no results"); //this is okay when method called in while loop to destroy all existing rooms
        return null;
    }

    private void GenerateFloor(int floorWidth, int floorHeight)
    {   //use BSP based method to generate floor 
        
        int maxSplits = 5;
        int minPartitionSize = 1;
        int numThickIterations = 1;//adds extra rooms to thicken floor 
        int maxThickRooms = 5;
        tree = new BSPTree(floorWidth, floorHeight);
        tree.GenerateTree(maxSplits, minPartitionSize);//partition floor into multiple split zones 

        //Debug.Log(tree.childNodes.Count);
        //create a room in each region
        for (int i = 0; i< tree.childNodes.Count ; i++)
        {
            BSPNode child = tree.childNodes[i];
            //Debug.Log(tree.childNodes.Count);
            //Debug.Log("now generating room from each region");
            //Debug.Log(child.RegionCoords[0]);
            //Debug.Log(child.RegionCoords[1]);
            GenerateRoomFromRegion(child);
        }
        //Debug.Log("occupied Zones:");
        //Debug.Log(OccupiedCoordinates.Count);
        //connect each room
        //Debug.Log("now connecting rooms:");
        RecurseConnectRooms(tree.headNode,floorWidth, floorHeight);
        //Debug.Log("occupied Zones:");
        //Debug.Log(OccupiedCoordinates.Count);
        //add extra rooms to make floor less skinny
        ThickenRooms(numThickIterations, maxThickRooms, floorWidth, floorHeight);

        //instantiate all rooms
        TranslateFloor();
        CreateRoomAtAllOccupiedZones();
        Debug.Log("floor generation complete");
    }
    
    private void DecorateFloor(int floorWidth, int floorHeight)
    {
        Debug.Log("~~~~~starting floor decoration~~~~~");
        gameObject.GetComponent<InteriorDecorator>().ConvertOccupiedZonesToImage(OccupiedCoordinates, floorWidth, floorHeight);
        gameObject.GetComponent<InteriorDecorator>().DecorateFloor(tree.childNodes);
        Debug.Log("~~~~~floor decoration complete ~~~~~");
    }

    private void TranslateFloor()
    {//finds the lowest room in the floor and translates the floor such that it is in front of the entrance 
        Vector2 offset = new Vector2(-1008.2f, -429.97f);//offset which aligned room coord 16,1
        //room grid size is 72x72
        //16*72 + b= -1008.2, b = -2160.2 (this should be x offset)
        //1*72 + b = -429.97, b = -501.97
        //find lowest room
        Vector3Int lowestRoom = OccupiedCoordinates[0];
        foreach (Vector3Int coord in OccupiedCoordinates)
        {
            if(coord.y < lowestRoom.y)
            {
                lowestRoom = coord;
            }
            firstRoom = lowestRoom;//update global reference for other methods 
        }
        Debug.Log("lowest room is:");
        Debug.Log(lowestRoom);
        //translate gride such that lowest room aligns with start room 
        Vector3 newGridLocation = new Vector3(-1*lowestRoom.x*72 + xoffset, -1 * lowestRoom.y*72 + yoffset);
        ZonesTilemap.transform.position = newGridLocation;
    }
    private void GenerateRoomFromRegion(BSPNode child)
    {   //takes a defined region and creates a room inside of it
        //currently only creates 1x1 rooms
        //Debug.Log("generatingRoomFromRegion");
        List<Vector2Int> region = child.RegionCoords;
        int STARTCOORD = 0;
        int ENDCOORD = 1;
        //define random subset region within region 
        int randPosx_1 = UnityEngine.Random.Range(region[STARTCOORD].x, region[ENDCOORD].x);
        int randPosy_1 = UnityEngine.Random.Range(region[STARTCOORD].y, region[ENDCOORD].y);
        int randPosx_2 = UnityEngine.Random.Range(randPosx_1, region[ENDCOORD].x);
        int randPosy_2 = UnityEngine.Random.Range(randPosy_1, region[ENDCOORD].y);
        //fill subset region with rooms
        for(int x = randPosx_1; x<=randPosx_2;x++)
        {
            for (int y = randPosy_1; y <= randPosy_2; y++)
            {
                OccupiedCoordinates.Add(new Vector3Int(x, y, 0));
            }
        }
        child.PopulatedRegionCoords.Add(new Vector2Int(randPosx_1, randPosy_1));
        child.PopulatedRegionCoords.Add(new Vector2Int(randPosx_2, randPosy_2));
        //Debug.Log("room generated with populated region:");
        //Debug.Log(child.PopulatedRegionCoords[STARTCOORD]);
        //Debug.Log(child.PopulatedRegionCoords[ENDCOORD]);
        //GenerateRoom(new Vector3Int(randPosx, randPosy, 0)); //all coordinates from occupied zones will be instantiated
    }
    private bool ThickenRooms(int numIterations,int maxThickRooms, int maxWidth, int maxHeight)
    {//adds extra rooms in empty spaces where there are two or more adjacent rooms 
        //returns nothing, just to exit function
        int remainingRooms = maxThickRooms;
        for(int iter=0; iter<numIterations;iter++)
        {
            for (int x = 0; x <= maxWidth; x++)
            {
                for (int y = 0; y <= maxHeight; y++)
                {
                    if (!CoordIsOccupied(new Vector2Int(x, y)))
                    {
                        int numAdjacentRooms = 0;
                        if (CoordIsOccupied(new Vector2Int(x + 1, y))) { numAdjacentRooms++; }
                        if (CoordIsOccupied(new Vector2Int(x - 1, y))) { numAdjacentRooms++; }
                        if (CoordIsOccupied(new Vector2Int(x, y + 1))) { numAdjacentRooms++; }
                        if (CoordIsOccupied(new Vector2Int(x, y - 1))) { numAdjacentRooms++; }
                        if (numAdjacentRooms >= 2)
                        {
                            OccupiedCoordinates.Add(new Vector3Int(x, y, 0));
                            remainingRooms--;
                        }
                        if (remainingRooms <= 0)
                        {
                            return false;
                        }

                    }
                }
            }
        }
        return true;
    }
    private void RecurseConnectRooms(BSPNode node, int maxWidth, int maxHeight)
    {//use recersive algorithm to connect rooms together 
        if(node.childNodes != null && node.childNodes.Count > 0)
        {
            //Debug.Log("down left");
            RecurseConnectRooms(node.childNodes[0], maxWidth, maxHeight);
            //Debug.Log("down right");
            RecurseConnectRooms(node.childNodes[1], maxWidth, maxHeight);
            //Debug.Log("connect");
            ConnectRegions(node.childNodes[0], node.childNodes[1], maxWidth, maxHeight);
        }

    }
    private void ConnectRegions(BSPNode child1, BSPNode child2, int maxWidth, int maxHeight)
    {
        //Debug.Log("connecting children at coordinates: (not shown)");
        //Debug.Log(child1.PopulatedRegionCoords.Count);
        //Debug.Log(child2.PopulatedRegionCoords.Count);
        Debug.Log(child1.PopulatedRegionCoords[0]);
        Debug.Log(child1.PopulatedRegionCoords[1]);
        Debug.Log(child2.PopulatedRegionCoords[0]);
        Debug.Log(child2.PopulatedRegionCoords[1]);
        int STARTCOORD = 0;
        int ENDCOORD = 1;

        int axisSolution = -10;
        if (child1.ParentNode.splitType == "x" )//nodes are vertical 
        {
            axisSolution = FindStraightConnectionAxis(child1, child2, "x");
            if (axisSolution != -10)//straight connect or adjacent
            {   //NOTE: FIX THIS, REGIONS MAY BE CONNECTED BUT NOT ATTATCHED 
                //try straight connect, otherwise L connect
               
                if (!RoomsFromZonesAreAdjacent(child1, child2))
                {
                    StraightGrowUntilConnectionMade(child1, child2, child1.ParentNode.splitType, axisSolution, maxWidth, maxHeight);
                }
                else
                {
                    //Debug.Log("rooms are already adjacent!");
                }
            }
            else
            {
                LGrowUntilConnectionMade(child1, child2, child1.ParentNode.splitType, maxWidth, maxHeight);
            }
            //add new stretch to parent populated area 
            if (Region1IsMoreRight(child1.PopulatedRegionCoords, child2.PopulatedRegionCoords))
            {
                Vector2Int parentStartNode = new Vector2Int(child2.PopulatedRegionCoords[STARTCOORD].x, child1.PopulatedRegionCoords[STARTCOORD].y);
                Vector2Int parentEndNode = new Vector2Int(child1.PopulatedRegionCoords[ENDCOORD].x, child2.PopulatedRegionCoords[ENDCOORD].y);
                child1.ParentNode.PopulatedRegionCoords.Add(parentStartNode);
                child1.ParentNode.PopulatedRegionCoords.Add(parentEndNode);
            }
            else
            {
                child1.ParentNode.PopulatedRegionCoords.Add(child1.PopulatedRegionCoords[STARTCOORD]);
                child1.ParentNode.PopulatedRegionCoords.Add(child2.PopulatedRegionCoords[ENDCOORD]);
            }
        }
        if (child1.ParentNode.splitType == "y")//nodes are horizontal
        {
            axisSolution = FindStraightConnectionAxis(child1, child2, "y");
            if (axisSolution != -10)//straight connect or adjacent 
            {
                
                if (!RoomsFromZonesAreAdjacent(child1, child2))//need to straight grow 
                {//NOTE: FIX THIS, REGIONS MAY BE CONNECTED BUT NOT ATTATCHED 
                    StraightGrowUntilConnectionMade(child1, child2, child1.ParentNode.splitType, axisSolution, maxWidth, maxHeight);
                }
                else
                {
                    //Debug.Log("rooms are already adjacent!");
                    
                }
            }
            else
            {
                LGrowUntilConnectionMade(child1, child2, child1.ParentNode.splitType, maxWidth, maxHeight);
            }
            //add new stretch to parent populated area (NOTE:similar to "x" split case, but switch nodes positions 
            if (Region1IsMoreUp(child1.PopulatedRegionCoords, child2.PopulatedRegionCoords))
            {
                Vector2Int parentStartNode = new Vector2Int(child1.PopulatedRegionCoords[STARTCOORD].x, child2.PopulatedRegionCoords[STARTCOORD].y);
                Vector2Int parentEndNode = new Vector2Int(child2.PopulatedRegionCoords[ENDCOORD].x, child1.PopulatedRegionCoords[ENDCOORD].y);
                child1.ParentNode.PopulatedRegionCoords.Add(parentStartNode);
                child1.ParentNode.PopulatedRegionCoords.Add(parentEndNode);
            }
            else
            {
                child1.ParentNode.PopulatedRegionCoords.Add(child1.PopulatedRegionCoords[STARTCOORD]);
                child1.ParentNode.PopulatedRegionCoords.Add(child2.PopulatedRegionCoords[ENDCOORD]);
            }
        }
    }
    private bool CoordIsOccupied(Vector2Int location)
    {   //checks if location is occupied (contained in OccupiedCoordinates list)
        foreach (Vector3Int coord in OccupiedCoordinates)
        {
            if(location.x == coord.x && location.y == coord.y)
            {
                //Debug.Log("location is occupied:");
                //Debug.Log(location);
                return true;
            }
        }
        return false;
    }
    private int FindStraightConnectionAxis(BSPNode node1, BSPNode node2, string splitType)
    {//determines if two populated regions can be connected through a straight line and provides a solution axis 
        int STARTCOORD = 0;
        int ENDCOORD = 1;
        int axisToConnect = -10;//will change if solution found 
        if (splitType == "x")
        {
            if(node1.PopulatedRegionCoords[STARTCOORD].x <= node2.PopulatedRegionCoords[ENDCOORD].x)
            {
                if(node1.PopulatedRegionCoords[ENDCOORD].x >= node2.PopulatedRegionCoords[STARTCOORD].x)
                {   //node can be straight connected 
                    if (node1.PopulatedRegionCoords[STARTCOORD].x > node2.PopulatedRegionCoords[STARTCOORD].x && node1.PopulatedRegionCoords[ENDCOORD].x < node2.PopulatedRegionCoords[ENDCOORD].x)
                    {   //node 1 is smaller and within node 2
                        axisToConnect = UnityEngine.Random.Range(node1.PopulatedRegionCoords[STARTCOORD].x, node1.PopulatedRegionCoords[ENDCOORD].x);
                    }
                    else if (node2.PopulatedRegionCoords[STARTCOORD].x > node1.PopulatedRegionCoords[STARTCOORD].x && node2.PopulatedRegionCoords[ENDCOORD].x < node1.PopulatedRegionCoords[ENDCOORD].x)
                    {   //node 2 is smaller and within node 1
                        axisToConnect = UnityEngine.Random.Range(node2.PopulatedRegionCoords[STARTCOORD].x, node2.PopulatedRegionCoords[ENDCOORD].x);
                    }
                    else if (node1.PopulatedRegionCoords[STARTCOORD].x <= node2.PopulatedRegionCoords[STARTCOORD].x)
                    {//node 1 is higher
                        axisToConnect = UnityEngine.Random.Range(node2.PopulatedRegionCoords[STARTCOORD].x, node1.PopulatedRegionCoords[ENDCOORD].x);
                    }
                    else if (node1.PopulatedRegionCoords[STARTCOORD].x >= node2.PopulatedRegionCoords[STARTCOORD].x)
                    {//node 2 is higher 
                        axisToConnect = UnityEngine.Random.Range(node1.PopulatedRegionCoords[STARTCOORD].x, node2.PopulatedRegionCoords[ENDCOORD].x);
                    }
                    //Debug.Log("can be straight connected with axis:");
                    //Debug.Log(axisToConnect);
                }
            }
        }
        else if (splitType == "y")
        {
            if (node1.PopulatedRegionCoords[STARTCOORD].y <= node2.PopulatedRegionCoords[ENDCOORD].y)
            {
                if (node1.PopulatedRegionCoords[ENDCOORD].y >= node2.PopulatedRegionCoords[STARTCOORD].y)
                {   //node can be straight connected 
                    if (node1.PopulatedRegionCoords[STARTCOORD].y > node2.PopulatedRegionCoords[STARTCOORD].y && node1.PopulatedRegionCoords[ENDCOORD].y < node2.PopulatedRegionCoords[ENDCOORD].y)
                    {   //node 1 is smaller and within node 2
                        axisToConnect = UnityEngine.Random.Range(node1.PopulatedRegionCoords[STARTCOORD].y, node1.PopulatedRegionCoords[ENDCOORD].y);
                    }
                    else if (node2.PopulatedRegionCoords[STARTCOORD].y > node1.PopulatedRegionCoords[STARTCOORD].y && node2.PopulatedRegionCoords[ENDCOORD].y < node1.PopulatedRegionCoords[ENDCOORD].y)
                    {   //node 2 is smaller and within node 1
                        axisToConnect = UnityEngine.Random.Range(node2.PopulatedRegionCoords[STARTCOORD].y, node2.PopulatedRegionCoords[ENDCOORD].y);
                    }
                    if (node1.PopulatedRegionCoords[STARTCOORD].y <= node2.PopulatedRegionCoords[STARTCOORD].y)
                    {//node 1 is higher
                        axisToConnect = UnityEngine.Random.Range(node2.PopulatedRegionCoords[STARTCOORD].y, node1.PopulatedRegionCoords[ENDCOORD].y);
                    }
                    if (node1.PopulatedRegionCoords[STARTCOORD].y >= node2.PopulatedRegionCoords[STARTCOORD].y)
                    {//node 2 is higher 
                        axisToConnect = UnityEngine.Random.Range(node1.PopulatedRegionCoords[STARTCOORD].y, node2.PopulatedRegionCoords[ENDCOORD].y);
                        
                    }

                    //Debug.Log("can be straight connected with axis:");
                    //Debug.Log(axisToConnect);
                }

            }
        }
        else
        {
            Debug.Log("WRONG SPLIT TYPE ARGUMENT GIVEN, FAKE NUMBER GIVEN");
            Debug.Log("SPLIT TYPE GIVEN");
            Debug.Log(splitType);

            return -99;
        }
        return axisToConnect;
    }
    private void StraightGrowUntilConnectionMade(BSPNode node1, BSPNode node2, string splitType, int axisSolution, int maxWidth, int maxHeight)
    {   //start from top / left and straight grow until another square reached
        //NOTE::: need to make sure starting point is next to an occupied zone to prevent disconnects 
        int STARTCOORD = 0;
        int ENDCOORD = 1;
        int timeout = 5000;
        //Debug.Log("attempting straight connect");
        if (splitType == "x")
        {
            int increment = 0;
            Vector2Int currentPoint;
            //determine which node is rightmost
            if (node2.PopulatedRegionCoords[STARTCOORD].y >= node1.PopulatedRegionCoords[ENDCOORD].y)
            {
                increment = 1;
                currentPoint = new Vector2Int(axisSolution, node2.PopulatedRegionCoords[STARTCOORD].y - increment);
            }
            else
            {
                increment = -1;
                currentPoint = new Vector2Int(axisSolution, node2.PopulatedRegionCoords[STARTCOORD].y - increment);
            }
            //determine current point as first place which is occupied (start at node 2 and work back)
            
            Debug.Log("current point for start search:");
            Debug.Log(currentPoint);
            for (int i = 0; i < timeout; i++)
            {
                
                if (CoordIsOccupied(currentPoint) || !CoordWithinBounds(currentPoint, maxWidth, maxHeight))
                {
                    break;
                }
                currentPoint = new Vector2Int(currentPoint.x, currentPoint.y - increment);//move down 1 

            }
            currentPoint = new Vector2Int(currentPoint.x, currentPoint.y + increment);
            Debug.Log("starting growth at");
            Debug.Log(currentPoint);
            OccupiedCoordinates.Add(new Vector3Int(currentPoint.x, currentPoint.y, 0));
            for(int i=0; i<timeout;i++)
            {
                currentPoint = new Vector2Int(currentPoint.x, currentPoint.y+increment);//move up 1 
                if(CoordIsOccupied(currentPoint) || !CoordWithinBounds(currentPoint, maxWidth, maxHeight))
                {
                    break;
                }
                else
                {
                    //Debug.Log("adding room at: ");
                    //Debug.Log(currentPoint);
                    OccupiedCoordinates.Add(new Vector3Int(currentPoint.x, currentPoint.y, 0));
                }
            }
        }
        if (splitType == "y")
        {
            int increment = 0;
            Vector2Int currentPoint;
            //determine wVector2Int currentPointhich node is rightmost
            if (node2.PopulatedRegionCoords[STARTCOORD].x >= node1.PopulatedRegionCoords[ENDCOORD].x)
            {
                increment = 1;
                currentPoint = new Vector2Int(node2.PopulatedRegionCoords[STARTCOORD].x - increment, axisSolution);
            }
            else
            {
                increment = -1;
                currentPoint = new Vector2Int(node2.PopulatedRegionCoords[ENDCOORD].x - increment, axisSolution);
            }
            //determine current point as first place which is occupied (start at node 2 and work back)
            
            Debug.Log("current point for start search:");
            Debug.Log(currentPoint);
            for (int i = 0; i < timeout; i++)
            { 
                if (CoordIsOccupied(currentPoint) || !CoordWithinBounds(currentPoint, maxWidth, maxHeight))
                {
                    break;
                }
                currentPoint = new Vector2Int(currentPoint.x - increment, currentPoint.y);//move forward 1 
            }
            currentPoint = new Vector2Int(currentPoint.x + increment, currentPoint.y );
            Debug.Log("starting growth at");
            Debug.Log(currentPoint);
            OccupiedCoordinates.Add(new Vector3Int(currentPoint.x, currentPoint.y, 0));
            for (int i = 0; i < timeout; i++)
            {
                currentPoint = new Vector2Int(currentPoint.x +increment, currentPoint.y);//move back 1 
                //Debug.Log("at");
                //Debug.Log(currentPoint);
                if (CoordIsOccupied(currentPoint) || !CoordWithinBounds(currentPoint, maxWidth, maxHeight))
                {
                    break;
                }
                else
                {
                    //Debug.Log("adding room at: ");
                    //Debug.Log(currentPoint);
                    OccupiedCoordinates.Add(new Vector3Int(currentPoint.x, currentPoint.y, 0));
                }
            }
            
        }
    }

    private bool CoordWithinBounds(Vector2Int coord,int maxWidth,int maxHeight)
    { 
        if(coord.x >= 0 && coord.x <=maxWidth && coord.y>=0 && coord.y <= maxHeight)
        {
            return true;
        }
        else
        {
            //Debug.Log("coordinates out of bounds");
            //Debug.Log(coord.x);
            //Debug.Log(coord.y);
            //Debug.Log(maxWidth);
            //Debug.Log(maxHeight);
            return false;
        }
    }
    private int DistanceBetweenCoords(Vector2Int coord1, Vector2Int coord2)
    {   //finds the D4 distance between coords
        int distance = Math.Abs(coord1.x - coord2.x) + Math.Abs(coord1.y - coord2.y);
        return distance;
    }
    private bool RoomsFromZonesAreAdjacent(BSPNode node1, BSPNode node2)
    {//expensive method to see if any two rooms from two node regions are adjacent 
        Debug.Log("check if rooms adjacent");
        foreach(Vector2Int n1 in OccupiedCoordinates)
        {
            if(CoordinateWithinRegion(n1, node1.PopulatedRegionCoords))
            {
                //Debug.Log("n1");
                //Debug.Log(n1);
                foreach (Vector2Int n2 in OccupiedCoordinates)
                {
                    if (CoordinateWithinRegion(n2, node2.PopulatedRegionCoords))
                    {
                        //Debug.Log("n2");
                        //Debug.Log(n2);
                        if (DistanceBetweenCoords(n1, n2)==1)
                        {
                            //Debug.Log("Adjacent!");
                            return true;
                        }
                    }
                }
            }
            
        }
        return false;
    }
    public bool CoordinateWithinRegion(Vector2 coord, List<Vector2Int> region)
    {   //determine if a coordinate is within a specified region
        int STARTCOORD = 0;
        int ENDCOORD = 1;
        if (coord.x>=region[STARTCOORD].x && coord.x <= region[ENDCOORD].x && coord.y >= region[STARTCOORD].y && coord.y <= region[ENDCOORD].y)
        {
            return true;
        }
        return false;
    }
    private void LGrowUntilConnectionMade(BSPNode node1, BSPNode node2, string splitType, int maxWidth, int maxHeight)
    {   //grow left/top most room vertically, then horizontal connect using straight grow
        //first find random solution on node 2 and then grow off node 1 to make that solution possible 
        Debug.Log("LconnectionToBeMade");
        int STARTCOORD = 0;
        int ENDCOORD = 1;
        int timeout = 5000;
        if (splitType == "x" || splitType == "y")
        {
            int axisToConnect_n1 = UnityEngine.Random.Range(node1.PopulatedRegionCoords[STARTCOORD].x, node1.PopulatedRegionCoords[ENDCOORD].x + 1);
            int axisToConnect_n2 = UnityEngine.Random.Range(node2.PopulatedRegionCoords[STARTCOORD].y, node2.PopulatedRegionCoords[ENDCOORD].y + 1);
            Vector2Int CornerPoint = new Vector2Int(axisToConnect_n1, axisToConnect_n2);
            Debug.Log("corner point is");
            Debug.Log(CornerPoint);
            //determine increment direction
            int increment;
            if(node1.PopulatedRegionCoords[STARTCOORD].y >= axisToConnect_n2)
            {
                increment = -1;
            }
            else
            {
                increment = 1;
            }
            Vector2Int currentPoint = new Vector2Int(axisToConnect_n1, node1.PopulatedRegionCoords[ENDCOORD].y+increment); 
            for (int i = 0; i < timeout; i++)
            {   //grow to corner point 
                if (currentPoint.y == axisToConnect_n2)//reached corner point 
                {
                    OccupiedCoordinates.Add(new Vector3Int(currentPoint.x, currentPoint.y, 0));
                    break;
                }
                else
                {
                    Debug.Log("adding room at: ");
                    Debug.Log(currentPoint);
                    OccupiedCoordinates.Add(new Vector3Int(currentPoint.x, currentPoint.y, 0));
                }
                currentPoint = new Vector2Int(currentPoint.x, currentPoint.y + increment);//move towards corner 
            }
            //straight connect from here
            //NOTE: since n1 always grows vertical to corner, straight connect will always act as "y" split 
            StraightGrowUntilConnectionMade(node1, node2, "y", axisToConnect_n2, maxWidth, maxHeight);
        }
    }
    public bool Region1IsMoreUp(List<Vector2Int> region1, List<Vector2Int> region2)
    {   //check if region 1 is up most region than region 2 
        int STARTCOORD = 0;
        int ENDCOORD = 1;
        if (region1[ENDCOORD].y > region2[ENDCOORD].y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool Region1IsMoreRight(List<Vector2Int> region1, List<Vector2Int> region2)
    {   //check if region is more rightmost than region 2 
        int STARTCOORD = 0;
        int ENDCOORD = 1;
        if (region1[ENDCOORD].x > region2[ENDCOORD].x)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void CreateRoomAtAllOccupiedZones()
    {   //instantiates a room at each coord specified by the Occupied Zone List
        foreach (Vector3Int coord in OccupiedCoordinates)
        {
            //Debug.Log("creating room at");
            //Debug.Log(coord);
            GenerateRoom(coord);
        }
    }
    public void GenerateRoom(Vector3Int RoomPosition)
    {
        //wait for initial loading of rooms
        //Entering from will hold value of direction needed to be open from recursive call
        //0 == FIRSTROOM //1 = UP //2 = RIGHT //3 = DOWN //4 = LEFT
        //Debug.Log("generatingRoom...");
        if (NumRooms >= 0)
        {
            NumRooms -= 1;
            //Debug.Log("NumRooms:" + NumRooms);
            //OccupiedCoordinates.Add(RoomPosition);//remove position to avoid overites ///CHECK THIS 
            //Debug.Log("generating room, number of rooms left: " + NumberOfRooms);
            GameObject r = null;
            //Instantate random room
            //make sure room is acceptable based on previous accessor
            r = Instantiate(RoomPrefab, RoomGrid.transform)
                        as GameObject;
                
            //set proper position
            r.gameObject.transform.parent = RoomGrid.transform;//set parent to roomgrid for proper sizing
            r.transform.position = (Vector2)ZonesTilemap.GetCellCenterWorld(RoomPosition) + new Vector2(xoffset, yoffset);//set room position to zone tile position

            //update room class
            Room room = r.GetComponent<Room>();
            room.LocalPosition = RoomPosition;
            room.RoomWasGenerated = true;//used to differentiate manually and generated rooms 
        }
        else
        {
            //No more rooms left to make
        }
    }
    //********************************************************************************************
    //*******************************************************************************************
    //******************************************************************************************
    //**OLD** method, fills all rooms with starting room
    /*
     * 
     * //connect generated floor to Connected zones (must be connected)
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

     * private void StartRoomGeneration()
    {
        Room.BranchType h1 = Room.BranchType.Hallway1;
        //if a starting tile is placed, start generation there
        if (StartZoneLocation != null)
            RandomlyGenerateRoom(StartZoneLocation,0, h1);
        //otherwise start at center of grid
        else
        RandomlyGenerateRoom(tileCoordinates[tileCoordinates.Count / 2], 0,h1);//choose starting point for room generation
    }
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
                room.transform.position = (Vector2)ZonesTilemap.GetCellCenterWorld(tileCoordinates[i]) + new Vector2(xoffset, yoffset);//set room position to zone tile position

            }
        }
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
                if (SpecialRoomsMustBeConnected)
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
                int randomint = UnityEngine.Random.Range(0, NormalRoomTypes.Count - 1);//pick random room
                for (int i = 0; i < NormalRoomTypes.Count - 1; i++)
                {
                    NormalRoomTypes[randomint].GetComponent<Room>().InitBranchEnterArr();//init arr to access it from prefab

                    if (NormalRoomTypes[randomint].GetComponent<Room>().BranchEnterArr[EnteringFrom - 1] == EnteringType)//check if selected room is acceptable
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
            r.transform.position = (Vector2)ZonesTilemap.GetCellCenterWorld(RoomPosition) + new Vector2(xoffset, yoffset);//set room position to zone tile position

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
    */
}

public class BSPNode
{
    public string splitType;//holds "x" split along x axis, "y" for split along y axis 
    public List<Vector2Int> RegionCoords;//holds two coordinates defining top left and bot right of square region
    public List<Vector2Int> PopulatedRegionCoords = new List<Vector2Int>();//holds two coordinates space used up by room that is generated 
    public BSPNode ParentNode;
    public InteriorData.ZoneType zoneType = InteriorData.ZoneType.NORMAL;
    public List<BSPNode> childNodes = new List<BSPNode>();//firstmode child is always top / left node
    
    public BSPNode(List<Vector2Int> region, BSPNode Parent)
    {
        RegionCoords = region;
        ParentNode = Parent;
        //Debug.Log("new BSP node created with region:");
        //Debug.Log(RegionCoords[0]);
        //Debug.Log(RegionCoords[1]);
    }

    public List<BSPNode> SplitNode(string splittype)
    {   //splits node in either x or y axis and creates new child nodes 
        int STARTPOINT = 0;
        int ENDPOINT = 1;
        List<Vector2Int> Region1 = new List<Vector2Int>();//left/up split
        List<Vector2Int> Region2 = new List<Vector2Int>();//right/down split
        splitType = splittype;
        //generate new regions 
        if (splitType == "x")//bot/top split
        {   
            //int midpoint_y = (RegionCoords[STARTPOINT].y - RegionCoords[ENDPOINT].y) / 2; //take mid point every time
            int offset = UnityEngine.Random.Range(1, Math.Abs(RegionCoords[ENDPOINT].y - RegionCoords[STARTPOINT].y));//used to determine location of split
            int midpoint_y = (RegionCoords[STARTPOINT].y + offset);
            //Debug.Log("x split:");
            //Debug.Log("midpoint_y");
            //Debug.Log(midpoint_y);
            Region1.Add(RegionCoords[STARTPOINT]);
            Region1.Add(new Vector2Int(RegionCoords[ENDPOINT].x, midpoint_y));
            Region2.Add(new Vector2Int(RegionCoords[STARTPOINT].x, midpoint_y+1));
            Region2.Add(RegionCoords[ENDPOINT]);

        }
        else if (splitType == "y")//left/right split
        {
            //int midpoint_x = (RegionCoords[STARTPOINT].x - RegionCoords[ENDPOINT].x) / 2;
            int offset = UnityEngine.Random.Range(1, Math.Abs(RegionCoords[ENDPOINT].x - RegionCoords[STARTPOINT].x));//used to determine location of split
            int midpoint_x = (RegionCoords[STARTPOINT].x + offset);
            //Debug.Log("y split:");
            //Debug.Log("midpoint_x");
            //Debug.Log(midpoint_x);
            Region1.Add(RegionCoords[STARTPOINT]);
            Region1.Add(new Vector2Int(midpoint_x, RegionCoords[ENDPOINT].y));
            Region2.Add(new Vector2Int(midpoint_x+1, RegionCoords[STARTPOINT].y));
            Region2.Add(RegionCoords[ENDPOINT]);
        }
        childNodes.Add(new BSPNode(Region1, this));
        childNodes.Add(new BSPNode(Region2, this));
        return childNodes;
    }

    public int RegionWidth()
    {
        int STARTPOINT = 0;
        int ENDPOINT = 1;
        return RegionCoords[ENDPOINT].x - RegionCoords[STARTPOINT].x;
    }
    public int RegionHeight()
    {
        int STARTPOINT = 0;
        int ENDPOINT = 1;
        return RegionCoords[ENDPOINT].y - RegionCoords[STARTPOINT].y;
    }
}

public class BSPTree
{
    public BSPNode headNode;
    public List<BSPNode> childNodes;

    public BSPTree(int regionWidth, int regionHeight)
    {
        Vector2Int startPoint = new Vector2Int(0,0);
        Vector2Int endPoint = new Vector2Int(regionWidth, regionHeight);
        List<Vector2Int> startRegion = new List<Vector2Int>();
        startRegion.Add(startPoint);
        startRegion.Add(endPoint);
        headNode = new BSPNode(startRegion, null);
    }
    public void GenerateTree(int maxSplits, int minPartitionSize)
    {
        int MaxIterations = 5000;//failsafe rather than using while
        int remainingSplits = maxSplits;
        List<BSPNode> currentChildren = new List<BSPNode>();
        currentChildren.Add(headNode);
        int j;
        for (int i=0;i<MaxIterations && remainingSplits >0;i++)
        {
            List<BSPNode> newChildren = new List<BSPNode>();//store new children after each level split 
            List<BSPNode> dormantChildren = new List<BSPNode>();//refresh current children with these but dont split 
            //Debug.Log("currentChildren count:");
            //Debug.Log(currentChildren.Count);
            
            //j=0;
            foreach (BSPNode child in currentChildren)
            {
                //Debug.Log("j");
                //Debug.Log(j);
                //j++;
                //Debug.Log("now splitting child with region:");
                //Debug.Log(child.RegionCoords[0]);
                //Debug.Log(child.RegionCoords[1]);
                List<BSPNode> splitNodes;
                if(remainingSplits > 0)
                {
                    if (child.RegionWidth() > 1 && child.RegionHeight() > 1)//split randomly in x or y 
                    {
                        int randNum = UnityEngine.Random.Range(0, 2);//chose random split direction
                        if (randNum == 0)
                        {
                            splitNodes = child.SplitNode("y");
                            //Debug.Log("split!");
                        }
                        else
                        {
                            splitNodes = child.SplitNode("x");
                            //Debug.Log("split!");
                        }
                        newChildren.Add(splitNodes[0]);
                        newChildren.Add(splitNodes[1]);
                        //Debug.Log("newChildren:");
                        //Debug.Log(newChildren);
                        remainingSplits -= 1;
                    }
                    else if (child.RegionWidth() > 1)//can only split in y
                    {
                        //Debug.Log("split!");
                        splitNodes = child.SplitNode("y");
                        newChildren.Add(splitNodes[0]);
                        newChildren.Add(splitNodes[1]);
                        remainingSplits -= 1;
                    }
                    else if (child.RegionHeight() > 1)//can only split in x
                    {
                        //Debug.Log("split!");
                        splitNodes = child.SplitNode("x");
                        newChildren.Add(splitNodes[0]);
                        newChildren.Add(splitNodes[1]);
                        remainingSplits -= 1;
                    }
                    else
                    {
                        Debug.Log("warning, cannot split region any further, cancelling...");
                        //Debug.Log(child.RegionCoords[0]);
                        //Debug.Log(child.RegionCoords[1]);
                        dormantChildren.Add(child); //retain these but dont try to split again 
                    }
                }
                else
                {
                    Debug.Log("warning, max splits reached, keeping this node as dormant");
                    //Debug.Log(child.RegionCoords[0]);
                    //Debug.Log(child.RegionCoords[1]);
                    dormantChildren.Add(child); //retain these but dont try to split again 
                }
            }//end foreach child 
            ///Debug.Log("new children");
            //Debug.Log(newChildren.Count);
            //Debug.Log(dormantChildren.Count);
            if(i >=MaxIterations -2)
            {
                Debug.Log("warning!! BSP tree growth ended due to max interation being reached");
                break;
            }
            currentChildren = new List<BSPNode>(newChildren);
            foreach (BSPNode child in dormantChildren)
            {
                //Debug.Log("adding dormant children");
                //Debug.Log(currentChildren.Count);
                currentChildren.Add(child);
                //Debug.Log(currentChildren.Count);

            }
            if(newChildren.Count == 0)
            {
                break;
            }
            //Debug.Log("current children");
            //Debug.Log(currentChildren.Count);
        }
        childNodes = new List<BSPNode>(currentChildren);
        //Debug.Log("final children count:");
        //Debug.Log(childNodes.Count);
    }

}
