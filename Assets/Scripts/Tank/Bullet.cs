using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public HitInfo Hit;
    public Rigidbody2D Rig;
    public TrailRenderer Trail;
    private Tank PrevRicochet;
    private Vector2 StartPoint;
    private Vector2 Target;
    private float StartDistance;
    private Transform PrevHit;
    private Coroutine ArmorEnter;
    private bool AfterOtherPart;
    private float CurrantZ;
    private bool OutOfView;

    private float CurrantDistance()
    {
        return ((Vector2)transform.position - StartPoint).magnitude;
    }
    private bool CanHit()
    {
        return !Hit.Atop || (CurrantDistance() / StartDistance > 0.8f);
    }

    private float Size;

    private void Start()
    {
        
    }
    public void OnStart(HitInfo Hit)
    {
        this.Hit = Hit;
        StartPoint = transform.position;
        OutOfView = (StartPoint - EndlessTerrain.viewerPosition).magnitude > EndlessTerrain.MaxDistanceView;
        Target = Hit.Destination;
        StartDistance = (StartPoint - Target).magnitude;
        switch(Hit.CannonType)
        {
            case Cannon.CannonsType.Cannon:
                Size = Hit.MM / 120;
                break;
            case Cannon.CannonsType.Rocket:
                Size = Hit.MM / 250;
                break;
        }
        
        transform.localScale = Vector2.one * Mathf.Sqrt(Size);
        Trail.widthMultiplier = Mathf.Sqrt(Size);
        CurrantZ = -5;
        SetZ(-5);
    }
    public void SetZ(float Z)
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, Z);
    }


    private void OnHit(Armor armor)
    {
        float AngleRicochet = Mathf.Abs(Vector2.Dot(transform.up, armor.transform.right)) * (1 - Random.Range(0, Hit.RicochetChanse));
        int CurrentArmorMM = Mathf.RoundToInt(armor.ArmorMM / (AngleRicochet));
        int Damage = 0;
        switch (Hit.Type)
        {
            case Cannon.RoundType.Piercing:
                {
                    if (Hit.Penetration > CurrentArmorMM) //Penetration
                    {
                        float PenetrationReserve = (Hit.Penetration - CurrentArmorMM) / Hit.Penetration;
                        Damage = Mathf.RoundToInt(Mathf.Sqrt(PenetrationReserve) * (1 + Random.Range(-Hit.DamageScatter, Hit.DamageScatter)) * Hit.Damage);
                        Vector2 Direction = transform.up;
                        armor.Hit(Damage, Hit.LinkDamage, transform.position, Hit.Mass * 0.5f, Direction, Hit.Owner, OnPartDestroyed);
                        Hit.Penetration -= CurrentArmorMM * 2;
                        if (Hit.Penetration < Hit.StartPenetration * 0.5f)
                        {
                            DestroyBullet();
                            return;
                        }
                            
                        Vector2 NewDir = (Random.Range(0, 2) == 1 ? transform.right : -transform.right) * 0.25f / (1 + Hit.Penetration / Hit.StartPenetration * 10);
                        Rig.velocity = (Rig.velocity.normalized + NewDir).normalized * Rig.velocity.magnitude;
                    }
                    else if (AngleRicochet < Hit.AngleRicochet) //Ricochet
                    {
                        float NewMagnitude = Rig.velocity.magnitude;
                        Vector2 ArmorNormalized = armor.transform.right;
                        Vector2 RandNormalized = new Vector2(Rig.velocity.normalized.x + Random.Range(-0.1f, 0.1f), Rig.velocity.normalized.y + Random.Range(-0.1f, 0.1f));
                        Vector2 NewNormalized = Vector2.Reflect(RandNormalized, ArmorNormalized);
                        Vector2 Direction = Rig.velocity.normalized;
                        transform.up = NewNormalized;
                        Rig.velocity = NewNormalized * NewMagnitude;
                        

                        armor.Hit(0, 0, transform.position, Hit.Mass * 0.25f, Direction, Hit.Owner, null);
                    }
                    else //No Penetration
                    {
                        Vector2 Direction = Vector2.zero;
                        armor.Hit(0, 0, transform.position, Hit.Mass, Direction, Hit.Owner, null);
                        DestroyBullet();
                    }
                }
                break;
            case Cannon.RoundType.Explosive:
                {
                    if(AngleRicochet > Hit.AngleRicochet)
                    {
                        float PenetrationReserve = Hit.Penetration / armor.ArmorMM;
                        PenetrationReserve = PenetrationReserve < 1 ? Mathf.Pow(PenetrationReserve, 2) : 1;
                        Damage = Mathf.RoundToInt(PenetrationReserve * (1 + Random.Range(-Hit.DamageScatter, Hit.DamageScatter)) * Hit.Damage);
                        Vector2 Direction = transform.up;
                        armor.Hit(Damage, Hit.LinkDamage, transform.position, (Hit.Mass + Hit.Damage / 10), Direction, Hit.Owner, null);
                        ExplodeBullet();
                    }
                    else
                    {
                        float NewMagnitude = Rig.velocity.magnitude;
                        Vector2 ArmorNormalized = armor.transform.right;
                        Vector2 RandNormalized = new Vector2(Rig.velocity.normalized.x + Random.Range(-0.1f, 0.1f), Rig.velocity.normalized.y + Random.Range(-0.1f, 0.1f));
                        Vector2 NewNormalized = Vector2.Reflect(RandNormalized, ArmorNormalized);
                        Vector2 Direction = Rig.velocity.normalized;
                        transform.up = NewNormalized;
                        Rig.velocity = NewNormalized * NewMagnitude;
                        Hit.Atop = false;

                        armor.Hit(0, 0, transform.position, Hit.Mass * 0.25f, Direction, Hit.Owner, null);
                    }
                }
                break;
        }
    }
    private void OnHit(StaticObject obj)
    {
        int Damage = 0;
        switch (Hit.Type)
        {
            case Cannon.RoundType.Piercing:
                {
                    if (Hit.Penetration > obj.StrongMM) //Penetration
                    {
                        float PenetrationReserve = (Hit.Penetration - obj.StrongMM) / Hit.Penetration;
                        Damage = Mathf.RoundToInt(Mathf.Sqrt(PenetrationReserve) * (1 + Random.Range(-Hit.DamageScatter, Hit.DamageScatter)) * Hit.Damage);
                        obj.GetHit(Damage, transform.position);
                        Hit.Penetration -= Mathf.RoundToInt(obj.StrongMM) * 2;
                        if(Hit.Penetration < obj.StrongMM)
                        {
                            DestroyBullet();
                            return;
                        }
                        Vector2 NewDir = (Random.Range(0, 2) == 1 ? transform.right : -transform.right) / (1 + Hit.Penetration / Hit.StartPenetration * 10);
                        Rig.velocity = (Rig.velocity.normalized + NewDir).normalized * Rig.velocity.magnitude;
                    }
                    else //No Penetration
                    {
                        obj.GetHit(0, transform.position);
                        DestroyBullet();
                    }
                }
                break;
            case Cannon.RoundType.Explosive:
                {
                    
                    float PenetrationReserve = (Hit.Penetration - obj.StrongMM) / Hit.Penetration;
                    PenetrationReserve = PenetrationReserve > 0 ? PenetrationReserve : 0;
                    Damage = Mathf.RoundToInt(Mathf.Sqrt(PenetrationReserve) * (1 + Random.Range(-Hit.DamageScatter, Hit.DamageScatter)) * Hit.Damage);
                    obj.GetHit(Damage, transform.position);
                    Hit.Penetration -= Mathf.RoundToInt(obj.StrongMM);
                    if (Hit.Penetration < obj.StrongMM)
                        ExplodeBullet();

                }
                break;
        }
    }
    private void OnHit(ObjectPart part)
    {
        int Damage = 0;
        switch (Hit.Type)
        {
            case Cannon.RoundType.Piercing:
                {
                    if (Hit.Penetration > part.StrongMM) //Penetration
                    {
                        float PenetrationReserve = (Hit.Penetration - part.StrongMM) / Hit.Penetration;
                        Damage = Mathf.RoundToInt(Mathf.Sqrt(PenetrationReserve) * (1 + Random.Range(-Hit.DamageScatter, Hit.DamageScatter)) * Hit.Damage);
                        part.GetHit(Damage);
                        Hit.Penetration -= Mathf.RoundToInt(part.StrongMM) * 2;
                        if (Hit.Penetration < part.StrongMM)
                        {
                            DestroyBullet();
                            return;
                        }
                            
                        Vector2 NewDir = (Random.Range(0, 2) == 1 ? transform.right : -transform.right) / (1 + Hit.Penetration / Hit.StartPenetration * 10);
                        Rig.velocity = (Rig.velocity.normalized + NewDir).normalized * Rig.velocity.magnitude;
                    }
                    else //No Penetration
                    {
                        part.GetHit(0);
                        DestroyBullet();
                    }
                }
                break;
            case Cannon.RoundType.Explosive:
                {
                    float PenetrationReserve = (Hit.Penetration - part.StrongMM) / Hit.Penetration;
                    PenetrationReserve = PenetrationReserve > 0 ? PenetrationReserve : 0;
                    Damage = Mathf.RoundToInt(Mathf.Sqrt(PenetrationReserve) * (1 + Random.Range(-Hit.DamageScatter, Hit.DamageScatter)) * Hit.Damage);
                    part.GetHit(Damage);
                    ExplodeBullet();
                }
                break;
        }
    }
    private void OnArmorEnter(Transform armor)
    {
        Vector3 Position = new Vector3(transform.position.x, transform.position.y, -2);
        ParticleEffect Effect = Instantiate(GameData.Particle, Position, transform.rotation, armor);
        Effect.transform.up = transform.up;
        Trail.gameObject.SetActive(false);
        float Power = Hit.Penetration / Hit.StartPenetration * Mathf.Sqrt((float)Hit.Damage / 200);
        Effect.ArmorHit(Power);
        SetZ(2);
    }
    private void OnArmorExit(Transform armor)
    {
        Vector3 Position = new Vector3(transform.position.x, transform.position.y, -2);
        ParticleEffect Effect = Instantiate(GameData.Particle, Position, transform.rotation, armor);
        Effect.transform.up = -transform.up;
        Trail.gameObject.SetActive(true);
        float Power = Mathf.Sqrt((float)Hit.Damage / 200) * 2;
        Effect.ArmorHit(Power);
        SetZ(CurrantZ);
    }
    private void OnPartDestroyed()
    {

    }

    private void ExplodeBullet()
    {
        ExplodeEffect();
        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, Hit.ExplosiveRadius, 1 << 10 | 1 << 13);
        Transform[] prevhit = new Transform[0];
        for(int i = 0; i < hit.Length; i++)
        {
            bool Oke = true;
            for(int a = 0; a < prevhit.Length; a++)
            {
                if(prevhit[a] == hit[i].transform.root)
                {
                    Oke = false;
                    break;
                }
            }
            if(Oke)
            {
                Vector2 Direction = (hit[i].transform.position - transform.position).normalized;
                float Distance = (transform.position - hit[i].transform.position).magnitude;
                RaycastHit2D check = Physics2D.Raycast(transform.position, Direction, Distance + 0.25f, 1 << 10 | 1 << 13);
                if(check.collider != null)
                {
                    bool yea = true;
                    for(int a = 0; a < prevhit.Length; a++)
                    {
                        if(prevhit[a] == hit[i].transform.root)
                        {
                            yea = false;
                            break;
                        }
                    }
                    if(yea)
                    {
                        Transform[] temp = prevhit;
                        prevhit = new Transform[temp.Length + 1];
                        for (int a = 0; a < temp.Length; a++)
                        {
                            prevhit[a] = temp[a];
                        }
                        prevhit[prevhit.Length - 1] = hit[i].transform.root;
                    }
                    switch(hit[i].tag)
                    {
                        case "Armor":
                            {
                                Armor armor = hit[i].GetComponent<Armor>();
                                float PenetrationReserve = Hit.Penetration / Hit.Penetration;
                                PenetrationReserve = PenetrationReserve < 1 ? Mathf.Pow(PenetrationReserve, 2) : 1;
                                int Damage = Mathf.RoundToInt(PenetrationReserve * (1 + Random.Range(-Hit.DamageScatter, Hit.DamageScatter)) * Hit.Damage / Mathf.Pow(Distance, 1.5f));
                                armor.Hit(Damage, Hit.LinkDamage, transform.position, (Hit.Mass + Hit.Damage / 10) / Distance * 0.5f, Direction, Hit.Owner, null);
                            }
                            break;
                        case "Object":
                            {
                                StaticObject Obj = hit[i].GetComponent<StaticObject>();
                                float PenetrationReserve = Hit.Penetration / Hit.Penetration;
                                PenetrationReserve = PenetrationReserve < 1 ? Mathf.Pow(PenetrationReserve, 2) : 1;
                                int Damage = Mathf.RoundToInt(PenetrationReserve * (1 + Random.Range(-Hit.DamageScatter, Hit.DamageScatter)) * Hit.Damage / Mathf.Pow(Distance, 1.5f));
                                Obj.GetHit(Damage, transform.position);
                            }
                            break;
                        case "ObjectPart":
                            {
                                ObjectPart Obj = hit[i].GetComponent<ObjectPart>();
                                float PenetrationReserve = Hit.Penetration / Hit.Penetration;
                                PenetrationReserve = PenetrationReserve < 1 ? Mathf.Pow(PenetrationReserve, 2) : 1;
                                int Damage = Mathf.RoundToInt(PenetrationReserve * (1 + Random.Range(-Hit.DamageScatter, Hit.DamageScatter)) * Hit.Damage * 2 / Distance);
                                Obj.GetHit(Damage);
                            }
                            break;
                    }
                }
            }
        }
        DestroyBullet();
    }
    private void ExplodeEffect()
    {
        ParticleEffect This = Instantiate(GameData.Particle.gameObject, transform.position, transform.rotation, null).GetComponent<ParticleEffect>();
        This.BulletExplosive(Hit.Mass / 5);
    }
    private void DestroyBullet()
    {
        Destroy(gameObject);
    }
    private bool CheckTower()
    {
        RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, transform.up, 5f, 1 << 10);
        for(int i = 0; i < hit.Length; i++)
        {
            if (hit[i].collider.tag == "Armor" && hit[i].collider.GetComponent<Armor>().Main.PartType == Part.Type.Tower &&
                hit[i].collider.GetComponent<Armor>().Main.GetComponent<Tower>().Obstacle)
                return true;
        }
        return false;
    }
    private bool CheckBody()
    {
        RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, transform.up, 5f, 1 << 10);
        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].collider.tag == "Armor" && hit[i].collider.GetComponent<Armor>().Main.PartType == Part.Type.Body)
                return true;
        }
        return false;
    }
    private bool GetChanse(float Chanse)
    {
        return Random.Range(0, 100) < Mathf.FloorToInt(Chanse * 100);
    }

    public void FixedUpdate()
    {
        transform.up = Vector2.Lerp(transform.up, Rig.velocity.normalized, 0.015f);
        if(Hit.Atop)
        {
            float ksize = Hit.ShellSizeOnTop - Mathf.Pow(Hit.ShellSizeOnTop * CurrantDistance() / StartDistance - 1, 2);
            ksize = ksize < 1 ? 1 : ksize;
            transform.localScale = new Vector3(Size * ksize, Size * ksize, 1);
            transform.position = new Vector3(transform.position.x, transform.position.y, -ksize);
            if(CurrantDistance() > StartDistance)
            {
                ExplodeBullet();
            }
        }
        if(!OutOfView && CurrantDistance() > EndlessTerrain.MaxDistanceView)
        {
            Destroy(gameObject);
        }
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!CanHit())
            return;
        if(collision.tag == "Armor" && (Hit.Owner == null || collision.transform.root != Hit.Owner.transform))
        {
            if (PrevHit == null || collision.transform.root != PrevHit.root)
            {
                Armor armor = collision.GetComponent<Armor>();
                if (PrevRicochet != null && PrevRicochet == armor.Main.GetTank())
                {
                    PrevRicochet = null;
                    return;
                }
                if (armor.Main.PartType == Part.Type.Body)
                {
                    if (CheckTower() && !AfterOtherPart && GetChanse(armor.Main.GetComponent<BodyPart>().ChanseTowerHit))
                    {
                        AfterOtherPart = true;
                        return;
                    }
                    else
                    {
                        OnArmorEnter(collision.transform);
                        PrevHit = collision.transform;
                        PrevRicochet = armor.Main.GetTank();
                        OnHit(armor);
                    }
                }
                else if (armor.Main.PartType == Part.Type.Tower)
                {
                    if (CheckBody() && !AfterOtherPart && GetChanse(armor.Main.GetComponent<Tower>().ChanseBodyHit))
                    {
                        AfterOtherPart = true;
                        return;
                    }
                    else
                    {
                        OnArmorEnter(collision.transform);
                        PrevHit = collision.transform;
                        PrevRicochet = armor.Main.GetTank();
                        OnHit(armor);
                    }
                }
            }
            else if(PrevHit != null && collision.transform.parent == PrevHit.parent)
            {
                OnArmorExit(collision.transform);
            }
        }
        else if (collision.tag == "Obstacle")
        {
            DestroyBullet();
        }
        else if(collision.tag == "Object")
        {
            SetZ(1);
            OnHit(collision.transform.GetComponent<StaticObject>()); 
        }
        else if(collision.tag == "ObjectPart")
        {
            SetZ(1);
            OnHit(collision.transform.GetComponent<ObjectPart>());
        }
    }
}
[System.Serializable]
public struct HitInfo
{

