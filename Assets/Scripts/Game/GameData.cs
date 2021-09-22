using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData active;
    public static bool InBuild;

    public GamePlay Play;
    [Header("-------Tanks-------")]
    public TankInfo[] Tanks;
    public TankInfo[] GetSortedTanks()
    {
        TankInfo[] info = new TankInfo[Tanks.Length];
        for (int i = 0; i < info.Length; i++)
        {
            info[i] = Tanks[i];
        }

        for (int i = 0; i < info.Length; i++)
        {
            for (int a = i + 1; a < info.Length; a++)
            {
                if (info[a].level < info[i].level)
                {
                    TankInfo temp = info[a];
                    info[a] = info[i];
                    info[i] = temp;
                }
            }
        }
        return info;
    }
    public PartInfo[] GetSortedParts(Part.Type type)
    {
        PartInfo[] part = new PartInfo[0];
        switch (type)
        {
            case Part.Type.Body:
                {
                    part = new PartInfo[Bodies.Length];
                    for (int i = 0; i < part.Length; i++)
                    {
                        part[i] = Bodies[i];
                    }

                    for (int i = 0; i < part.Length; i++)
                    {
                        for (int a = i + 1; a < part.Length; a++)
                        {
                            if (part[a].level < part[i].level)
                            {
                                PartInfo temp = part[a];
                                part[a] = part[i];
                                part[i] = temp;
                            }
                        }
                    }
                    break;
                }
            case Part.Type.Engine:
                {
                    part = new PartInfo[Engines.Length];
                    for (int i = 0; i < part.Length; i++)
                    {
                        part[i] = Engines[i];
                    }

                    for (int i = 0; i < part.Length; i++)
                    {
                        for (int a = i + 1; a < part.Length; a++)
                        {
                            if (part[a].level < part[i].level)
                            {
                                PartInfo temp = part[a];
                                part[a] = part[i];
                                part[i] = temp;
                            }
                        }
                    }
                    break;
                }
            case Part.Type.Gun:
                {
                    part = new PartInfo[Guns.Length];
                    for (int i = 0; i < part.Length; i++)
                    {
                        part[i] = Guns[i];
                    }

                    for (int i = 0; i < part.Length; i++)
                    {
                        for (int a = i + 1; a < part.Length; a++)
                        {
                            if (part[a].level < part[i].level)
                            {
                                PartInfo temp = part[a];
                                part[a] = part[i];
                                part[i] = temp;
                            }
                        }
                    }
                    break;
                }
            case Part.Type.Tower:
                {
                    part = new PartInfo[Towers.Length];
                    for (int i = 0; i < part.Length; i++)
                    {
                        part[i] = Towers[i];
                    }

                    for (int i = 0; i < part.Length; i++)
                    {
                        for (int a = i + 1; a < part.Length; a++)
                        {
                            if (part[a].level < part[i].level)
                            {
                                PartInfo temp = part[a];
                                part[a] = part[i];
                                part[i] = temp;
                            }
                        }
                    }
                    break;
                }
            case Part.Type.Track:
                {
                    part = new PartInfo[Tracks.Length];
                    for (int i = 0; i < part.Length; i++)
                    {
                        part[i] = Tracks[i];
                    }

                    for (int i = 0; i < part.Length; i++)
                    {
                        for (int a = i + 1; a < part.Length; a++)
                        {
                            if (part[a].level < part[i].level)
                            {
                                PartInfo temp = part[a];
                                part[a] = part[i];
                                part[i] = temp;
                            }
                        }
                    }
                    break;
                }
        }
        return part;
    }
    [Header("-------Parts-------")]
    public PartInfo[] Bodies;
    public PartInfo[] Towers;
    public PartInfo[] Tracks;
    public PartInfo[] Guns;
    public PartInfo[] Engines;
    public PartInfo[] Electro;
    [Header("-------TempPlayerTank-------")]
    public TankBuildTemp PlayersTank;
    [Header("-------GlobalSettings-------")]
    public HouseBuildData[] HouseBuild;
    public static HouseBuildData[] Builds;
    [Range(0, 1f)]
    public float GameHardLevel;
    public static float HardLevel;
    public bool EffectEnable;
    public static bool EffectOn;
    public int TargetFPS;
    public float DamageCoeficent;
    public static float DamageC = 0;
    public float MaximumSpeed;
    public static float MaxSpeed = 75;
    public float BulletSpeed;
    public static float BulletSpeedC;
    public float LinerDrag = 1;
    public static float LinerDragC;
    public AnimationCurve WheelFixTurn;
    public static AnimationCurve FixTurn;
    [Header("-------EffectsMassive-------")]
    public ParticleEffect MyParticleSystem;
    public static ParticleEffect Particle;
    public ParticleSystem FireEffectParticle;
    public static ParticleSystem FireEffect;
    public ParticleSystem FireRocketEffectParticle;
    public static ParticleSystem FireRocketEffect;
    public ParticleSystem SmokeEffectParticle;
    public static ParticleSystem SmokeEffect;
    public ParticleSystem ArmorHitParticle;
    public static ParticleSystem ArmorHit;
    public ParticleSystem PartDestroyedParticle;
    public static ParticleSystem PartDestoryed;
    public ParticleSystem ExplosiveParticle;
    public static ParticleSystem Explosive;
    public ParticleSystem BulletExplosiveParticle;
    public static ParticleSystem BulletExplosive;
    public ParticleSystem ExplosiveSmokeParticle;
    public static ParticleSystem ExplosiveSmoke;
    public ParticleSystem ObjectDestoryedParticle;
    public static ParticleSystem ObjectDestoryed;
    [Header("-------ColorMassive-------")]
    public Color[] PartsColor;
    public static Color[] Colors;
    [Header("-------CrossHairs------")]
    public GameObject Crosshair;
    public static GameObject CrossHair;
    public static Color GetRandColor()
    {
        return Colors[Random.Range(0, Colors.Length)];
    }
    public Part GetPart(int Index, Part.Type Type)
    {
        switch (Type)
        {
            case Part.Type.Body:
                return Bodies[Index].part;
            case Part.Type.Engine:
                return Engines[Index].part;
            case Part.Type.Gun:
                return Guns[Index].part;
            case Part.Type.Tower:
                return Towers[Index].part;
            case Part.Type.Track:
                return Tracks[Index].part;
        }
        return null;
    }
    public PartInfo GetPartInfo(int Index, Part.Type Type)
    {
        switch (Type)
        {
            case Part.Type.Body:
                return Bodies[Index];
            case Part.Type.Engine:
                return Engines[Index];
            case Part.Type.Gun:
                return Guns[Index];
            case Part.Type.Tower:
                return Towers[Index];
            case Part.Type.Track:
                return Tracks[Index];
        }
        return new PartInfo();
    }
    public TankInfo GetTankInfo(int Index)
    {
        return Tanks[Index];
    }
    public void SavePlayerTank(Tank tank)
    {
        StartCoroutine(SavePlayerTankCour(tank));
    }
    private IEnumerator SavePlayerTankCour(Tank tank)
    {
        if (tank.gameObject.scene.IsValid())
        {
            PlayersTank = new TankBuildTemp(tank);
        }
        else
        {
            Tank temptank = Instantiate(tank);
            temptank.Initialized = false;
            temptank.InitializeParts();
            yield return new WaitForFixedUpdate();
            PlayersTank = new TankBuildTemp(temptank);
            Destroy(temptank.gameObject);
        }
    }

    public void SetSettings()
    {
        EffectOn = EffectEnable;
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = TargetFPS;
        DamageC = DamageCoeficent;
        MaxSpeed = MaximumSpeed;
        BulletSpeedC = BulletSpeed;
        LinerDragC = LinerDrag;
        FixTurn = new AnimationCurve(WheelFixTurn.keys);
        Builds = HouseBuild;
        HardLevel = GameHardLevel;

        Particle = MyParticleSystem;
        FireEffect = FireEffectParticle;
        FireRocketEffect = FireRocketEffectParticle;
        SmokeEffect = SmokeEffectParticle;
        ArmorHit = ArmorHitParticle;
        Explosive = ExplosiveParticle;
        PartDestoryed = PartDestroyedParticle;
        ExplosiveSmoke = ExplosiveSmokeParticle;
        ObjectDestoryed = ObjectDestoryedParticle;
        BulletExplosive = BulletExplosiveParticle;

        Colors = PartsColor;

        CrossHair = Crosshair;
    }
    public void Save()
    {
        SaveSystem.Save(this);
    }
    public void Load()
    {
        SaveData data = SaveSystem.Load();
        if(data != null)
        {
            PlayersTank = data.TankBuild;
        }
        else
        {
            SavePlayerTank(Tanks[0].tank);
        }
    }


    private void Awake()
    {
        active = this;
        SetSettings();
        Load();
    }
    private void FixedUpdate()
    {
        if(Input.GetKey(KeyCode.T))
        {
            InBuild = !InBuild;
        }
    }

}


