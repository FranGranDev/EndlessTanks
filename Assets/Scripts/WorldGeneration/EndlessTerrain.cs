using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class EndlessTerrain : MonoBehaviour
{
    const float vieweMoveThreshholdsForChunckUpdate = 5f;
    const float sqrvieweMoveThreshholdsForChunckUpdate = vieweMoveThreshholdsForChunckUpdate * vieweMoveThreshholdsForChunckUpdate;
    public Transform Viewer;
    public static Vector2 viewerPosition;
    public static Vector2 viewerChunckPosition;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    public static EndlessTerrain active;
    public BoxCollider2D[] Edge;
    public Material material;
    public GameObject Chunck;
    public Scene Level;
    public LODInfo LOD;
    public static float MaxDistanceView;
    public static float[] DistanceDeleteRange;
    public int ChunckScale;
    private int ChunckSize;
    private int ChunksVisible;
    private Vector2 MapSize;
    private int SimpleChunckX;
    private int SimpleChunckY;
    private int[] ChunksDeleteRange;
    private bool Endless;


    public static Dictionary<Vector2, ObjectData> TerrainChunckObjectData = new Dictionary<Vector2, ObjectData>();
    public static void OnEnemyDestroyed(Vector2 MapPos, int Index)
    {
        if(TerrainChunckObjectData.ContainsKey(MapPos))
        {
            TerrainChunckObjectData[MapPos].EnemyMap[Index].Exist = false;
        }
    }
    public static void OnHouseDestroyed(Vector2 MapPos, int Index)
    {
        if (TerrainChunckObjectData.ContainsKey(MapPos))
        {
            TerrainChunckObjectData[MapPos].HouseMap[Index].Exist = false;
        }
    }
    public static void OnHousePartDestroyed(Vector2 MapPos, int Index, int PartIndex)
    {
        if (TerrainChunckObjectData.ContainsKey(MapPos))
        {
            TerrainChunckObjectData[MapPos].HouseMap[Index].Parts[PartIndex] = true;
        }
    }
    public static void OnPlantDestroyed(Vector2 MapPos, int Index)
    {
        if (TerrainChunckObjectData.ContainsKey(MapPos))
        {
            TerrainChunckObjectData[MapPos].PlantMap[Index].Exist = false;
        }
    }
    public static void OnPlantPartDestroyed(Vector2 MapPos, int Index, int PartIndex)
    {
        if (TerrainChunckObjectData.ContainsKey(MapPos))
        {
            TerrainChunckObjectData[MapPos].PlantMap[Index].Parts[PartIndex] = true;
        }
    }
    public static void OnOtherDestroyed(Vector2 MapPos, int Index)
    {
        if (TerrainChunckObjectData.ContainsKey(MapPos))
        {
            TerrainChunckObjectData[MapPos].OtherMap[Index].Exist = false;
        }
    }
    public static void OnOtherPartDestroyed(Vector2 MapPos, int Index, int PartIndex)
    {
        if (TerrainChunckObjectData.ContainsKey(MapPos))
        {
            TerrainChunckObjectData[MapPos].OtherMap[Index].Parts[PartIndex] = true;
        }
    }

    Dictionary<Vector2, TerrainChunk> TerrainChunckDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> LastUpdateChank = new List<TerrainChunk>();

    public void Awake()
    {
        active = this;
        mapGenerator = FindObjectOfType<MapGenerator>();
    }
    public void Update()
    {
        viewerPosition = new Vector2(Viewer.position.x, Viewer.position.y);
        if(Endless)
        {
            if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrvieweMoveThreshholdsForChunckUpdate)
            {
                viewerPositionOld = viewerPosition;
                UpdateVisibleChunks();
            }
        }
    }
    public void Start()
    {
        switch (MapGenerator.BuildType)
        {
            case MapGenerator.BuildTypes.Endless:
                OnEndlessStart();
                break;
            case MapGenerator.BuildTypes.Simple:
                OnLevelStart();
                break;
        }
    }

    private void OnEndlessStart()
    {
        Endless = true;
        MaxDistanceView = LOD.ViewedDst;
        DistanceDeleteRange = new float[2];
        DistanceDeleteRange[0] = LOD.DestroyDstMin;
        DistanceDeleteRange[1] = LOD.DestroyDstMax;
        ChunckSize = MapGenerator.ChunckSize;
        ChunksVisible = Mathf.RoundToInt(MaxDistanceView / ChunckSize);
        ChunksDeleteRange = new int[2];
        ChunksDeleteRange[0] = Mathf.RoundToInt(DistanceDeleteRange[0] / ChunckSize);
        ChunksDeleteRange[1] = Mathf.RoundToInt(DistanceDeleteRange[1] / ChunckSize);
        UpdateVisibleChunks();

        StartCoroutine(SetGraph());
        StartCoroutine(ClearGC());
    }
    private void OnLevelStart()
    {
        ChunckSize = MapGenerator.ChunckSize;

        MapSize = new Vector2(MapGenerator.MapSizeX, MapGenerator.MapSizeY);
        MaxDistanceView = MapSize.magnitude;
        SimpleChunckX = Mathf.RoundToInt(MapSize.x / ChunckSize);
        SimpleChunckY = Mathf.RoundToInt(MapSize.y / ChunckSize);

        mapGenerator.Cam.transform.position = MapSize / 2;

        Bounds bound = new Bounds();
        bound.min = Vector2.zero;
        bound.max = MapSize;
        mapGenerator.Cam.SetBound(true, bound);

        CreateSimpleMap();
        CreateBounds();
    }

    private IEnumerator SetGraph()
    {
        GridGraph graph = AstarPath.active.data.FindGraphWhichInheritsFrom(typeof(GridGraph)) as GridGraph;
        int Size = Mathf.RoundToInt(MaxDistanceView * 0.4f);
        graph.SetDimensions(Size, Size, 4);
        yield return new WaitForSeconds(0.1f);
        graph.Scan();
        yield break;
    }
    private IEnumerator ClearGC()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            if(Scene.Player.Rig.velocity.magnitude < 5)
            {
                Resources.UnloadUnusedAssets();
            }
            
        }
    }

    public void UpdateVisibleChunks()
    {
        for(int i = 0; i < LastUpdateChank.Count; i++)
        {
            LastUpdateChank[i].SetVisible(false);
        }
        LastUpdateChank.Clear();

        int CurranteChuckX = Mathf.RoundToInt(viewerPosition.x / ChunckSize);
        int CurranteChuckY = Mathf.RoundToInt(viewerPosition.y / ChunckSize);
        viewerChunckPosition = new Vector2(CurranteChuckX, CurranteChuckY);

        for(int yOffset = -ChunksVisible; yOffset <= ChunksVisible; yOffset += ChunckScale)
        {
            for (int xOffset = -ChunksVisible; xOffset <= ChunksVisible; xOffset += ChunckScale)
            {
                Vector2 viewedChuckCoord = new Vector2(CurranteChuckX + xOffset, CurranteChuckY + yOffset);
                if(TerrainChunckDictionary.ContainsKey(viewedChuckCoord))
                {
                    TerrainChunckDictionary[viewedChuckCoord].UpdateChunk();
                }
                else
                {
                    TerrainChunckDictionary.Add(viewedChuckCoord, new TerrainChunk(viewedChuckCoord, ChunckSize, ChunckScale, Chunck, transform, material, new AddObjInfo(true)));
                }
            }
        }
        for (int yOffset = -ChunksDeleteRange[1]; yOffset <= ChunksDeleteRange[1]; yOffset++)
        {
            for (int xOffset = -ChunksDeleteRange[1]; xOffset <= ChunksDeleteRange[1]; xOffset++)
            {
                if(Mathf.Abs(yOffset) > ChunksDeleteRange[0] || Mathf.Abs(xOffset) > ChunksDeleteRange[0])
                {
                    Vector2 viewedChuckCoord = new Vector2(CurranteChuckX + xOffset, CurranteChuckY + yOffset);
                    if(TerrainChunckDictionary.ContainsKey(viewedChuckCoord))
                    {
                        TerrainChunckDictionary[viewedChuckCoord].DeleteChunk();
                        TerrainChunckDictionary.Remove(viewedChuckCoord);
                    }
                }
            }
        }
    }
    public void CreateSimpleMap()
    {
        for (int yOffset = 0; yOffset < SimpleChunckY; yOffset += ChunckScale)
        {
            for (int xOffset = 0; xOffset < SimpleChunckX; xOffset += ChunckScale)
            {
                Vector2 viewedChuckCoord = new Vector2(xOffset, yOffset);
                TerrainChunckDictionary.Add(viewedChuckCoord, new TerrainChunk(viewedChuckCoord, ChunckSize, ChunckScale, Chunck, transform, material, new AddObjInfo(false, true, true, true, true)));
            }
        }
    }
    private void CreateBounds()
    {
        for(int i = 0; i < Edge.Length; i++)
        {
            switch(i)
            {
                case 0:
                    Edge[i].offset = new Vector2(10, MapSize.y / 2);
                    Edge[i].size = new Vector2(0.1f, MapSize.y);
                    break;
                case 1:
                    Edge[i].offset = new Vector2(MapSize.x / 2, MapSize.y - 10);
                    Edge[i].size = new Vector2(MapSize.x, 0.1f);
                    break;
                case 2:
                    Edge[i].offset = new Vector2(MapSize.x - 5, MapSize.y / 10);
                    Edge[i].size = new Vector2(0.1f, MapSize.y);
                    break;
                case 3:
                    Edge[i].offset = new Vector2(MapSize.x / 2, 10);
                    Edge[i].size = new Vector2(MapSize.x, 0.1f);
                    break;
            }
        }
    }

    public class TerrainChunk
    {
        GameObject MeshObject;
        Vector2 Position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshRenderer meshRendererWater;
        MeshFilter meshFilter;
        MeshFilter MeshFilterWater;
        MeshCollider meshCollider;

        TerrainSurface surface;

        AstarPath Astar;

        MapData mapData;

        AddObjInfo ObjInfo;
        bool MapDataRecived;
        int prevLodIndex = -1;


        public TerrainChunk(Vector2 Coord, int Size, int Scale, GameObject Chunck, Transform Parent, Material material, AddObjInfo ObjInfo)
        {
            this.ObjInfo = ObjInfo;
            Position = Coord * Size;
            bounds = new Bounds(Position, Vector2.one * Size);
            Vector3 PositionV3 = new Vector3(Position.x, Position.y, 0f);

            MeshObject = Instantiate(Chunck, PositionV3, Quaternion.identity, Parent);
            MeshObject.transform.localScale = Vector3.one * Scale;
            surface = MeshObject.transform.GetChild(0).GetComponent<TerrainSurface>();
            meshRenderer = MeshObject.transform.GetChild(0).GetComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshRendererWater = MeshObject.transform.GetChild(1).GetComponent<MeshRenderer>();
            meshRendererWater.material = material;
            meshFilter = MeshObject.transform.GetChild(0).GetComponent<MeshFilter>();
            MeshFilterWater = MeshObject.transform.GetChild(1).GetComponent<MeshFilter>();
            meshCollider = MeshObject.transform.GetChild(0).GetComponent<MeshCollider>();

            SetVisible(false);

            mapGenerator.RequestMapData(Position, OnMapDataRecived);
        }

        void OnMapDataRecived(MapData mapData)
        {
            this.mapData = mapData;
            MapDataRecived = true;

            if (meshRenderer == null)
                return;

            Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.ColorMap, mapData.xSize, mapData.ySize);
            Texture2D WaterTexture = TextureGenerator.TextureFromColourMap(mapData.WaterColorMap, mapData.xSize, mapData.ySize);
            meshRenderer.material.mainTexture = texture;
            meshRendererWater.material.mainTexture = WaterTexture;

            
            UpdateChunk();

            mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);

        }
        void OnMeshDataReceived(MeshData meshData)
        {
            Vector3[] Normals = meshData.CreateNormals();
            Mesh mesh = meshData.CreateMesh2D();
             
            meshFilter.mesh = mesh;
            MeshFilterWater.mesh = mesh;
            meshCollider.sharedMesh = mesh;
            

            mapGenerator.RequestSurfaceData(mapData, Normals, OnSurfaceDataReceived);
        }
        void OnSurfaceDataReceived(SurfaceData surfaceData)
        {
            surface.SetData(surfaceData);

            if (!TerrainChunckObjectData.ContainsKey(Position))
            {
                mapGenerator.RequestObjectData(mapData, OnObjectDataReceived);
            }
            else
            {
                OnObjectDataReceived(TerrainChunckObjectData[Position]);
            }
        }
        void OnObjectDataReceived(ObjectData objectData)
        {
            if (!TerrainChunckObjectData.ContainsKey(Position))
            {
                TerrainChunckObjectData.Add(Position, objectData);
            }
                

            AddGameObjects(objectData);

            OnTerrainBuilded();
        }
        void OnTerrainBuilded()
        {
            
        }

        public void UpdateChunk()
        {
            if (MapDataRecived)
            {
                bool nowVisible = Visible();
                if (nowVisible)
                {
                    LastUpdateChank.Add(this);
                }
                SetVisible(nowVisible);
            }
        }
        public bool Visible()
        {
            return (MapGenerator.BuildType == MapGenerator.BuildTypes.Simple || 
                   ((viewerPosition.x < bounds.max.x + MaxDistanceView &&
                     viewerPosition.y < bounds.max.y + MaxDistanceView) &&
                    (viewerPosition.x > bounds.min.x - MaxDistanceView &&
                     viewerPosition.y > bounds.min.y - MaxDistanceView)));
        }

        void AddGameObjects(ObjectData objectData)
        {
            #region EnemyCreate
            if(ObjInfo.EnemyCreate && false)
            { 
                for (int i = 0; i < objectData.EnemyMap.Length; i++)
                {
                    if (objectData.EnemyMap[i].Exist)
                    {
                        Tank ThisEnemy = Instantiate(objectData.EnemyMap[i].enemy, objectData.EnemyMap[i].Position, Quaternion.identity, null);
                        AiController AiControll = ThisEnemy.GetComponent<AiController>();
                        AiControll.ChangeControllerType(AiController.ControllerType.Battle);


                        EndlessAiStuff AiDisable = ThisEnemy.gameObject.AddComponent<EndlessAiStuff>();
                        ThisEnemy.gameObject.AddComponent<CircleCollider2D>();
                        bool On = (viewerPosition - (Vector2)ThisEnemy.transform.position).magnitude > MaxDistanceView + 1;
                        AiDisable.SetOnStart(On, Position, i);


                    }
                }
            }

            #endregion
            #region PlantCreate
            if (ObjInfo.PlantCreate)
            {
                for (int i = 0; i < objectData.PlantMap.Length; i++)
                {
                    if (objectData.PlantMap[i].Exist)
                    {
                        GameObject ThisPlant = Instantiate(objectData.PlantMap[i].Object, objectData.PlantMap[i].Position, objectData.PlantMap[i].Rotation, MeshObject.transform);
                        ThisPlant.GetComponent<StaticObject>().OnStart(mapData.Position, i, objectData.PlantMap[i].Parts, objectData.PlantMap[i].Size, objectData.PlantMap[i].color);
                    }
                }
            }
            #endregion
            #region ObjCreate
            if (ObjInfo.ObjCreate)
            {
                for (int i = 0; i < objectData.OtherMap.Length; i++)
                {
                    if (objectData.OtherMap[i].Exist)
                    {
                        GameObject ThisOther = Instantiate(objectData.OtherMap[i].Object, objectData.OtherMap[i].Position, objectData.OtherMap[i].Rotation, MeshObject.transform);
                        ThisOther.GetComponent<StaticObject>().OnStart(mapData.Position, i, objectData.OtherMap[i].Parts, objectData.OtherMap[i].Size, objectData.OtherMap[i].color);
                    }
                }
            }
            #endregion
            #region HouseCreate
            if (ObjInfo.HouseCreate)
            {
                for (int i = 0; i < objectData.HouseMap.Length; i++)
                {
                    if (objectData.HouseMap[i].Exist)
                    {
                        GameObject ThisHouse = Instantiate(objectData.HouseMap[i].Object, objectData.HouseMap[i].Position, objectData.HouseMap[i].Rotation, MeshObject.transform);
                        ThisHouse.GetComponent<StaticObject>().OnStart(mapData.Position, i, objectData.HouseMap[i].Parts, objectData.HouseMap[i].Size, objectData.HouseMap[i].color);
                    }
                }
            }
            #endregion
            #region SpecialCreate
            if (ObjInfo.SpecialCreate)
            {
                for (int i = 0; i < objectData.SpecialMap.Length; i++)
                {
                    if (objectData.SpecialMap[i].Exist)
                    {
                        GameObject ThisSpecial = Instantiate(objectData.SpecialMap[i].Object, objectData.SpecialMap[i].Position, objectData.SpecialMap[i].Rotation, MeshObject.transform);
                        ThisSpecial.GetComponent<StaticObject>().OnStart(mapData.Position, i, objectData.SpecialMap[i].Parts, objectData.SpecialMap[i].Size, objectData.SpecialMap[i].color);
                    }
                }
            }
            #endregion

        }

        public void DeleteChunk()
        {
            if(meshFilter.sharedMesh != null)
                meshFilter.sharedMesh.Clear(false);
            if(MeshFilterWater.sharedMesh != null)
                MeshFilterWater.sharedMesh.Clear(false);
            DestroyImmediate(meshFilter.sharedMesh);
            DestroyImmediate(MeshFilterWater.sharedMesh);
            DestroyImmediate(MeshObject);
        }
        public void SetVisible(bool visible)
        {
            if(MeshObject != null)
                MeshObject.SetActive(visible);
        }
        public bool isVisible()
        {
            return MeshObject.activeSelf;
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int ViewedDst;
        public int DestroyDstMin;
        public int DestroyDstMax;
        public bool PreLoadWorld;
        public int ChunckToPreLoad;
    }

    public struct AddObjInfo
    {
        public bool EnemyCreate;
        public bool PlantCreate;
        public bool ObjCreate;
        public bool HouseCreate;
        public bool SpecialCreate;

        public AddObjInfo(bool EnemyCreate, bool PlantCreate, bool ObjCreate, bool HouseCreate, bool SpecialCreate)
        {
            this.EnemyCreate = EnemyCreate;
            this.PlantCreate = PlantCreate;
            this.ObjCreate = ObjCreate;
            this.HouseCreate = HouseCreate;
            this.SpecialCreate = SpecialCreate;
        }
        public AddObjInfo(bool Create)
        {
            EnemyCreate = Create;
            PlantCreate = Create;
            ObjCreate = Create;
            HouseCreate = Create;
            SpecialCreate = Create;
        }
    }
}
