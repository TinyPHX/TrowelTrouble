using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header(" --- Attributes --- ")]
    private bool initialized = false;
    private int wavesCompleted = 0;
    private int currentWave = 0;
    private int enemyIncreasePerWave = 2;//TODO: Create a generated number based on current wave and completed to increase difficulty;
    private int currentWaveEnemies = 0;
    private int waveDurationMultiplier = 5;
    private int newWaveStartingDelayInSeconds = 5;
    private int waveDurationInSeconds = 0;

    [Header(" --- Prefabs --- ")]
    [SerializeField]
    private GameObject enemyBrickPrefab;
    public List<GameObject> enemies = new List<GameObject>();
    
    private void Awake()
    {
        this.initialized = true;
        StartWave();
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
            if (enemyComponent.isDead == false)
            {
                enemyComponent.ResetToDefaults();
                newEnemies.Add(enemyComponent.gameObject);
            }
            else
            {
                Destroy(e);
            }
        }

        this.enemies = newEnemies;
        Invoke("EndWave", waveDurationInSeconds);
    }

    private void StartWave()
    {
        this.currentWave++;
        this.wavesCompleted = currentWave - 1;
        this.waveDurationInSeconds = GetWaveDurationInSeconds();
        if (this.currentWave > 0)
        {
            Invoke("SpawnEnemies", newWaveStartingDelayInSeconds);
        }
        else
        {
          SpawnEnemies();
        }
    }

    private void EndWave()
    {
        StartWave();
    }

    private int GetWaveDurationInSeconds()
    {
        return this.enemies.Count * this.waveDurationMultiplier;
    }
}
