using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;

public class InteriorData : MonoBehaviour
{   //this class is the interface used to interact with the interior database
    public static IDataReader reader;
    public static IDbConnection dbcon;
    public static int[,] HorizontalCorridorHitMissStruct = {{-1,0,-1},{1,1,1},{-1,0,-1}};
    [SerializeField] debugCanvas debugCanvas;
    public static bool databaseHasBeenDownloaded = false;
    
    public enum ZoneType
    {//NOTE: do NOT change these without consulting database entries 
        NORMAL =0, 
        MEDICAL=1, 
        HANGAR=2, 
        COMMAND=3, 
        ARMOURY=4, 
        ENGINEERING=5, 
        OUTSIDE=6, //used to communicate zone is outside of floor / out of bounds 
        DIFFERENT =7, //used to communicate a zone is different than a given floor 
        CORRIDOR = 8, 
        AIRLOCK = 9 //used for first room 
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
        DownloadDatabase();
        //load walls from db
        //List<WallEntry> entries =  LoadWallsFromCriteria(WallEntry.Side.LEFT, WallEntry.OpeningType.CLOSED, ZoneType.NORMAL);
        //entries[0].PrintData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DownloadDatabase()
    {
        debugCanvas.log("download db");
        bool usingUnity = false;
#if UNITY_EDITOR_WIN
        usingUnity = true;
        databaseHasBeenDownloaded = true;
#endif
#if UNITY_ANDROID
        if (!usingUnity && !DatabaseAlreadyDownloaded())
        {
            StartCoroutine(DownloadFile());//use this to extract database from .apk
        }
        else
        {
            debugCanvas.log("databased already downloaded");
            databaseHasBeenDownloaded = true; 
        }
#endif

    }
    public void ConnectToDatabase()
    {
        //open database connection
        bool usingEditor = false;
        string connection = "";
#if UNITY_EDITOR_WIN
        connection = "URI=file:" + Application.dataPath + "/StreamingAssets/Databases/Resources/interiorDatabase.db3";
        usingEditor = true;
#endif
#if UNITY_ANDROID
        if (!usingEditor)
        {
            //debugCanvas.log("see if application.persistent path broken" );
            //debugCanvas.log(Application.persistentDataPath.ToString());
            connection = "URI=file:" + Application.persistentDataPath + "/interiorDatabase.db3";
            //debugCanvas.log("attempting to connect to database at:");
            //debugCanvas.log(connection);
            //PrintFilesAtPath(connection);
        }
#endif
        IDbConnection dbcon1 = new SqliteConnection(connection);
        //debugCanvas.log("dbcon is null? ");
        //debugCanvas.log((dbcon1 == null).ToString());
        dbcon = dbcon1;
        try
        {
            dbcon.Open();
            //debugCanvas.log("database successfully opened");
        }
        catch (System.Exception e)
        {
            debugCanvas.log("error: connection to data base could not be made");
        }
        //Debug.Log("*****Database connection established*****");
    }

    public void CloseConnection()
    {   //NOTE: failure to close connection will cause database to lockout, need to close unity 
        //debugCanvas.log("attempt to close connection");
        dbcon.Close();
        //debugCanvas.log("success");
        //Debug.Log("*****Database connection closed*****");
    }

    public IEnumerator DownloadFile()
    {   //method used to extract database from android apk
        debugCanvas.log("startingDownloadFile");
        var webRequest = new UnityWebRequest("jar: file://" + Application.dataPath + "!/assets/" + "Databases/Resources/interiorDatabase.db3", UnityWebRequest.kHttpVerbGET);
        string path = Path.Combine(Application.persistentDataPath, "interiorDatabase.db3");
        webRequest.downloadHandler = new DownloadHandlerFile(path);
        yield return webRequest.SendWebRequest();
        //once web request has been served
        databaseHasBeenDownloaded = true;
        if (webRequest.isNetworkError || webRequest.isHttpError)
        {
            Debug.LogError(webRequest.error);
            debugCanvas.log("network error");
        }
        else
        {
            Debug.Log("File successfully downloaded and saved to " + path);
            debugCanvas.log("File successfully downloaded and saved to " + path);
            //PrintFilesAtPath(path);
        }

    }

    public bool DatabaseAlreadyDownloaded()
    {   //returns true if database has already been downloaded, preventing errors
        string path = Path.Combine(Application.persistentDataPath, "interiordata.db3");
        return Directory.Exists(path);
    }
    public void PrintFilesAtPath(string path)
    {   //prints all the file names at the specified path, for debugging 
        debugCanvas.log("list of path: " + path);
        string[] files       = Directory.GetFiles(path);
        string[] directories = Directory.GetDirectories(path);
        if(files !=  null  && files.Length > 0)
        {
            debugCanvas.log(files.Length.ToString() + " files found");
            foreach (string filename in files)
            {
                debugCanvas.log(filename);
            }
        }
        else
        {
            debugCanvas.log("no files found");
        }
        if (directories != null && directories.Length > 0)
        {
            debugCanvas.log(directories.Length.ToString() + " directories found");
            foreach (string dirName in directories)
            {
                debugCanvas.log(dirName);
            }
        }
        else
        {
            debugCanvas.log("no directories found");
        }


    }


