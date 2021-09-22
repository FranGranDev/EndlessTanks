using UnityEngine;

public class TerrainSurface : MonoBehaviour
{
    public int xSize;
    public int ySize;
    public SurfaceData Data;
    public bool DataReady;

    public void SetData(SurfaceData surfaceData)
    {
        Data = surfaceData;
        xSize = Data.xSize;
        ySize = Data.ySize;
        DataReady = true;
    }

    public Vector3 GetNormal(Vector2Int coord)
    {
        if (!DataReady)
            return -Vector3.forward;
        return Data.Normals[coord.y * xSize + coord.x];
    }
    public float GetHeight(Vector2Int coord)
    {
        if (!DataReady)
            return 0.4f;
        return Data.Height[coord.x, coord.y];
    }
    public float GetClutch(Vector2Int coord)
    {
        if (!DataReady)
            return 0.5f;
        return Data.Clutch[coord.x ,coord.y];
    }
    public float GetSpeed(Vector2Int coord)
    {
        if (!DataReady)
            return 0.5f;
        return Data.MaxSpeed[coord.x, coord.y];
    }
    public LandType.SurfaceType GetSurface(Vector2Int coord)
    {
        if (!DataReady)
            return LandType.SurfaceType.Ground;
        return Data.Surface[coord.x, coord.y];
    }
    public Color GetColor(Vector2Int coord)
    {
        if (!DataReady)
            return Color.white;
        return Data.color[coord.x, coord.y];
    }
}

