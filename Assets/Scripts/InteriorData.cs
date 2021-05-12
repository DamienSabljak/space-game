using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;

public class InteriorData : MonoBehaviour
{   //this class is the interface used to interact with the interior database
    public static IDataReader reader;
    public static IDbConnection dbcon;
    public static int[,] HorizontalCorridorHitMissStruct = {{-1,0,-1},{1,1,1},{-1,0,-1}};
    
    public enum ZoneType
    {//NOTE: do NOT change these without consulting database entries 
        NORMAL =0, 
        HOSPITAL=1, 
        HANGAR=2, 
        SECURITY=3, 
        ARMOURY=4, 
        ENGINEERING=5, 
        OUTSIDE=6, //used to communicate zone is outside of floor / out of bounds 
        DIFFERENT =7, //used to communicate a zone is different than a given floor 
        CORRIDOR = 8
    }
    public enum TemplateType
    {//NOTE: do NOT change these without consulting database entries 
        NONE = 0,
        CORRIDOR =1, 
        CLOSET=2, 
        HANGAR=3, 
    }

    void Start()
    {
        //load walls from db
        //List<WallEntry> entries =  LoadWallsFromCriteria(WallEntry.Side.LEFT, WallEntry.OpeningType.CLOSED, ZoneType.NORMAL);
        //entries[0].PrintData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void ConnectToDatabase()
    {
        //open database connection
        string connection = "URI=file:" + Application.dataPath + "/Databases/interiorDatabase.db3";
        IDbConnection dbcon1 = new SqliteConnection(connection);
        dbcon = dbcon1;
        dbcon.Open();
        //Debug.Log("*****Database connection established*****");
    }

    public static void CloseConnection()
    {   //NOTE: failure to close connection will cause database to lockout, need to close unity 
        dbcon.Close();
        //Debug.Log("*****Database connection closed*****");
    }

    public static List<WallEntry> LoadAllWallEntries()
    {
        ConnectToDatabase();
        //read all values from table 
        string query = "SELECT * FROM Walls ";
        IDbCommand cmnd_read = dbcon.CreateCommand();
        cmnd_read.CommandText = query;
        reader = cmnd_read.ExecuteReader();

        //store returned entries in list of objects 
        List<WallEntry> returnedEntries = new List<WallEntry>();
        int timeout = 5000;
        for(int i=0; i<timeout;i++)
        {
            //note each .read() command returns a new entry, with fields accessible by indexing
            //.read() will return false when no other entries can be pulled 
            if (reader.Read() == true)
            {
                returnedEntries.Add(new WallEntry(reader[0].ToString(), reader[1].ToString(),int.Parse(reader[2].ToString()), int.Parse(reader[3].ToString()), int.Parse(reader[4].ToString())));
            }
            else
            {
                break;
            }
        }
        CloseConnection();
        return returnedEntries;
    }
    public static List<WallEntry> LoadWallsFromCriteria(WallEntry.Side side, WallEntry.OpeningType openingType, ZoneType zoneType)
    {
        ConnectToDatabase();
        //create query
        string query = "SELECT * FROM Walls ";
        string filters = "WHERE ";
        filters += " Side = " + ((int)side).ToString();
        filters += " AND OpeningType = " + ((int)openingType).ToString();
        filters += " AND Zone = " + ((int)zoneType).ToString();
        query += filters;
        //send request 
        //Debug.Log("Query:"); Debug.Log(query);
        IDbCommand cmnd_read = dbcon.CreateCommand();
        cmnd_read.CommandText = query;
        reader = cmnd_read.ExecuteReader();

        //store returned entries in list of objects 
        List<WallEntry> returnedEntries = new List<WallEntry>();
        int timeout = 5000;
        for (int i = 0; i < timeout; i++)
        {
            //note each .read() command returns a new entry, with fields accessible by indexing
            //.read() will return false when no other entries can be pulled 
            if (reader.Read() == true)
            {
                returnedEntries.Add(new WallEntry(reader[0].ToString(), reader[1].ToString(), int.Parse(reader[2].ToString()), int.Parse(reader[3].ToString()), int.Parse(reader[4].ToString())));
            }
            else
            {
                break;
            }
        }
        CloseConnection();
        return returnedEntries;
    }
}

public class WallEntry
{
    public string name;
    public string pathToPrefab;
    public Side side;
    public OpeningType openingType;
    public InteriorData.ZoneType zoneType;

    public enum Side
    {//NOTE: do NOT change these without consulting database entries 
        TOP=0, LEFT=1, RIGHT=2, BOTTOM=3
    }
    public enum OpeningType
    {//NOTE: do NOT change these without consulting database entries 
        OPEN = 0,//meaning no wall 
        CLOSED = 1,
        SINGLEDOOR = 2,
        DOUBLEDOOR = 3
    }

    public WallEntry(string name_db,string path_db, int side_db,int zone_db,int opening_db)
    {
        name = name_db;
        pathToPrefab = path_db;
        side = (Side)side_db;
        zoneType = (InteriorData.ZoneType)zone_db;
        openingType = (OpeningType)opening_db;
    }
    public void PrintData()
    {
        Debug.Log("~~~WALL ENTRY DATA~~~");
        Debug.Log(name);
        Debug.Log(pathToPrefab);
        Debug.Log(side);
        Debug.Log(zoneType);
        Debug.Log(openingType);
    }
}
