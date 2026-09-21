using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RaceManager : MonoBehaviour
{
    [Header("Race Configuration")]
    [SerializeField] private int totalLaps = 3;
    [SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();

    [Header("References")]
    [SerializeField] private CarController playerCar;
    [SerializeField] private RaceHUD raceHUD;

    private int currentLap = 1;
    private int nextExpectedCheckpointIndex = 0;
    private bool raceRunning = false;
    private bool raceFinished = false;

    private float currentLapTime = 0f;
    private float bestLapTime = float.MaxValue;
    private float totalTime = 0f;
    private readonly List<float> completedLapTimes = new List<float>();

    private Checkpoint lastPassedCheckpoint;

    private void Awake()
    {
        for (int i = 0; i < checkpoints.Count; i++)
        {
            if (checkpoints[i] != null)
                checkpoints[i].Initialize(this, i);
        }

        if (checkpoints.Count > 0 && checkpoints[0] != null)
            lastPassedCheckpoint = checkpoints[0];
    }

    private void Start()
    {
        if (playerCar != null)
            playerCar.SetControlsEnabled(false);

        if (raceHUD != null)
        {
            raceHUD.UpdateLapCounter(currentLap, totalLaps);
            raceHUD.UpdateBestLapTime(bestLapTime, false);
        }

        StartCoroutine(CountdownRoutine());
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.rKey.wasPressedThisFrame && !raceFinished)
        {
            ResetCarToLastCheckpoint();
        }

        if (raceRunning && !raceFinished)
        {
            currentLapTime += Time.deltaTime;
            totalTime += Time.deltaTime;

            if (raceHUD != null)
            {
                raceHUD.UpdateLapTime(currentLapTime);
                raceHUD.UpdateTotalTime(totalTime);
            }
        }

        if (raceHUD != null && playerCar != null)
        {
            raceHUD.UpdateSpeed(playerCar.SpeedKmh);
        }
    }

    private IEnumerator CountdownRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (raceHUD != null) raceHUD.SetCountdownText("3");
        yield return new WaitForSeconds(1f);

        if (raceHUD != null) raceHUD.SetCountdownText("2");
        yield return new WaitForSeconds(1f);

        if (raceHUD != null) raceHUD.SetCountdownText("1");
        yield return new WaitForSeconds(1f);

        if (raceHUD != null) raceHUD.SetCountdownText("GO!");
        if (playerCar != null) playerCar.SetControlsEnabled(true);
        raceRunning = true;

        nextExpectedCheckpointIndex = 1;

        yield return new WaitForSeconds(1f);
        if (raceHUD != null) raceHUD.SetCountdownText("");
    }

    public void OnCarPassedCheckpoint(Checkpoint checkpoint, CarController car)
    {
        if (car != playerCar || !raceRunning || raceFinished) return;

        if (checkpoint.Index == nextExpectedCheckpointIndex)
        {
            lastPassedCheckpoint = checkpoint;

            if (checkpoint.Index == 0)
            {
                CompleteLap();
                nextExpectedCheckpointIndex = 1;
            }
            else
            {
                nextExpectedCheckpointIndex++;
                if (nextExpectedCheckpointIndex >= checkpoints.Count)
                    nextExpectedCheckpointIndex = 0;
            }
        }
    }

    private void CompleteLap()
    {
        completedLapTimes.Add(currentLapTime);

        bool isNewBest = false;
        if (currentLapTime < bestLapTime)
        {
            bestLapTime = currentLapTime;
            isNewBest = true;
        }

        if (raceHUD != null)
            raceHUD.UpdateBestLapTime(bestLapTime, isNewBest);

        currentLapTime = 0f;

        if (currentLap < totalLaps)
        {
            currentLap++;
            if (raceHUD != null)
                raceHUD.UpdateLapCounter(currentLap, totalLaps);
        }
        else
        {
            FinishRace();
        }
    }

    private void FinishRace()
    {
        raceFinished = true;
        raceRunning = false;

        if (playerCar != null)
            playerCar.SetControlsEnabled(false);

        if (raceHUD != null)
            raceHUD.ShowFinishScreen(totalTime, bestLapTime, completedLapTimes);
    }

    public void ResetCarToLastCheckpoint()
    {
        if (playerCar == null || lastPassedCheckpoint == null) return;

        Transform targetTransform = lastPassedCheckpoint.transform;
        Collider col = lastPassedCheckpoint.GetComponent<Collider>();
        Vector3 spawnPosition = col != null ? col.bounds.center : targetTransform.position;

        Vector3 targetPos = spawnPosition + Vector3.up * 0.2f;
        playerCar.Teleport(targetPos, targetTransform.rotation);
    }
}