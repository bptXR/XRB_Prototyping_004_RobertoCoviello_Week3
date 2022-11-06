using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    public float timeLeft = 30;
    [SerializeField] private bool timerOn;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private WaveSpawner waveSpawner;

    private void Awake() => timeLeft = (float)waveSpawner.waveDuration;

    private void OnEnable() => timerOn = true;

    private void OnDisable() => timerOn = false;

    private void Update()
    {
        if (!timerOn) return;
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            UpdateTimer(timeLeft);
        }
        else
        {
            timeLeft = 0;
            timerOn = false;
            RestartTimer();
        }
    }

    private void UpdateTimer(float currentTime)
    {
        currentTime += 1;

        float seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.SetText($"Time Left: {seconds}");
    }

    private void RestartTimer()
    {
        waveSpawner.StartNextWave();
        timeLeft = (float)waveSpawner.waveDuration;
        timerOn = true;
    }
}