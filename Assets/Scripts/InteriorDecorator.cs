using System.Collections.Generic;
using UnityEngine;

public class InteriorDecorator : MonoBehaviour
{   //this class is an extentsion to the floor class, providing methods for decorating the interior of the floor 
    // Start is called before the first frame update
    int[,] zonesImg;//holds an "image like" reference to the occupied zones, for use of morphological operators 
    int[,] alreadyDecoratedImg;//image which holds room coordinates that have been already decorated by a template (such as corridor)
    [SerializeField] InteriorData database;
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
        alreadyDecoratedImg = new int[height+1, width+1];//initialize to zero, width and height are inclusive (+1)
        foreach (Vector3Int coord in occupiedZones)
        {
            zonesImg[coord.y,coord.x] = 1;
        }
        //Debug.Log("ZonesImg:");
        //PrintImg(zonesImg);  
    }
    public static void PrintImg(int[,] img)
    {
        string row_str = "";
        for (int row = img.GetLength(0)-1; row >=0; row--)
        {
            //string temp = row_str + "\n";
            //temp = temp + str_total;
            //str_total = temp;
            for (int col = 0; col < img.GetLength(1); col++)
            {
                row_str += img[row, col];
            }
            Debug.Log(row_str);
            row_str = "";
        }
        
    }
    public void DecorateFloor(List<BSPNode> treeLeaves)
    {   //tree leaves define which rooms are in which zones 
        debugCanvas.currentDebugCanvas.log("DecorateFloor called");
        //generation order:
        //choose random zones 
        AssignRegionsAsSpecialZones(treeLeaves);
        //decorate template matching areas
        DecorateStartingRoom(treeLeaves[0]);
        DecorateHorizontalCoridors();
        //decorate zones
        foreach (BSPNode leaf in treeLeaves)
        {
            switch (leaf.zoneType)
            {
                case (InteriorData.ZoneType.NORMAL):
                    DecorateNormalZone(leaf);
                    break;
                case (InteriorData.ZoneType.ENGINEERING):
                    DecorateEngineeringZone(leaf);
                    break;
                case (InteriorData.ZoneType.ARMOURY):
                    DecorateArmouryZone(leaf);
                    break;
                case (InteriorData.ZoneType.MEDICAL):
                    DecorateMedZone(leaf);
                    break;
                case (InteriorData.ZoneType.COMMAND):
                    DecorateCommandZone(leaf);
                    break;
                case (InteriorData.ZoneType.HANGAR):
                    DecorateHangarZone(leaf);
                    break;
                default:
                    Debug.Log("******* WARNING: DECORATE METHOD FOR GIVEN ZONE NOT SPECIFIED*********");
                    break;
            }
        }
        
    }
    public void DecorateNormalZone(BSPNode leaf)
    {
        Debug.Log("Decorating Normal Zone");
        //populate random furniture 
        //NEED TO IMPLEMENT 
        DecorateRoomsByZone(leaf);
    }
    public void DecorateEngineeringZone(BSPNode leaf)
    {
        //decorate walls
        DecorateRoomsByZone(leaf);
    }
    public void DecorateArmouryZone(BSPNode leaf)
    {
        //decorate walls
        DecorateRoomsByZone(leaf);
    }
    public void DecorateMedZone(BSPNode leaf)
    {
        //decorate walls
        DecorateRoomsByZone(leaf);
    }
    public void DecorateCommandZone(BSPNode leaf)
    {
        //decorate walls
        DecorateRoomsByZone(leaf);
    }
    public void DecorateHangarZone(BSPNode leaf)
    {
        //decorate walls
        DecorateRoomsByZone(leaf);
    }
    public void DecorateStartingRoom(BSPNode anyLeaf)
    {   //decorate starting room 
        //note any leaf is just passed to DetectNeighborZones but doesnt really matter
        Vector3Int roomCoords =  GetComponent<Floor>().firstRoom;
        alreadyDecoratedImg[roomCoords.y, roomCoords.x] = 1;//let other decorators know this room is decorated 
        //place walls 
        InteriorData.ZoneType[] neighborZones = DetectNeighborZones((Vector2Int)roomCoords, anyLeaf);
        for (int neighbor = 0; neighbor < neighborZones.Length; neighbor++)
        {
            if(neighbor == (int) WallEntry.Side.BOTTOM)
            {   //entrance is always from bottom 
                //place 2D wall
                WallEntry wall = SelectRandomWallFromCriteria((WallEntry.Side)neighbor, WallEntry.OpeningType.SINGLEDOOR, InteriorData.ZoneType.NORMAL);//neighbor holds side info 
                PlaceTilemapAtLocation(wall.pathToPrefab, (Vector2Int)roomCoords);
            }
            else if (neighborZones[neighbor] == InteriorData.ZoneType.OUTSIDE)
            {
                //place closed wall (normal type, but can have custom later)
                WallEntry wall = SelectRandomWallFromCriteria((WallEntry.Side)neighbor, WallEntry.OpeningType.CLOSED, InteriorData.ZoneType.NORMAL);//neighbor holds side info 
                PlaceTilemapAtLocation(wall.pathToPrefab, (Vector2Int) roomCoords);
            }
            else
            {
                //place 2D wall
                WallEntry wall = SelectRandomWallFromCriteria((WallEntry.Side)neighbor, WallEntry.OpeningType.DOUBLEDOOR, InteriorData.ZoneType.NORMAL);//neighbor holds side info 
                PlaceTilemapAtLocation(wall.pathToPrefab, (Vector2Int)roomCoords);
            }
        }
        //place floors, furniture
        FloorEntry floor = SelectRandomFloorFromCriteria(InteriorData.ZoneType.NORMAL);
        PlaceTilemapAtLocation(floor.pathToPrefab, (Vector2Int) roomCoords);
        FurnitureEntry furniture = SelectRandomFurnitureFromCriteria(InteriorData.ZoneType.AIRLOCK);
        PlaceTilemapAtLocation(furniture.pathToPrefab, (Vector2Int) roomCoords);
        //need to create furniture prefab
    }
    public void DecorateHorizontalCoridors()
    {
        //first Detect Corridors 
        ConvolutionKernel HorizontalCorridorKernel = new ConvolutionKernel(InteriorData.HorizontalCorridorHitMissStruct);
        int[,] img_hitMiss = HorizontalCorridorKernel.HitMissConvolveImage(zonesImg);//create image of matching corridor zones 
        //print("****** HIT MISS IMG *****");
        //PrintImg(img_hitMiss);
        for (int row = 0; row < img_hitMiss.GetLength(0); row++)
        {
            for (int col = 0; col < img_hitMiss.GetLength(1); col++)
            {
                if(img_hitMiss[row,col] == 1 && alreadyDecoratedImg[row,col] == 0)
                {   //room is a part of a corridor 
                    alreadyDecoratedImg[row, col] = 1;//let other decorators know this room is decorated 
                    //place walls, decorations 
                    //top wall
                    WallEntry  wall = SelectRandomWallFromCriteria(WallEntry.Side.TOP, WallEntry.OpeningType.CLOSED, InteriorData.ZoneType.CORRIDOR);
                    PlaceTilemapAtLocation(wall.pathToPrefab, new Vector2Int(col, row));
                    //bot wall
                    wall = SelectRandomWallFromCriteria(WallEntry.Side.BOTTOM, WallEntry.OpeningType.CLOSED, InteriorData.ZoneType.NORMAL);
                    PlaceTilemapAtLocation(wall.pathToPrefab, new Vector2Int(col, row));
                    //floors
                    FloorEntry floor = SelectRandomFloorFromCriteria(InteriorData.ZoneType.NORMAL);
                    PlaceTilemapAtLocation(floor.pathToPrefab, new Vector2Int(col, row));
                }
            }
        }
    }
    private void DecorateRoomsByZone(BSPNode leaf)
    {
        //create walls and flooring 
        int STARTCOORD = 0;
        int ENDCOORD = 1;
        for (int row = leaf.RegionCoords[STARTCOORD].y; row <= leaf.RegionCoords[ENDCOORD].y; row++)
        {
            for (int col = leaf.RegionCoords[STARTCOORD].x; col <= leaf.RegionCoords[ENDCOORD].x; col++)
            {
                if (ImgAtCoordIsOccupied(new Vector2Int(col, row)) && alreadyDecoratedImg[row,col] != 1)
                {   //room exists and hasnt yet been decorated 
                    
                    //Debug.Log("now bulding walls for coords:");
                    //Debug.Log(new Vector2Int(col, row));
                    InteriorData.ZoneType[] neighborZones = DetectNeighborZones(new Vector2Int(col, row), leaf);
                    for (int neighbor = 0; neighbor < neighborZones.Length; neighbor++)
                    {   //for each side of room, place wall depending on neighboring zone 
                        //Debug.Log("Neighbor " + ((WallEntry.Side)neighbor).ToString() + " is:");
                        //Debug.Log(leaf.zoneType);
                        //Debug.Log(neighborZones[neighbor]);
                        if (neighborZones[neighbor] == InteriorData.ZoneType.OUTSIDE)
                        {   //neighbor is outside 
                            //place Closed Wall
                            WallEntry wall = SelectRandomWallFromCriteria((WallEntry.Side)neighbor, WallEntry.OpeningType.CLOSED, leaf.zoneType);//neighbor holds side info 
                            PlaceTilemapAtLocation(wall.pathToPrefab, new Vector2Int(col, row));
                            
                        }
                        else if (neighborZones[neighbor] == leaf.zoneType)
                        {
                            //do nothing 
                        }
                        else
                        {   //neighbour is a different zone and not outside 
                            //place 1D wall 
                            WallEntry wall = SelectRandomWallFromCriteria((WallEntry.Side)neighbor, WallEntry.OpeningType.SINGLEDOOR, leaf.zoneType);
                            PlaceTilemapAtLocation(wall.pathToPrefab, new Vector2Int(col, row));
                        }
                    }
                    //flooring and furniture
                    FloorEntry floor = SelectRandomFloorFromCriteria(leaf.zoneType);
                    PlaceTilemapAtLocation(floor.pathToPrefab, new Vector2Int(col, row));
                    FurnitureEntry furniture = SelectRandomFurnitureFromCriteria(leaf.zoneType);
                    PlaceTilemapAtLocation(furniture.pathToPrefab, new Vector2Int(col, row));
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
        //Debug.Log("TOP - CoordWithinImg is " + CoordWithinImg(new Vector2Int(roomCoord.x, roomCoord.y + 1)).ToString());
        //Debug.Log("TOP - ImgAtCoordIsOccuped is " + (ImgAtCoordIsOccupied(new Vector2Int(roomCoord.x, roomCoord.y + 1))).ToString() );
        //Debug.Log(new Vector2Int(roomCoord.x, roomCoord.y + 1));
        //Debug.Log(zonesImg[roomCoord.y+1, roomCoord.x]);
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
        List<WallEntry>  possibleWalls = database.LoadWallsFromCriteria(side, openingType, zoneType);
        int randIndex = UnityEngine.Random.Range(0, possibleWalls.Count);
        return possibleWalls[randIndex];
    }

    public FloorEntry SelectRandomFloorFromCriteria(InteriorData.ZoneType zoneType)
    {   //select a random wall entry which meets specified criteria 
        List<FloorEntry> possibleFloors = database.LoadFloorsFromCriteria(zoneType);
        int randIndex = UnityEngine.Random.Range(0, possibleFloors.Count);
        return possibleFloors[randIndex];
    }
    public FurnitureEntry SelectRandomFurnitureFromCriteria(InteriorData.ZoneType zoneType)
    {   //select a random wall entry which meets specified criteria 
        List<FurnitureEntry> possibleFurniture = database.LoadFurnitureFromCriteria(zoneType);
        int randIndex = UnityEngine.Random.Range(0, possibleFurniture.Count);
        return possibleFurniture[randIndex];
    }

    public void PlaceTilemapAtLocation(string pathToWallPrefab, Vector2Int location)
    {
        //Debug.Log("placing wall at location:");
        //Debug.Log(pathToWallPrefab);
        //Debug.Log(location);
        //get room reference 
        Room room = GetComponent<Floor>().FindRoomAtZone(new Vector3Int(location.x, location.y,0));
        //Debug.Log(room.gameObject);
        //access prefab through path name 
        string path = "RoomLayouts/" + pathToWallPrefab;//relative path from all resources folders 
        
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
        List<WallEntry> bottomWalls =  database.LoadWallsFromCriteria(WallEntry.Side.BOTTOM, WallEntry.OpeningType.CLOSED, InteriorData.ZoneType.NORMAL);
        List<WallEntry> leftWalls = database.LoadWallsFromCriteria(WallEntry.Side.LEFT, WallEntry.OpeningType.CLOSED, InteriorData.ZoneType.NORMAL);
        List<WallEntry> rightWalls = database.LoadWallsFromCriteria(WallEntry.Side.RIGHT, WallEntry.OpeningType.CLOSED, InteriorData.ZoneType.NORMAL);
        List<WallEntry> topWalls = database.LoadWallsFromCriteria(WallEntry.Side.TOP, WallEntry.OpeningType.CLOSED, InteriorData.ZoneType.NORMAL);
        //note: order of placement will affect rendering 
        PlaceTilemapAtLocation(leftWalls[0].pathToPrefab, location);//- for now pick first choice
        PlaceTilemapAtLocation(rightWalls[0].pathToPrefab, location);//- for now pick first choice
        PlaceTilemapAtLocation(bottomWalls[0].pathToPrefab, location);//- for now pick first choice
        PlaceTilemapAtLocation(topWalls[0].pathToPrefab, location);//- for now pick first choice
    }
    public void AssignRegionsAsSpecialZones(List<BSPNode> treeLeaves)
    {   //assign zones such as hospital, engineering etc to regions determined by leaves of node 
        
        int numZones = InteriorData.ZoneType.GetNames(typeof(InteriorData.ZoneType)).Length;
        int randZone = UnityEngine.Random.Range(0, numZones);
        //assign random zone 
        for(int i=0; i<treeLeaves.Count && i<5;i++)
        {
            int randLeaf = UnityEngine.Random.Range(0, treeLeaves.Count);
            treeLeaves[randLeaf].zoneType = (InteriorData.ZoneType) i+1;//NOTE: this has been hard coded to only assign zones 1-5 
            Debug.Log("assigned leaf " + randLeaf.ToString() + " as zone " + ((InteriorData.ZoneType)(i + 1)).ToString());
        }
        //treeLeaves[randLeaf].zoneType = (InteriorData.ZoneType)randZone;
        //treeLeaves[randLeaf].zoneType = InteriorData.ZoneType.ENGINEERING;
        //Debug.Log("assigned leaf " + randLeaf.ToString() + " as engineering zone ");
    }


}

class ConvolutionKernel
{
    public int[,] kernel;

    public ConvolutionKernel(int[,] kernel_set)
    {
        kernel = kernel_set;
    }

    public int[,] HitMissConvolveImage(int[,] img)
    {//returns a bitmap image of size img with matching pixels for a hit miss operation
        //1 = hit //0=miss, //-1=dontCare
        int[,] img_hitmiss = new int[img.GetLength(0),img.GetLength(1)];
        int padAmt = (kernel.GetLength(0) - 1) / 2;
        int[,] img_padded = CreatePaddedImage(img, padAmt);
        int kernelSum = SumMatrix(kernel);
        for (int row = 0; row < img.GetLength(0); row++)
        {
            for (int col = 0; col < img.GetLength(1); col++)
            {
                int row_pad = row + padAmt; 
                int col_pad = col + padAmt;
                //Debug.Log("PadAmt");
                //Debug.Log(padAmt);
                //Debug.Log("now inspecting pixel: " + col.ToString() + "," + row.ToString());
                //InteriorDecorator.PrintImg(kernel);
                //Debug.Log("subImage:");
                //InteriorDecorator.PrintImg(CreateSubsetMatrix(img_padded, row_pad - padAmt, row_pad + padAmt, col_pad - padAmt, col_pad + padAmt));
                if (HitMissConvolve(       CreateSubsetMatrix(img_padded, row_pad - padAmt, row_pad + padAmt, col_pad - padAmt, col_pad + padAmt)))
                {
                    //Debug.Log("Match!");
                    img_hitmiss[row, col] = 1;
                }
            }
        }
        return img_hitmiss;
    }

    public int[,] CreateSubsetMatrix(int[,] img, int y_start, int  y_end, int x_start, int x_end)
    {
        int[,] img_sub = new int[y_end - y_start + 1, x_end - x_start + 1];
        for (int row = 0; row < img_sub.GetLength(0); row++)
        {
            for (int col = 0; col < img_sub.GetLength(1); col++)
            {
                int row_sub = row + y_start;
                int col_sub = col + x_start;
                //Debug.Log(row_sub.ToString() + "  " + col_sub.ToString());
                //Debug.Log("img dimensions: " + img.GetLength(0).ToString() +" "+ img.GetLength(1).ToString());
                //Debug.Log("row_sub, col_sub:  " +row_sub.ToString() + " "+ col_sub.ToString() );
                img_sub[row, col] = img[row_sub, col_sub];
            }
        }
        return img_sub;

    }

    public int[,] ElementWiseMultiply(int[,] img_sub)
    {   //convolve img_sub with saved convolution kernel 
        int[,] product = new int[kernel.GetLength(0),kernel.GetLength(1)];
        for (int row = 0; row < img_sub.GetLength(0); row++)
        {
            for (int col = 0; col < img_sub.GetLength(1); col++)
            {
                product[row,col] =  kernel[row, col] * img_sub[row, col];
            }
        }
        return product;
    }

    public bool HitMissConvolve(int[,] img_sub)
    {   //returns true if the pixel being examined matches the hit miss struct 
        for (int row = 0; row < img_sub.GetLength(0); row++)
        {
            for (int col = 0; col < img_sub.GetLength(1); col++)
            {
                if(kernel[row,col] != img_sub[row,col] && kernel[row,col] != -1)
                {
                    return false;
                }
            }
        }
        return true;
    }
    public int Convolve(int[,] img_sub)
    {
        int[,] product = ElementWiseMultiply(img_sub);
        int sum = SumMatrix(product);
        return sum;
    }

    public int SumMatrix(int[,] matrix)
    {
        int sum = 0;
        for (int row = 0; row < matrix.GetLength(0); row++)
        {
            for (int col = 0; col < matrix.GetLength(1); col++)
            {
                sum += matrix[row, col];
            }
        }
        return sum;
    }

    public int[,] CreatePaddedImage(int[,] img, int padAmt)
    {   //creates a padded image for edge cases in the image during convolution
        //padded sections are 0 
        int[,] img_padded = new int[img.GetLength(0) + 2 * padAmt, img.GetLength(1) + 2 * padAmt];
        for (int row = 0; row < img.GetLength(0); row++)
        {
            for (int col = 0; col < img.GetLength(1); col++)
            {
                int row_padded = row + padAmt;
                int col_padded = col + padAmt;
                img_padded[row_padded, col_padded] = img[row, col];
            }
        }
        return img_padded;
    }

}
