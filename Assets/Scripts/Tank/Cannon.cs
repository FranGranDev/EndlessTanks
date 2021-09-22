using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public enum CannonsType { Cannon, Rocket}
    public CannonsType CannonType;
    public enum RoundType { Piercing, Explosive }
    public RoundType ShellType;
    public enum FireTypes {Forward, Atop}
    public FireTypes FireType;
    public int Damage;
    [Range(0, 1f)]
    public float ModuleDamage;
    public int Penetration;
    [Range(0, 1)]
    public float Scatter;
    [Range(0, 1)]
    public float MaxRicochetAngle;
    [Range(0, 1)]
    public float ExstraRicochetChanse;
    [Range(0, 1)]
    public float DamageScatter;
    public float TopPointShellSize;
    public float ExplosiveRadius;
    private float DistanceToTarget;
    private float GunLenght;
    public float RecoilPower;
    public float BulletSpeed;
    public float BulletMass;
    public float MM;

    private Transform FirePoint;
    private Transform EffectPoint;
    private Coroutine ReloadCour;
    public GameObject Bullet;
    private Part part;

    public float ReloadTime;
    public float ReloadInClipTime;
    private float GetReloadTime()
    {
        if (BulletInClip > 1)
        {
            return (ReloadInClipTime);
        }
        else
        {

            return (ReloadTime);
        }
    }
    public int BulletInClip;
    private int CurrantBullet;
    public bool Reloaded;
    public bool Reloading;

    public void Start()
    {
        OnStart();
    }
    public void OnStart()
    {
        FirePoint = transform.GetChild(0);
        EffectPoint = transform.GetChild(1);
        GunLenght = (FirePoint.position - EffectPoint.position).magnitude;
        part = GetComponent<Part>();
        if (Reloaded)
        {
            CurrantBullet = BulletInClip;
        }
        else
        {
            StartCoroutine(MakeReload());
        }
    }

    public void Fire(Vector2 SelfVelocity, Vector2 Target, Tank tank, SideOwn.type Side)
    {
        if(Reloaded)
        {
            CurrantBullet--;
            Reloaded = false;
            switch (FireType)
            {
                case FireTypes.Atop:
                    FireAtop(SelfVelocity, Target, tank, Side);
                    break;
                case FireTypes.Forward:
                    FireForward(SelfVelocity, Target, tank, Side);
                    break;
            }
            if (ReloadCour == null)
            {
                ReloadCour = StartCoroutine(MakeReload());
            }
        }
        else if(ReloadCour == null)
        {
            ReloadCour = StartCoroutine(MakeReload());
        }
        
    }
    private void FireForward(Vector2 SelfVelocity, Vector2 Target, Tank tank, SideOwn.type Side)
    {
        Bullet NowBullet = Instantiate(Bullet, FirePoint.position, FirePoint.rotation, null).GetComponent<Bullet>();
        HitInfo hit = new HitInfo(Damage, ShellType, CannonType, Side, ModuleDamage, Penetration, MaxRicochetAngle, ExstraRicochetChanse, DamageScatter, ExplosiveRadius, Target, FireType == FireTypes.Atop, TopPointShellSize, BulletMass, MM, tank);
        NowBullet.OnStart(hit);
        Rigidbody2D BulRig = NowBullet.GetComponent<Rigidbody2D>();
        Vector2 ScatterDir = new Vector2(transform.up.x + Random.Range(-Scatter, Scatter) * 0.5f, transform.up.y + Random.Range(-Scatter, Scatter) * 0.5f).normalized;
        BulRig.velocity = SelfVelocity * 0.8f + ((Vector2)NowBullet.transform.up + ScatterDir) * BulletSpeed * GameData.BulletSpeedC;
        if(isActiveAndEnabled)
            StartCoroutine(Recoil(BulRig.velocity, NowBullet, tank));
        FireEffect(SelfVelocity);
    }
    private void FireAtop(Vector2 SelfVelocity, Vector2 Target, Tank tank, SideOwn.type Side)
    {
        Bullet NowBullet = Instantiate(Bullet, FirePoint.position, FirePoint.rotation, null).GetComponent<Bullet>();
        HitInfo hit = new HitInfo(Damage, ShellType, CannonType, Side, ModuleDamage, Penetration, MaxRicochetAngle, ExstraRicochetChanse, DamageScatter, ExplosiveRadius, Target, FireType == FireTypes.Atop, TopPointShellSize, BulletMass, MM, tank);
        NowBullet.OnStart(hit);
        Rigidbody2D BulRig = NowBullet.GetComponent<Rigidbody2D>();
        Vector2 ScatterDir = new Vector2(transform.up.x + Random.Range(-Scatter, Scatter) * 0.5f, transform.up.y + Random.Range(-Scatter, Scatter) * 0.5f).normalized;
        BulRig.velocity = SelfVelocity * 0.8f + ((Vector2)NowBullet.transform.up + ScatterDir) * BulletSpeed * GameData.BulletSpeedC;
        if (isActiveAndEnabled)
            StartCoroutine(Recoil(BulRig.velocity, NowBullet, tank));
        FireEffect(SelfVelocity);
    }
    private void FireEffect(Vector2 Velocity)
    {
        switch (CannonType)
        {
            case CannonsType.Cannon:
                {
                    float GunPower = MM / 50;
                    ParticleEffect This = Instantiate(GameData.Particle.gameObject, EffectPoint.position, EffectPoint.rotation, null).GetComponent<ParticleEffect>();
                    This.CannonFire(Velocity, GunPower);
                }
                break;
            case CannonsType.Rocket:
                {
                    float GunPower = MM / 90;
                    ParticleEffect This = Instantiate(GameData.Particle.gameObject, EffectPoint.position, EffectPoint.rotation, null).GetComponent<ParticleEffect>();
                    This.RocketFire(Velocity, GunPower);
                }
                break;
        }
        
        
    }
    public IEnumerator Recoil(Vector2 Velocity, Bullet Bull, Tank tank)
    {
        if (tank == null)
            yield break;
        yield return new WaitForSeconds(GunLenght / Velocity.magnitude);
        tank.Recoil(Velocity, GetComponent<Part>(), GetReloadTime(), BulletMass / 100f, RecoilPower);
        yield break;

    }
    public IEnumerator MakeReload()
    {
        Reloading = true;
        if (CurrantBullet <= 0)
        {
            CurrantBullet = BulletInClip;
            yield return new WaitForSeconds(ReloadTime);
        }
        else
        {
            
            yield return new WaitForSeconds(ReloadInClipTime);
        }
        Reloading = false;
        Reloaded = true;
        ReloadCour = null;
        yield break;
    }

    public void FixedUpdate()
    {

    }
}
