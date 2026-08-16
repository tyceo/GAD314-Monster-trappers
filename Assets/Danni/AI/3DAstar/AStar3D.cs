using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AStar3D : MonoBehaviour
{
    [Header("Refs")]
    public Grid3D grid;
    public Transform start;
    public Transform target;
    public bool preventDiagonalCornerCutting = true;

    [Header("Visuals")]
    public bool showVisuals = true;
    public Color openSetColor;
    public Color closedSetColor;
    public Color finalPathColor;
    public Color startCellColor;
    public Color goalCellColor;
    
    /// <summary>
    /// for the 3D version I defined these additional rules for walkable cells:
    /// - not an obstacle
    /// - can be a ladder (in approximation to)
    /// - have a valid floor: within a vertical range and with an acceptable upward normal
    /// </summary>
    [Header("Travel Rules")]
    public float maxCellFloorDistance = 0.6f; // how far below a cell can it still be considered to be walkable/supported by a floor
    public float minFloorUpDot = 0.65f; // minimum dot with Vector3.up for a surface to count as floor
    public float cellGap = 0.02f; // the box is slightly smaller than the cell so that it doesnt bleed into other components

    [Header("Internal Trackers")]
    private List<Vector3Int> pathCellCoordinates = new List<Vector3Int>(); // as opposed to Vector2Int[x,z]
    private HashSet<Vector3Int> openSetCellCoordinates = new HashSet<Vector3Int>();
    private HashSet<Vector3Int> closedSetCellCoordinates = new HashSet<Vector3Int>();
    private Vector3Int startCell;
    private Vector3Int goalCell;
    private bool hasSolution;

    [Header("Coroutine Setting")]
    public float courutineSpeed = 0.05f;
    private Coroutine astarCoroutine;

    private int cellCountX => grid.CellsX;
    private int cellCountY => grid.CellsY; // added y dimension
    private int cellCountZ => grid.CellsZ;
    
    private Vector3 worldOrigin
    {
        get
        {
            // bottom/left/front 2d origin becomes bottom/left/bottom/front in 3d
            float totalX = cellCountX * grid.cellSize;
            float totalY = cellCountY * grid.cellSize;
            float totalZ = cellCountZ * grid.cellSize;
            return new Vector3(
                // offset by half so it starts at (0,0,0)
                grid.areaCenter.x - totalX * 0.5f,
                grid.areaCenter.y - totalY * 0.5f,
                grid.areaCenter.z - totalZ * 0.5f
            );
        }
    }

    #region public functions

    /*public void RunAStarImmediately()
    {
        if (!Application.isPlaying) return; // added because running in editor was glitching my pc
        StopCoroutineIfRunning();

        pathCellCoordinates.Clear();
        openSetCellCoordinates.Clear();
        closedSetCellCoordinates.Clear();
        hasSolution = false;

        if (grid == null || start == null || target == null) return;

        int startIndexX;
        int startIndexY;
        int startIndexZ;
        int goalIndexX;
        int goalIndexY;
        int goalIndexZ;

        if (!TryConvertWorldToCell(start.position, out startIndexX, out startIndexY, out startIndexZ)) return; // 3 indices instead of 2
        if (!TryConvertWorldToCell(target.position, out goalIndexX, out goalIndexY, out goalIndexZ)) return;
        
        // walkability check is slightly more complex in this version as opposed to just checking for clearance
        if (!IsCellWalkable(startIndexX, startIndexY, startIndexZ) || !IsCellWalkable(goalIndexX, goalIndexY, goalIndexZ)) return;

        startCell = new Vector3Int(startIndexX, startIndexY, startIndexZ);
        goalCell = new Vector3Int(goalIndexX, goalIndexY, goalIndexZ);

        InternallyRunAStarImmediately();
    }*/
    
    public void RunAStarImmediately()
    {
        if (!Application.isPlaying) return;
        StopCoroutineIfRunning();

        pathCellCoordinates.Clear();
        openSetCellCoordinates.Clear();
        closedSetCellCoordinates.Clear();
        hasSolution = false;

        if (grid == null || start == null || target == null)
        {
            Debug.Log($"AStar bail: grid={grid != null} start={start != null} target={target != null}");
            return;
        }

        int startIndexX, startIndexY, startIndexZ, goalIndexX, goalIndexY, goalIndexZ;

        bool startConverted = TryConvertWorldToCell(start.position, out startIndexX, out startIndexY, out startIndexZ);
        bool goalConverted = TryConvertWorldToCell(target.position, out goalIndexX, out goalIndexY, out goalIndexZ);
        Debug.Log($"startConverted={startConverted} goalConverted={goalConverted}");
        if (!startConverted || !goalConverted) return;

        bool startWalkable = IsCellWalkable(startIndexX, startIndexY, startIndexZ);
        bool goalWalkable = IsCellWalkable(goalIndexX, goalIndexY, goalIndexZ);
        Debug.Log($"start cell=({startIndexX},{startIndexY},{startIndexZ}) walkable={startWalkable} | goal cell=({goalIndexX},{goalIndexY},{goalIndexZ}) walkable={goalWalkable}");
        if (!startWalkable || !goalWalkable) return;

        startCell = new Vector3Int(startIndexX, startIndexY, startIndexZ);
        goalCell = new Vector3Int(goalIndexX, goalIndexY, goalIndexZ);
        Debug.Log($"start cell=({startIndexX},{startIndexY},{startIndexZ}) world={grid.CellCenterWorld(startIndexX, startIndexY, startIndexZ)} walkable={startWalkable} | goal cell=({goalIndexX},{goalIndexY},{goalIndexZ}) world={grid.CellCenterWorld(goalIndexX, goalIndexY, goalIndexZ)} walkable={goalWalkable}");
        InternallyRunAStarImmediately();
    }

    public void RunAStarGradually()
    {
        if (!Application.isPlaying) return;
        StopCoroutineIfRunning();

        pathCellCoordinates.Clear();
        openSetCellCoordinates.Clear();
        closedSetCellCoordinates.Clear();
        hasSolution = false;

        if (grid == null || start == null || target == null) return;

        int startIndexX;
        int startIndexY;
        int startIndexZ;
        int goalIndexX;
        int goalIndexY;
        int goalIndexZ;

        if (!TryConvertWorldToCell(start.position, out startIndexX, out startIndexY, out startIndexZ)) return;
        if (!TryConvertWorldToCell(target.position, out goalIndexX, out goalIndexY, out goalIndexZ)) return;

        if (!IsCellWalkable(startIndexX, startIndexY, startIndexZ) || !IsCellWalkable(goalIndexX, goalIndexY, goalIndexZ)) return;

        startCell = new Vector3Int(startIndexX, startIndexY, startIndexZ);
        goalCell = new Vector3Int(goalIndexX, goalIndexY, goalIndexZ);

        astarCoroutine = StartCoroutine(GradualAStarInternalCorouine(courutineSpeed));
    }

    public void StopCoroutineIfRunning()
    {
        if (astarCoroutine != null)
        {
            if (!Application.isPlaying) return;
            StopCoroutine(astarCoroutine);
            astarCoroutine = null;
        }
    }

    public List<Vector3> GetWorldPath()
    {
        List<Vector3> worldPoints = new List<Vector3>(pathCellCoordinates.Count); // 3d conversion
        for (int index = 0; index < pathCellCoordinates.Count; index++)
        {
            Vector3Int cell = pathCellCoordinates[index];
            worldPoints.Add(grid.CellCenterWorld(cell.x, cell.y, cell.z));
        }
        return worldPoints;
    }

    #endregion

    #region internal coroutine and immediate functionalities

    private IEnumerator GradualAStarInternalCorouine(float secondsPause)
{
    int gridWidth = cellCountX;
    int gridHeight = cellCountY;
    int gridDepth = cellCountZ;
    // cells are now defined by 3d arrays, and tracked/calculated with the extra for loop along added y axis
    int[,,] movementCostFromStartGrid = new int[gridWidth, gridHeight, gridDepth];
    int[,,] heuristicCostToTargetGrid = new int[gridWidth, gridHeight, gridDepth];
    int[,,] totalEstimatedCostGrid = new int[gridWidth, gridHeight, gridDepth];
    int[,,] parentIndexXGrid = new int[gridWidth, gridHeight, gridDepth];
    int[,,] parentIndexYGrid = new int[gridWidth, gridHeight, gridDepth];
    int[,,] parentIndexZGrid = new int[gridWidth, gridHeight, gridDepth];
    bool[,,] isInClosedSetGrid = new bool[gridWidth, gridHeight, gridDepth];
    bool[,,] isInOpenSetGrid = new bool[gridWidth, gridHeight, gridDepth];

    for (int rowY = 0; rowY < gridHeight; rowY++)
    {
        for (int rowZ = 0; rowZ < gridDepth; rowZ++)
        {
            for (int columnX = 0; columnX < gridWidth; columnX++)
            {
                movementCostFromStartGrid[columnX, rowY, rowZ] = int.MaxValue;
                heuristicCostToTargetGrid[columnX, rowY, rowZ] = 0;
                totalEstimatedCostGrid[columnX, rowY, rowZ] = int.MaxValue;
                parentIndexXGrid[columnX, rowY, rowZ] = -1;
                parentIndexYGrid[columnX, rowY, rowZ] = -1;
                parentIndexZGrid[columnX, rowY, rowZ] = -1;
                isInClosedSetGrid[columnX, rowY, rowZ] = false;
                isInOpenSetGrid[columnX, rowY, rowZ] = false;
            }
        }
    }

    List<Vector3Int> openListCoordinates = new List<Vector3Int>();

    movementCostFromStartGrid[startCell.x, startCell.y, startCell.z] = 0;
    // 3d octile heuristic
    heuristicCostToTargetGrid[startCell.x, startCell.y, startCell.z] =
        CalculateOctileHeuristic(startCell.x, startCell.y, startCell.z, goalCell.x, goalCell.y, goalCell.z);
    totalEstimatedCostGrid[startCell.x, startCell.y, startCell.z] =
        movementCostFromStartGrid[startCell.x, startCell.y, startCell.z] + heuristicCostToTargetGrid[startCell.x, startCell.y, startCell.z];

    openListCoordinates.Add(startCell);
    isInOpenSetGrid[startCell.x, startCell.y, startCell.z] = true;
    openSetCellCoordinates.Add(startCell);

    while (openListCoordinates.Count > 0)
    {
        // same selection method as 2D
        int bestOpenListIndex = 0;
        Vector3Int bestOpenCell = openListCoordinates[0];

        for (int listIndex = 1; listIndex < openListCoordinates.Count; listIndex++)
        {
            Vector3Int candidateCell = openListCoordinates[listIndex];
            int candidateTotalEstimatedCost = totalEstimatedCostGrid[candidateCell.x, candidateCell.y, candidateCell.z];
            int bestTotalEstimatedCost = totalEstimatedCostGrid[bestOpenCell.x, bestOpenCell.y, bestOpenCell.z];
            int candidateHeuristic = heuristicCostToTargetGrid[candidateCell.x, candidateCell.y, candidateCell.z];
            int bestHeuristic = heuristicCostToTargetGrid[bestOpenCell.x, bestOpenCell.y, bestOpenCell.z];

            if (candidateTotalEstimatedCost < bestTotalEstimatedCost ||
                (candidateTotalEstimatedCost == bestTotalEstimatedCost && candidateHeuristic < bestHeuristic))
            {
                bestOpenListIndex = listIndex;
                bestOpenCell = candidateCell;
            }
        }

        Vector3Int currentCell = bestOpenCell;

        openListCoordinates.RemoveAt(bestOpenListIndex);
        isInOpenSetGrid[currentCell.x, currentCell.y, currentCell.z] = false;
        isInClosedSetGrid[currentCell.x, currentCell.y, currentCell.z] = true;
        openSetCellCoordinates.Remove(currentCell);
        closedSetCellCoordinates.Add(currentCell);

        yield return new WaitForSeconds(secondsPause);

        if (currentCell == goalCell)
        {
            // when path is found, reconstruct on 3 axes with 3 parent arrays
            ReconstructPath(parentIndexXGrid, parentIndexYGrid, parentIndexZGrid,
                            currentCell.x, currentCell.y, currentCell.z,
                            startCell.x, startCell.y, startCell.z);
            hasSolution = true;
            yield break;
        }
        // neighbors across 26 offsets
        for (int neighborOffsetAlongY = -1; neighborOffsetAlongY <= 1; neighborOffsetAlongY++)
        {
            for (int neighborOffsetAlongZ = -1; neighborOffsetAlongZ <= 1; neighborOffsetAlongZ++)
            {
                for (int neighborOffsetAlongX = -1; neighborOffsetAlongX <= 1; neighborOffsetAlongX++)
                {
                    if (neighborOffsetAlongX == 0 && neighborOffsetAlongY == 0 && neighborOffsetAlongZ == 0) continue;

                    int neighborIndexX = currentCell.x + neighborOffsetAlongX;
                    int neighborIndexY = currentCell.y + neighborOffsetAlongY;
                    int neighborIndexZ = currentCell.z + neighborOffsetAlongZ;

                    if (neighborIndexX < 0 || neighborIndexX >= gridWidth ||
                        neighborIndexY < 0 || neighborIndexY >= gridHeight ||
                        neighborIndexZ < 0 || neighborIndexZ >= gridDepth)
                        continue;

                    if (!IsCellWalkable(neighborIndexX, neighborIndexY, neighborIndexZ)) continue;

                    if (!IsMoveAllowedByLadder(currentCell.x, currentCell.y, currentCell.z,
                                                   neighborIndexX, neighborIndexY, neighborIndexZ,
                                                   neighborOffsetAlongX, neighborOffsetAlongY, neighborOffsetAlongZ))
                        continue;

                    bool isDiagonalMove =
                        (Mathf.Abs(neighborOffsetAlongX) + Mathf.Abs(neighborOffsetAlongY) + Mathf.Abs(neighborOffsetAlongZ)) >= 2;

                    if (preventDiagonalCornerCutting && isDiagonalMove)
                    {
                        // in 3d, preventing corner cutting requires the component (area)'s faces to be passable
                        if (!PassableDiagonally(currentCell.x, currentCell.y, currentCell.z,
                                                neighborOffsetAlongX, neighborOffsetAlongY, neighborOffsetAlongZ))
                            continue;
                    }

                    int axisCount = Mathf.Abs(neighborOffsetAlongX) + Mathf.Abs(neighborOffsetAlongY) + Mathf.Abs(neighborOffsetAlongZ);
                    int stepCost = (axisCount == 1) ? 10 : (axisCount == 2 ? 14 : 17);
                    
                    // now uses multiplier scaled cost 
                    int baseStepCost = stepCost; 
                    float destinationMultiplier = GetMovementCostMultiplier(neighborIndexX, neighborIndexY, neighborIndexZ);
                    int adjustedStepCost = Mathf.Max(1, Mathf.RoundToInt(baseStepCost * destinationMultiplier));

                    int tentativeMovementCostFromStart =
                        movementCostFromStartGrid[currentCell.x, currentCell.y, currentCell.z] + adjustedStepCost; // now uses adjustedStepCost

                    if (isInClosedSetGrid[neighborIndexX, neighborIndexY, neighborIndexZ] &&
                        tentativeMovementCostFromStart < movementCostFromStartGrid[neighborIndexX, neighborIndexY, neighborIndexZ])
                    {
                        isInClosedSetGrid[neighborIndexX, neighborIndexY, neighborIndexZ] = false;
                        isInOpenSetGrid[neighborIndexX, neighborIndexY, neighborIndexZ] = true;
                        openListCoordinates.Add(new Vector3Int(neighborIndexX, neighborIndexY, neighborIndexZ));
                        openSetCellCoordinates.Add(new Vector3Int(neighborIndexX, neighborIndexY, neighborIndexZ));
                        closedSetCellCoordinates.Remove(new Vector3Int(neighborIndexX, neighborIndexY, neighborIndexZ));
                    }

                    if (!isInOpenSetGrid[neighborIndexX, neighborIndexY, neighborIndexZ] &&
                        !isInClosedSetGrid[neighborIndexX, neighborIndexY, neighborIndexZ])
                    {
                        isInOpenSetGrid[neighborIndexX, neighborIndexY, neighborIndexZ] = true;
                        openListCoordinates.Add(new Vector3Int(neighborIndexX, neighborIndexY, neighborIndexZ));
                        openSetCellCoordinates.Add(new Vector3Int(neighborIndexX, neighborIndexY, neighborIndexZ));
                    }

                    if (tentativeMovementCostFromStart >= movementCostFromStartGrid[neighborIndexX, neighborIndexY, neighborIndexZ])
                        continue;

                    movementCostFromStartGrid[neighborIndexX, neighborIndexY, neighborIndexZ] = tentativeMovementCostFromStart;

                    int heuristicCost =
                        CalculateOctileHeuristic(neighborIndexX, neighborIndexY, neighborIndexZ,
                                                 goalCell.x, goalCell.y, goalCell.z);
                    heuristicCostToTargetGrid[neighborIndexX, neighborIndexY, neighborIndexZ] = heuristicCost;

                    totalEstimatedCostGrid[neighborIndexX, neighborIndexY, neighborIndexZ] =
                        movementCostFromStartGrid[neighborIndexX, neighborIndexY, neighborIndexZ] +
                        heuristicCostToTargetGrid[neighborIndexX, neighborIndexY, neighborIndexZ];

                    parentIndexXGrid[neighborIndexX, neighborIndexY, neighborIndexZ] = currentCell.x;
                    parentIndexYGrid[neighborIndexX, neighborIndexY, neighborIndexZ] = currentCell.y;
                    parentIndexZGrid[neighborIndexX, neighborIndexY, neighborIndexZ] = currentCell.z;
                }
            }
        }
    }

    hasSolution = false;
    pathCellCoordinates.Clear();
}

    private void InternallyRunAStarImmediately()
{
    int gridWidth = cellCountX;
    int gridHeight = cellCountY;
    int gridDepth = cellCountZ;
    int[,,] movementCostFromStartGrid = new int[gridWidth, gridHeight, gridDepth];
    int[,,] heuristicCostToTargetGrid = new int[gridWidth, gridHeight, gridDepth];
    int[,,] totalEstimatedCostGrid = new int[gridWidth, gridHeight, gridDepth];
    int[,,] parentIndexXGrid = new int[gridWidth, gridHeight, gridDepth];
    int[,,] parentIndexYGrid = new int[gridWidth, gridHeight, gridDepth];
    int[,,] parentIndexZGrid = new int[gridWidth, gridHeight, gridDepth];
    bool[,,] isInClosedSetGrid = new bool[gridWidth, gridHeight, gridDepth];
    bool[,,] isInOpenSetGrid = new bool[gridWidth, gridHeight, gridDepth];

    for (int rowY = 0; rowY < gridHeight; rowY++)
    {
        for (int rowZ = 0; rowZ < gridDepth; rowZ++)
        {
            for (int columnX = 0; columnX < gridWidth; columnX++)
            {
                movementCostFromStartGrid[columnX, rowY, rowZ] = int.MaxValue;
                heuristicCostToTargetGrid[columnX, rowY, rowZ] = 0;
                totalEstimatedCostGrid[columnX, rowY, rowZ] = int.MaxValue;
                parentIndexXGrid[columnX, rowY, rowZ] = -1;
                parentIndexYGrid[columnX, rowY, rowZ] = -1;
                parentIndexZGrid[columnX, rowY, rowZ] = -1;
                isInClosedSetGrid[columnX, rowY, rowZ] = false;
                isInOpenSetGrid[columnX, rowY, rowZ] = false;
            }
        }
    }

    List<Vector3Int> openListCoordinates = new List<Vector3Int>();

    movementCostFromStartGrid[startCell.x, startCell.y, startCell.z] = 0;
    heuristicCostToTargetGrid[startCell.x, startCell.y, startCell.z] =
        CalculateOctileHeuristic(startCell.x, startCell.y, startCell.z, goalCell.x, goalCell.y, goalCell.z);
    totalEstimatedCostGrid[startCell.x, startCell.y, startCell.z] =
        movementCostFromStartGrid[startCell.x, startCell.y, startCell.z] + heuristicCostToTargetGrid[startCell.x, startCell.y, startCell.z];

    openListCoordinates.Add(startCell);
    isInOpenSetGrid[startCell.x, startCell.y, startCell.z] = true;
    openSetCellCoordinates.Add(startCell);

    while (openListCoordinates.Count > 0)
    {
        int bestOpenListIndex = 0;
        Vector3Int bestOpenCell = openListCoordinates[0];

        for (int listIndex = 1; listIndex < openListCoordinates.Count; listIndex++)
        {
            Vector3Int candidateCell = openListCoordinates[listIndex];
            int candidateTotalEstimatedCost = totalEstimatedCostGrid[candidateCell.x, candidateCell.y, candidateCell.z];
            int bestTotalEstimatedCost = totalEstimatedCostGrid[bestOpenCell.x, bestOpenCell.y, bestOpenCell.z];
            int candidateHeuristic = heuristicCostToTargetGrid[candidateCell.x, candidateCell.y, candidateCell.z];
            int bestHeuristic = heuristicCostToTargetGrid[bestOpenCell.x, bestOpenCell.y, bestOpenCell.z];

            if (candidateTotalEstimatedCost < bestTotalEstimatedCost ||
                (candidateTotalEstimatedCost == bestTotalEstimatedCost && candidateHeuristic < bestHeuristic))
            {
                bestOpenListIndex = listIndex;
                bestOpenCell = candidateCell;
            }
        }

        Vector3Int currentCell = bestOpenCell;

        openListCoordinates.RemoveAt(bestOpenListIndex);
        isInOpenSetGrid[currentCell.x, currentCell.y, currentCell.z] = false;
        isInClosedSetGrid[currentCell.x, currentCell.y, currentCell.z] = true;
        openSetCellCoordinates.Remove(currentCell);
        closedSetCellCoordinates.Add(currentCell);

        if (currentCell == goalCell)
        {
            ReconstructPath(parentIndexXGrid, parentIndexYGrid, parentIndexZGrid,
                            currentCell.x, currentCell.y, currentCell.z,
                            startCell.x, startCell.y, startCell.z);
            hasSolution = true;
            return;
        }

        for (int neighborOffsetAlongY = -1; neighborOffsetAlongY <= 1; neighborOffsetAlongY++)
        {
            for (int neighborOffsetAlongZ = -1; neighborOffsetAlongZ <= 1; neighborOffsetAlongZ++)
            {
                for (int neighborOffsetAlongX = -1; neighborOffsetAlongX <= 1; neighborOffsetAlongX++)
                {
                    if (neighborOffsetAlongX == 0 && neighborOffsetAlongY == 0 && neighborOffsetAlongZ == 0) continue;

                    int neighborIndexX = currentCell.x + neighborOffsetAlongX;
                    int neighborIndexY = currentCell.y + neighborOffsetAlongY;
                    int neighborIndexZ = currentCell.z + neighborOffsetAlongZ;

                    if (neighborIndexX < 0 || neighborIndexX >= gridWidth ||
                        neighborIndexY < 0 || neighborIndexY >= gridHeight ||
                        neighborIndexZ < 0 || neighborIndexZ >= gridDepth)
                        continue;

                    if (!IsCellWalkable(neighborIndexX, neighborIndexY, neighborIndexZ)) continue;

                    if (!IsMoveAllowedByLadder(currentCell.x, currentCell.y, currentCell.z,
                                                   neighborIndexX, neighborIndexY, neighborIndexZ,
                                                   neighborOffsetAlongX, neighborOffsetAlongY, neighborOffsetAlongZ))
                        continue;

                    bool isDiagonalMove =
                        (Mathf.Abs(neighborOffsetAlongX) + Mathf.Abs(neighborOffsetAlongY) + Mathf.Abs(neighborOffsetAlongZ)) >= 2;

                    if (preventDiagonalCornerCutting && isDiagonalMove)
                    {
                        if (!PassableDiagonally(currentCell.x, currentCell.y, currentCell.z,
                                                neighborOffsetAlongX, neighborOffsetAlongY, neighborOffsetAlongZ))
                            continue;
                    }

                    int axisCount = Mathf.Abs(neighborOffsetAlongX) + Mathf.Abs(neighborOffsetAlongY) + Mathf.Abs(neighborOffsetAlongZ);
                    int stepCost = (axisCount == 1) ? 10 : (axisCount == 2 ? 14 : 17);
                    int baseStepCost = stepCost;
                    float destinationMultiplier = GetMovementCostMultiplier(neighborIndexX, neighborIndexY, neighborIndexZ);
                    int adjustedStepCost = Mathf.Max(1, Mathf.RoundToInt(baseStepCost * destinationMultiplier));
                    // ----------------------------------------------------------------------

                    int tentativeMovementCostFromStart =
                        movementCostFromStartGrid[currentCell.x, currentCell.y, currentCell.z] + adjustedStepCost; 

                    if (isInClosedSetGrid[neighborIndexX, neighborIndexY, neighborIndexZ] &&
                        tentativeMovementCostFromStart < movementCostFromStartGrid[neighborIndexX, neighborIndexY, neighborIndexZ])
                    {
                        isInClosedSetGrid[neighborIndexX, neighborIndexY, neighborIndexZ] = false;
                        isInOpenSetGrid[neighborIndexX, neighborIndexY, neighborIndexZ] = true;
                        openListCoordinates.Add(new Vector3Int(neighborIndexX, neighborIndexY, neighborIndexZ));
                        openSetCellCoordinates.Add(new Vector3Int(neighborIndexX, neighborIndexY, neighborIndexZ));
                        closedSetCellCoordinates.Remove(new Vector3Int(neighborIndexX, neighborIndexY, neighborIndexZ));
                    }

                    if (!isInOpenSetGrid[neighborIndexX, neighborIndexY, neighborIndexZ] &&
                        !isInClosedSetGrid[neighborIndexX, neighborIndexY, neighborIndexZ])
                    {
                        isInOpenSetGrid[neighborIndexX, neighborIndexY, neighborIndexZ] = true;
                        openListCoordinates.Add(new Vector3Int(neighborIndexX, neighborIndexY, neighborIndexZ));
                        openSetCellCoordinates.Add(new Vector3Int(neighborIndexX, neighborIndexY, neighborIndexZ));
                    }

                    if (tentativeMovementCostFromStart >= movementCostFromStartGrid[neighborIndexX, neighborIndexY, neighborIndexZ])
                        continue;

                    movementCostFromStartGrid[neighborIndexX, neighborIndexY, neighborIndexZ] = tentativeMovementCostFromStart;

                    int heuristicCost =
                        CalculateOctileHeuristic(neighborIndexX, neighborIndexY, neighborIndexZ,
                                                 goalCell.x, goalCell.y, goalCell.z);
                    heuristicCostToTargetGrid[neighborIndexX, neighborIndexY, neighborIndexZ] = heuristicCost;

                    totalEstimatedCostGrid[neighborIndexX, neighborIndexY, neighborIndexZ] =
                        movementCostFromStartGrid[neighborIndexX, neighborIndexY, neighborIndexZ] +
                        heuristicCostToTargetGrid[neighborIndexX, neighborIndexY, neighborIndexZ];

                    parentIndexXGrid[neighborIndexX, neighborIndexY, neighborIndexZ] = currentCell.x;
                    parentIndexYGrid[neighborIndexX, neighborIndexY, neighborIndexZ] = currentCell.y;
                    parentIndexZGrid[neighborIndexX, neighborIndexY, neighborIndexZ] = currentCell.z;
                }
            }
        }
    }

    hasSolution = false;
    pathCellCoordinates.Clear();
}

    #endregion

    #region heuristic, cost multiplication and walk-back helpers
    /// <summary>
    /// this function is different from 2d in that:
    // - it is extended to use as many 3-axis diagonals as possible (reduces X, Y, Z together) at $17 each
    // - then use as many 2-axis diagonals as possible (reduces two axes together) at $14 each
    // - whatever remains on a single axis then becomes straight steps at $10 each
    /// </summary>
    /// <param name="fromIndexX"></param>
    /// <param name="fromIndexY"></param>
    /// <param name="fromIndexZ"></param>
    /// <param name="toIndexX"></param>
    /// <param name="toIndexY"></param>
    /// <param name="toIndexZ"></param>
    /// <returns></returns>
    private int CalculateOctileHeuristic(int fromIndexX, int fromIndexY, int fromIndexZ,
                                         int toIndexX, int toIndexY, int toIndexZ)
    {
        int distanceAlongX = Mathf.Abs(fromIndexX - toIndexX);
        int distanceAlongY = Mathf.Abs(fromIndexY - toIndexY);
        int distanceAlongZ = Mathf.Abs(fromIndexZ - toIndexZ);

        int threeAxisDiagonalStepCount = Mathf.Min(distanceAlongX, Mathf.Min(distanceAlongY, distanceAlongZ));
        distanceAlongX -= threeAxisDiagonalStepCount;
        distanceAlongY -= threeAxisDiagonalStepCount;
        distanceAlongZ -= threeAxisDiagonalStepCount;

        int twoAxisDiagonalStepCount =
            (distanceAlongX == 0) ? Mathf.Min(distanceAlongY, distanceAlongZ) :
            (distanceAlongY == 0) ? Mathf.Min(distanceAlongX, distanceAlongZ) :
                                     Mathf.Min(distanceAlongX, distanceAlongY);

        int straightStepCount =
            (distanceAlongX + distanceAlongY + distanceAlongZ) - 2 * twoAxisDiagonalStepCount;

        return 17 * threeAxisDiagonalStepCount + 14 * twoAxisDiagonalStepCount + 10 * straightStepCount;
    }

    private void ReconstructPath(
        int[,,] parentIndexXGrid,
        int[,,] parentIndexYGrid,
        int[,,] parentIndexZGrid,
        int endIndexX, int endIndexY, int endIndexZ,
        int startIndexX, int startIndexY, int startIndexZ)
    {
        pathCellCoordinates.Clear();

        int safetyCounter = cellCountX * cellCountY * cellCountZ;
        int walkIndexX = endIndexX;
        int walkIndexY = endIndexY;
        int walkIndexZ = endIndexZ;

        while (safetyCounter-- > 0)
        {
            pathCellCoordinates.Add(new Vector3Int(walkIndexX, walkIndexY, walkIndexZ));
            if (walkIndexX == startIndexX && walkIndexY == startIndexY && walkIndexZ == startIndexZ) break;

            int parentX = parentIndexXGrid[walkIndexX, walkIndexY, walkIndexZ];
            int parentY = parentIndexYGrid[walkIndexX, walkIndexY, walkIndexZ];
            int parentZ = parentIndexZGrid[walkIndexX, walkIndexY, walkIndexZ];

            if (parentX < 0 || parentY < 0 || parentZ < 0) break;

            walkIndexX = parentX;
            walkIndexY = parentY;
            walkIndexZ = parentZ;
        }

        pathCellCoordinates.Reverse();
    }

    private bool TryConvertWorldToCell(Vector3 worldPosition, out int cellIndexX, out int cellIndexY, out int cellIndexZ)
    {
        Vector3 origin = worldOrigin;

        float relativeX = (worldPosition.x - origin.x) / grid.cellSize;
        float relativeY = (worldPosition.y - origin.y) / grid.cellSize;
        float relativeZ = (worldPosition.z - origin.z) / grid.cellSize;

        cellIndexX = Mathf.FloorToInt(relativeX);
        cellIndexY = Mathf.FloorToInt(relativeY);
        cellIndexZ = Mathf.FloorToInt(relativeZ);

        if (cellIndexX < 0 || cellIndexX >= cellCountX ||
            cellIndexY < 0 || cellIndexY >= cellCountY ||
            cellIndexZ < 0 || cellIndexZ >= cellCountZ)
            return false;

        return true;
    }
    
    /// <summary>
    /// this helper returns a value that multiplies the base per-step cost when the minion is ENTERING the given cell (x,y,z)
    /// Design choices:
    /// - it samples the destination cell the minion is moving into and charge the cost to that cell upon entry
    /// - this will affect his behavior at terrain boundaries (pays for what he steps onto)
    /// - effects can naturally stack (e.g., Mud 1.5 × puddle 1.2 = 1.8). it's clamped tho so massive stacks don't explode search costs
    /// </summary>
    private float GetMovementCostMultiplier(int cellIndexX, int cellIndexY, int cellIndexZ)
    {
        // where to sample
        Vector3 cellCenterWorld = grid.CellCenterWorld(cellIndexX, cellIndexY, cellIndexZ);
        float halfExtentPerAxis = (grid.cellSize * 0.5f) - cellGap;
        if (halfExtentPerAxis < 0.01f)
        {
            // a guard so that the box never goes negative/inverted
            halfExtentPerAxis = 0.01f;
        }
        Vector3 halfExtents = new Vector3(halfExtentPerAxis, halfExtentPerAxis, halfExtentPerAxis);
        
        // uses Physics overlap to find all cost providers in the area
        Collider[] overlappingColliders = Physics.OverlapBox(
            cellCenterWorld,
            halfExtents,
            Quaternion.identity,
            ~0,
            QueryTriggerInteraction.Collide
        );
        
        // local var for the multiplier total
        float combinedMultiplier = 1.0f;

        for (int colliderIndex = 0; colliderIndex < overlappingColliders.Length; colliderIndex++)
        {
            Collider currentCollider = overlappingColliders[colliderIndex];
            if (currentCollider == null) continue;

            // find the multiplier interface on this collider
            IMovementCostInfo[] movementCostProviders =
                currentCollider.GetComponents<IMovementCostInfo>();

            for (int providerIndex = 0; providerIndex < movementCostProviders.Length; providerIndex++)
            {
                IMovementCostInfo movementCostProvider = movementCostProviders[providerIndex];
                if (movementCostProvider == null) continue;
                // <1.0 will speed up minion; >1 slows him down
                float providerMultiplier = movementCostProvider.GetCostMultiplier();
                // guard soo that the multiplier never turn the cost to zero/negative
                if (providerMultiplier < 0.05f) providerMultiplier = 0.05f;

                combinedMultiplier *= providerMultiplier;
            }
        }

        // more clamps!
        if (combinedMultiplier < 0.05f) combinedMultiplier = 0.05f;
        if (combinedMultiplier > 10.0f) combinedMultiplier = 10.0f;

        return combinedMultiplier;
}


    #endregion

    #region walkability and area types (floors, obstacles, ladders, no flying)

    /// <summary>
    // rather than a single Physics.CheckBox against the obstacle layer, 3d checks for components:
    // UPDATE: now checks for IMovementCostInfo interface for assessing walkability
    // (which is not too different/better than layers but more extendable i think)
    // - if has an obstacle component, ot walkable.
    // - if a Ladder, walkable (“climb” region)
    // - otherwise require a valid floor beneath; treats open air as not walkable
    /// </summary>
    /// <param name="cellIndexX"></param>
    /// <param name="cellIndexY"></param>
    /// <param name="cellIndexZ"></param>
    /// <returns></returns>
    private bool IsCellWalkable(int cellIndexX, int cellIndexY, int cellIndexZ)
    {
        if (CellContainsComponent<Obstacle>(cellIndexX, cellIndexY, cellIndexZ)) return false;

        if (CellContainsInterface<IMovementCostInfo>(cellIndexX, cellIndexY, cellIndexZ)) return true;

        return CellHasFloorBelow(cellIndexX, cellIndexY, cellIndexZ);
    }
    /// <summary>
    /// this method ensures that vertical moves are allowed ONLY if both the current and next cells are in a ladder
    /// </summary>
    /// <param name="currentIndexX"></param>
    /// <param name="currentIndexY"></param>
    /// <param name="currentIndexZ"></param>
    /// <param name="nextIndexX"></param>
    /// <param name="nextIndexY"></param>
    /// <param name="nextIndexZ"></param>
    /// <param name="neighborOffsetAlongX"></param>
    /// <param name="neighborOffsetAlongY"></param>
    /// <param name="neighborOffsetAlongZ"></param>
    /// <returns></returns>
    // private bool IsMoveAllowedByLadder(
    //     int currentIndexX, int currentIndexY, int currentIndexZ,
    //     int nextIndexX, int nextIndexY, int nextIndexZ,
    //     int neighborOffsetAlongX, int neighborOffsetAlongY, int neighborOffsetAlongZ)
    // {
    //     if (neighborOffsetAlongY == 0) return true;
    //
    //     bool currentInsideLadder = CellContainsComponent<Ladder>(currentIndexX, currentIndexY, currentIndexZ);
    //     bool nextInsideLadder = CellContainsComponent<Ladder>(nextIndexX, nextIndexY, nextIndexZ);
    //     return currentInsideLadder && nextInsideLadder;
    // }
    private bool IsMoveAllowedByLadder(
        int currentIndexX, int currentIndexY, int currentIndexZ,
        int nextIndexX, int nextIndexY, int nextIndexZ,
        int neighborOffsetAlongX, int neighborOffsetAlongY, int neighborOffsetAlongZ)
    {
        return true;
    }
    private bool CellContainsComponent<Type>(int cellIndexX, int cellIndexY, int cellIndexZ) where Type : Component //gets the component parameter
    {
        Vector3 center = grid.CellCenterWorld(cellIndexX, cellIndexY, cellIndexZ);
        // halfExtent = 1.00 * 0.5 - 0.02 = 0.50 - 0.02 = 0.48 as my cells are 1 in size
        Vector3 halfExtents = Vector3.one * (grid.cellSize * 0.5f - cellGap);
        Collider[] colliders = Physics.OverlapBox(center, halfExtents, Quaternion.identity, ~0, QueryTriggerInteraction.Collide);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null && colliders[i].GetComponent<Type>() != null) return true;
        }
        return false;
    }
    private bool CellContainsInterface<TInterface>(int cellIndexX, int cellIndexY, int cellIndexZ) where TInterface : class
    {
        Vector3 center = grid.CellCenterWorld(cellIndexX, cellIndexY, cellIndexZ);
        Vector3 halfExtents = Vector3.one * (grid.cellSize * 0.5f - cellGap);
        Collider[] colliders = Physics.OverlapBox(center, halfExtents, Quaternion.identity, ~0, QueryTriggerInteraction.Collide);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (ColliderHasInterface<TInterface>(colliders[i])) return true;
        }
        return false;
    }

    // checks if the component on this collider implements the multiplier interface
    private bool ColliderHasInterface<TInterface>(Collider collider) where TInterface : class
    {
        if (collider == null) return false;
        // get all components and check for interface
        Component[] all = collider.GetComponents<Component>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null) continue;
            // since get component won't do with interfaces...
            // here I check if these components can be casted as interfaces,
            // if not null, then it means it has the interface
            if (all[i] as TInterface != null) return true;
        }
        return false;
    }

    private bool CellHasFloorBelow(int cellIndexX, int cellIndexY, int cellIndexZ)
    {
        Vector3 center = grid.CellCenterWorld(cellIndexX, cellIndexY, cellIndexZ);
        Ray ray = new Ray(center + Vector3.up * (grid.cellSize * 0.25f), Vector3.down);
        float maximumDistance = Mathf.Max(maxCellFloorDistance, 0.001f);

        if (Physics.Raycast(ray, out RaycastHit hit, maximumDistance + grid.cellSize, ~0, QueryTriggerInteraction.Collide))
        {
            if (hit.collider != null && hit.collider.GetComponent<Floor>() != null)
            {
                if (Vector3.Dot(hit.normal, Vector3.up) >= minFloorUpDot)
                {
                    float verticalDistance = center.y - hit.point.y;
                    return verticalDistance >= 0f && verticalDistance <= maximumDistance;
                }
            }
        }
        return false;
    }

    private bool PassableDiagonally(int currentIndexX, int currentIndexY, int currentIndexZ,
                                    int neighborOffsetAlongX, int neighborOffsetAlongY, int neighborOffsetAlongZ)
    {
        if (neighborOffsetAlongX != 0 && !IsCellWalkable(currentIndexX + neighborOffsetAlongX, currentIndexY, currentIndexZ)) return false;
        if (neighborOffsetAlongY != 0 && !IsCellWalkable(currentIndexX, currentIndexY + neighborOffsetAlongY, currentIndexZ)) return false;
        if (neighborOffsetAlongZ != 0 && !IsCellWalkable(currentIndexX, currentIndexY, currentIndexZ + neighborOffsetAlongZ)) return false;

        int axisInvolvedCount = Mathf.Abs(neighborOffsetAlongX) + Mathf.Abs(neighborOffsetAlongY) + Mathf.Abs(neighborOffsetAlongZ);
        if (axisInvolvedCount == 3)
        {
            if (!IsCellWalkable(currentIndexX + neighborOffsetAlongX, currentIndexY + neighborOffsetAlongY, currentIndexZ)) return false;
            if (!IsCellWalkable(currentIndexX + neighborOffsetAlongX, currentIndexY, currentIndexZ + neighborOffsetAlongZ)) return false;
            if (!IsCellWalkable(currentIndexX, currentIndexY + neighborOffsetAlongY, currentIndexZ + neighborOffsetAlongZ)) return false;
        }
        return true;
    }

    #endregion

    #region debug visuals

    private void OnDrawGizmos()
    {
      if (!showVisuals || grid == null) return;

    Vector3 overlayCubeSize = Vector3.one * grid.cellSize * 0.98f;
    
    // the easier the path, the brighter the cell and vice versa
    Color ShadeCellByMultiplier(Color baseColor, float colorMultiplier)
    {
        // habitual clamps
        colorMultiplier = Mathf.Clamp(colorMultiplier, 0.05f, 10f);
        // unity function that converts R/G/B to Hue/Saturation/Volume
        Color.RGBToHSV(baseColor, out float hue, out float saturation, out float volume);
        if (colorMultiplier < 1f)
        {
            // shading scales up to 1 as colorMultiplier approaches 0, so im clamping it 
            float shading = Mathf.Clamp01(1f - colorMultiplier);
            volume = Mathf.Clamp01(volume + shading * 0.35f);          // brighten a bit
            saturation = Mathf.Clamp01(saturation - shading * 0.25f); // desaturate a bit
        }
        else if (colorMultiplier > 1f)
        {
            // clamping
            float shading = Mathf.Clamp01((Mathf.Min(colorMultiplier, 5f) - 1f) / 4f);  // 0..1
            volume = Mathf.Clamp01(volume * (1f - 0.45f * shading));  // darken a bit
            saturation = Mathf.Clamp01(saturation + 0.25f * shading);// add a bit saturation
        }
        return Color.HSVToRGB(hue, saturation, volume);
    }

    Gizmos.color = openSetColor;
    foreach (Vector3Int cell in openSetCellCoordinates)
    {
        Vector3 position = grid.CellCenterWorld(cell.x, cell.y, cell.z);
        float colorMultiplier = GetMovementCostMultiplier(cell.x, cell.y, cell.z);
        Gizmos.color = ShadeCellByMultiplier(openSetColor, colorMultiplier);

        Gizmos.DrawCube(position, overlayCubeSize);
    }

    Gizmos.color = closedSetColor;
    foreach (Vector3Int cell in closedSetCellCoordinates)
    {
        Vector3 position = grid.CellCenterWorld(cell.x, cell.y, cell.z);
        float colorMultiplier = GetMovementCostMultiplier(cell.x, cell.y, cell.z);
        Gizmos.color = ShadeCellByMultiplier(closedSetColor, colorMultiplier);

        Gizmos.DrawCube(position, overlayCubeSize);
    }

    if (hasSolution && pathCellCoordinates.Count > 0)
    {
        Gizmos.color = finalPathColor;
        for (int index = 0; index < pathCellCoordinates.Count; index++)
        {
            Vector3Int cell = pathCellCoordinates[index];
            Vector3 position = grid.CellCenterWorld(cell.x, cell.y, cell.z);

            // final path cell also tinted by area/multiplier
            float colorMultiplier = GetMovementCostMultiplier(cell.x, cell.y, cell.z);
            Gizmos.color = ShadeCellByMultiplier(finalPathColor, colorMultiplier);

            Gizmos.DrawCube(position, overlayCubeSize * 0.9f);
        }
    }

    if (start != null)
    {
        if (TryConvertWorldToCell(start.position, out int startIndexX, out int startIndexY, out int startIndexZ))
        {
            Gizmos.color = startCellColor;
            Gizmos.DrawCube(grid.CellCenterWorld(startIndexX, startIndexY, startIndexZ), overlayCubeSize * 0.8f);
        }
    }

    if (target != null)
    {
        if (TryConvertWorldToCell(target.position, out int goalIndexX, out int goalIndexY, out int goalIndexZ))
        {
            Gizmos.color = goalCellColor;
            Gizmos.DrawCube(grid.CellCenterWorld(goalIndexX, goalIndexY, goalIndexZ), overlayCubeSize * 0.8f);
        }
    }

    }

    #endregion
}
