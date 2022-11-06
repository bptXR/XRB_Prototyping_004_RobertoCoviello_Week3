using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI wavesText;
    [SerializeField] private Transform[] spawnLocations;

    private float _spawnInterval;

    public int currWave;
    public int waveDuration = 30;

    public void StartNextWave()
    {
        
    }

    private void SpawnEnemy()
    {
        GameObject enemy = EnemyPool.instance.GetPooledEnemy();

        if (enemy == null) return;
        enemy.transform.position = spawnLocations[Random.Range(0,6)].position;
        enemy.SetActive(true);
    }
}