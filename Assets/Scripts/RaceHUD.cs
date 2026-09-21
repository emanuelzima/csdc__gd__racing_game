using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RaceHUD : MonoBehaviour
{
    [Header("HUD Text Elements")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text lapCounterText;
    [SerializeField] private TMP_Text currentLapTimeText;
    [SerializeField] private TMP_Text bestLapTimeText;
    [SerializeField] private TMP_Text totalTimeText;

    [Header("Best Time Highlighting")]
    [SerializeField] private Color normalBestTimeColor = Color.white;
    [SerializeField] private Color highlightBestTimeColor = Color.yellow;
    [SerializeField] private float highlightDuration = 1.5f;

    [Header("Finish Panel")]
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private TMP_Text finishSummaryText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;

    private Coroutine highlightCoroutine;

    private void Awake()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartRace);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        if (bestLapTimeText != null)
            bestLapTimeText.color = normalBestTimeColor;
    }

    public void SetCountdownText(string text)
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(!string.IsNullOrEmpty(text));
            countdownText.text = text;
        }
    }

    public void UpdateSpeed(float speedKmh)
    {
        if (speedText != null)
            speedText.text = $"{Mathf.RoundToInt(speedKmh)} km/h";
    }

    public void UpdateLapCounter(int currentLap, int totalLaps)
    {
        if (lapCounterText != null)
            lapCounterText.text = $"Lap {currentLap}/{totalLaps}";
    }

    public void UpdateLapTime(float timeSeconds)
    {
        if (currentLapTimeText != null)
            currentLapTimeText.text = $"Time: {FormatTime(timeSeconds)}";
    }

    public void UpdateBestLapTime(float timeSeconds, bool isNewBest = false)
    {
        if (bestLapTimeText == null) return;

        bestLapTimeText.text = timeSeconds < float.MaxValue 
            ? $"Best: {FormatTime(timeSeconds)}" 
            : "Best: --:--.--";

        if (isNewBest)
        {
            if (highlightCoroutine != null)
                StopCoroutine(highlightCoroutine);

            highlightCoroutine = StartCoroutine(HighlightRoutine());
        }
    }

    private IEnumerator HighlightRoutine()
    {
        bestLapTimeText.color = highlightBestTimeColor;
        yield return new WaitForSeconds(highlightDuration);
        bestLapTimeText.color = normalBestTimeColor;
        highlightCoroutine = null;
    }

    public void UpdateTotalTime(float timeSeconds)
    {
        if (totalTimeText != null)
            totalTimeText.text = $"Total: {FormatTime(timeSeconds)}";
    }

    public void ShowFinishScreen(float totalTime, float bestLapTime, List<float> lapTimes)
    {
        if (finishPanel == null) return;

        finishPanel.SetActive(true);

        if (finishSummaryText != null)
        {
            string summary = $"Race Finished!\n\nTotal Time: {FormatTime(totalTime)}\nBest Lap: {FormatTime(bestLapTime)}\n\nLaps:\n";
            for (int i = 0; i < lapTimes.Count; i++)
            {
                summary += $"Lap {i + 1}: {FormatTime(lapTimes[i])}\n";
            }
            finishSummaryText.text = summary;
        }
    }

    public void RestartRace()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int fraction = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
        return $"{minutes:00}:{seconds:00}.{fraction:00}";
    }
}