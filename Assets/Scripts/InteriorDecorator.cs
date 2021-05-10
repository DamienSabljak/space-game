using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InteriorDecorator : MonoBehaviour
{   //this class is an extentsion to the floor class, providing methods for decorating the interior of the floor 
    // Start is called before the first frame update
    int[,] zonesImg;//holds an "image like" reference to the occupied zones, for use of morphological operators 
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ConvertOccupiedZonesToImage(List<Vector3Int> occupiedZones, int width, int height)
    {   //converts the OccupiedZones list into a 2D "Image" for decorator methods
        zonesImg = new int[height+1, width+1];//initialize to zero, width and height are inclusive (+1)
        foreach (Vector3Int coord in occupiedZones)
        {
            zonesImg[coord.y,coord.x] = 1;
        }
        Debug.Log("ZonesImg:");
        PrintZoneImg();
    }
    public void PrintZoneImg()
    {
        string str_total = "";
        string row_str = "";
        for (int row = zonesImg.GetLength(0)-1; row >=0; row--)
        {
            //string temp = row_str + "\n";
            //temp = temp + str_total;
            //str_total = temp;
            for (int col = 0; col < zonesImg.GetLength(1); col++)
            {
                row_str += zonesImg[row, col];
            }
            Debug.Log(row_str);
            row_str = "";
        }
        
    }

    public void DecorateFloor(List<BSPNode> treeLeaves)
    {   //tree leaves define which rooms are in which zones 

        //generation order
        //determine zones
        //determine templates
        //decorate

        foreach (BSPNode leaf in treeLeaves)
        {
            switch (leaf.zoneType)
            {
                case (InteriorData.ZoneType.NORMAL):
                    DecorateNormalZone(leaf);
                    break;
            }
                

        }
        
    }
    public void DecorateNormalZone(BSPNode leaf)
    {
        Debug.Log("Decorating Normal Zone");
        //populate random furniture 
        //NEED TO IMPLEMENT 
        //create walls
        int STARTCOORD = 0;
        int ENDCOORD = 1;
        for(int row= leaf.RegionCoords[STARTCOORD].y; row<= leaf.RegionCoords[ENDCOORD].y; row++)
        {
            for (int col = leaf.RegionCoords[STARTCOORD].x; col <= leaf.RegionCoords[ENDCOORD].x; col++)
            {
                if(ImgAtCoordIsOccupied(new Vector2Int(col, row)))
                {
                    Debug.Log("now bulding walls for coords:");
                    Debug.Log(new Vector2Int(col, row));
                    InteriorData.ZoneType[] neighborZones = DetectNeighborZones(new Vector2Int(col, row), leaf);
                    for (int neighbor = 0; neighbor < neighborZones.Length; neighbor++)
                    {   //for each side of room, place wall depending on neighboring zone 
                        Debug.Log("Neighbor "+((WallEntry.Side)neighbor).ToString() + " is:");
                        //Debug.Log(leaf.zoneType);
                        Debug.Log(neighborZones[neighbor]);
                        if (neighborZones[neighbor] == InteriorData.ZoneType.OUTSIDE)
                        {   //neighbor is outside 
                            //place Closed Wall
                            WallEntry wall = SelectRandomWallFromCriteria((WallEntry.Side)neighbor, WallEntry.OpeningType.CLOSED, leaf.zoneType);//neighbor holds side info 
                            PlaceWallAtLocation(wall.pathToPrefab, new Vector2Int(col, row));
                        }
                        else if (neighborZones[neighbor] == leaf.zoneType)
                        {
                            //do nothing 
                        }
                        else
                        {   //neighbour is a different zone and not outside 
                            //place 1D wall 
                            WallEntry wall = SelectRandomWallFromCriteria((WallEntry.Side)neighbor, WallEntry.OpeningType.SINGLEDOOR, leaf.zoneType);
                            PlaceWallAtLocation(wall.pathToPrefab, new Vector2Int(col, row));
                        }
                    }
                }
            }
        }
    }
    public InteriorData.ZoneType[] DetectNeighborZones(Vector2Int roomCoord, BSPNode leaf)
    {   //return 4x1 array containing int corresponding to wether or not the neighbours are within the same region 
        //null is used to indicate to room next to it 
        InteriorData.ZoneType[] neighborZones = new InteriorData.ZoneType[4];
        //Left Neighbour 
        if(CoordWithinImg(new Vector2Int(roomCoord.x-1, roomCoord.y)) && ImgAtCoordIsOccupied(new Vector2Int(roomCoord.x - 1, roomCoord.y)) )
        {   //left neighbour is inside floor 
            if(GetComponent<Floor>().CoordinateWithinRegion(new Vector2Int(roomCoord.x-1,roomCoord.y), leaf.RegionCoords) )
            {
                neighborZones[(int)WallEntry.Side.LEFT] = leaf.zoneType;//same zone 
            }
            else
            {
                neighborZones[(int)WallEntry.Side.LEFT] = InteriorData.ZoneType.DIFFERENT;//different zone 
            }
        }
        else
        {
            neighborZones[(int)WallEntry.Side.LEFT] = InteriorData.ZoneType.OUTSIDE;
        }
        //Right Neighbour 
        if (CoordWithinImg(new Vector2Int(roomCoord.x + 1, roomCoord.y)) && ImgAtCoordIsOccupied(new Vector2Int(roomCoord.x + 1, roomCoord.y)))
        {   //neighbour is inside floor
            if (GetComponent<Floor>().CoordinateWithinRegion(new Vector2Int(roomCoord.x + 1, roomCoord.y), leaf.RegionCoords))
            {
                neighborZones[(int)WallEntry.Side.RIGHT] = leaf.zoneType;//same zone 
            }
            else
            {
                neighborZones[(int)WallEntry.Side.RIGHT] = InteriorData.ZoneType.DIFFERENT;//different zone 
            }
        }
        else
        {
            neighborZones[(int)WallEntry.Side.RIGHT] = InteriorData.ZoneType.OUTSIDE;
        }
        //Top Neighbour 
        Debug.Log("TOP - CoordWithinImg is " + CoordWithinImg(new Vector2Int(roomCoord.x, roomCoord.y + 1)).ToString());
        Debug.Log("TOP - ImgAtCoordIsOccuped is " + (ImgAtCoordIsOccupied(new Vector2Int(roomCoord.x, roomCoord.y + 1))).ToString() );
        Debug.Log(new Vector2Int(roomCoord.x, roomCoord.y + 1));
        Debug.Log(zonesImg[roomCoord.y+1, roomCoord.x]);
        if (CoordWithinImg(new Vector2Int(roomCoord.x, roomCoord.y + 1)) && ImgAtCoordIsOccupied(new Vector2Int(roomCoord.x, roomCoord.y + 1)))
        {   //neighbour is inside floor
            if (GetComponent<Floor>().CoordinateWithinRegion(new Vector2Int(roomCoord.x, roomCoord.y + 1), leaf.RegionCoords))
            {
                neighborZones[(int)WallEntry.Side.TOP] = leaf.zoneType;//same zone 
            }
            else
            {
                neighborZones[(int)WallEntry.Side.TOP] = InteriorData.ZoneType.DIFFERENT;//different zone 
            }
        }
        else
        {
            neighborZones[(int)WallEntry.Side.TOP] = InteriorData.ZoneType.OUTSIDE;
        }
        //Bottom Neighbour 
        if (CoordWithinImg(new Vector2Int(roomCoord.x, roomCoord.y - 1)) && ImgAtCoordIsOccupied(new Vector2Int(roomCoord.x, roomCoord.y - 1)))
        {   //neighbour is inside floor 
            if (GetComponent<Floor>().CoordinateWithinRegion(new Vector2Int(roomCoord.x, roomCoord.y - 1), leaf.RegionCoords))
            {
                neighborZones[(int)WallEntry.Side.BOTTOM] = leaf.zoneType;//same zone 
            }
            else
            {
                neighborZones[(int)WallEntry.Side.BOTTOM] = InteriorData.ZoneType.DIFFERENT;//different zone 
            }
        }
        else
        {
            neighborZones[(int)WallEntry.Side.BOTTOM] = InteriorData.ZoneType.OUTSIDE;
        }
        //return array
        return neighborZones;
    }
    public bool CoordWithinImg(Vector2Int coord)
    {
        //checks if coord within img, preventing index errors
        if(coord.x>=0 && coord.x <zonesImg.GetLength(1) && coord.y >= 0 && coord.y < zonesImg.GetLength(0))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool ImgAtCoordIsOccupied(Vector2Int coord)
    {
        if(zonesImg[coord.y, coord.x] >0)
        {
            return true;
        }
        return false;
    }
    //create and add floor data script object to refference here

    public WallEntry SelectRandomWallFromCriteria(WallEntry.Side side, WallEntry.OpeningType openingType, InteriorData.ZoneType zoneType)
    {   //select a random wall entry which meets specified criteria 
        List<WallEntry>  possibleWalls = InteriorData.LoadWallsFromCriteria(side, openingType, zoneType);
        int randIndex = UnityEngine.Random.Range(0, possibleWalls.Count - 1);
        return possibleWalls[randIndex];
    }
    public void PlaceWallAtLocation(string pathToWallPrefab, Vector2Int location)
    {
        //Debug.Log("placing wall at location:");
        //Debug.Log(pathToWallPrefab);
        //Debug.Log(location);
        //get room reference 
        Room room = GetComponent<Floor>().FindRoomAtZone(new Vector3Int(location.x, location.y,0));
        //Debug.Log(room.gameObject);
        //access prefab through path name 
        string path = "RoomLayouts/Walls/" + pathToWallPrefab;//relative path from all resources folders 
        
        UnityEngine.Object pPrefab = Resources.Load(path); // note: not .prefab!
        //instantiate and center prefab 
        if(pPrefab == null)
        {
            Debug.Log("WARNING: PREFAB REFFERENCE AT GIVEN PATH IS NULL, CHECK RESOURCES FOLDER AND DATABASE ENTRIES");
            Debug.Log(path);
        }
        GameObject pNewObject = (GameObject)GameObject.Instantiate(pPrefab, Vector3.zero, Quaternion.identity);
        pNewObject.transform.parent = room.interiorGrid.transform;//If you get error, check to make sure only proper rooms are in room grid 
        pNewObject.transform.localPosition = new Vector3(0, 0, 0);//center on new parent 
    }
    public void ConstructSimpleRoom(InteriorData.ZoneType zoneType, Vector2Int location)
    {
        //place 4 walls 
        List<WallEntry> bottomWalls =  InteriorData.LoadWallsFromCriteria(WallEntry.Side.BOTTOM, WallEntry.OpeningType.CLOSED, InteriorData.ZoneType.NORMAL);
        List<WallEntry> leftWalls =  InteriorData.LoadWallsFromCriteria(WallEntry.Side.LEFT, WallEntry.OpeningType.CLOSED, InteriorData.ZoneType.NORMAL);
        List<WallEntry> rightWalls =  InteriorData.LoadWallsFromCriteria(WallEntry.Side.RIGHT, WallEntry.OpeningType.CLOSED, InteriorData.ZoneType.NORMAL);
        List<WallEntry> topWalls =  InteriorData.LoadWallsFromCriteria(WallEntry.Side.TOP, WallEntry.OpeningType.CLOSED, InteriorData.ZoneType.NORMAL);
        //note: order of placement will affect rendering 
        PlaceWallAtLocation(leftWalls[0].pathToPrefab, location);//- for now pick first choice
        PlaceWallAtLocation(rightWalls[0].pathToPrefab, location);//- for now pick first choice
        PlaceWallAtLocation(bottomWalls[0].pathToPrefab, location);//- for now pick first choice
        PlaceWallAtLocation(topWalls[0].pathToPrefab, location);//- for now pick first choice
    }

    public void AssignRegionsAsSpecialZones(List<BSPNode> treeLeaves)
    {   //assign zones such as hospital, engineering etc to regions determined by leaves of node 

    }


}
