using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private int index;
    private RaceManager raceManager;

    public int Index => index;

    public void Initialize(RaceManager manager, int checkpointIndex)
    {
        raceManager = manager;
        index = checkpointIndex;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (raceManager == null) return;

        CarController car = other.GetComponentInParent<CarController>();
        if (car != null)
        {
            raceManager.OnCarPassedCheckpoint(this, car);
        }
    }
}