using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    public string Name;
    public int Level;
    public int Hp;
    private int MaxHp;
    public float Mass;
    public bool Destroyed;

    public Track.MoveType MovementType;
    private bool WheelMove()
    {
        return MovementType == Track.MoveType.Wheel;
    }
    [Range(0, 5f)]
    public float MaxSpeed;
    [Range(0, 2f)]
    public float Acceleration;
    [Range(0, 2f)]
    public float RotationSpeed;
    [Range(0, 2f)]
    public float LinerDrag;
    [Range(0, 2f)]
    public float Slip;

    public const float RigAcceleration = 50f;

    [HideInInspector]
    public bool Builded;
    [HideInInspector]
    public bool Initialized;

    public Color TankColor;
    public float Lenght()
    {
        return Main.part.Lenght();
    }

    public bool SpecialVehicle;

    public float Power;
    private float Gaz;
    private float Brake;
    private float Turning;
    private bool Firing;
    private bool SpecAction;
    private float Drift()
    {
        float Scale = Mathf.Abs(1 - Mathf.Abs(Vector2.Dot(Rig.velocity.normalized, transform.up)));
        return Scale * Mathf.Sqrt(Mathf.Abs(Power));
    }
    private float Speed()
    {
        return Rig.velocity.magnitude / RigAcceleration;
    }
    private float FixTurn()
    {
        float Velocity = Speed() * 0.9f;
        Velocity = Velocity > 1 ? 1 : Velocity;

        return GameData.FixTurn.Evaluate(Velocity);
    }
    private float Scrolling()
    {
        float Dot = Mathf.Abs(Vector2.Dot(transform.up, Rig.velocity.normalized));
        float ThisSlip = Mathf.Abs(Power * RigAcceleration - Rig.velocity.magnitude) * Acceleration / (Rig.velocity.magnitude * 1f + 0.25f) * Slip * Dot;
        return ThisSlip < 2 ? ThisSlip : 2;
    }
    public float InOther;
    public float Clutch;
    public float MaxTrackSpeed;
    public float TrackDiff;
    public float MassLoad;
    public float Radius;

    private float GetClutch(TankPart part)
    {
        float LandClutch = GetLandClutch(part.part.transform.position);
        float TrackClutch = part.track.Clutch;
        return (LandClutch + TrackClutch) / 2;
    }
    private float GetMaxSpeed(TankPart part)
    {
        float LandSpeed = GetLandSpeed(part.part.transform.position);
        float TrackSpeed = part.track.MaxSpeed;
        return (LandSpeed * TrackSpeed);
    }
    public TankPart Main;
    public TankPart[] Tower;
    private float AngleFromVector(Vector2 Dir)
    {
        int Right = Vector2.Dot(transform.right, Dir) < 0 ? 1 : -1;
        if(Dir.x <= 0)
        {
            return Right * Vector2.Angle(transform.up, Dir);
        }
        else
        {
            return Right * Vector2.Angle(transform.up, Dir);
        }
    }
    private Vector2 GetTowerDirection(Vector2 Direction, int tower)
    {
        if(AngleFromVector(Direction) > Main.body.TowersLimit[tower].MaxAngle)
        {
            return new Vector2(Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.z + 90 + Main.body.TowersLimit[tower].MaxAngle)), Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.z + 90 + Main.body.TowersLimit[tower].MaxAngle)));
        }
        else if(AngleFromVector(Direction) < Main.body.TowersLimit[tower].MinAngle)
        {
            return new Vector2(Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.z + 90 + Main.body.TowersLimit[tower].MinAngle)), Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.z + 90 + Main.body.TowersLimit[tower].MinAngle)));
        }
        else
        {
            return Direction;
        }
    }
    public TankPart[] Gun;
    public bool GunsReady()
    {
        for(int i = 0; i < Gun.Length; i++)
        {
            if (!Gun[i].cannon.Reloaded)
                return false;
        }
        return true;
    }
    public TankPart[] Engine;
    public TankPart[] LeftTrack;
    public TankPart[] RightTrack;
    public TankPart[] Special;

    public Rigidbody2D Rig;
    private Vector2 GravityForce;
    public SideOwn Side;
    public TerrainSurface Terrain;

    private void Awake()
    {
        Rig = GetComponent<Rigidbody2D>();
    }
    public void Start()
    {
        StartCoroutine(OnStart());
    }
    public IEnumerator OnStart()
    {
        if (!Builded)
        {
            Radius = Main.part.GetComponent<SpriteRenderer>().bounds.size.y;
            Side = GetComponent<SideOwn>();
            yield return new WaitForFixedUpdate();
            InitializeParts();
            Builded = true;
        }
        yield return new WaitForFixedUpdate();
        GetTerrainCoord(transform.position);
        yield break;
    }
    public void InitializeParts()
    {
        if (Initialized)
            return;
        Main.part.Parent = this;
        Main = new TankPart(Main.part);
        for(int i = 0; i < Main.part.Links.Length; i++)
        {
            if (Main.part.Links[i].Point.childCount > 0)
            {
                Part part = Main.part.Links[i].Point.GetChild(0).GetComponent<Part>();
                Part.Linking[] Links = PartManipulation.GetLinksForPins(transform, part, Main.part.Links);
                PartManipulation.InstallPart(this, part, Links);
                for(int a = 0; a < part.Links.Length; a++)
                {
                    if (part.Links[a].Point.childCount > 0)
                    {
                        Part SecondPart = part.Links[a].Point.GetChild(0).GetComponent<Part>();
                        Part.Linking[] SecondLinks = PartManipulation.GetLinksForPins(transform, SecondPart, part.Links);
                        PartManipulation.InstallPart(this, SecondPart, SecondLinks);

                    } 
                }
            }
        }
        RecalculateTank();
        SetColor();
        Initialized = true;
    }
    public void ClearParts()
    {
        Gun = new TankPart[0];
        Tower = new TankPart[0];
        RightTrack = new TankPart[0];
        LeftTrack = new TankPart[0];
        Engine = new TankPart[0];
        Special = new TankPart[0];
    }
    public void AddRig()
    {
        if(Rig == null)
        {
            Rig = gameObject.AddComponent<Rigidbody2D>();
            Rig.gravityScale = 0;
            Rig.angularDrag = 0.5f;
            Rig.drag = 1f;
        }
    }
    public void AddPlayerTrigger()
    {
        CircleCollider2D circle = gameObject.AddComponent<CircleCollider2D>();
        circle.isTrigger = true;
        circle.radius = 2;
    }
    public void SetColor(float[] color)
    {
        Color thisColor = new Color(color[0], color[1], color[2]);
        Main.part.SetColor(thisColor);
        for (int i = 0; i < Gun.Length; i++)
        {
            Gun[i].part.SetColor(thisColor);
        }
        for (int i = 0; i < Tower.Length; i++)
        {
            Tower[i].part.SetColor(thisColor);
        }
        for (int i = 0; i < RightTrack.Length; i++)
        {
            RightTrack[i].part.SetColor(thisColor);
        }
        for (int i = 0; i < LeftTrack.Length; i++)
        {
            LeftTrack[i].part.SetColor(thisColor);
        }
        for (int i = 0; i < Engine.Length; i++)
        {
            Engine[i].part.SetColor(thisColor);
        }
        for(int i = 0; i < Special.Length; i++)
        {
            Special[i].part.SetColor(thisColor);
        }
    }
    public void SetColor()
    {
        Main.part.SetColor(TankColor);
        for (int i = 0; i < Gun.Length; i++)
        {
            Gun[i].part.SetColor(TankColor);
        }
        for (int i = 0; i < Tower.Length; i++)
        {
            Tower[i].part.SetColor(TankColor);
        }
        for (int i = 0; i < RightTrack.Length; i++)
        {
            RightTrack[i].part.SetColor(TankColor);
        }
        for (int i = 0; i < LeftTrack.Length; i++)
        {
            LeftTrack[i].part.SetColor(TankColor);
        }
        for (int i = 0; i < Engine.Length; i++)
        {
            Engine[i].part.SetColor(TankColor);
        }
        for (int i = 0; i < Special.Length; i++)
        {
            Special[i].part.SetColor(TankColor);
        }
    }
    public void SetColor(Color color)
    {
        TankColor = color;
        Main.part.SetColor(TankColor);
        for (int i = 0; i < Gun.Length; i++)
        {
            Gun[i].part.SetColor(TankColor);
        }
        for (int i = 0; i < Tower.Length; i++)
        {
            Tower[i].part.SetColor(TankColor);
        }
        for (int i = 0; i < RightTrack.Length; i++)
        {
            RightTrack[i].part.SetColor(TankColor);
        }
        for (int i = 0; i < LeftTrack.Length; i++)
        {
            LeftTrack[i].part.SetColor(TankColor);
        }
        for (int i = 0; i < Engine.Length; i++)
        {
            Engine[i].part.SetColor(TankColor);
        }
        for (int i = 0; i < Special.Length; i++)
        {
            Special[i].part.SetColor(TankColor);
        }
    }

    public void OnInstallPart(Part part)
    {
        switch (part.PartType)
        {
            case Part.Type.Body:
                Main = new TankPart(part);
                break;
            case Part.Type.Tower:
                TankPart[] tempTower = Tower;
                Tower = new TankPart[tempTower.Length + 1];
                for(int i = 0; i < tempTower.Length; i++)
                {
                    Tower[i] = tempTower[i];
                }
                Tower[Tower.Length - 1] = new TankPart(part);
                
                break;
            case Part.Type.Gun:
                TankPart[] tempGun = Gun;
                Gun = new TankPart[tempGun.Length + 1];
                for (int i = 0; i < tempGun.Length; i++)
                {
                    Gun[i] = tempGun[i];
                }
                
                Gun[Gun.Length - 1] = new TankPart(part);

                break;
            case Part.Type.Engine:
                TankPart[] tempEngine = Engine;
                Engine = new TankPart[tempEngine.Length + 1];
                for (int i = 0; i < tempEngine.Length; i++)
                {
                    Engine[i] = tempEngine[i];
                }
                Engine[Engine.Length - 1] = new TankPart(part);
                break;
            case Part.Type.Track:
                if(part.ParentLink.Right)
                {
                    TankPart[] tempTrack = RightTrack;
                    RightTrack = new TankPart[tempTrack.Length + 1];
                    for (int i = 0; i < tempTrack.Length; i++)
                    {
                        RightTrack[i] = tempTrack[i];
                    }
                    RightTrack[RightTrack.Length - 1] = new TankPart(part);
                }
                else
                {
                    TankPart[] tempTrack = LeftTrack;
                    LeftTrack = new TankPart[tempTrack.Length + 1];
                    for (int i = 0; i < tempTrack.Length; i++)
                    {
                        LeftTrack[i] = tempTrack[i];
                    }
                    LeftTrack[LeftTrack.Length - 1] = new TankPart(part);
                }
                StartCoroutine(ActivateEffects(part));
                break;
            case Part.Type.Special:
                TankPart[] tempSpecial = Special;
                Special = new TankPart[tempSpecial.Length + 1];
                for (int i = 0; i < tempSpecial.Length; i++)
                {
                    Special[i] = tempSpecial[i];
                }
                Special[Special.Length - 1] = new TankPart(part);
                break;
        }
        if(Builded)
        {
            RecalculateTank();
        }
    }
    private IEnumerator ActivateEffects(Part part)
    {
        yield return new WaitForFixedUpdate();
        for (int i = 0; i < part.SubLinks.Length; i++)
        {
            for (int a = 0; a < Main.part.Links[part.SubLinks[i]].Effects.Length; a++)
            {
                Main.part.Links[part.SubLinks[i]].Effects[a].SetActive(true);
            }
        }
        yield break;
    }
    public void OnDeInstallPart(Part part)
    {
        switch (part.PartType)
        {
            case Part.Type.Tower:
                TankPart[] tempTower = Tower;
                Tower = new TankPart[tempTower.Length - 1];
                for(int i = 0; i < tempTower.Length - 1; i++)
                {
                    if(tempTower[i].part == part)
                    {
                        tempTower[i] = tempTower[tempTower.Length - 1];
                    }
                    Tower[i] = tempTower[i];
                }
                break;
            case Part.Type.Gun:
                TankPart[] tempGun = Gun;
                if(tempGun.Length - 1 >= 0)
                    Gun = new TankPart[tempGun.Length - 1];
                else
                    Gun = new TankPart[0];
                for (int i = 0; i < tempGun.Length - 1; i++)
                {
                    if (tempGun[i].part == part)
                    {
                        tempGun[i] = tempGun[tempGun.Length - 1];
                    }
                    Gun[i] = tempGun[i];
                }
                break;
            case Part.Type.Engine:
                TankPart[] tempEngine = Engine;
                Engine = new TankPart[tempEngine.Length - 1];
                for (int i = 0; i < tempEngine.Length - 1; i++)
                {
                    if (tempEngine[i].part == part)
                    {
                        tempEngine[i] = tempEngine[tempEngine.Length - 1];
                    }
                    Engine[i] = tempEngine[i];
                }
                break;
            case Part.Type.Track:
                for (int i = 0; i < part.SubLinks.Length; i++)
                {
                    for(int a = 0; a < Main.part.Links[part.SubLinks[i]].Effects.Length; a++)
                    {
                        Main.part.Links[part.SubLinks[i]].Effects[a].SetActive(false);
                    }
                }
                if (part.ParentLink.Parent.Links[part.NumInLinks].Right)
                {
                    TankPart[] tempTrack = RightTrack;
                    RightTrack = new TankPart[tempTrack.Length - 1];
                    for (int i = 0; i < tempTrack.Length - 1; i++)
                    {
                        if (tempTrack[i].part == part)
                        {
                            tempTrack[i] = tempTrack[tempTrack.Length - 1];
                        }
                        RightTrack[i] = tempTrack[i];
                    }
                }
                else
                {
                    TankPart[] tempTrack = LeftTrack;
                    LeftTrack = new TankPart[tempTrack.Length - 1];
                    for (int i = 0; i < tempTrack.Length - 1; i++)
                    {
                        if (tempTrack[i].part == part)
                        {
                            tempTrack[i] = tempTrack[tempTrack.Length - 1];
                        }
                        LeftTrack[i] = tempTrack[i];
                    }
                }
                break;
            case Part.Type.Special:
                TankPart[] tempSpecial = Special;
                Special = new TankPart[tempSpecial.Length - 1];
                for (int i = 0; i < tempSpecial.Length - 1; i++)
                {
                    if (tempSpecial[i].part == part)
                    {
                        tempSpecial[i] = tempSpecial[tempSpecial.Length - 1];
                    }
                    Special[i] = tempSpecial[i];
                }
                break;
        }
        RecalculateTank();
    }

    public int GetTowerIndex(Part gun)
    {
        for (int i = 0; i < Tower.Length; i++)
        {
            for (int a = 0; a < Tower[i].part.Links.Length; a++)
            {
                if (Tower[i].part.Links[a].InstalledPart == gun)
                    return i;
            }
        }
        return -1;
    }

    public void MakeLikeImage(Transform Pos)
    {
        Builded = true;
        Destroy(GetComponent<AiController>());
        InitializeParts();
        transform.localScale = Vector3.one * 6.5f / Lenght();
        if (Main.NotNull)
        {
            Main.part.Sprite.sortingOrder = 2;
            Main.part.SetCollision(false);
            for(int a = 0; a < Main.part.PartPlace.Length; a++)
            {
                Main.part.PartPlace[a].sortingOrder = 2;
            }
        }
        for (int i = 0; i < Gun.Length; i++)
        {
            Gun[i].part.Sprite.sortingOrder = 2;
            Gun[i].part.SetCollision(false);
            for(int a = 0; a < Gun[i].part.PartPlace.Length; a++)
            {
                Gun[i].part.PartPlace[a].sortingOrder = 2;
            }
        }
        for (int i = 0; i < Tower.Length; i++)
        {
            Tower[i].part.Sprite.sortingOrder = 2;
            Tower[i].part.SetCollision(false);
            for (int a = 0; a < Tower[i].part.PartPlace.Length; a++)
            {
                Tower[i].part.PartPlace[a].sortingOrder = 2;
            }
        }
        for (int i = 0; i < RightTrack.Length; i++)
        {
            RightTrack[i].part.Sprite.sortingOrder = 2;
            RightTrack[i].part.SetCollision(false);
            for (int a = 0; a < RightTrack[i].part.PartPlace.Length; a++)
            {
                RightTrack[i].part.PartPlace[a].sortingOrder = 2;
            }
        }
        for (int i = 0; i < LeftTrack.Length; i++)
        {
            LeftTrack[i].part.Sprite.sortingOrder = 2;
            LeftTrack[i].part.SetCollision(false);
            for (int a = 0; a < LeftTrack[i].part.PartPlace.Length; a++)
            {
                LeftTrack[i].part.PartPlace[a].sortingOrder = 2;
            }
        }
        for (int i = 0; i < Engine.Length; i++)
        {
            Engine[i].part.Sprite.sortingOrder = 2;
            Engine[i].part.SetCollision(false);
            for (int a = 0; a < Engine[i].part.PartPlace.Length; a++)
            {
                Engine[i].part.PartPlace[a].sortingOrder = 2;
            }
        }

        Rig.bodyType = RigidbodyType2D.Static;
        transform.position = Pos.position;
        transform.rotation = Pos.rotation;
        transform.parent = Pos;
    }
    public void GetHit(Tank Enemy, Part.Type PartType)
    {
        Hp = CalculateHp();
        GetEnemy(Enemy);
        if(Hp <= 0)
        {
            DestroyTank();
        }
        else if(PartType == Part.Type.Body &&
        Main.NotNull && Random.Range(0, 100) < Main.body.BoomChanse)
        {
            DestroyTank();
        }
    }
    public void GetForce(Vector2 Position, Vector2 Direction, float BulletMass)
    {
        Vector2 DirBul = ((Vector2)transform.position - Position).normalized;
        int Right = Vector2.Dot(DirBul, transform.right) > 0 ? 1 : -1;
        int Up = Vector2.Dot(DirBul, transform.up) > 0 ? 1 : -1;
        float Power = Mathf.Sqrt(((Vector2)transform.position - Position).magnitude);
        float AngularDot = Mathf.Pow(Vector2.Dot(transform.right, Direction), 2);
        float Angular = Up * Right / Power * AngularDot;
        Rig.angularVelocity += Angular * BulletMass / Mass * 360f;
        Rig.velocity += Direction / Power * BulletMass / Mass * 5f;
    }
    public bool isBodyClear()
    {
        return Tower.Length == 0 && Engine.Length == 0 && RightTrack.Length == 0 && LeftTrack.Length == 0;
    }
    public int CalculateHp()
    {
        int Hp = 0;
        if (Main.NotNull)
        {
            Hp = Main.part.Hp;
        }
        /*
        for (int i = 0; i < Gun.Length; i++)
        {
            Hp += Gun[i].part.Hp;
        }
        for (int i = 0; i < Tower.Length; i++)
        {
            Hp += Tower[i].part.Hp;
        }
        for (int i = 0; i < RightTrack.Length; i++)
        {
            Hp += RightTrack[i].part.Hp;
        }
        for (int i = 0; i < LeftTrack.Length; i++)
        {
            Hp += LeftTrack[i].part.Hp;
        }
        for (int i = 0; i < Engine.Length; i++)
        {
            Hp += Engine[i].part.Hp;
        }
        */
        return Hp;
    }
    public int CalculateMaxHp()
    {
        int Hp = 0;
        if(Main.NotNull)
        {
            Hp = Main.part.MaxHp;
        }
        /*
        for (int i = 0; i < Gun.Length; i++)
        {
            Hp += Gun[i].part.MaxHp;
        }
        for (int i = 0; i < Tower.Length; i++)
        {
            Hp += Tower[i].part.MaxHp;
        }
        for (int i = 0; i < RightTrack.Length; i++)
        {
            Hp += RightTrack[i].part.MaxHp;
        }
        for (int i = 0; i < LeftTrack.Length; i++)
        {
            Hp += LeftTrack[i].part.MaxHp;
        }
        for (int i = 0; i < Engine.Length; i++)
        {
            Hp += Engine[i].part.MaxHp;
        }
        */
        return Hp;
    }
    public float CalculateMass()
    {
        float Mass = Main.part.Mass;
        for (int i = 0; i < Gun.Length; i++)
        {
            Mass += Gun[i].part.Mass;
        }
        for (int i = 0; i < Tower.Length; i++)
        {
            Mass += Tower[i].part.Mass;
        }
        for (int i = 0; i < RightTrack.Length; i++)
        {
            Mass += RightTrack[i].part.Mass;
        }
        for (int i = 0; i < LeftTrack.Length; i++)
        {
            Mass += LeftTrack[i].part.Mass;
        }
        for (int i = 0; i < Engine.Length; i++)
        {
            Mass += Engine[i].part.Mass;
        }
        return Mass;
    }
    public float[] CalculateMaxSpeedAndAcceleration()
    {
        float EngineMaxSpeed = 0;
        float EngineHorsePower = 0;
        float MaxMass = 0;
        int EngineNum = Engine.Length;
        for(int i = 0; i < EngineNum; i++)
        {
            EngineHorsePower += Engine[i].engine.HorsePower;
            MaxMass += Engine[i].engine.MaxMass;
        }
        for(int i = 0; i < EngineNum; i++)
        {
            EngineMaxSpeed += Engine[i].engine.MaxSpeed / EngineNum;
        }
        if (Main.NotNull)
        {
            EngineHorsePower = EngineHorsePower < Main.body.MaxEnginePower ? EngineHorsePower : Main.body.MaxEnginePower;
        }
        float MaxSpeed = EngineMaxSpeed * ((MaxMass / Mass) < 1 ? (MaxMass / Mass) : 1);
        float Acceleration = Mathf.Sqrt(EngineHorsePower / Mass * 0.03f); 

        return new float[]{MaxSpeed, Acceleration};

    }
    public void RecalculateTank()
    {
        if(Rig == null)
        {
            Rig = GetComponent<Rigidbody2D>();
        }
        if(Radius == 0)
        {
            Radius = Main.part.GetComponent<SpriteRenderer>().bounds.size.y;
        }
        if (Side == null)
        {
            Side = GetComponent<SideOwn>();
        }
        MaxHp = CalculateMaxHp();
        Hp = CalculateHp();
        Mass = CalculateMass();
        Rig.mass = Mass;
        float[] SpeedAndAcceleration = CalculateMaxSpeedAndAcceleration();
        MaxSpeed = SpeedAndAcceleration[0];
        Acceleration = SpeedAndAcceleration[1];
    }

    public float SetPartHp()
    {
        if (MaxHp == 0)
            return 1;
        return (float)Hp / (float)MaxHp;
    }
    public void OnReplacingBody(bool End)
    {
        
    }
    public void DestroyTank()
    {
        for (int i = 0; i < Main.part.Links.Length; i++)
        {
            if (Main.part.Links[i].Installed && !Main.part.Links[i].Static)
            {
                Part partFirst = Main.part.Links[i].InstalledPart;
                float PartMass = partFirst.Mass;
                partFirst.AddForce(Rig.velocity, 0);
                partFirst.OnPartDestroyed(Mathf.Pow(Mass / (PartMass + 1), 0.5f));
            }
            else if(Main.part.Links[i].Static && Main.part.Links[i].Installed)
            {
                for (int a = 0; a < Main.part.Links[i].InstalledPart.Links.Length; a++)
                {
                    if (Main.part.Links[i].InstalledPart.Links[a].Installed)
                    {
                        float PartMass = Main.part.Links[i].InstalledPart.Links[a].InstalledPart.Mass;
                        Main.part.Links[i].InstalledPart.Links[a].InstalledPart.OnPartDestroyed(Mathf.Pow(Mass / (PartMass + 1), 0.5f));
                    }
                }
            }
        }
        Main.part.OnPartDestroyed(0);
        Special = new TankPart[0];
        Tower = new TankPart[0];
        Gun = new TankPart[0];
        Engine = new TankPart[0];
        RightTrack = new TankPart[0];
        LeftTrack = new TankPart[0];
        Main = new TankPart();
        OnTankDestroyed();
    }
    public void OnTankDestroyed()
    {
        ExplosiveEffect();
        Destroyed = true;
        if(SpecialVehicle)
        {
            Destroy(gameObject);
        }
    }
    private void ExplosiveEffect()
    {
        Vector3 Position = new Vector3(transform.position.x, transform.position.y, 0);
        ParticleEffect Effect = Instantiate(GameData.Particle, Position, transform.rotation, null);
        Effect.VehicleExplosive(Mass / 10);
    }
    private void RideEffect(ParticleSystem particle, float Power)
    {
        
        if (particle == null || !GameData.EffectOn)
            return;
        bool On = Power > 0.01f;
        particle.gameObject.SetActive(On);
        var Main = particle.main;
        Main.startSize = new ParticleSystem.MinMaxCurve(0.25f * Power, 0.5f * Power);
        Color color = GetLandColor(particle.transform.position);
        Main.startColor = color * 0.75f;
        Main.startSpeed = new ParticleSystem.MinMaxCurve(0 * Power, 10 * Power);
        var Emission = particle.emission;
        Emission.rateOverTime = 50 * Power;
    }
    private float RideEffectPower()
    {
        float Scale = (Scrolling() * 0.5f + Drift()) + Mathf.Sqrt(Power) * Gaz * 1f / 10;
        return Scale > 0.1f ? Scale : Scale * Scale;
    }

    public void Drive(Controller.MoveInput input)
    {
        if (Destroyed || GameData.InBuild)
            return;
        #region Maniputaliton
        if (input.isManipulation)
        {
            Power = Mathf.Lerp(Power, 0, 3f / MassLoad * Clutch / (Mathf.Abs(Power) * 2f + 0.01f) * Time.deltaTime);
            Rig.velocity = Vector2.Lerp(Rig.velocity, Vector2.zero, (Clutch / MaxTrackSpeed) / MassLoad * (Drift() + 1) * 2f * Time.deltaTime);
            Rig.angularVelocity = Mathf.Lerp(Rig.angularVelocity, 0f, 20f * Clutch / (Drift() + 1f) * Time.deltaTime);
            return;
        }
        #endregion
        #region Movement
        Gaz = input.Move > 0 ? input.Move : 0;
        Brake = input.Move < 0 ? input.Move : 0;
        Turning = Mathf.Lerp(Turning, input.Rotation * 30f, 0.1f);
        MovementType = Track.MoveType.Track;
        LinerDrag = GameData.LinerDragC;
        Clutch = 0;
        MaxTrackSpeed = 0;
        MassLoad = 0;
        RotationSpeed = 0;
        TrackDiff = 0;
        Slip = 0;
        float DriveWheel = 0;
        int Back = Power >= 0 ? 1 : -1;

        int TrackNum = LeftTrack.Length + RightTrack.Length;
        for (int i = 0; i < LeftTrack.Length; i++)
        {
            if (LeftTrack[i].track.Move == Track.MoveType.Wheel &&
                LeftTrack[i].part.ParentLink.Front ||
                LeftTrack[i].part.ParentLink.Back)
            {
                DriveWheel++;
                MovementType = Track.MoveType.Wheel;
            }


            float ThisClutch = GetClutch(LeftTrack[i]);
            float ThisMassLoad = Track.GetMassOverLoad(LeftTrack[i].track.MaxMass, Mass / TrackNum);
            float ThisMaxTrackSpeed = GetMaxSpeed(LeftTrack[i]);

            TrackDiff -= ThisMassLoad * ThisClutch / (ThisMaxTrackSpeed + 1) / TrackNum * 0.25f;
            Clutch += ThisClutch / TrackNum;
            MaxTrackSpeed += ThisMaxTrackSpeed / TrackNum;
            MassLoad += ThisMassLoad / TrackNum;
            RotationSpeed += LeftTrack[i].track.RotationSpeed * ThisMassLoad / TrackNum;
        }
        for (int i = 0; i < RightTrack.Length; i++)
        {
            if (RightTrack[i].track.Move == Track.MoveType.Wheel &&
                RightTrack[i].part.ParentLink.Front ||
                RightTrack[i].part.ParentLink.Back)
            {
                DriveWheel++;
                MovementType = Track.MoveType.Wheel;
            }

            float ThisClutch = GetClutch(RightTrack[i]);
            float ThisMassLoad = Track.GetMassOverLoad(RightTrack[i].track.MaxMass, Mass / TrackNum);
            float ThisMaxTrackSpeed = GetMaxSpeed(RightTrack[i]);

            TrackDiff += ThisMassLoad * ThisClutch / (ThisMaxTrackSpeed + 1) / TrackNum * 0.25f;
            Clutch += ThisClutch / TrackNum;
            MaxTrackSpeed += ThisMaxTrackSpeed / TrackNum;
            MassLoad += ThisMassLoad / TrackNum;
            RotationSpeed += RightTrack[i].track.RotationSpeed * ThisMassLoad / TrackNum;
        }
        if (Clutch == 0)
        {
            Clutch = 2;
            MassLoad = 0f;
        }
        Slip = 0.25f / Clutch;
        DriveWheel = 0.5f * Mathf.Sqrt(DriveWheel);
        RotationSpeed *= (Mathf.Sqrt(Mathf.Sqrt(Acceleration)) * 2 * MassLoad);
        float Scroll = Scrolling();

        if (input.Move > 0.1f)
        {
            Power = Mathf.Lerp(Power, input.Move * MaxSpeed * MaxTrackSpeed * MassLoad, Acceleration * GetNormalPower(transform.position) * Mathf.Sqrt(Clutch) * MassLoad / (MaxSpeed + 0.01f) / (Power * 0.5f + 1f) * Time.deltaTime);
        }
        else if (input.Move < -0.1f)
        {
            Power = Mathf.Lerp(Power, input.Move * MaxSpeed * MassLoad * MaxTrackSpeed * 0.25f, Acceleration * GetNormalPower(transform.position) * Mathf.Sqrt(Clutch) * MassLoad / (Scroll * 2f + 1) / (MaxSpeed + 0.01f) * 4 * Time.deltaTime);
        }
        else
        {
            Power = Mathf.Lerp(Power, 0, 3f / (MassLoad + 0.1f) * Mathf.Sqrt(Clutch) / (Mathf.Abs(Power) * 2f + 0.01f) * Time.deltaTime);
        }
        if (Power < 0.001)
        {
            InOther = 0;
        }

        if (Power > MaxSpeed * MaxTrackSpeed)
        {
            Power = Mathf.Lerp(Power, MaxSpeed * MaxTrackSpeed, 0.25f / (1 + MaxTrackSpeed * 10));
        }

        if (WheelMove())
        {
            Power = Mathf.Lerp(Power, 0, Clutch / (MassLoad + 0.1f) * (Drift() * Drift() * 5f + InOther) * Time.deltaTime);
            Rig.velocity = Vector2.Lerp(Rig.velocity, Vector2.zero, (Drift() * 0.5f + LinerDrag + InOther) / (MaxTrackSpeed * 2 + 1f) / (Speed() * 5f + 1) * Mathf.Sqrt(Clutch) / (MassLoad + 0.1f) * 0.25f * Time.deltaTime);
            Rig.velocity = Vector2.Lerp(Rig.velocity, transform.up * Power * GetNormalPower(transform.position) * (1.025f - Mathf.Abs(TrackDiff)) * RigAcceleration, Clutch * (MassLoad + 0.1f) / (Scroll * 2f + 1) / (Drift() * Drift() * (Speed() * 2f + 1) / (Clutch + 0.5f) * 15f + 1f) * 5f * Time.deltaTime);


            Rig.angularVelocity = Mathf.Lerp(Rig.angularVelocity, (input.Rotation * DriveWheel * Back + TrackDiff * Mathf.Sqrt(Mathf.Abs(Power)) * 6f) * FixTurn() * RotationSpeed * Mathf.Sqrt(Clutch) * (1 + Mathf.Abs(Power) * 0.25f) * 60f, Clutch * 10f * Time.deltaTime);
            Rig.centerOfMass = Main.part.transform.localPosition;
            for (int i = 0; i < LeftTrack.Length; i++)
            {
                if (LeftTrack[i].track.Move == Track.MoveType.Wheel)
                {
                    if (LeftTrack[i].part.ParentLink.Front)
                    {
                        LeftTrack[i].part.transform.localRotation = Quaternion.Euler(0, 0, Turning);
                    }
                    else if (LeftTrack[i].part.ParentLink.Back)
                    {
                        LeftTrack[i].part.transform.localRotation = Quaternion.Euler(0, 0, -Turning);
                    }
                }
                RideEffect(LeftTrack[i].part.ParentLink.RideEffect, RideEffectPower());
            }
            for (int i = 0; i < RightTrack.Length; i++)
            {
                if (RightTrack[i].track.Move == Track.MoveType.Wheel)
                {
                    if (RightTrack[i].part.ParentLink.Front)
                    {
                        RightTrack[i].part.transform.localRotation = Quaternion.Euler(0, 0, Turning);
                    }
                    else if (RightTrack[i].part.ParentLink.Back)
                    {
                        RightTrack[i].part.transform.localRotation = Quaternion.Euler(0, 0, -Turning);
                    }
                }
                RideEffect(RightTrack[i].part.ParentLink.RideEffect, RideEffectPower());
            }
        }
        else
        {
            Power = Mathf.Lerp(Power, 0, Clutch / (MassLoad + 0.1f) * (Drift() * 2f) * 1f * Time.deltaTime);
            Rig.velocity = Vector2.Lerp(Rig.velocity, transform.up * Power * GetNormalPower(transform.position) * (1.025f - Mathf.Abs(TrackDiff)) * RigAcceleration, Clutch / (MassLoad + 0.1f) / (Drift() * 7.5f + 1f) * 3f * Time.deltaTime);
            Rig.velocity = Vector2.Lerp(Rig.velocity, Vector2.zero, (Clutch / (MaxTrackSpeed + 0.01f)) / (MassLoad + 0.1f) * (Drift() + LinerDrag) * 0.5f * Time.deltaTime);
            Rig.angularVelocity = Mathf.Lerp(Rig.angularVelocity, (input.Rotation * Back + TrackDiff * Mathf.Sqrt(Mathf.Abs(Power)) * 6f) * RotationSpeed * Clutch * (1 + Mathf.Abs(Power) * 0.25f) * 45f, 7.5f * Time.deltaTime);
            Rig.centerOfMass = Main.part.transform.localPosition;
        }
        if (Rig.velocity.magnitude > GameData.MaxSpeed)
        {
            Rig.velocity = Rig.velocity.normalized * GameData.MaxSpeed;
        }

        #endregion
        #region Tower
        for (int i = 0; i < Tower.Length; i++)
        {
            Vector2 Direction = (input.MousePos - (Vector2)Tower[i].part.transform.position).normalized;
            float Speed = Mathf.Pow(Vector2.Dot(Direction, Tower[i].part.transform.up), 4f) * 5f;
            Tower[i].part.transform.up = Vector2.Lerp(Tower[i].part.transform.up, GetTowerDirection(Direction, Tower[i].part.NumInLinks), Tower[i].tower.RotationSpeed * (5f + Speed) * Time.deltaTime);
        }

        if (input.Fire && !Firing)
        {
            StartCoroutine(Fire(input.MousePos));
        }
        #endregion
        #region Special
        if (input.Special && !SpecAction)
        {
            StartCoroutine(SpecialAction());
        }
        #endregion
    }
    public void Gravity()
    {
        if (Destroyed)
            return;
        int TrackNum = LeftTrack.Length + RightTrack.Length;
        if(TrackNum == 0)
        {
            Vector3 Direction = GetLandNormal(transform.position);
            float x = Mathf.Abs(Vector2.Dot(Direction, transform.up));
            float PowerForce = Vector2.Dot(Direction, transform.up);
            float ThisClutch = 0.25f;
            float NormalPower = (1 - Mathf.Abs(Direction.z)) * 0.25f;
            NormalPower = NormalPower > 0.01f ? NormalPower : 0;
            GravityForce = Vector2.Lerp(GravityForce, (Vector2)Direction * NormalPower * MapGenerator.Gravity, 0.05f);

            Rig.AddForceAtPosition(GravityForce / ThisClutch * 8f, transform.position, ForceMode2D.Impulse);
            Rig.velocity += GravityForce * 0.25f;
        }
        else
        {
            for(int i = 0; i < LeftTrack.Length; i++)
            {
                Transform Track = LeftTrack[i].part.transform;
                Vector3 Direction = GetLandNormal(Track.position);
                float x = Mathf.Abs(Vector2.Dot(Direction, Track.up));
                float PowerForce = Vector2.Dot(Direction, Track.up) / TrackNum;
                float NormalPower = (1 - GetNormalPower(Track.position)) * MassLoad / TrackNum;
                NormalPower = NormalPower * TrackNum > Clutch * 0.25f ? Mathf.Sqrt(NormalPower) : 0;
                float ThisClutch = GetClutch(LeftTrack[i]);
                GravityForce = Vector2.Lerp(GravityForce, (Vector2)Direction * NormalPower * MapGenerator.Gravity, 0.025f);
                float PowerMultiplue = GravityForce.magnitude * PowerForce * (x + 0.25f) * 0.5f;
                Power += Power > 0 ? PowerMultiplue : PowerMultiplue * 0.5f;
                Rig.AddForceAtPosition(GravityForce / ThisClutch * 6f, Track.position, ForceMode2D.Impulse);
                Rig.angularVelocity += (GravityForce.magnitude * ThisClutch * 250f);
            }
            for (int i = 0; i < RightTrack.Length; i++)
            {
                Transform Track = RightTrack[i].part.transform;
                Vector3 Direction = GetLandNormal(Track.position);
                float x = Mathf.Abs(Vector2.Dot(Direction, Track.up));
                float PowerForce = Vector2.Dot(Direction, Track.up) / TrackNum;
                float NormalPower = (1 - GetNormalPower(Track.position)) * MassLoad / TrackNum;
                NormalPower = NormalPower * TrackNum > Clutch * 0.25f ? Mathf.Sqrt(NormalPower) : 0;
                float ThisClutch = GetClutch(RightTrack[i]);
                GravityForce = Vector2.Lerp(GravityForce, (Vector2)Direction * NormalPower * MapGenerator.Gravity, 0.025f);
                float PowerMultiplue = GravityForce.magnitude * PowerForce * (x + 0.25f) * 0.5f;
                Power += Power > 0 ? PowerMultiplue : PowerMultiplue * 0.5f;
                Rig.AddForceAtPosition(GravityForce / ThisClutch * 6f, Track.position, ForceMode2D.Impulse);
                Rig.angularVelocity -= (GravityForce.magnitude * ThisClutch * 250f);
            }
        }
        
    }


    public void GetEnemy(Tank enemy)
    {
        if (enemy == null)
            return;
        if(transform.tag == "Ai")
        {
            GetComponent<AiController>().GetEnemy(enemy);
        }
    }

    public Vector2Int GetTerrainCoord(Vector2 Position)
    {
        if (Terrain == null)
            return Vector2Int.zero;
        RaycastHit hit;
        Physics.Raycast((Vector3)Position - Vector3.forward, Vector3.forward, out hit, 5f, 1 << 8);
        Vector2Int Coord = Vector2Int.RoundToInt(hit.textureCoord * new Vector2(Terrain.xSize, Terrain.ySize));
        return Coord;
    }
    public TerrainSurface GetTerrain()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position - Vector3.forward, Vector3.forward, out hit, 5f, 1 << 8);
        if (hit.collider == null)
            return null;
        return hit.transform.GetComponent<TerrainSurface>();
    }
    public Vector3 GetLandNormal(Vector2 Position)
    {
        if (Terrain == null)
            return -Vector3.forward;
        Vector2Int Pos = GetTerrainCoord(Position);
        return Terrain.GetNormal(Pos);
    }
    public Color GetLandColor(Vector2 Position)
    {
        if (Terrain == null)
            return Color.white;
        RaycastHit hit;
        Physics.Raycast((Vector3)Position - Vector3.forward, Vector3.forward, out hit, 5f, 1 << 8);
        Vector2Int Coord = Vector2Int.RoundToInt(hit.textureCoord * new Vector2(Terrain.xSize, Terrain.ySize));

        return Terrain.GetColor(Coord);
    }
    public float GetNormalPower(Vector2 pos)
    {
        if (Terrain == null)
            return 0;
        Vector3 Direction = GetLandNormal(pos);
        return Direction.z * Direction.z;
    }
    public float GetLandClutch(Vector2 Position)
    {
        if (Terrain == null)
            return 1;
        Vector2Int Pos = GetTerrainCoord(Position);
        return Terrain.GetClutch(GetTerrainCoord(Pos));
    }
    public float GetLandSpeed(Vector2 Position)
    {
        if (Terrain == null)
            return 1;
        Vector2Int Pos = GetTerrainCoord(Position);
        return Terrain.GetSpeed(GetTerrainCoord(Pos));
    }

    private IEnumerator Fire(Vector2 Target)
    {
        Firing = true;
        for(int i = 0; i < Gun.Length; i++)
        {
            if (!Gun[i].cannon.Reloaded)
                continue;
            Vector2 Dir = (Target - (Vector2)Gun[i].part.transform.position).normalized;
            if(Vector2.Dot(Dir, Gun[i].part.transform.up) > 0.9f)
            {
                Gun[i].part.GetComponent<Cannon>().Fire(Rig.velocity, Target, this, Side.Team);
                yield return new WaitForSeconds(0.25f);
                Firing = false;
                break;
            }  
        }
        Firing = false;
        yield break;
    }
    public void Recoil(Vector2 Velocity, Part cannon, float ReloadTime, float BulletMass, float Power)
    {
        if(GetTowerIndex(cannon) >= Tower.Length || Tower[GetTowerIndex(cannon)].part == null)
        {
            return;
        }
        if(tag == "Player")
        {
            Controller.active.HaveFired();
        }
        TankPart ThisTower = Tower[GetTowerIndex(cannon)];
        ThisTower.tower.Recoil(ReloadTime);
        float BulletSpeed = Velocity.magnitude * Power;
        float MassVs = BulletMass / Mass;
        float Angular = Vector2.Dot(transform.right, ThisTower.part.transform.up);
        float AngularPower = Angular * Main.part.GetLink(ThisTower.part).Point.localPosition.y;
        Rig.velocity -= Velocity.normalized * BulletSpeed * MassVs * (2f - Mathf.Abs(AngularPower)) * 10f;
        Rig.angularVelocity += AngularPower * BulletSpeed * MassVs * 360f;
    }
    private IEnumerator SpecialAction()
    {
        SpecAction = true;
        for(int i = 0; i < Special.Length; i++)
        {
            if(Special[i].special != null)
            {
                Special[i].special.Action();
            }
        }
        yield return new WaitForSeconds(0.5f);
        SpecAction = false;
        yield break;
    }

    public void FixedUpdate()
    {
        if (GameData.InBuild)
            return;
        Terrain = GetTerrain();
        Gravity();
    }
    [System.Serializable]
    public struct TankPart
    {
        public Part part;
        public Cannon cannon;
        public Track track;
        public Engine engine;
        public Tower tower;
        public BodyPart body;
        public SpecialPart special;
        public bool NotNull;

        public TankPart(Part part)
        {
            this.part = part;
            cannon = null;
            track = null;
            engine = null;
            tower = null;
            body = null;
            special = null;
            NotNull = true;
            switch(part.PartType)
            {
                case Part.Type.Body:
                    body = part.GetComponent<BodyPart>();
                    break;
                case Part.Type.Engine:
                    engine = part.GetComponent<Engine>();
                    break;
                case Part.Type.Gun:
                    cannon = part.GetComponent<Cannon>();
                    break;
                case Part.Type.Tower:
                    tower = part.GetComponent<Tower>();
                    break;
                case Part.Type.Track:
                    track = part.GetComponent<Track>();
                    break;
                case Part.Type.Special:
                    special = part.GetComponent<SpecialPart>();
                    break;
            }
        }
    }
}