    public List<WallEntry> LoadWallsFromCriteria(WallEntry.Side side, WallEntry.OpeningType openingType, ZoneType zoneType)
    {
        //debugCanvas.log("Load Walls from criteria called");
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
        //debugCanvas.log("Query:"); debugCanvas.log(query);
        //debugCanvas.log("attempt creatcommand, null?");
        IDbCommand cmnd_read = dbcon.CreateCommand();
        debugCanvas.log((cmnd_read == null).ToString());
        //debugCanvas.log("success");
        cmnd_read.CommandText = query;
        //debugCanvas.log("attempt reader");
        reader = cmnd_read.ExecuteReader();
        //debugCanvas.log("success");

        //store returned entries in list of objects 
        List<WallEntry> returnedEntries = new List<WallEntry>();
        int timeout = 5000;
        for (int i = 0; i < timeout; i++)
        {
            //note each .read() command returns a new entry, with fields accessible by indexing
            //.read() will return false when no other entries can be pulled 
            if (reader.Read() == true)
            {
                //debugCanvas.log("new entry read");
                returnedEntries.Add(new WallEntry(reader[0].ToString(), reader[1].ToString(), int.Parse(reader[2].ToString()), int.Parse(reader[3].ToString()), int.Parse(reader[4].ToString())));
            }
            else
            {
                break;
            }
        }
        CloseConnection();
        //debugCanvas.log(returnedEntries.Count.ToString() + " wall entries returned");
        return returnedEntries;
    }
    public List<FloorEntry> LoadFloorsFromCriteria(ZoneType zoneType)
    {
        ConnectToDatabase();
        //create query
        string query = "SELECT * FROM Floors ";
        string filters = "WHERE ";
        filters += " Zone = " + ((int)zoneType).ToString();
        query += filters;
        //send request 
        //Debug.Log("Query:"); Debug.Log(query);
        IDbCommand cmnd_read = dbcon.CreateCommand();
        cmnd_read.CommandText = query;
        reader = cmnd_read.ExecuteReader();

        //store returned entries in list of objects 
        List<FloorEntry> returnedEntries = new List<FloorEntry>();
        int timeout = 5000;
        for (int i = 0; i < timeout; i++)
        {
            //note each .read() command returns a new entry, with fields accessible by indexing
            //.read() will return false when no other entries can be pulled 
            if (reader.Read() == true)
            {
                returnedEntries.Add(new FloorEntry(reader[0].ToString(), reader[1].ToString(), int.Parse(reader[2].ToString() )));
            }
            else
            {
                break;
            }
        }
        CloseConnection();
        return returnedEntries;
    }
    public List<FurnitureEntry> LoadFurnitureFromCriteria(ZoneType zoneType)
    {
        ConnectToDatabase();
        //create query
        string query = "SELECT * FROM Furniture ";
        string filters = "WHERE ";
        filters += " Zone = " + ((int)zoneType).ToString();
        query += filters;
        //send request 
        //Debug.Log("Query:"); Debug.Log(query);
        IDbCommand cmnd_read = dbcon.CreateCommand();
        cmnd_read.CommandText = query;
        reader = cmnd_read.ExecuteReader();

        //store returned entries in list of objects 
        List<FurnitureEntry> returnedEntries = new List<FurnitureEntry>();
        int timeout = 5000;
        for (int i = 0; i < timeout; i++)
        {
            //note each .read() command returns a new entry, with fields accessible by indexing
            //.read() will return false when no other entries can be pulled 
            if (reader.Read() == true)
            {
                returnedEntries.Add(new FurnitureEntry(reader[0].ToString(), reader[1].ToString(), int.Parse(reader[2].ToString())));
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
        pathToPrefab = "Walls/"+path_db;
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

public class FloorEntry
{
    public string name;
    public string pathToPrefab;
    public InteriorData.ZoneType zoneType;

    public FloorEntry(string name_db, string path_db, int zone_db)
    {
        name = name_db;
        pathToPrefab = "Floors/"+path_db;
        zoneType = (InteriorData.ZoneType)zone_db;
    }
    public void PrintData()
    {
        Debug.Log("~~~FLOOR ENTRY DATA~~~");
        Debug.Log(name);
        Debug.Log(pathToPrefab);
        Debug.Log(zoneType);
    }
}

public class FurnitureEntry
{
    public string name;
    public string pathToPrefab;
    public InteriorData.ZoneType zoneType;

    public FurnitureEntry(string name_db, string path_db, int zone_db)
    {
        name = name_db;
        pathToPrefab = "Furniture/" + path_db;
        zoneType = (InteriorData.ZoneType)zone_db;
    }
    public void PrintData()
    {
        Debug.Log("~~~FURNITURE ENTRY DATA~~~");
        Debug.Log(name);
        Debug.Log(pathToPrefab);
        Debug.Log(zoneType);
    }
}