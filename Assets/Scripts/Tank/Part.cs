using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Part : MonoBehaviour
{
    public string Name;
    public int MaxHp;
    public int Hp;
    public bool Destroyed;
    public bool NotInteractive;
    public float Mass; //tonn
    public enum Type {Body, Tower, Engine, Track, Gun, Special}
    public Linking[] Links;
    public Transform[] Pin;
    public Linking ParentLink;
    public Collider2D[] TowerArmor;
    public int NumInLinks;
    public int[] SubLinks;
    public Type PartType;
    public int Index;
    public int level;

    public bool OnTank;
    public bool OnMouse;
    public bool Secondary;
    public bool InOther;
    public Tank Parent;
    public TerrainSurface Terrain;
    public Color PartColor;
    public Transform TopPoint;
    public float Lenght()
    {
        return Mathf.Abs(Sprite.bounds.extents.y * 2);
    }
    public float Size()
    {
        return Sprite.bounds.extents.magnitude;
    }
    public SpriteRenderer Sprite;
    public SpriteRenderer[] PartPlace;
    private Coroutine CollisionCour;
    private Coroutine EnterCour;
    public Rigidbody2D Rig;
    private Vector2 GravityForce;
    public Collider2D Collider;
    private Tank PrevContack;

    public void Awake()
    {
        Hp = MaxHp;
        TryGetComponent(out Rig);
        Collider = GetComponent<Collider2D>();
        Sprite = GetComponent<SpriteRenderer>();
        if (Pin.Length == 0)
        {
            Pin = new Transform[1] {transform};
        }
        SubLinks = new int[Pin.Length];
    }
    void Start()
    {
        SetColor(PartColor);

        if (Rig != null)
            Rig.mass = Mass;
        InitializeLinks();
        if(Pin.Length == 0)
        {
            Pin = new Transform[1] { transform };
        }
        if(transform.parent == null)
        {
            AddRig();
        }
        for(int i = 0; i < Links.Length; i++)
        {
            Links[i].ThisHeight = Links[i].Point.localPosition.z;
            Links[i].Parent = this;
        }
    }
    public void FixedUpdate()
    {
        if(!OnMouse && !OnTank && Rig != null && !GameData.InBuild)
        {
            Gravity();
        }
    }

    private void Gravity()
    {
        Terrain = GetTerrain();

        Vector3 Direction = GetLandNormal(transform.position);
        float NormalPower = (1 - Mathf.Abs(Direction.z * Direction.z * Direction.z));
        GravityForce = (Vector2)Direction * NormalPower * NormalPower * MapGenerator.Gravity;
        Rig.velocity = Vector2.Lerp(Rig.velocity, GravityForce * 55f, 0.05f);

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
    private IEnumerator GettingTerrain()
    {
        while (true)
        {
            if (!OnTank && !OnMouse)
            {
                Terrain = GetTerrain();
            }
            yield return new WaitForSeconds(1f);
        }
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

    public void LikeNewPart()
    {
        Destroyed = false;
        NotInteractive = false;
        transform.localScale = Vector3.one;
        transform.parent = null;
        AddRig();
    }
    
    public void InitializeLinks()
    {
        if (transform.parent == null)
        {
            for (int i = 0; i < Links.Length; i++)
            {
                if (Links[i].Point.childCount > 0 && Links[i].Point.GetChild(0).gameObject.activeSelf)
                {
                    Part part = Links[i].Point.GetChild(0).GetComponent<Part>();
                    Links[i].InstallPart(part);
                }
            }
        }
    }
    public void SetColor(Color color)
    {
        GetComponent<SpriteRenderer>().material.SetColor("_Color", color);
        for (int i = 0; i < PartPlace.Length; i++)
        {
            PartPlace[i].material.SetColor("_Color", color);
        }
    }
    public void SetColor(float[] color)
    {
        Color RealColor = new Color(color[0], color[1], color[2]);
        GetComponent<SpriteRenderer>().material.SetColor("_Color", RealColor);
        for (int i = 0; i < PartPlace.Length; i++)
        {
            PartPlace[i].material.SetColor("_Color", RealColor);
        }
    }

    public void OnInstall(Linking Link)
    {
        transform.parent = Link.Point;
        Parent = Link.Point.transform.root.GetComponent<Tank>();
        DeleteRig();
        ParentLink = Link;
        Destroyed = false;
        transform.localPosition = new Vector3(transform.localPosition.x,
                                              transform.localPosition.y,
                                                       Link.PointHeight);
        if(Collider == null)
        {
            Debug.Log(this);
        }
        Collider.isTrigger = true;
        for (int i = 0; i < TowerArmor.Length; i++)
        {
            TowerArmor[i].isTrigger = true;
        }
        CollisionCour = null;
        if (Rig != null)
        {
            Rig.bodyType = RigidbodyType2D.Kinematic;
            Rig.velocity = Vector2.zero;
            Rig.angularVelocity = 0f;
        }
        if (!Link.FreeRotation)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        if(Link.Reversed)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 180);
        }
        if (Parent != null)
        {
            Parent.OnInstallPart(this);
            for (int i = 0; i < Links.Length; i++)
            {
                if (Links[i].Installed)
                {
                    Parent.OnInstallPart(Links[i].InstalledPart);
                    Links[i].InstalledPart.Parent = Parent;
                    Links[i].InstalledPart.OnTank = true;
                }
            }
        }
        OnTank = true;
    }
    public void OnDeInstalled(bool MouseTake)
    {
        if (ParentLink != null && ParentLink.Static)
            return;
        if(Parent != null)
        {
            Parent.OnDeInstallPart(this);
            for (int i = 0; i < Links.Length; i++)
            {
                if (Links[i].Installed)
                {
                    Parent.OnDeInstallPart(Links[i].InstalledPart);
                    Links[i].InstalledPart.Parent = null;
                    Links[i].InstalledPart.OnTank = false;
                }
            }
        }

        AddRig();
        ParentLink = null;
        Parent = null;
        OnTank = false;
        transform.parent = null;
        if(MouseTake)
        {
            OnTake();
        }
        else
        {
            SetCollision(true);
        }
    }

    public void AddForce(Vector2 Force, float Angular)
    {
        StartCoroutine(AddForceCour(Force, Angular));
    }
    private IEnumerator AddForceCour(Vector2 Force, float Angular)
    {
        yield return new WaitForFixedUpdate();
        if (Rig != null)
        {
            Rig.velocity += Force;
            Rig.angularVelocity += Angular;
        }
        yield break;
    }
    private void AddRig()
    {
        if(GetComponent<Rigidbody2D>() != null)
        {
            Rig = GetComponent<Rigidbody2D>();
        }
        else
        {
            Rig = gameObject.AddComponent<Rigidbody2D>();
        }
        Rig.bodyType = RigidbodyType2D.Dynamic;
        Rig.mass = Mass;
        Rig.gravityScale = 0;
        Rig.drag = 5f;
        Rig.angularDrag = 5f;
    }
    private void DeleteRig()
    {
        if (Rig != null)
            Destroy(Rig);
        Rig = null;
    }
    public void ChangeBodyParent(Tank tank)
    {
        Parent = tank;
        transform.parent = tank.transform;
        transform.localPosition = Vector2.zero;
        transform.up = tank.transform.up;
        SetCollision(true);
        DeleteRig();
        tank.OnInstallPart(this);
    }
    public void OnParentDisapear(float BoomPower)
    {
        if (ParentLink.Static)
            return;
        OnDeInstalled(false);
        ThrowOut(BoomPower);
    }
    public void OnTake()
    {
        OnMouse = true;
        SetCollision(false);
        if(Rig != null)
        {
            Rig.bodyType = RigidbodyType2D.Kinematic;
        }
        SetPositionZ();
    }
    public void OnDrop(Vector2 Velocity)
    {
        OnMouse = false;
        SetCollision(true);
        if(Rig != null)
        {
            Rig.bodyType = RigidbodyType2D.Dynamic;
            Rig.velocity += Velocity;
        }
        SetPositionZ();
    }
    private IEnumerator ReduseDrag(float delay)
    {
        if (Rig == null)
            yield break;
        float temp = Rig.drag;
        Rig.drag = 1f;
        yield return new WaitForSeconds(delay);
        if (Rig == null)
            yield break;
        Rig.drag = temp;
        yield break;
    }
    public void SetCollision(bool On)
    {
        if(CollisionCour != null)
        {
            StopCoroutine(CollisionCour);
        }
        CollisionCour = StartCoroutine(SetCollisionCour(On));
    }
    private IEnumerator SetCollisionCour(bool On)
    {
        if (On)
        {
            yield return new WaitForSeconds(1f);
            while (InOther)
            {
                yield return new WaitForSeconds(0.25f);
            }
            Collider.isTrigger = false;
            for (int i = 0; i < TowerArmor.Length; i++)
            {
                TowerArmor[i].isTrigger = false;
            }
        }
        else
        {
            Collider.isTrigger = true;
            for (int i = 0; i < TowerArmor.Length; i++)
            {
                TowerArmor[i].isTrigger = true;
            }
        }
        CollisionCour = null;
        yield break;
    }
    private void OtherEnter()
    {
        if(EnterCour == null)
        {
            EnterCour = StartCoroutine(OtherEnterCour());
        }
    }
    private IEnumerator OtherEnterCour()
    {
        if(PartType == Type.Body)
        {
            EnterCour = null;
            yield break;
        }
        yield return new WaitForSeconds(0.25f);
        Collider.isTrigger = true;
        for (int i = 0; i < TowerArmor.Length; i++)
        {
            TowerArmor[i].isTrigger = true;
        }
        yield return new WaitForSeconds(Mass / 4);
        while(InOther)
        {
            yield return new WaitForSeconds(0.25f);
        }
        Collider.isTrigger = false;
        for (int i = 0; i < TowerArmor.Length; i++)
        {
            TowerArmor[i].isTrigger = false;
        }
        EnterCour = null;
        yield break;
    }
    public void SetPositionZ()
    {
        float Height = 0;
        switch(PartType)
        {
            case Type.Body:
                Height = -1.5f;
                break;
            case Type.Engine:
                Height = -0.2f;
                break;
            case Type.Gun:
                Height = -0.3f;
                break;
            case Type.Tower:
                Height = -0.4f;
                break;
            case Type.Track:
                Height = -0.1f;
                break;
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, Height);
    }

    public void GetHitFromArmor(int Damage, float LinksDamage, int[] LinksIndex, Tank Enemy, Action callback)
    {
        bool LinksFilled = false;
        for (int i = 0; i < LinksIndex.Length; i++)
        {
            int PartDamage = Mathf.FloorToInt(LinksDamage * Damage / LinksIndex.Length);
            if(LinksFilled = LinksIndex[i] != -1 && Links[LinksIndex[i]].InstalledPart != null)
                Links[LinksIndex[i]].InstalledPart.GetHit(PartDamage, callback);
        }
        LinksDamage = LinksFilled ? LinksDamage : 0f;
        GetHit(Mathf.FloorToInt((1 - LinksDamage) * Damage), callback);


        if(Parent != null)
        {
            Parent.GetHit(Enemy, PartType);
        }
    }
    public void GetHit(int Damage, Action callback)
    {
        if(transform.root.tag == "Player")
        {
            Hp -= Mathf.RoundToInt(Damage * GameData.HardLevel);
        }
        else
        {
            Hp -= Mathf.RoundToInt(Damage);
        }
        
        if (Parent != null)
        {
            Parent.GetHit(null, PartType);
        }
        if (Hp <= 0 && !Destroyed)
        {
            Destroyed = true;
            callback?.Invoke();
            if (Parent != null)
            {
                OnPartDestroyed(1.5f + Damage / 250f);
            }
        }
    }
    public void GetForce(Vector2 Position, Vector2 Direction, float BulletMass)
    {
        if (Rig == null)
            return;
        Vector2 DirBul = ((Vector2)transform.position - Position).normalized;
        int Right = Vector2.Dot(DirBul, transform.right) > 0 ? 1 : -1;
        int Up = Vector2.Dot(DirBul, transform.up) > 0 ? 1 : -1;
        float Power = Mathf.Sqrt(((Vector2)transform.position - Position).magnitude);
        float AngularDot = Mathf.Pow(Vector2.Dot(transform.right, Direction), 2);
        float Angular = Up * Right / Power * AngularDot;
        Rig.angularVelocity += Angular * BulletMass / Mass * 45f;
        Rig.velocity += Direction / Power * BulletMass / Mass * 0.5f;
    }
    public void SetHp(float Coefficient)
    {
        Hp = Mathf.FloorToInt(MaxHp * Coefficient);
    }
    public void OnPartDestroyed(float BoomPower)
    {
        Hp = Mathf.FloorToInt(MaxHp * 0.1f);
        if(ParentLink != null)
        {
            if(PartType == Type.Body)
            {
                Parent.OnTankDestroyed();
            }
            for (int i = 0; i < Pin.Length; i++)
            {
                DestroyEffect(Mass / Parent.Mass, Pin[i].position);
            }
            
            ParentLink.DeInstallPart(this);
        }
        AddRig();
        SetCollision(true);
        if(PartType != Type.Body)
            ThrowOut(BoomPower);
        ThrowLinks(BoomPower);
    }
    private void DestroyEffect(float Power, Vector3 Pos)
    {
        Vector3 Position = new Vector3(Pos.x, Pos.y, -2);
        ParticleEffect Effect = Instantiate(GameData.Particle, Position, transform.rotation, transform);
        Effect.PartDestroyed(1 + Power);
    }
    public void ThrowOut(float BoomPower)
    {
        int x = UnityEngine.Random.Range(0, 2);
        int y = UnityEngine.Random.Range(0, 2);
        Vector2 Boom = new Vector2((x * 2 - 1) * UnityEngine.Random.Range(0.5f, 1f), (y * 2 - 1) * UnityEngine.Random.Range(0.5f, 1f));
        Vector2 Direction = ((Vector2)((Parent != null ? Parent.transform.position : transform.position) - transform.position) * 2 + Boom).normalized;
        if (Rig != null)
        {
            StartCoroutine(ReduseDrag(2f));
            Rig.bodyType = RigidbodyType2D.Dynamic;
            Rig.velocity += (Boom) * BoomPower * 10;
            Rig.angularVelocity += (x * 2 - 1) * UnityEngine.Random.Range(0.5f, 1f) * BoomPower * 360f;
        }
        SetPositionZ();
        SetCollision(true);
    }
    public void ThrowLinks(float ThrowPower)
    {
        for(int i = 0; i < Links.Length; i++)
        {
            if(Links[i].SubInstalled)
            {
                Links[i].SubInstalled = false;
                Links[i].InstalledPart = null;
            }
            if(Links[i].Installed && !Links[i].Static)
            {
                Links[i].InstalledPart.OnParentDisapear(ThrowPower);
                Links[i].Installed = false;
                Links[i].InstalledPart = null;
            }
            else if(Links[i].Static && Links[i].Installed)
            {
                for(int a = 0; a < Links[i].InstalledPart.Links.Length; a++)
                {
                    if (Links[i].InstalledPart.Links[a].Installed && !Links[i].InstalledPart.Links[a].Static)
                    {
                        Links[i].InstalledPart.Links[a].InstalledPart.OnParentDisapear(ThrowPower);
                        Links[i].InstalledPart.Links[a].Installed = false;
                        Links[i].InstalledPart.Links[a].InstalledPart = null;
                    }
                }
            }
        }
    }

    public Tank GetTank()
    {
        return Parent;
    }
    public Part GetPart(Type partType)
    {
        for (int i = 0; i < Links.Length; i++)
        {
            if (Links[i].PartType == partType && Links[i].Installed)
            {
                return Links[i].InstalledPart;
            }
        }
        return null;
    }
    public Linking GetLink(Part part)
    {
        for(int i = 0; i < Links.Length; i++)
        {
            if (Links[i].Installed && Links[i].InstalledPart == part)
                return Links[i];
        }
        return null;
    }

    public void MakeLikeImage(Transform Pos)
    {
        if(GetComponent<Collider2D>() != null)
        {
            Destroy(GetComponent<Collider2D>());
        }
        Sprite.sortingOrder = 2;
        for(int i = 0; i < PartPlace.Length; i++)
        {
            PartPlace[i].sortingOrder = 2;
        }

        float size = 6.5f * transform.localScale.x / Lenght();
        transform.localScale = Vector3.one * size;
        
        if(Rig != null)
        {
            Destroy(Rig);
        }
        Vector2 CurrantePos = new Vector2(Pos.position.x, Pos.position.y);
        transform.position = CurrantePos;
        transform.rotation = Pos.rotation;
        transform.parent = Pos;
        transform.localScale = Vector3.one *
        (transform.localScale.x > 100 ? 100 : transform.localScale.x);
    }


    public void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.tag == "Part" && PartType != Type.Body)
            InOther = true;
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (!OnTank && collision.collider.tag == "Armor" && PartType != Type.Body && PartType != Type.Tower)
        {
            OtherEnter();
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Part")
            InOther = false;
    }

    [System.Serializable]
    public class Linking
    {
        public string name;
        public Part Parent;
        public int Index;
        public Transform Point;
        public GameObject[] Effects;
        public ParticleSystem RideEffect;
        public bool Static;
        public bool Reversed;
        public bool PointOnUp;
        public float ThisHeight;
        public bool FreeRotation;
        public float PointHeight;
        public bool Right; //For tracks and wheel
        public bool Front; //For tracks and wheel mean can front turn
        public bool Back;  //For tracks and wheel mean can back turn

        public Type PartType;
        public Part InstalledPart;
        public bool Installed;
        public bool SubInstalled;

        public void InstallPart(Part part)
        {
            InstalledPart = part;
            part.NumInLinks = Index;
            if(part.SubLinks.Length > 0)
            {
                part.SubLinks[0] = Index;
            }

            Installed = true;
            part.OnInstall(this);
        }
        public void SubInstallPart(Part part, int PinNum)
        {
            InstalledPart = part;
            part.SubLinks[PinNum] = Index;
            SubInstalled = true;
        }
        public void DeInstallPart(Part part)
        {
            InstalledPart = null;
            Installed = false;
            SubInstalled = false;
            if (part.ParentLink != null)
            {
                for (int i = 1; i < part.SubLinks.Length; i++)
                {
                    part.ParentLink.Parent.Links[part.SubLinks[i]].SubDeInstall();
                }
            }
            part.OnDeInstalled(part.OnMouse);
        }
        public void SubDeInstall()
        {
            InstalledPart = null;
            Installed = false;
            SubInstalled = false;
        }
        public void SetBody()
        {
            if (InstalledPart == null)
                return;
            InstalledPart.OnInstall(this);
        }
    }
}

