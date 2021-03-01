using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour {

    [SerializeField] public Grid RoomGrid; //grid to hold all room tilemaps in 
    [SerializeField] public GameObject ui;//stores reference to UI object with ui script for calls 
    [Header("Spawn Points")]
    [SerializeField] GameObject EnemySpawnPoint;
    [SerializeField] int MinEnemies=0;
    [SerializeField] int MaxEnemies=4;
    [Header("Spawn Waves")]
    [SerializeField] List<GameObject> SpawnWaves;
    private int CurrentWave = 0;

    [Header("Spawn Clusters")]
    [SerializeField] GameObject EnemySpawnCluster;
    [SerializeField] GameObject ScrapSpawnCluster;
    [Header("Probabilites")]
    [SerializeField] [Range(0,1)] float EnemySpawnProbability = 0.5f;
    [SerializeField] [Range(0,1)] float ScrapSpawnProbability = 0.25f;
    private bool EnemiesHaveBeenSpawned = false;
    

    // Use this for initialization
    void Start () {
        EnemiesHaveBeenSpawned = false;//needed for new scenes loaded
    }

    // Update is called once per frame
    void Update () {
        if (Floor.RoomsHaveBeenGenerated && !EnemiesHaveBeenSpawned)
        {
            SpawnEnemies();
            EnemiesHaveBeenSpawned = true;
        }
        
	}

    //Find a way to call this, have to wait for rooms to be completed
    public void SpawnEnemies()
    {
        //extract number of enemies from wave object
        int amountToBeSpawned;
        GameObject newEnemy;
        SpawnWave spawnWave;
        Debug.Log("CurrentLevel: " + LevelManager.currentLevelnum);
        if (LevelManager.currentLevelnum-1 >= SpawnWaves.Count)
        {
            Debug.Log("WARNING: A call was made to access a wave level that wasnt in the list, using first wave as placeholder");
            spawnWave = SpawnWaves[0].GetComponent<SpawnWave>();
        }
        else
            spawnWave = SpawnWaves[LevelManager.currentLevelnum-1].GetComponent<SpawnWave>();

        //cycle through each enemy type in wave
        for (int i=0; i< spawnWave.Enemytypes.Count;i++)
        {
            if (spawnWave.Enemytypes[i] == null)
                Debug.Log("WARNING: a null has been encountered, make sure the wave objects used do not have null's in the enemyTypes list");

            amountToBeSpawned = spawnWave.EnemyAmounts[i];//clone copy object variable to not mutate it 
            //cycle through each room
            int timeout = 0;
            while (amountToBeSpawned > 0 && timeout < 10000)
            {
                foreach (Transform room in RoomGrid.transform)
                {

                    if (room.GetComponent<Room>() != null && room.GetComponent<Room>().EnableGeneration == true)//used so only wanted rooms have enemies spawned
                    {
                        int enemiesInThisRoom = Random.Range(1, amountToBeSpawned);//multiple enemies could be spawned in the same room
                        for (int j = 0; j < enemiesInThisRoom; j++)
                        {
                            Debug.Log("amount to be spawned: " + amountToBeSpawned);
                            newEnemy = Instantiate(spawnWave.Enemytypes[i], room.position, Quaternion.identity);//**HAVING ISSUES WITH THIS LINE NOT RUNNING**
                            Debug.Log("enemy placed from SpawnEnemies()!!");
                            newEnemy.transform.parent = room;
                            //spawn in random place in room
                            newEnemy.transform.localPosition = new Vector2(Random.Range(Room.RoomLocalBounds[0], Room.RoomLocalBounds[1]), Random.Range(Room.RoomLocalBounds[2], Room.RoomLocalBounds[3]));//place in random spot
                            amountToBeSpawned -= 1;
                            Level.CurrentLevel.remainingEnemies += 1;//keep track of enemies on current level
                            //****TO FIX: Find way to make sure enemies arent spawned on walls*****

                        }
                        //CreateEnemySpawnCluster(room);
                    }
                    if (amountToBeSpawned <= 0)
                        break;
                }
                timeout++;
            }
            Debug.Log("amount to be spawned: " + amountToBeSpawned);
        }
        ui.GetComponent<UI>().DisplayGameAlert("Level " + LevelManager.currentLevelnum + ": " + Level.CurrentLevel.remainingEnemies.ToString() + " Enemies remaining");
    }

    //***OLDDD METHOD***
    
    public void PlaceScrapSpawnClusters()
    {
        //cycle through each room
        foreach (Transform room in RoomGrid.transform)
        {
            //Debug.Log("checking room");
            if (room.gameObject.GetComponent<Room>() != null)
            {
                if (Random.Range(0, 1f) < ScrapSpawnProbability)
                {
                    CreateScrapSpawnCluster(room);
                }
            }
        }
    }

    private void CreateEnemySpawnCluster(Transform room)
    {
        //  Debug.Log("Breathing Life into the world");
        GameObject spawnCluster = Instantiate(EnemySpawnCluster, room.position, Quaternion.identity);
        //center spawn point
        spawnCluster.transform.parent = room;
        spawnCluster.transform.localPosition = new Vector2(-72.0f, 5.6f);//found by checking in game runtime
    }

    private void CreateScrapSpawnCluster(Transform room)
    {
        //  Debug.Log("Breathing Life into the world");
        GameObject spawnCluster = Instantiate(ScrapSpawnCluster, room.position, Quaternion.identity);
        //center spawn point
        spawnCluster.transform.parent = room;
        spawnCluster.transform.localPosition = new Vector2(Random.Range(Room.RoomLocalBounds[0], Room.RoomLocalBounds[1]), Random.Range(Room.RoomLocalBounds[2], Room.RoomLocalBounds[3]));//place in random spot
    }
    
}
