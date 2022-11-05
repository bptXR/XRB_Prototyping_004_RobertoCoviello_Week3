using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Enemies;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player2 : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    [SerializeField] private HealthBar healthBar;
    [SerializeField] private TimerRestart timer;
    
    [SerializeField] private Image gameOverImage;
    [SerializeField] private Image[] gameOverImageSides;
    //[SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject waveSpawner;

    [SerializeField] private AudioClip[] getHitSounds;
    [SerializeField] private AudioClip dieSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioSource audioSource;

    private Enemy[] _enemies;

    private void Awake()
    {
        Time.timeScale = 1f;
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        audioSource.enabled = true;
    }

    public void TakeDamage(int playerHealth)
    {
        currentHealth = playerHealth;
        healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            StartCoroutine(GameOver());
        }
        else
        {
            Sounds(getHitSounds);
        }
    }

    private IEnumerator GameOver()
    {
        waveSpawner.SetActive(false);
        audioSource.PlayOneShot(dieSound);
        yield return new WaitWhile(() => audioSource.isPlaying);
        audioSource.PlayOneShot(gameOverSound);
        Time.timeScale = 0.1f;
        foreach (var side in gameOverImageSides)
        {
            side.DOFade(1, 1);
        }
        gameOverImage.DOFade(1, 1).OnComplete(() => timer.enabled = true);
        yield return new WaitWhile(() => audioSource.isPlaying);
        audioSource.enabled = false;
    }

    private void Sounds(AudioClip[] clips)
    {
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }
}