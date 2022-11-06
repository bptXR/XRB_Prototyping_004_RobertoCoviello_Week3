using System;
using Player;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] attackSounds;

    private NavMeshAgent _enemyNavMesh;
    private Transform _playerTransform;
    private Gladiator _gladiator;
    private Animator _anim;

    private static readonly int Idle = Animator.StringToHash("Idle");

    private void Awake()
    {
        _enemyNavMesh = GetComponent<NavMeshAgent>();
        _gladiator = FindObjectOfType<Gladiator>();
        _playerTransform = _gladiator.transform;
        _anim = GetComponent<Animator>();
    }

    private void Start()
    {
        _enemyNavMesh.speed = Random.Range(1.5f, 2.8f);
    }

    private void Update()
    {
        _enemyNavMesh.SetDestination(_playerTransform.position);
        if (!_gladiator.isDead) return;
        StopEnemy();
        print("Player Dead");
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