using UnityEngine;

public class Grid3D : MonoBehaviour
{
     [Header("World")]
    public Vector3 areaCenter = Vector3.zero;
    public Vector3 areaSizeXYZ = new Vector3(10f, 10f, 10f);

    [Header("Grid")]
    public float cellSize = 1f;

    [Header("Gizmos")]
    public bool drawGizmos = true;
    public Color lineColor = new Color(0f, 0.6f, 1f, 0.6f);

    public int CellsX => Mathf.Max(1, Mathf.CeilToInt(areaSizeXYZ.x / Mathf.Max(0.0001f, cellSize)));
    public int CellsY => Mathf.Max(1, Mathf.CeilToInt(areaSizeXYZ.y / Mathf.Max(0.0001f, cellSize)));
    public int CellsZ => Mathf.Max(1, Mathf.CeilToInt(areaSizeXYZ.z / Mathf.Max(0.0001f, cellSize)));

    Vector3 Origin()
    {
        return new Vector3(
            areaCenter.x - CellsX * cellSize * 0.5f,
            areaCenter.y - CellsY * cellSize * 0.5f,
            areaCenter.z - CellsZ * cellSize * 0.5f
        );
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || cellSize <= 0f) return;

        Gizmos.color = lineColor;

        Vector3 gridOriginWorld = Origin();
        float totalSizeX = CellsX * cellSize;
        float totalSizeY = CellsY * cellSize;
        float totalSizeZ = CellsZ * cellSize;
        for (int cellIndexY = 0; cellIndexY <= CellsY; cellIndexY++)
        {
            float worldPosY = gridOriginWorld.y + cellIndexY * cellSize;
            for (int cellIndexZ = 0; cellIndexZ <= CellsZ; cellIndexZ++)
            {
                float worldPosZ = gridOriginWorld.z + cellIndexZ * cellSize;
                Gizmos.DrawLine(
                    new Vector3(gridOriginWorld.x, worldPosY, worldPosZ),
                    new Vector3(gridOriginWorld.x + totalSizeX, worldPosY, worldPosZ)
                );
            }
        }
        for (int cellIndexX = 0; cellIndexX <= CellsX; cellIndexX++)
        {
            float worldPosX = gridOriginWorld.x + cellIndexX * cellSize;
            for (int cellIndexZ = 0; cellIndexZ <= CellsZ; cellIndexZ++)
            {
                float worldPosZ = gridOriginWorld.z + cellIndexZ * cellSize;
                Gizmos.DrawLine(
                    new Vector3(worldPosX, gridOriginWorld.y, worldPosZ),
                    new Vector3(worldPosX, gridOriginWorld.y + totalSizeY, worldPosZ)
                );
            }
        }
        for (int cellIndexX = 0; cellIndexX <= CellsX; cellIndexX++)
        {
            float worldPositionX = gridOriginWorld.x + cellIndexX * cellSize;
            for (int cellIndexY = 0; cellIndexY <= CellsY; cellIndexY++)
            {
                float worldPositionY = gridOriginWorld.y + cellIndexY * cellSize;
                Gizmos.DrawLine(
                    new Vector3(worldPositionX, worldPositionY, gridOriginWorld.z),
                    new Vector3(worldPositionX, worldPositionY, gridOriginWorld.z + totalSizeZ)
                );
            }
        }
    }
    // [Header("World")]
    // public Vector3 areaCenter = Vector3.zero;
    // public Vector3 areaSizeXYZ = new Vector3(10f, 10f, 10f);
    //
    // [Header("Grid")]
    // public float cellSize = 1f;
    // // public float agentRadius = 0.25f;
    // // public float clearanceHeight = 2f; 
    // //
    // // [Header("Layers")]
    // // public LayerMask obstacleMask; 
    //
    // public int CellsX => Mathf.Max(1, Mathf.CeilToInt(areaSizeXYZ.x / cellSize));
    // public int CellsY => Mathf.Max(1, Mathf.CeilToInt(areaSizeXYZ.y / cellSize));
    // public int CellsZ => Mathf.Max(1, Mathf.CeilToInt(areaSizeXYZ.z / cellSize));
    //
    public Vector3 CellCenterWorld(int cellIndexX, int cellIndexY, int cellIndexZ)
    {
        Vector3 origin = new Vector3(
            areaCenter.x - CellsX * cellSize * 0.5f,
            areaCenter.y - CellsY * cellSize * 0.5f,
            areaCenter.z - CellsZ * cellSize * 0.5f
        );
    
        return new Vector3(
            origin.x + (cellIndexX + 0.5f) * cellSize,
            origin.y + (cellIndexY + 0.5f) * cellSize,
            origin.z + (cellIndexZ + 0.5f) * cellSize
        );
    }
}