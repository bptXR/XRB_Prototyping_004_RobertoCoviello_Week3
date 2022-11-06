using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI wavesText;
    [SerializeField] private Transform[] spawnLocations;

    private double _spawnInterval;
    private bool _canSpawn;

    public int currWave = 0;
    public double waveDuration = 30;

    private void Awake()
    {
        currWave = 0;
        _spawnInterval = waveDuration / EnemyPool.instance.amountOfEnemies;
    }

    private void OnEnable()
    {
        StartNextWave();
    }

    public void StartNextWave()
    {
        List<GameObject> enemies = EnemyPool.instance.pooledEnemies;
        foreach (var enemy in enemies)
        {
            enemy.SetActive(false);
        }

        _canSpawn = true;
        currWave++;
        wavesText.SetText($"Current Wave: {currWave}");
    }

    private void Update()
    {
        if (!_canSpawn) return;
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        GameObject enemy = EnemyPool.instance.GetPooledEnemy();
        _spawnInterval -= Time.deltaTime;
        if (!(_spawnInterval <= 0)) return;
        
        if (enemy != null)
        {
            enemy.transform.position = spawnLocations[Random.Range(0, 6)].position;
            enemy.SetActive(true);
        }
        else
        {
            _canSpawn = false;
        }
    }
}