using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Pathfinding;

public class MapGenerator : MonoBehaviour
{
    public enum BuildTypes { Endless, Simple };
    public BuildTypes Type;
    public static BuildTypes BuildType;

    public float SetGravity;
    public static float Gravity;

    public int SizeChunck;
    public int xSize;
    public int ySize;
    public static int MapSizeX;
    public static int MapSizeY;
    public static int ChunckSize;
    [Range(0, 3)]
    private int MapDetailLevel;
    public static int MapDetail;
    [Range(0, 3)]
    private int MeshDetailLevel;
    [Range(0, 3)]
    private int SurfaceDetailLevel;
    public NoiseMap.NormalizeNode NormalizeMod;

    public enum ColoredType {Solid, Gradient}
    public ColoredType LandColorType;
    public enum ObjectsType {Plants, Other, House, Special};
    public Biom[] Bioms;

    public int Seed;
    public static int GlobalSeed;
    public Vector2 Offset;
    public float Scale;
    public float Height;
    public AnimationCurve MapCurve;
    public AnimationCurve MeshCurve;
    public AnimationCurve PlantTypeCurve;
    public AnimationCurve HouseTypeCurve;
    public AnimationCurve ObjectTypeCurve;
    public AnimationCurve EnemyTypeCurve;
    public int MaxEnemyOnChunk;
    public int MaxPlantOnChunk;
    public int MaxObjectOnChunk;
    public int MaxBuildingsOnChunck;
    public CameraMovement Cam;

    public TerrainSettings[] Settings;

    public float TerrainRound(float Height, int k)
    {
        return Mathf.Round(Height * k) / k;
    }
    public Color AlphaColor(Color color, float Alpha)
    {
        if (Alpha > 1)
            Alpha = 1;
        return new Color(color.r, color.g, color.b, Alpha);
    }

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
    Queue<MapThreadInfo<SurfaceData>> SurfaceDataThreadInfoQueue = new Queue<MapThreadInfo<SurfaceData>>();
    Queue<MapThreadInfo<ObjectData>> ObjectDataThreadInfoQueue = new Queue<MapThreadInfo<ObjectData>>();


    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate {
            MapDataThread(centre, MapDetailLevel, callback);
        };

