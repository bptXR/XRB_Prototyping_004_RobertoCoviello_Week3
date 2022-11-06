using Interaction;
using UnityEngine;

public class GameMenu : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip gameSound;
    [SerializeField] private AudioClip menuSound;

    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject restartScreen;
    [SerializeField] private GameObject playerCanvas;
    [SerializeField] private Timer timer;
    [SerializeField] private WaveSpawner waveSpawner;

    [SerializeField] private XRJoystick joystick;
    [SerializeField] private XRPushButton jumpButton;

    private int _startingRound = 1;
    private int _currentRound;
    
    private void Awake()
    {
        joystick.enabled = false;
        jumpButton.enabled = false;
        restartScreen.SetActive(false);
        playerCanvas.SetActive(false);
    }

    private void Start()
    {
        audioSource.PlayOneShot(menuSound);
    }

    public void OnStartPressed()
    {
        playerCanvas.SetActive(true);
        startScreen.SetActive(false);
        waveSpawner.enabled = true;
        timer.enabled = true;
        _currentRound = _startingRound;
    }

    public void OnReloadPressed()
    {
        
    }

    public void GameOverScreen()
    {
        joystick.enabled = false;
        jumpButton.enabled = false;
        playerCanvas.SetActive(false);
    }
}