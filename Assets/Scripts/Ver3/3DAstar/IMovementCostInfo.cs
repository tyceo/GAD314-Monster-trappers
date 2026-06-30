using UnityEngine;

public interface IMovementCostInfo
{
    // 1 = normal, >1 = harder/slower, <1 = easier/faster
    float GetCostMultiplier(); 
}
