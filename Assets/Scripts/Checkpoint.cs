using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private int index;
    private RaceManager raceManager;

    public int Index => index;

    public void Initialize(RaceManager manager, int checkpointIndex)
    {
        raceManager = manager;
        index = checkpointIndex;
    }

    private void OnTriggerEnter(Collider other)
    {
        CarController car = other.GetComponentInParent<CarController>();
        if (car != null && raceManager != null)
        {
            raceManager.OnCarPassedCheckpoint(this, car);
        }
    }
}