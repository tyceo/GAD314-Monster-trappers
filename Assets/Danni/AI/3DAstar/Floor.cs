using UnityEngine;

public class Floor : MonoBehaviour, IMovementCostInfo
{
    public float movementCostMultiplier = 1.0f;

    public float GetCostMultiplier()
    {
        return movementCostMultiplier;
    }
}
