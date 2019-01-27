using System.Collections.Generic;
using UnityEngine;
using TP.ExtensionMethods;

public class EnemyController : MonoBehaviour
{
    [Header(" --- Attributes --- ")]
    private bool initialized = false;
    public int wavesCompleted = 0;
    public int currentWave = 0;
    public int enemyIncreasePerWave = 2;//TODO: Create a generated number based on current wave and completed to increase difficulty;
    public int currentWaveEnemies = 0;
    public int waveDurationMultiplier = 5;//TODO: Add a ratio so the time doesn't exponentially per block
    public int newWaveStartingDelayInSeconds = 0;
    public int waveDurationInSeconds = 0;
    public float waveDurationMultiplierGrowth = .6f;

    [Header(" --- Prefabs --- ")]
    [SerializeField]
    private List<GameObject> enemyBrickPrefab;
    public List<GameObject> enemies = new List<GameObject>();

    private void Awake()
    {
        this.initialized = true;
        StartWave();
    }

    private void SpawnEnemies()
    {
        List<int> itemIndexesToDelete = new List<int>();
        for (int i = 0; i < this.enemies.Count; i++)
        {
            EnemyBrick enemyComponent = enemies[i].GetComponent<EnemyBrick>();
            if (enemyComponent.isDead)
            {
                var destroyed = enemyComponent.Destroy();
                if (destroyed)
                {
                    enemies.Remove(enemies[i]);
                    i -= 1;
                }
                itemIndexesToDelete.Add(i);
            }
            else
            {
                enemyComponent.ResetToDefaults();
            }
        }

        for (int i = 0; i < enemyIncreasePerWave; i++)
        {
            GameObject brick = enemyBrickPrefab.PickRandom();
            GameObject enemy = Instantiate(brick);
            enemies.Add(enemy);
        }
        this.currentWaveEnemies += enemyIncreasePerWave;

        //foreach (var i in itemIndexesToDelete)
        //{
        //        enemies[i].Destroy();
        //    enemies.Remove(enemies[i]);
        //}

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
        return (int)((this.enemies.Count * this.waveDurationMultiplier) * waveDurationMultiplierGrowth);
    }
}
