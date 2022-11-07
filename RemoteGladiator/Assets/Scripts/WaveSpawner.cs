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

    private bool _canSpawn;
    private float _spawnTimer = 1f;

    public int currWave = 0;
    public double waveDuration = 30;

    private void Awake() => currWave = 0;

    private void OnEnable() => StartNextWave();

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

        if (enemy != null)
        {
            _spawnTimer -= Time.deltaTime;
            if (!(_spawnTimer <= 0)) return;
            enemy.transform.position = spawnLocations[Random.Range(0, 6)].position;
            enemy.SetActive(true);
            _spawnTimer = 1.5f - (currWave * 0.05f);
            if (!(_spawnTimer <= 0)) return;
            _spawnTimer = 0;
        }
        else
        {
            _canSpawn = false;
        }
    }
}