[System.Serializable]
public struct BuildPartTemp
{
    public int PartIndex;
    public int PartType;
    public int[] SubLinks;
    public int ParentLinkIndex;
    public bool Static;
    public float[] color;
    public float[] Position;

    public BuildPartTemp(Part part)
    {
        PartIndex = part.Index;
        PartType = (int)part.PartType;
        color = new float[3]
        {part.PartColor.r,
         part.PartColor.g,
         part.PartColor.b};
        Static = part.ParentLink.Static;
        Vector2 Offset = part.transform.position - part.Parent.transform.position;
        Position = new float[2] { Offset.x, Offset.y };

        if (part.PartType == Part.Type.Body)
        {
            ParentLinkIndex = -1;
            SubLinks = part.SubLinks;
        }
        else if(part.ParentLink.Parent.PartType == Part.Type.Body)
        {
            ParentLinkIndex = -1;
            SubLinks = part.SubLinks;
        }
        else
        {
            ParentLinkIndex = part.ParentLink.Parent.NumInLinks;
            SubLinks = part.SubLinks;
        }

    }
}
[System.Serializable]
public struct TankBuildTemp
{
    public string Name;
    public float[] color; 

    public BuildPartTemp Body;
    public BuildPartTemp[] Towers;
    public BuildPartTemp[] Guns;
    public BuildPartTemp[] Engines;
    public BuildPartTemp[] LeftTracks;
    public BuildPartTemp[] RightTracks;
    public BuildPartTemp[] Electro;

