using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Enemy Wave Config")]
public class WaveConfig : ScriptableObject {

    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject pathPrefab;
    [SerializeField] float spawnTime = 0.5f;
    [SerializeField] float randSpawnFactor = 0.3f;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] int enemyCount = 5;
    

    public GameObject getEnemyPrefab() { return enemyPrefab; }
    public GameObject getPathPrefab() { return pathPrefab; }
    public List<Transform> getWaypoints() {
        var waveWaypoints = new List<Transform>();
        foreach (Transform child in pathPrefab.transform)
        { waveWaypoints.Add(child); }

        return waveWaypoints; }

    public float getSpawnTime() { return spawnTime; }
    public float getRandSpawnFactor() { return randSpawnFactor; }
    public float getMoveSpeed() { return moveSpeed; }
    public int getEnemyCount() { return enemyCount; }
    



}
