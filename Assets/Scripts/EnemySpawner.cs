using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    [SerializeField] List<WaveConfig> waveConfigs;
    int startingWave = 0;
    int enemyCount;
    [SerializeField] bool looping = false;


	// Use this for initialization
	IEnumerator Start () {
        do
        { 
            yield return StartCoroutine(SpawnAllWaves());
        } while (looping);
	}

    private IEnumerator SpawnAllWaves()
    {
        foreach(WaveConfig wave in waveConfigs){
            yield return StartCoroutine(SpawnAllEnemiesInWave(wave));
        }

    }

    private IEnumerator SpawnAllEnemiesInWave(WaveConfig waveConfig)
    {
        enemyCount = waveConfig.getEnemyCount();
        for (int i = 0; i < enemyCount; i++)
        {
            //Instantiate method: what, where, rotation
            var newEnemy = Instantiate(
                waveConfig.getEnemyPrefab(),
                waveConfig.getWaypoints()[0].transform.position,
                Quaternion.identity);
            newEnemy.GetComponent<EnemyPathing>().setWaveConfig(waveConfig);

            yield return new WaitForSeconds(waveConfig.getSpawnTime());
        }

    }

    // Update is called once per frame
    void Update () {
		
	}
}
