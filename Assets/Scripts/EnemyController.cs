using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private bool initialized = false;
    public List<GameObject> enemies = new List<GameObject>();
    private int wavesCompleted = 0;
    private int currentWave = 0;
    private int enemyIncreasePerWave = 2;//TODO: Create a generated number based on current wave and completed to increase difficulty;
    private int currentWaveEnemies = 0;
    private int waveLengthSeconds = 30;
    [SerializeField]
    private GameObject enemyBrickPrefab;

    private void Awake()
    {
        SpawnEnemies();
        this.initialized = true;

    }

    private void SpawnEnemies()
    {
        List<GameObject> newEnemies = new List<GameObject>();
        int enemiesToSpawn = currentWaveEnemies + enemyIncreasePerWave;
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GameObject enemy = Instantiate(enemyBrickPrefab);
            newEnemies.Add(enemy);
        }
        this.currentWaveEnemies += enemiesToSpawn;
        foreach (var e in enemies)
        {
            EnemyBrick enemyComponent = e.GetComponent<EnemyBrick>();
            if (enemyComponent.IsDead == false)
            {
                newEnemies.Add(e);
            }
        }

        this.enemies = newEnemies;
        this.currentWave++;
        this.wavesCompleted = currentWave - 1;
    }


}
