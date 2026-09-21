using UnityEngine;

public class KillZone : MonoBehaviour
{
    [SerializeField] private RaceManager raceManager;

    private void Awake()
    {
        if (raceManager == null)
            raceManager = Object.FindFirstObjectByType<RaceManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        CarController car = other.GetComponentInParent<CarController>();
        if (car != null && raceManager != null)
        {
            raceManager.ResetCarToLastCheckpoint();
        }
    }
}