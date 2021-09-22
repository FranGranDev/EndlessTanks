using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Threading;


public static class MeshGeneration
{
    public static MeshData GenerateTerrainMesh(MapData mapData, AnimationCurve Curve, float Height, int DetailLevel)
    {
        AnimationCurve HeightCurve = new AnimationCurve(Curve.keys);
        int SimpleIncreament = Mathf.RoundToInt(Mathf.Pow(2, DetailLevel));
        int MapDetail = Mathf.RoundToInt(Mathf.Pow(2, MapGenerator.MapDetail));
        int MeshSizeX = (mapData.xSize) / SimpleIncreament;
        int MeshSizeY = (mapData.ySize) / SimpleIncreament;
        
        MeshData mesh = new MeshData(MeshSizeX, MeshSizeY);
        mesh.Position = mapData.Position;

        for (int y = 0, i = 0; y < MeshSizeX; y++)
        {
            for (int x = 0; x < MeshSizeY; x++)
            {
                int RealX = x * SimpleIncreament;
                int RealY = y * SimpleIncreament;
                float LandY = HeightCurve.Evaluate(mapData.HeightMap[RealX, RealY]);
                float CurrantY = LandY * Height;
                mesh.verties[i] = new Vector3(RealX * MapDetail, RealY * MapDetail, -CurrantY);
                mesh.uvs[i] = new Vector2((float)x / MeshSizeX, (float)y / MeshSizeY);
                i++;
            }
        }
        int Triang = 0;
        int Vert = 0;
        for (int y = 0; y < MeshSizeX - 1; y++)
        {
            for (int x = 0; x < MeshSizeY - 1 ; x++)
            {
               
                mesh.triangles[Triang] = Vert;
                mesh.triangles[Triang + 1] = Vert + MeshSizeX;
                mesh.triangles[Triang + 2] = Vert + 1;
                mesh.triangles[Triang + 3] = Vert + 1;
                mesh.triangles[Triang + 4] = Vert + MeshSizeX;
                mesh.triangles[Triang + 5] = Vert + MeshSizeX + 1;
                Vert++;
                Triang += 6;
                
            }
            Vert++;
        }
        
        return mesh;
    }
}
public class MeshData
{
    public Vector2 Position;
    public Vector3[] verties;
    public int[] triangles;
    public Vector2[] uvs;
    public Color[] colors;

    public MeshData(int xSize, int ySize)
    {
        verties = new Vector3[(xSize) * (ySize)];
        uvs = new Vector2[(xSize) * (ySize)];
        triangles = new int[(xSize) * (ySize) * 6];
        colors = new Color[(xSize) * (ySize)];
    }

    public Mesh CreateMesh2D()
    {
        Mesh mesh = new Mesh();
        for (int i = 0; i < verties.Length; i++)
        {
            verties[i] = new Vector3(verties[i].x, verties[i].y, 0);
        }
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = verties;
        mesh.triangles = triangles;

        mesh.uv = uvs;


        return mesh;
    }
    public Vector3[] CreateNormals()
    {
        Mesh mesh = new Mesh();

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = verties;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();


        return mesh.normals;
    }
}

