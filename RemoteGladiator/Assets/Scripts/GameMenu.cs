using System.Collections;
using System.Collections.Generic;
using Interaction;
using UnityEngine;

public class GameMenu : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip gameSound;

    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject restartScreen;
    [SerializeField] private GameObject playerCanvas;

    [SerializeField] private XRJoystick joystick;
    [SerializeField] private XRPushButton jumpButton;
    
    public void GameOverScreen()
    {
        
    }
}