    public TankBuildTemp(Tank tank)
    {
        Name = tank.Name;
        color = new float[3]
        {
            tank.TankColor.r,
            tank.TankColor.g,
            tank.TankColor.b
        };

        Body = new BuildPartTemp(tank.Main.part);

        Towers = new BuildPartTemp[tank.Tower.Length];
        for(int i = 0; i < tank.Tower.Length; i++)
        {
            Towers[i] = new BuildPartTemp(tank.Tower[i].part);
        }
        Guns = new BuildPartTemp[tank.Gun.Length];
        for (int i = 0; i < tank.Gun.Length; i++)
        {
            Guns[i] = new BuildPartTemp(tank.Gun[i].part);
        }
        Engines = new BuildPartTemp[tank.Engine.Length];
        for (int i = 0; i < tank.Engine.Length; i++)
        {
            Engines[i] = new BuildPartTemp(tank.Engine[i].part);
        }
        LeftTracks = new BuildPartTemp[tank.LeftTrack.Length];
        for (int i = 0; i < tank.LeftTrack.Length; i++)
        {
            LeftTracks[i] = new BuildPartTemp(tank.LeftTrack[i].part);
        }
        RightTracks = new BuildPartTemp[tank.RightTrack.Length];
        for (int i = 0; i < tank.RightTrack.Length; i++)
        {
            RightTracks[i] = new BuildPartTemp(tank.RightTrack[i].part);
        }
        Electro = new BuildPartTemp[0];
    }
}

[System.Serializable]
public struct PartInfo
{
    public enum NationType {General, Soviet, German, USA};
    public NationType Nation;
    public string Name;
    public int index;
    public int level;
    public Part part;
    public Color color;

    public BuildInfo buildInfo;
}
[System.Serializable]
public struct TankInfo
{
    public enum NationType { General, Soviet, German, USA };
    public NationType Nation;
    public string Name;
    public float level;
    public Tank tank;
    public Color color;

    public BuildInfo buildInfo;
}
[System.Serializable]
public struct BuildInfo
{
    public int Cost;
    public int CountOwn;
    public int CountOnScene;

    public bool Own()
    {
        return CountOwn > 0;
    }
    public void Buy()
    {
        CountOwn++;
    }
    public bool CanSpawn()
    {
        return CountOwn - CountOnScene > 0;
    }
    public void Spawn()
    {
        CountOnScene++;
    }
    public void Return()
    {
        CountOnScene--;
    }
}

[System.Serializable]
public struct GamePlay
{
    public GameTeams[] Teams;
    public GameLevel[] Levels;
    public BattleRoyale[] Royales;
}

[System.Serializable]
public struct GameTeams
{
    public string Name;
    public Enemy[] RedTeam;
    public Enemy[] BlueTeam;
}
[System.Serializable]
public struct GameLevel
{
    public string Name;
    public Enemy[] EnemyOwn;
    public bool HaveBoss;
    public Enemy Boss;
}
[System.Serializable]
public struct BattleRoyale
{
    public string Name;
    public Enemy[] EnemyOwn;
}
[System.Serializable]
public struct Enemy
{
    public string Name;
    public Tank tank;
    public int index;

    public Enemy(string Name, Tank tank)
    {
        this.Name = Name;
        this.tank = tank;
        this.index = -1;
    }
    public Enemy(TankInfo info)
    {
        this.Name = info.Name;
        this.tank = info.tank;
        this.index = -1;
    }
}

[System.Serializable]
public struct HousePart
{
    public enum Type {Left, Right, Top, Botton, Center};
    public Type ObjType;
    public ObjectPart Part;
}
[System.Serializable]
public struct HouseBuildData
{
    public ObjectPart LeftTop;
    public ObjectPart RightTop;
    
    public ObjectPart CenterLeft;
    public ObjectPart CenterRight;
    public ObjectPart Left;
    public ObjectPart Right;
    public ObjectPart Top;
    public ObjectPart Bottom;
    public ObjectPart Special;

    public ObjectPart Floor;
    public ObjectPart FloorLeft;
    public ObjectPart FloorRight;
    public ObjectPart FloorTop;
    public ObjectPart FloorBottom;
}