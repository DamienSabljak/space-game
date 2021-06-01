using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour {

    [SerializeField] public Grid RoomGrid; //grid to hold all room tilemaps in 
    [SerializeField] public GameObject ui;//stores reference to UI object with ui script for calls 
    [Header("Spawn Parameters")]
    [SerializeField] public int maxEnemiesPerRoom;//regardless of enemy type 

    [Header("Spawn Waves")]
    [SerializeField] List<GameObject> SpawnWaves;
    public static bool EnemiesHaveBeenSpawned = false;
    

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
    {   //main method for generating enemies on a floor
        //method uses premade wave objects to determine generation data (such as number of enemies)
        //method goes over each room, places 1-3 enemies of each type randomly in a room
        //if the # of enemies to be spawned isnt reached, restart process again 
        //****TO FIX: Find way to make sure enemies arent spawned on walls*****
        
        Debug.Log("~~~~~~~~~~~ enemies being spawned... ~~~~~~~~~~~~~");
        //extract number of enemies from wave object
        SpawnWave spawnWave;
        //Debug.Log("CurrentLevel: " + LevelManager.currentLevelnum);

        //select spawn wave to retreive info from 
        if (LevelManager.currentLevelnum-1 >= SpawnWaves.Count)
        {
            Debug.Log("WARNING: A call was made to access a wave level that wasnt in the list, using first wave as placeholder");
            spawnWave = SpawnWaves[0].GetComponent<SpawnWave>();
        }
        else
            spawnWave = SpawnWaves[LevelManager.currentLevelnum-1].GetComponent<SpawnWave>();
        List<int> remainingEnemiesToSpawn = new List<int>(spawnWave.EnemyAmounts);
        //cycle through each enemy type in wave
            //cycle through each room
            int timeout = 0;
            while (!AllEnemiesSpawned(remainingEnemiesToSpawn) && timeout < 100)
            {
                foreach (Transform room in RoomGrid.transform)//access rooms via childs of roomgrid gameobject
                {

                    if (room.GetComponent<Room>() != null && room.gameObject.GetComponent<Room>().RoomWasGenerated == true)
                    {   //object is indeed a room and it was generated
                        CycleEnemyTypesAndSpawn(room.GetComponent<Room>(), remainingEnemiesToSpawn, spawnWave);
                    }
                    if (AllEnemiesSpawned(remainingEnemiesToSpawn))
                        break;
                }
                timeout++;
            }
            if(timeout >= 100)
        {
            Debug.Log("WARNING: SPAWNENEMIES HAS TIMED OUT");
        }
        ui.GetComponent<UI>().DisplayGameAlert("Level " + LevelManager.currentLevelnum + ": " + Level.CurrentLevel.remainingEnemies.Count.ToString() + " Enemies remaining");
    }

    private void SpawnEnemyInRandomPosition(Room room, GameObject enemyType)
    {   //spawn enemy of enemyType in a random postition inside room
        //Debug.Log("spawn enemy in random position");
        //instantiate enemy and place within room
        GameObject newEnemy = Instantiate(enemyType, room.transform.position, Quaternion.identity);
        newEnemy.transform.parent = room.transform;
        newEnemy.transform.localPosition = new Vector2(Random.Range(Room.RoomLocalBounds[0], Room.RoomLocalBounds[1]), Random.Range(Room.RoomLocalBounds[2], Room.RoomLocalBounds[3]));
        Level.CurrentLevel.remainingEnemies.Add(newEnemy);//keep track of enemies on current level
        room.EnemiesSpawnedInThisRoom++;
    }
    
    private void CycleEnemyTypesAndSpawn(Room room, List<int> remainingEnemiesToSpawn, SpawnWave spawnWave)
    {//places a couple enemies of each enemy type in a given room 
        //Debug.Log("~cycle spawn enemy in room~");
        for (int enemyType_ind = 0; enemyType_ind < remainingEnemiesToSpawn.Count; enemyType_ind++)
        {   //for each enemy type 

            int enemiesSpawnedInThisRoom = Random.Range(1, 3);
            for (int j = 0; j < enemiesSpawnedInThisRoom && room.GetComponent<Room>().EnemiesSpawnedInThisRoom < maxEnemiesPerRoom; j++)
            {   //for given random enemies 
                SpawnEnemyInRandomPosition(room.GetComponent<Room>(), spawnWave.Enemytypes[enemyType_ind]);
                remainingEnemiesToSpawn[enemyType_ind] -= 1;
                if (remainingEnemiesToSpawn[enemyType_ind] <= 0)
                    break;

            }
        }
    }

    private bool AllEnemiesSpawned(List<int> remainingEnemiesToSpawn)
    {
        for(int i=0;i<remainingEnemiesToSpawn.Count;i++)
        {
            if (remainingEnemiesToSpawn[i] > 0);
            return false;
        }
        return true;
    }

    //***OLDDD METHODS***
    /*
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
    */
}
