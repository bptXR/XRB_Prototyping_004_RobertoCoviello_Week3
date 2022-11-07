using System;
using Player;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] attackSounds;
    
    private WaveSpawner _waveSpawner;
    private NavMeshAgent _enemyNavMesh;
    private Transform _playerTransform;
    private Gladiator _gladiator;
    private Animator _anim;

    private static readonly int Idle = Animator.StringToHash("Idle");

    private void Awake()
    {
        _waveSpawner = FindObjectOfType<WaveSpawner>();
        _enemyNavMesh = GetComponent<NavMeshAgent>();
        _gladiator = FindObjectOfType<Gladiator>();
        _playerTransform = _gladiator.transform;
        _anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        var speed = Random.Range(1.5f, 2.8f);
        speed += 0.1f * _waveSpawner.currWave;
        _enemyNavMesh.speed = speed;
    }

    private void FixedUpdate()
    {
        _enemyNavMesh.SetDestination(_playerTransform.position);
        if (!_gladiator.isDead) return;
        StopEnemy();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Sounds(attackSounds);
    }

    private void Sounds(AudioClip[] clips)
    {
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }

    private void StopEnemy()
    {
        _enemyNavMesh.isStopped = true;
        audioSource.enabled = false;
        _anim.SetTrigger(Idle);
    }
}