using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimerRestart : MonoBehaviour
{
    public float timeLeft = 10;
    [SerializeField] private bool timerOn;
    [SerializeField] private TextMeshProUGUI timerText;

    private void OnEnable()
    {
        timerOn = true;
    }

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
            RestartGame();
        }
    }

    private void UpdateTimer(float currentTime)
    {
        currentTime += 1;

        float seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.SetText(string.Format("{0}", seconds));
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}