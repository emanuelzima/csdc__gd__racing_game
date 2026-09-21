using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RaceManager : MonoBehaviour
{
    [Header("Race Configuration")]
    [SerializeField] private int totalLaps = 3;
    [SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();

    [Header("Target Car")]
    [SerializeField] private CarController playerCar;

    private int currentLap = 1;
    private int nextExpectedCheckpointIndex = 0;
    private bool raceStarted = false;
    private Checkpoint lastPassedCheckpoint;
    private Rigidbody playerRb;

    public int CurrentLap => currentLap;
    public int TotalLaps => totalLaps;

    private void Awake()
    {
        if (playerCar != null)
        {
            playerRb = playerCar.GetComponent<Rigidbody>();
        }

        for (int i = 0; i < checkpoints.Count; i++)
        {
            if (checkpoints[i] != null)
            {
                checkpoints[i].Initialize(this, i);
            }
        }

        if (checkpoints.Count > 0 && checkpoints[0] != null)
        {
            lastPassedCheckpoint = checkpoints[0];
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
        {
            ResetCarToLastCheckpoint();
        }
    }

    public void OnCarPassedCheckpoint(Checkpoint checkpoint, CarController car)
    {
        if (car != playerCar) return;

        if (checkpoint.Index == nextExpectedCheckpointIndex)
        {
            lastPassedCheckpoint = checkpoint;

            if (checkpoint.Index == 0)
            {
                if (!raceStarted)
                {
                    raceStarted = true;
                    Debug.Log("Rennen gestartet!");
                }
                else
                {
                    CompleteLap();
                }
                nextExpectedCheckpointIndex = 1;
            }
            else
            {
                nextExpectedCheckpointIndex++;

                if (nextExpectedCheckpointIndex >= checkpoints.Count)
                {
                    nextExpectedCheckpointIndex = 0;
                }
            }
        }
    }

    private void CompleteLap()
    {
        if (currentLap < totalLaps)
        {
            currentLap++;
            Debug.Log($"Runde {currentLap}/{totalLaps} gestartet");
        }
        else
        {
            Debug.Log("Rennen beendet!");
        }
    }

    private void ResetCarToLastCheckpoint()
    {
        if (playerCar == null || lastPassedCheckpoint == null) return;

        Transform targetTransform = lastPassedCheckpoint.transform;
        Vector3 spawnPosition = targetTransform.position;

        Collider col = lastPassedCheckpoint.GetComponent<Collider>();
        if (col != null)
        {
            spawnPosition = col.bounds.center;
        }

        Vector3 targetPos = spawnPosition + Vector3.up * 0.2f;
        Quaternion targetRot = targetTransform.rotation;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.position = targetPos;
            playerRb.rotation = targetRot;
        }

        playerCar.transform.position = targetPos;
        playerCar.transform.rotation = targetRot;

        Physics.SyncTransforms();
    }
}