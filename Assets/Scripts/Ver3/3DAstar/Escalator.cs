using UnityEngine;

public class Escalator : MonoBehaviour, IMovementCostInfo
{
    public float movementCostMultiplier = .6f;

    public float GetCostMultiplier()
    {
        return movementCostMultiplier;
    }
}