        new Thread(threadStart).Start();
    }
    void MapDataThread(Vector2 centre, int MapDetail, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(centre, MapDetail);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, MeshDetailLevel, callback);
        };

        new Thread(threadStart).Start();
    }
    void MeshDataThread(MapData mapData, int MeshDetail, Action<MeshData> callback)
    {
        MeshData meshData = MeshGeneration.GenerateTerrainMesh(mapData, MeshCurve, Height, MeshDetailLevel);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    public void RequestSurfaceData(MapData mapData, Vector3[] Normals, Action<SurfaceData> callback)
    {
        ThreadStart threadStart = delegate {
            SurfaceDataThread(mapData, Normals, SurfaceDetailLevel, callback);
        };

        new Thread(threadStart).Start();
    }
    void SurfaceDataThread(MapData mapData, Vector3[] Normals, int Detail, Action<SurfaceData> callback)
    {
        SurfaceData surfaceData = GenerateSurfaceData(mapData, Normals, Detail);
        lock (SurfaceDataThreadInfoQueue)
        {
            SurfaceDataThreadInfoQueue.Enqueue(new MapThreadInfo<SurfaceData>(callback, surfaceData));
        }
    }

    public void RequestObjectData(MapData mapData, Action<ObjectData> callback)
    {
        ThreadStart threadStart = delegate {
            ObjectDataThread(mapData, callback);
        };

        new Thread(threadStart).Start();
    }
    void ObjectDataThread(MapData mapData, Action<ObjectData> callback)
    {
        ObjectData objectData = GenerateObjectData(mapData);
        lock (SurfaceDataThreadInfoQueue)
        {
            ObjectDataThreadInfoQueue.Enqueue(new MapThreadInfo<ObjectData>(callback, objectData));
        }
    }


    private void Awake()
    {
        GlobalSeed = Seed;
        BuildType = Type;
        MapSizeX = xSize;
        MapSizeY = ySize;
        ChunckSize = SizeChunck;
        MapDetail = MapDetailLevel;
        Gravity = SetGravity;
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
        if (SurfaceDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < SurfaceDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<SurfaceData> threadInfo = SurfaceDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
        if (ObjectDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < ObjectDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<ObjectData> threadInfo = ObjectDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

    }

    public MapData GenerateMapData(Vector2 Center, int DetailLevel)
    {
        float ThisScale = Scale * ChunckSize / 64f;
        int Detail = Mathf.RoundToInt(Mathf.Pow(2, DetailLevel));
        int xRealSize = ChunckSize / Detail + 1;
        int yRealSize = ChunckSize / Detail + 1;

        float[,] MapHeight = NoiseMap.GenerateNoiseMap(xRealSize, yRealSize, Seed, Center + Offset, ThisScale, Settings[0].OctaveSettings, NormalizeMod);
        int[,] MapBiom = NoiseMap.GenerateBiomsNoiseMap(xRealSize, yRealSize, Seed, Center + Offset, ThisScale, Settings[3].OctaveSettings, NormalizeMod);
        Color[,] MapColor = new Color[(xRealSize), (yRealSize)];
        Color[,] MapWaterColor = new Color[(xRealSize), (yRealSize)];
        AnimationCurve ThisCurve = new AnimationCurve(MapCurve.keys);

        for (int y = 0; y < yRealSize; y++)
        {
            for (int x = 0; x < xRealSize; x++)
            {
                int BiomIndex = MapBiom[x, y];
                MapHeight[x, y] = ThisCurve.Evaluate(MapHeight[x, y]);
                MapColor[x, y] = Bioms[BiomIndex].LandColor.Evaluate(TerrainRound(MapHeight[x, y], 200));
                if (MapHeight[x, y] < (Bioms[BiomIndex].GetMaxHeight("EdgeWater")))
                {
                    MapWaterColor[x, y] = AlphaColor(MapColor[x, y], 0.8f - MapHeight[x, y]);
                }
                else
                {
                    MapWaterColor[x, y] = new Color(0, 0, 0, 0);
                }
            }
        }

        return new MapData(xRealSize, yRealSize, MapHeight, MapBiom, MapColor, MapWaterColor, Center, ThisCurve);
    }

    public SurfaceData GenerateSurfaceData(MapData mapData, Vector3[] Normals, int DetailLevel)
    {
        int SimpleIncreament = Mathf.RoundToInt(Mathf.Pow(2, DetailLevel));
        int SizeX = mapData.xSize / SimpleIncreament;
        int SizeY = mapData.ySize / SimpleIncreament;

        LandType.SurfaceType[,] Surface = new LandType.SurfaceType[(SizeY), (SizeY)];
        Color[,] color = mapData.ColorMap;
        float[,] Height = new float[(SizeX), (SizeY)];
        float[,] Clutch = new float[(SizeX), (SizeY)];
        float[,] MaxSpeed = new float[(SizeX), (SizeY)];

        for (int y = 0; y < SizeY; y++)
        {
            for(int x = 0; x < SizeX; x++)
            {
                int RealX = x * SimpleIncreament;
                int RealY = y * SimpleIncreament;
                int BiomIndex = mapData.BiomMap[RealX, RealY];
                Height[x, y] = mapData.HeightMap[RealX, RealY];
                Surface[x, y] = Bioms[BiomIndex].GetLandSurface(Height[x, y]);
                Clutch[x, y] = Bioms[BiomIndex].GetLandClutch(Height[x, y]);
                MaxSpeed[x, y] = Bioms[BiomIndex].GetLandMaxSpeed(Height[x, y]);
            }
        }

        return new SurfaceData(Height, Surface, color, Clutch, MaxSpeed, Normals);
    }

    public ObjectData GenerateObjectData(MapData mapData)
    {
        EnemyNoiseMap[] EnemyMap = NoiseMap.GenerateEnemyNoiseMap(mapData.HeightMap, Bioms, mapData.BiomMap, MaxEnemyOnChunk, Seed, mapData.Position + Offset, mapData.Position, Scale, EnemyTypeCurve, Settings[4].OctaveSettings);
        ObjectNoiseMap[] SpecialMap = NoiseMap.GenerateObjectNoiseMap(mapData.HeightMap, Bioms, mapData.BiomMap, 1, new ObjectNoiseMap[0], ObjectsType.Special, Seed, mapData.Position + Offset, mapData.Position, Scale, PlantTypeCurve, Settings[8].OctaveSettings);
        ObjectNoiseMap[] HouseMap = NoiseMap.GenerateObjectNoiseMap(mapData.HeightMap, Bioms, mapData.BiomMap, MaxBuildingsOnChunck, SpecialMap, ObjectsType.House, Seed, mapData.Position + Offset, mapData.Position, Scale, HouseTypeCurve, Settings[6].OctaveSettings);
        ObjectNoiseMap[] PlantMap = NoiseMap.GenerateObjectNoiseMap(mapData.HeightMap, Bioms, mapData.BiomMap, MaxPlantOnChunk, HouseMap, ObjectsType.Plants, Seed, mapData.Position + Offset, mapData.Position, Scale, PlantTypeCurve, Settings[5].OctaveSettings);
        ObjectNoiseMap[] OtherMap = NoiseMap.GenerateObjectNoiseMap(mapData.HeightMap, Bioms, mapData.BiomMap, MaxObjectOnChunk, HouseMap, ObjectsType.Other, Seed, mapData.Position + Offset, mapData.Position, Scale, ObjectTypeCurve, Settings[7].OctaveSettings);


        return new ObjectData(EnemyMap, PlantMap, OtherMap, HouseMap, SpecialMap);
    }

}


public struct MapData
{
    public int xSize;
    public int ySize;
    public float[,] HeightMap;
    public int[,] BiomMap; 
    public Color[,] ColorMap;
    public Color[,] WaterColorMap;
    public Vector2 Position;
    public AnimationCurve Curve;
    public MapData(int xSize, int ySize, float[,] HeightMap, int[,] BiomMap, Color[,] ColorMap, Color[,] WaterColorMap, Vector2 Position, AnimationCurve Curve)
    {
        this.xSize = xSize;
        this.ySize = ySize;
        this.HeightMap = HeightMap;
        this.BiomMap = BiomMap;
        this.ColorMap = ColorMap;
        this.WaterColorMap = WaterColorMap;
        this.Position = Position;
        this.Curve = new AnimationCurve(Curve.keys);
    }

}

public struct SurfaceData
{
    public int xSize;
    public int ySize;
    public float[,] Height;
    public LandType.SurfaceType[,] Surface;
    public Color[,] color;
    public float[,] Clutch;
    public float[,] MaxSpeed;
    public Vector3[] Normals;


    public SurfaceData(float[,] Height, LandType.SurfaceType[,] Surface, Color[,] color, float[,] Clutch, float[,] MaxSpeed, Vector3[] Normals)
    {
        xSize = Surface.GetLength(0) - 1;
        ySize = Surface.GetLength(0) - 1;
        this.Height = Height;
        this.Surface = Surface;
        this.Clutch = Clutch;
        this.MaxSpeed = MaxSpeed;
        this.Normals = Normals;
        this.color = color;

    }
}

public struct ObjectData
{
    public EnemyNoiseMap[] EnemyMap;
    public ObjectNoiseMap[] PlantMap;
    public ObjectNoiseMap[] OtherMap;
    public ObjectNoiseMap[] HouseMap;
    public ObjectNoiseMap[] SpecialMap;

    public void OnEnemyDestroyed(int index)
    {
        EnemyMap[index].Exist = false;
    }

    public ObjectData(EnemyNoiseMap[] EnemyMap, ObjectNoiseMap[] PlantMap, ObjectNoiseMap[] OtherMap, ObjectNoiseMap[] HouseMap, ObjectNoiseMap[] SpecialMap)
    {
        this.EnemyMap = EnemyMap;
        this.PlantMap = PlantMap;
        this.HouseMap = HouseMap;
        this.OtherMap = OtherMap;
        this.SpecialMap = SpecialMap;
    }
}


[System.Serializable]
public struct TerrainSettings
{
    public string Name;
    public float Height;
    public NoiseSettings[] OctaveSettings;
}
[System.Serializable]
public struct NoiseSettings
{
    public string Name;
    public const float MaxAmplitude = 1f;
    [Range(0f, MaxAmplitude)]
    public float Amplitude;
    [Range(0.01f, 35f)]
    public float Frequency;
}

[System.Serializable]
public struct Biom
{
    public string Name;
    public Gradient LandColor;
    public Color PlantsColor;
    public Color HouseColor;
    public LandType[] Land;
    public LandType[] SpecialLand;
    public Tank[] Enemys;

    [Range(0, 1)]
    public float PlantConcentrate;
    [Range(0, 1)]
    public float HouseConcentrate;
    [Range(0, 1)]
    public float OtherConcentrate;
    [Range(0, 1)]
    public float SpecialConcentrate;

    public LandType GetLand(float AverangeDepth)
    {
        for (int i = 0; i < Land.Length; i++)
        {
            if (AverangeDepth > Land[i].MinHeight && AverangeDepth < Land[i].MaxHeight)
                return Land[i];
        }
        return Land[0];
    }
    public Color GetLandColor(float AverangeDepth)
    {
        for (int i = 0; i < Land.Length; i++)
        {
            if (AverangeDepth > Land[i].MinHeight && AverangeDepth < Land[i].MaxHeight)
                return Land[i].color;
        }
        return Land[0].color;
    }
    public LandType.SurfaceType GetLandSurface(float AverangeDepth)
    {
        for (int i = 0; i < Land.Length; i++)
        {
            if (AverangeDepth > Land[i].MinHeight && AverangeDepth < Land[i].MaxHeight)
                return Land[i].Surface;
        }
        return Land[0].Surface;
    }
    public LandType.SurfaceType GetLandSurface(string Name)
    {
        for (int i = 0; i < Land.Length; i++)
        {
            if (Land[i].name == Name)
                return Land[i].Surface;
        }
        return Land[0].Surface;
    }
    public float GetLandClutch(float AverangeDepth)
    {
        for (int i = 0; i < Land.Length; i++)
        {
            if (AverangeDepth > Land[i].MinHeight && AverangeDepth < Land[i].MaxHeight)
                return Land[i].Clutch;
        }
        return Land[0].Clutch;
    }
    public float GetLandMaxSpeed(float AverangeDepth)
    {
        for (int i = 0; i < Land.Length; i++)
        {
            if (AverangeDepth > Land[i].MinHeight && AverangeDepth < Land[i].MaxHeight)
                return Land[i].MaxSpeed;
        }
        return Land[0].MaxSpeed;
    }
    public float GetSpecialMaxHeight(string Name)
    {
        for (int i = 0; i < SpecialLand.Length; i++)
        {
            if (SpecialLand[i].name == Name)
                return SpecialLand[i].MaxHeight;
        }
        return Land[0].MaxHeight;
    }
    public Color GetLandColor(string Name)
    {
        for (int i = 0; i < Land.Length; i++)
        {
            if (Land[i].name == Name)
                return Land[i].color;
        }
        return Land[0].color;
    }
    public Color GetSpecialColor(string Name)
    {
        for (int i = 0; i < SpecialLand.Length; i++)
        {
            if (SpecialLand[i].name == Name)
                return SpecialLand[i].color;
        }
        return SpecialLand[0].color;
    }
    public LandType.SurfaceType GetSpecialSurface(string Name)
    {
        for (int i = 0; i < SpecialLand.Length; i++)
        {
            if (SpecialLand[i].name == Name)
                return SpecialLand[i].Surface;
        }
        return SpecialLand[0].Surface;
    }
    public float GetMaxHeight(string Name)
    {
        for (int i = 0; i < Land.Length; i++)
        {
            if (Land[i].name == Name)
                return Land[i].MaxHeight;
        }
        return Land[0].MaxHeight;
    }
}
[System.Serializable]
public struct LandType
{
    public string name;
    public float MinHeight;
    public float MaxHeight;
    public Color color;
    
    public enum SurfaceType {DeepWater, Water, Sand, Ground, Mountain, HeightMountain};
    public SurfaceType Surface;
    [Range(0, 1f)]
    public float Clutch;
    [Range(0, 1f)]
    public float MaxSpeed;

    public Buildings[] Plants;
    public Buildings[] SimpleObj;
    public Buildings[] Buildings;
    public Buildings[] SpecialObject;

}
[System.Serializable]
public struct Buildings
{
    public GameObject Object;
    public StaticObject Scrips;
}


[System.Serializable]
public struct EnemyNoiseMap
{
    public bool Exist;
    public Vector2 Position;
    public Vector2 MapCenter;
    public float Index;
    public int Level;
    public Tank enemy;

    public EnemyNoiseMap(bool Exist, Vector2 Position, Vector2 MapCenter, Tank enemy, float Index, int Level)
    {
        this.Exist = Exist;
        this.Position = Position;
        this.MapCenter = MapCenter;
        this.Index = Index;
        this.Level = Level;
        this.enemy = enemy;
    }
}
[System.Serializable]
public struct ObjectNoiseMap
{
    public bool Exist;
    public bool[] Parts;
    public int[] Info;
    public Vector2 Position;
    public Quaternion Rotation;
    public GameObject Object;
    public Vector2 MapCenter;
    public float Size;
    public Color color;

    public ObjectNoiseMap(bool Exist, bool[] Parts, int[] Info, Vector2 Position, Quaternion Rotation, Vector2 MapCenter, GameObject Object, Color color, float Size)
    {
        this.Exist = Exist;
        this.Parts = Parts;
        this.Position = Position;
        this.Rotation = Rotation;
        this.MapCenter = MapCenter;
        this.Object = Object;
        this.Size = Size;
        this.Info = Info;
        this.color = color;
    }
}

public static class NoiseMap
{
    public enum NormalizeNode { Local, Global}

    public static float[,] GenerateNoiseMap(int xSize, int ySize, int Seed, Vector2 Offset, float Scale, NoiseSettings[] Settings, NormalizeNode normalize)
    {
        float[,] Map = new float[xSize, ySize];
        float MaxNoise = float.MinValue;
        float MinNoise = float.MaxValue;
        float HalfxSize = xSize * 0.5f;
        float HalfySize = ySize * 0.5f;
        System.Random prng = new System.Random(Seed);
        float offsetX = prng.Next(-100000, 100000) + Offset.x;
        float offsetY = prng.Next(-100000, 100000) - Offset.y;

        float MaxPossibleHeight = 0;
        for (int i = 0; i < Settings.Length; i++)
        {
            MaxPossibleHeight += NoiseSettings.MaxAmplitude;
        }

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                float NoiseHeight = 0;

                for (int i = 0; i < Settings.Length; i++)
                {
                    float xSample = (x - HalfxSize + offsetX) / xSize * Settings[i].Frequency * Scale;
                    float zSample = (y - HalfySize - offsetY) / ySize * Settings[i].Frequency * Scale;
                    float Noise = Mathf.PerlinNoise(xSample, zSample) * 2f - 1f;
                    NoiseHeight += Noise * Settings[i].Amplitude;
                }
                if (NoiseHeight > MaxNoise)
                {
                    MaxNoise = NoiseHeight;
                }
                else if (NoiseHeight < MinNoise)
                {
                    MinNoise = NoiseHeight;
                }

                Map[x, y] = NoiseHeight;
            }
        }
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (normalize == NormalizeNode.Local)
                {
                    Map[x, y] = Mathf.InverseLerp(MinNoise, MaxNoise, Map[x, y]);
                }
                else
                {
                    float NormalizedHeight = (Map[x, y] + 1f) / (2f * MaxPossibleHeight / 5f);
                    Map[x, y] = Mathf.Clamp(NormalizedHeight, 0, 1);
                }
            }
        }
        return Map;
    }
    public static float[,] GenerateLinesNoiseMap(int xSize, int ySize, int Seed, Vector2 Offset, float Scale, NoiseSettings[] Settings, NormalizeNode normalize)
    {
        float[,] Map = new float[xSize, ySize];
        float HalfxSize = xSize * 0.5f;
        float HalfySize = ySize * 0.5f;
        System.Random prng = new System.Random(Seed);
        float offsetX = prng.Next(-100000, 100000) + Offset.x;
        float offsetY = prng.Next(-100000, 100000) - Offset.y;

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                float NoiseHeightOctave = 0;


                float xSampleWidth = (x + offsetX) / xSize * Scale * Settings[1].Frequency;
                float ySampleWidth = (x + offsetX) / xSize * Scale * Settings[1].Frequency;
                float NoiseWidth = 1 + Mathf.PerlinNoise(xSampleWidth, ySampleWidth) * Settings[1].Amplitude * Settings[0].Frequency;

                float xSampleOctave = (x + offsetX) / xSize * Settings[0].Frequency * Scale;
                float zSampleOctave = (y - offsetY) / ySize * Settings[0].Frequency * Scale;
                float NoiseOctave = Mathf.PerlinNoise(xSampleOctave, zSampleOctave);
                NoiseHeightOctave = NoiseOctave * Settings[0].Amplitude;

                Map[x, y] = (Mathf.Round(NoiseHeightOctave) - Mathf.Round(NoiseHeightOctave / NoiseWidth));
                /*
                float xSampleWidth2 = (x + offsetX) / xSize * Scale * Settings[2].Frequency;
                float ySampleWidth2 = (x + offsetX) / xSize * Scale * Settings[2].Frequency;
                float NoiseWidth2 = 1 + Mathf.PerlinNoise(xSampleWidth2, ySampleWidth2) * Settings[2].Amplitude * Settings[0].Frequency;

                float xSampleOctave2 = (x + offsetX) / xSize * Settings[3].Frequency * Scale;
                float zSampleOctave2 = (y - offsetY) / ySize * Settings[3].Frequency * Scale;
                float NoiseOctave2 = Mathf.PerlinNoise(xSampleOctave2, zSampleOctave2);
                NoiseHeightOctave2 = NoiseOctave2 * Settings[3].Amplitude;

                if(Map[x, y] == 0)
                    Map[x, y] = (Mathf.Round(NoiseHeightOctave2) - Mathf.Round(NoiseHeightOctave2 / NoiseWidth2)) * 0.5f;
            */
            }
        }
        return Map;
    }
    public static int[,] GenerateBiomsNoiseMap(int xSize, int ySize, int Seed, Vector2 Offset, float Scale, NoiseSettings[] Settings, NormalizeNode normalize)
    {
        int[,] Map = new int[xSize, ySize];
        float HalfxSize = xSize * 0.5f;
        float HalfySize = ySize * 0.5f;
        System.Random prng = new System.Random(Seed);
        float offsetX = prng.Next(-100000, 100000) + Offset.x;
        float offsetY = prng.Next(-100000, 100000) - Offset.y;
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int i = 0; i < Settings.Length - 1; i++)
                {
                    if(Map[x, y] == 0)
                    {
                        int NoiseHeight = 0;
                        float xSample = ((x - HalfxSize + offsetX) / xSize - i * 10f) * Settings[i].Frequency * Scale;
                        float zSample = ((y - HalfySize - offsetY) / ySize - i * 10f) * Settings[i].Frequency * Scale;
                        float Noise = Mathf.PerlinNoise(xSample, zSample);
                        NoiseHeight = Mathf.RoundToInt(Mathf.Pow(Noise * Settings[i].Amplitude, 1f)) * (i + 1);
                        Map[x, y] = NoiseHeight;
                    }
                    if (i == Settings.Length - 1 && Map[x, y] == 0)
                    {
                        Map[x, y] = Settings.Length - 1;
                    }

                }
            }
        }
        return Map;
    }
    public static float[,] GenerateBiomsNoiseMapTest(int xSize, int ySize, int Seed, Vector2 Offset, float Scale, NoiseSettings[] Settings, NormalizeNode normalize)
    {
        float[,] Map = new float[xSize, ySize];
        float HalfxSize = xSize * 0.5f;
        float HalfySize = ySize * 0.5f;
        System.Random prng = new System.Random(Seed);
        float offsetX = prng.Next(-100000, 100000) + Offset.x;
        float offsetY = prng.Next(-100000, 100000) - Offset.y;
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int i = 0; i < Settings.Length - 1; i++)
                {
                    if (Map[x, y] == 0)
                    {
                        float NoiseHeight = 0;
                        float xSample = ((x - HalfxSize + offsetX) / xSize - i * 10f) * Settings[i].Frequency * Scale;
                        float zSample = ((y - HalfySize - offsetY) / ySize - i * 10f) * Settings[i].Frequency * Scale;
                        float Noise = Mathf.PerlinNoise(xSample, zSample);
                        NoiseHeight = Mathf.RoundToInt(Mathf.Pow(Noise * Settings[i].Amplitude, 1f)) * (i + 1) * 0.1f;
                        Map[x, y] = NoiseHeight;
                    }
                    if (i == Settings.Length - 1 && Map[x, y] == 0)
                    {
                        Map[x, y] = Settings.Length - 2;
                    }

                }
            }
        }
        return Map;
    }
    public static EnemyNoiseMap[] GenerateEnemyNoiseMap(float[,] HeightMap, Biom[] bioms, int[,] BiomsMap, int MaxEnemyOnChunck, int Seed, Vector2 Offset, Vector2 Center, float Scale, AnimationCurve Curve, NoiseSettings[] Settings)
    {
        int xSize = Mathf.FloorToInt(Mathf.Log(MaxEnemyOnChunck + 1, 2));
        int ySize = xSize;
        int ChunckOfset = HeightMap.GetLength(0) / (xSize + 1);
        EnemyNoiseMap[] EnemyMap = new EnemyNoiseMap[xSize * ySize];

        float HalfxSize = xSize * 0.5f;
        float HalfySize = ySize * 0.5f;
        System.Random prng = new System.Random(Seed);
        float offsetX = prng.Next(-100000, 100000) + Offset.x;
        float offsetY = prng.Next(-100000, 100000) - Offset.y;

        
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                int RealX = x * ChunckOfset;
                int RealY = y * ChunckOfset;
                float Height = HeightMap[RealX, RealY];
                AnimationCurve ThisCurve = new AnimationCurve(Curve.keys);
                Tank[] Enemys = bioms[BiomsMap[RealX, RealY]].Enemys;
                Vector2 Position = new Vector3(RealX, RealY, -1);

                    

                float xConcentrate = (x - HalfxSize + offsetX) / xSize * Settings[0].Frequency * Scale;
                float zConcentrate = (y - HalfySize - offsetY) / ySize * Settings[0].Frequency * Scale;
                float NoiseConcentrate = Mathf.PerlinNoise(xConcentrate, zConcentrate);
                float Concentrate = NoiseConcentrate * Settings[0].Amplitude;

                float xExist = (x - HalfxSize + offsetX) / xSize * Settings[1].Frequency * Scale;
                float zExist = (y - HalfySize - offsetY) / ySize * Settings[1].Frequency * Scale;
                float Noise = Mathf.PerlinNoise(xExist, zExist);
                bool Exist = Mathf.Round(0.1f + Noise * Settings[1].Amplitude * Concentrate) == 1;

                float MinHeight = bioms[BiomsMap[x, y]].GetMaxHeight("Sand");
                float MaxHeight = bioms[BiomsMap[x, y]].GetMaxHeight("Mountain");
                if (HeightMap[RealX, RealY] > MaxHeight || HeightMap[RealX, RealY] < MinHeight)
                    Exist = false;

                float TypeX = (x - HalfxSize + offsetX) / xSize * Settings[2].Frequency * Scale;
                float TypeY = (y - HalfySize - offsetY) / ySize * Settings[2].Frequency * Scale;
                int EnemyType = Mathf.RoundToInt(ThisCurve.Evaluate(Mathf.PerlinNoise(TypeX, TypeY)) * (Enemys.Length - 1));
                if (Enemys.Length == 0)
                {
                    Exist = false;
                }


                Tank ThisEnemy =  Exist ? Enemys[EnemyType] : null;

                EnemyMap[y * xSize + x] = new EnemyNoiseMap(Exist, Center + Position, Center, ThisEnemy, 0, 1);
            }
        }
        return EnemyMap;
    }
    public static ObjectNoiseMap[] GenerateObjectNoiseMap(float[,] HeightMap, Biom[] bioms, int[,] BiomsMap, int MaxObjectOnChunck, ObjectNoiseMap[] Other, MapGenerator.ObjectsType Type, int Seed, Vector2 Offset, Vector2 Center, float Scale, AnimationCurve Curve, NoiseSettings[] Settings)
    {
        int xSize = Mathf.FloorToInt(Mathf.Log(MaxObjectOnChunck + 1, 2));
        int ySize = xSize;
        int ChunckOfset = HeightMap.GetLength(0) / (xSize);
        ObjectNoiseMap[] EnemyMap = new ObjectNoiseMap[xSize * ySize];

        float HalfxSize = xSize * 0.5f;
        float HalfySize = ySize * 0.5f;
        System.Random prng = new System.Random(Seed);
        float offsetX = prng.Next(-100000, 100000) + Offset.x;
        float offsetY = prng.Next(-100000, 100000) - Offset.y;


        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                int RealX = x * ChunckOfset;
                int RealY = y * ChunckOfset;
                float Height = HeightMap[RealX, RealY];
                Biom ThisBiom = bioms[BiomsMap[x, y]];
                LandType Land = ThisBiom.GetLand(Height);
                AnimationCurve ThisCurve = new AnimationCurve(Curve.keys);

                float xConcentrate = (x - HalfxSize + offsetX) / xSize * Settings[0].Frequency * Scale;
                float zConcentrate = (y - HalfySize - offsetY) / ySize * Settings[0].Frequency * Scale;
                float NoiseConcentrate = Mathf.PerlinNoise(xConcentrate, zConcentrate);
                float BiomC = 1;
                switch (Type)
                {
                    case MapGenerator.ObjectsType.House:
                        BiomC = ThisBiom.HouseConcentrate;
                        break;
                    case MapGenerator.ObjectsType.Plants:
                        BiomC = ThisBiom.PlantConcentrate;
                        break;
                    case MapGenerator.ObjectsType.Other:
                        BiomC = ThisBiom.OtherConcentrate;
                        break;
                    case MapGenerator.ObjectsType.Special:
                        BiomC = ThisBiom.SpecialConcentrate;
                        break;
                }
                
                float Concentrate = (NoiseConcentrate) * BiomC * Settings[0].Amplitude;

                float xExist = (x - HalfxSize + offsetX) / xSize * Settings[1].Frequency * Scale;
                float zExist = (y - HalfySize - offsetY) / ySize * Settings[1].Frequency * Scale;
                float Noise = Mathf.PerlinNoise(xExist, zExist);
                bool Exist = Mathf.Round(0.15f + Noise * Settings[1].Amplitude * Concentrate) == 1;
                if(!Exist)
                {
                    return EnemyMap;
                }

                float RandOffsetX = (x - HalfxSize + offsetX) / xSize * Settings[2].Frequency * Scale;
                float RandOffsetY = (y - HalfySize - offsetY) / ySize * Settings[2].Frequency * Scale;
                float RandNoise = Mathf.PerlinNoise(RandOffsetX, RandOffsetY) * 2 - 1;

                float xScale = (x - HalfxSize + offsetX) / xSize * Settings[4].Frequency * Scale;
                float zScale = (y - HalfySize - offsetY) / ySize * Settings[4].Frequency * Scale;
                float NoiseScale = Mathf.PerlinNoise(xScale, zScale);
                float Size = 0.5f + NoiseScale * 1.5f;

                float TypeX = (x - HalfxSize + offsetX) / xSize * Settings[3].Frequency * Scale;
                float TypeY = (y - HalfySize - offsetY) / ySize * Settings[3].Frequency * Scale;
                float TypeNoise = ThisCurve.Evaluate(Mathf.PerlinNoise(TypeX, TypeY));

                int ObjectType = 0;
                Color color = Color.white;
                GameObject Object = null;
                bool[] Parts = new bool[0];
                int[] Info = new int[0];

                if (Other.Length > (y * xSize + x + 1) && Other[y * xSize + x].Exist)
                {
                    Exist = false;
                    return EnemyMap;
                }
                else
                {
                    switch (Type)
                    {
                        case MapGenerator.ObjectsType.House:
                            {
                                ObjectType = Mathf.RoundToInt(TypeNoise * (Land.Buildings.Length - 1));
                                color = bioms[BiomsMap[x, y]].HouseColor;

                                if (Land.Buildings.Length > 0)
                                {
                                    Object = Land.Buildings[ObjectType].Object;
                                    Parts = new bool[Land.Buildings[ObjectType].Scrips.Parts.Length];
                                }
                                else
                                {
                                    Exist = false;
                                }
                            }
                            break;
                        case MapGenerator.ObjectsType.Plants:
                            {
                                ObjectType = Mathf.RoundToInt(TypeNoise * (Land.Plants.Length - 1));
                                color = bioms[BiomsMap[x, y]].PlantsColor;

                                if (Land.Buildings.Length > 0)
                                {
                                    Object = Land.Plants[ObjectType].Object;
                                    Parts = new bool[Land.Plants[ObjectType].Scrips.Parts.Length];
                                }
                                else
                                {
                                    Exist = false;
                                }
                            }
                            break;
                        case MapGenerator.ObjectsType.Other:
                            {
                                ObjectType = Mathf.RoundToInt(TypeNoise * (Land.SimpleObj.Length - 1));
                                color = bioms[BiomsMap[x, y]].PlantsColor;

                                if (Land.SimpleObj.Length > 0)
                                {
                                    Object = Land.SimpleObj[ObjectType].Object;
                                    Parts = new bool[Land.SimpleObj[ObjectType].Scrips.Parts.Length];
                                }
                                else
                                {
                                    Exist = false;
                                }
                            }
                            break;
                        case MapGenerator.ObjectsType.Special:
                            {
                                ObjectType = Mathf.RoundToInt(TypeNoise * (Land.SpecialObject.Length - 1));
                                color = Color.white;

                                if (Land.Buildings.Length > 0)
                                {
                                    Object = Land.SpecialObject[ObjectType].Object;
                                    Parts = new bool[Land.SpecialObject[ObjectType].Scrips.Parts.Length];
                                }
                                else
                                {
                                    Exist = false;
                                }
                            }
                            break;
                    }
                }

                Vector2 RandOffset = new Vector2(RandNoise * 2, RandNoise * 2);
                Quaternion Rotation = Quaternion.Euler(0, 0, RandNoise * 180);
                
                
                Vector2 Position = new Vector2(RealX, RealY); //Fix
                EnemyMap[y * xSize + x] = new ObjectNoiseMap(Exist, Parts, Info, Center + Position + RandOffset * 5, Rotation, Center, Object, color, Size);
            }
        }
        return EnemyMap;
    }
    public static int GenerateStaticBiom(int Seed, int MaxBiom)
    {
        System.Random prng = new System.Random(Seed);
        return prng.Next(0, MaxBiom);
    }
    public static Part[] GenerateStaticPart(int Seed, int num, int a, Vector2 Map)
    {
        System.Random prngIndex = new System.Random(Seed + a + Mathf.RoundToInt(Map.x - Map.y));
        System.Random prngType = new System.Random(Seed + num + Mathf.RoundToInt(Map.x + Map.y));
        int Type = prngType.Next(0, 10);

        if(Type >= 0 && Type <= 2)
        {
            Part part = GameData.active.Tracks[prngIndex.Next(0, GameData.active.Tracks.Length)].part;
            return new Part[2] { part, part };
        }
        else if(Type > 2 && Type <= 3)
        {
            return new Part[1] { GameData.active.Towers[prngIndex.Next(0, GameData.active.Towers.Length)].part };
        }
        else if (Type > 3 && Type <= 4)
        {
            return new Part[1] { GameData.active.Bodies[prngIndex.Next(0, GameData.active.Bodies.Length)].part };
        }
        else if (Type > 4 && Type <= 6)
        {
            return new Part[1] { GameData.active.Guns[prngIndex.Next(0, GameData.active.Guns.Length)].part };
        }
        else if (Type > 6 && Type <= 8)
        {
            return new Part[1] { GameData.active.Engines[prngIndex.Next(0, GameData.active.Engines.Length)].part };
        }
        else if (Type > 8 && Type <= 9)
        {
            return new Part[1] { GameData.active.Electro[prngIndex.Next(0, GameData.active.Electro.Length)].part };
        }

        return new Part[1] { GameData.active.Engines[prngIndex.Next(0, GameData.active.Engines.Length)].part };
    }
}
