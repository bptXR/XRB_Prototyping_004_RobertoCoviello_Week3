using System.Collections.Generic;
using Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject restartScreen;
    [SerializeField] private GameObject playerCanvas;
    [SerializeField] private Timer timer;
    [SerializeField] private WaveSpawner waveSpawner;

    [SerializeField] private XRJoystick joystick;
    [SerializeField] private XRPushButton jumpButton;

    private void Awake()
    {
        restartScreen.SetActive(false);
        playerCanvas.SetActive(false);
        startScreen.SetActive(true);
        waveSpawner.enabled = false;
        jumpButton.enabled = false;
        joystick.enabled = false;
        timer.enabled = false;
    }

    public void OnStartPressed()
    {
        playerCanvas.SetActive(true);
        startScreen.SetActive(false);
        waveSpawner.enabled = true;
        jumpButton.enabled = true;
        joystick.enabled = true;
        timer.enabled = true;
    }

    public void OnReloadPressed() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    
    public void GameOverScreen()
    {
        restartScreen.SetActive(true);
        playerCanvas.SetActive(false);
        waveSpawner.enabled = false;
        jumpButton.enabled = false;
        joystick.enabled = false;
        timer.enabled = false;
    }
}