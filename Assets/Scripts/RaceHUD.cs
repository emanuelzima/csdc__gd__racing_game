using TMPro;
using UnityEngine;

public class RaceHUD : MonoBehaviour
{
    [Header("HUD Text Elements")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text lapCounterText;
    [SerializeField] private TMP_Text currentLapTimeText;
    [SerializeField] private TMP_Text bestLapTimeText;
    [SerializeField] private TMP_Text totalTimeText;

    [Header("Finish Panel")]
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private TMP_Text finishSummaryText;

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

    public void UpdateBestLapTime(float timeSeconds)
    {
        if (bestLapTimeText != null)
        {
            bestLapTimeText.text = timeSeconds < float.MaxValue 
                ? $"Best: {FormatTime(timeSeconds)}" 
                : "Best: --:--.--";
        }
    }

    public void UpdateTotalTime(float timeSeconds)
    {
        if (totalTimeText != null)
            totalTimeText.text = $"Total: {FormatTime(timeSeconds)}";
    }

    public void ShowFinishScreen(float totalTime, float bestLapTime)
    {
        if (finishPanel != null)
        {
            finishPanel.SetActive(true);
            if (finishSummaryText != null)
            {
                finishSummaryText.text = $"Race Finished!\n\nTotal Time: {FormatTime(totalTime)}\nBest Lap: {FormatTime(bestLapTime)}";
            }
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int fraction = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
        return $"{minutes:00}:{seconds:00}.{fraction:00}";
    }
}