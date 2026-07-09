using UnityEngine;
using System.Collections.Generic;

public class PathVisualizer : MonoBehaviour
{
     public LineRenderer line;
    public Color lineColor;
    public float lineWidth;
    public bool updateEveryFrame = true;

    // private AStar astar2D;
    private AStar3D astar3D;

    private readonly List<Vector3> tempPos = new List<Vector3>();
    private int lastCount = -1;
    private int lastHash = 0;

    void Awake()
    {
        astar3D = GetComponent<AStar3D>();
        // astar2D = GetComponent<AStar>();

        if (!line)
        {
            // i am just going to make and configure the line in here
            var lineObj = new GameObject("PathLine");
            lineObj.transform.SetParent(transform, false);
            line = lineObj.AddComponent<LineRenderer>();
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            line.textureMode = LineTextureMode.Stretch;
            line.alignment = LineAlignment.View;
            line.material = new Material(Shader.Find("Sprites/Default"));
        }
        line.enabled = false;
        line.widthMultiplier = lineWidth;
        line.startColor = lineColor;
        line.endColor   = lineColor;
    }

    void OnEnable() { RefreshLine(true); }

    void Update()
    {
        if (updateEveryFrame) RefreshLine(false);
    }

    public void RefreshNow() { RefreshLine(true); }

    private void RefreshLine(bool force)
    {
        tempPos.Clear();
        if (astar3D != null)
            tempPos.AddRange(astar3D.GetWorldPath());
        // else if (astar2D != null)
        //     tempPos.AddRange(astar2D.GetWorldPath());
        // below chunk is logic for change detection:
        // count points in the path
        int count = tempPos.Count;
        // here I borrowed a "hash = hash * 31 + itemHash" pattern i found on stack overflow (see apa7)
        // create a hash of the positions to detect if their content changes
        // start from a prime number (17) and mix each element with another prime multiplier (31)
        int hash = 17;
        for (int i = 0; i < count; i++)
        {
            // vector3.GetHashCode() gives an int based on the x,y,z 
            // this then checks if anything changed
            hash = hash * 31 + tempPos[i].GetHashCode();
        }

        // to decide whether to update the LineRenderer:
        // refresh if:
        // - the caller forced a refresh (force == true), OR
        // - the number of points changed (count != lastCount), OR
        // - the sequence content changed (hash != lastHash)
        bool changed = force || count != lastCount || hash != lastHash;
        if (!changed)
            return;  // return if nothing new to draw

        lastCount = count;
        lastHash  = hash;

        if (count == 0)
        {
            line.enabled = false;
            return;
        }
        line.enabled = true;
        line.positionCount = count;
        line.widthMultiplier = lineWidth;
        line.startColor = lineColor;
        line.endColor   = lineColor;
        line.SetPositions(tempPos.ToArray());
    }
}

