using UnityEngine;

public class Mud : MonoBehaviour, IMovementCostInfo
{
    public float movementCostMultiplier = 1.5f;

    public float GetCostMultiplier()
    {
        return movementCostMultiplier;
    }
}