    public float Damage;
    public SideOwn.type Team;
    public Cannon.RoundType Type;
    public Cannon.CannonsType CannonType;
    [Range(0, 1f)]
    public float LinkDamage;
    public float Penetration;
    public float StartPenetration;
    public float AngleRicochet; // (0, 1) (0, 90)
    public float RicochetChanse;
    public float DamageScatter;
    public float ExplosiveRadius;
    public float Mass;
    public float MM;
    public bool Atop;
    public Vector2 Destination;
    public float ShellSizeOnTop;
    public Tank Owner;
    public HitInfo(int Damage, Cannon.RoundType Type, Cannon.CannonsType CannonType, SideOwn.type Team, float LinkDamage, int Penetration, float AngleRicochet, float RicochetChanse, float DamageScatter, float ExplosiveRadius, Vector2 Destination, bool Atop, float ShellSizeOnTop, float Mass, float MM, Tank Owner)
    {
        this.Damage = Damage;
        this.Type = Type;
        this.Team = Team;
        this.Penetration = Penetration;
        this.StartPenetration = Penetration;
        this.AngleRicochet = AngleRicochet;
        this.RicochetChanse = RicochetChanse;
        this.DamageScatter = DamageScatter;
        this.Owner = Owner;
        this.ExplosiveRadius = ExplosiveRadius;
        this.LinkDamage = LinkDamage;
        this.Mass = Mass;
        this.MM = MM;
        this.CannonType = CannonType;
        this.Atop = Atop;
        this.Destination = Destination;
        this.ShellSizeOnTop = ShellSizeOnTop;
    }
}
