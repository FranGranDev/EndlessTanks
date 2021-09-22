using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffect : MonoBehaviour
{
    public Rigidbody2D Rig;

    public void CannonFire(Vector2 Velocity, float GunPower)
    {
        StartCoroutine(CannonFireCour(Velocity, GunPower));
    }
    private IEnumerator CannonFireCour(Vector2 Velocity, float GunPower)
    {
        Rig.bodyType = RigidbodyType2D.Dynamic;
        transform.position += Vector3.forward * -2;
        Rig.velocity = Velocity;
        Rig.drag = 0.5f;
        ParticleSystem ThisFire = Instantiate(GameData.FireEffect, transform.position, transform.rotation, transform);
        var Main = ThisFire.main;
        Main.startSize = new ParticleSystem.MinMaxCurve(0.2f * Mathf.Sqrt(GunPower), 0.5f * Mathf.Sqrt(GunPower));
        Main.startSpeed = new ParticleSystem.MinMaxCurve(5f * GunPower, 50f * GunPower);
        Main.startLifetime = 0.5f * Mathf.Sqrt(GunPower);
        ThisFire.Play();
        yield return new WaitForSeconds(0.1f);
        Rig.drag = 2;
        ParticleSystem ThisSmoke = Instantiate(GameData.SmokeEffect, transform.position, transform.rotation, transform);
        var MainSmoke = ThisSmoke.main;
        MainSmoke.startSize = new ParticleSystem.MinMaxCurve(0.25f * GunPower, 1f * GunPower);
        MainSmoke.startSpeed = new ParticleSystem.MinMaxCurve(1f * GunPower, 3f * GunPower);
        MainSmoke.startLifetime = 0.5f * GunPower;
        var Emission = ThisSmoke.emission;
        short count = (short)(100 * GunPower * GunPower);
        count = count < 100 ? count : (short)100;
        Emission.SetBurst(0, new ParticleSystem.Burst(0, count));
        ThisSmoke.Play();
        yield return new WaitForSeconds(ThisSmoke.main.startLifetime.constantMax);
        Destroy(gameObject);
        yield break;
    }

    public void RocketFire(Vector2 Velocity, float GunPower)
    {
        StartCoroutine(RocketFireCour(Velocity, GunPower));
    }
    private IEnumerator RocketFireCour(Vector2 Velocity, float GunPower)
    {
        Rig.bodyType = RigidbodyType2D.Dynamic;
        transform.position += Vector3.forward * -2;
        Rig.velocity = Velocity;
        Rig.drag = 0.5f;
        ParticleSystem ThisFire = Instantiate(GameData.FireRocketEffect, transform.position, transform.rotation, transform);
        var Main = ThisFire.main;
        Main.startSize = new ParticleSystem.MinMaxCurve(0.2f * Mathf.Sqrt(GunPower), 0.5f * Mathf.Sqrt(GunPower));
        Main.startSpeed = new ParticleSystem.MinMaxCurve(50f * GunPower, 100f * GunPower);
        Main.startLifetime = 0.3f * Mathf.Sqrt(GunPower);
        ThisFire.Play();
        yield return new WaitForSeconds(0.1f);
        Rig.drag = 2;
        ParticleSystem ThisSmoke = Instantiate(GameData.SmokeEffect, transform.position, transform.rotation, transform);
        var MainSmoke = ThisSmoke.main;
        MainSmoke.startSize = new ParticleSystem.MinMaxCurve(0.25f * GunPower, 1f * GunPower);
        MainSmoke.startSpeed = new ParticleSystem.MinMaxCurve(1f * GunPower, 3f * GunPower);
        MainSmoke.startLifetime = 0.5f * GunPower;
        var Emission = ThisSmoke.emission;
        short count = (short)(100 * GunPower * GunPower);
        count = count < 100 ? count : (short)100;
        Emission.SetBurst(0, new ParticleSystem.Burst(0, count));
        ThisSmoke.Play();
        yield return new WaitForSeconds(ThisSmoke.main.startLifetime.constantMax);
        Destroy(gameObject);
        yield break;
    }

    public void BulletExplosive(float Power)
    {
        if (!GameData.EffectOn)
        {
            Destroy(gameObject);
            return;
        }
        StartCoroutine(BulletExplosiveCour(Power));
    }
    private IEnumerator BulletExplosiveCour(float Power)
    {
        transform.position += Vector3.forward * -2;
        Vector3 ExplosivePos = transform.position + Vector3.forward * -1;
        ParticleSystem ThisFire = Instantiate(GameData.BulletExplosive, ExplosivePos, transform.rotation, transform);
        var Main = ThisFire.main;
        Main.startSize = new ParticleSystem.MinMaxCurve(0.1f * Mathf.Sqrt(Power), 0.5f * Mathf.Sqrt(Power));
        Main.startSpeed = new ParticleSystem.MinMaxCurve(0, 10f * Power);
        Main.startLifetime = 0.5f * Mathf.Sqrt(Power);
        ThisFire.Play();
        yield return new WaitForSeconds(0.1f);
        ParticleSystem ThisSmoke = Instantiate(GameData.ExplosiveSmoke, transform.position, transform.rotation, transform);
        var MainSmoke = ThisSmoke.main;
        MainSmoke.startSize = new ParticleSystem.MinMaxCurve(0.5f * Power, 1f * Power);
        MainSmoke.startSpeed = new ParticleSystem.MinMaxCurve(0, 5 * Power);
        MainSmoke.startLifetime = 0.5f * Power;
        ThisSmoke.Play();
        yield return new WaitForSeconds(ThisSmoke.main.startLifetime.constantMax);
        Destroy(gameObject);
        yield break;
    }


    public void ArmorHit(float Power)
    {
        if (!GameData.EffectOn)
        {
            Destroy(gameObject);
            return;
        }
        StartCoroutine(ArmorHitCour(Power));
    }
    private IEnumerator ArmorHitCour(float Power)
    {
        Rig.bodyType = RigidbodyType2D.Kinematic;
        ParticleSystem ThisHit = Instantiate(GameData.ArmorHit, transform.position, transform.rotation, transform);
        var Main = ThisHit.main;
        Main.startSize = new ParticleSystem.MinMaxCurve(0.1f * Mathf.Sqrt(Power), 0.2f * Mathf.Sqrt(Power));
        Main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 10f * Power);
        Main.startLifetime = 0.5f * Mathf.Sqrt(Power);
        ThisHit.Play();
        yield return new WaitForSeconds(Main.startLifetime.constantMax);
        Destroy(gameObject);
        yield break;
    }

    public void PartDestroyed(float Power)
    {
        if (!GameData.EffectOn)
        {
            Destroy(gameObject);
            return;
        }
        StartCoroutine(PartDestroyedCour(Power));
    }
    private IEnumerator PartDestroyedCour(float Power)
    {
        Rig.bodyType = RigidbodyType2D.Kinematic;
        ParticleSystem ThisHit = Instantiate(GameData.PartDestoryed, transform.position, transform.rotation, transform);
        var Main = ThisHit.main;
        Main.startSize = new ParticleSystem.MinMaxCurve(0.5f * Power, 1f * Power);
        Main.startSpeed = new ParticleSystem.MinMaxCurve(0, 0);
        Main.startLifetime = 1f * Mathf.Sqrt(Power);
        ThisHit.Play();
        yield return new WaitForSeconds(Main.startLifetime.constantMax);
        Destroy(gameObject);
        yield break;
    }

    public void VehicleExplosive(float Power)
    {
        if (!GameData.EffectOn)
        {
            Destroy(gameObject);
            return;
        }
        StartCoroutine(VehicleExplosiveCour(Power));
    }
    private IEnumerator VehicleExplosiveCour(float Power)
    {
        transform.position += Vector3.forward * -2;
        Vector3 ExplosivePos = transform.position + Vector3.forward * -1;
        ParticleSystem ThisFire = Instantiate(GameData.Explosive, ExplosivePos, transform.rotation, transform);
        var Main = ThisFire.main;
        Main.startSize = new ParticleSystem.MinMaxCurve(0.5f * Mathf.Sqrt(Power), 1f * Mathf.Sqrt(Power));
        Main.startSpeed = new ParticleSystem.MinMaxCurve(0, 25f * Power);
        Main.startLifetime = 1f * Mathf.Sqrt(Power);
        ThisFire.Play();
        yield return new WaitForSeconds(0.1f);
        ParticleSystem ThisSmoke = Instantiate(GameData.ExplosiveSmoke, transform.position, transform.rotation, transform);
        var MainSmoke = ThisSmoke.main;
        MainSmoke.startSize = new ParticleSystem.MinMaxCurve(1f * Power, 2f * Power);
        MainSmoke.startSpeed = new ParticleSystem.MinMaxCurve(0, 20);
        MainSmoke.startLifetime = 2f * Power;
        ThisSmoke.Play();
        yield return new WaitForSeconds(ThisSmoke.main.startLifetime.constantMax);
        Destroy(gameObject);
        yield break;
    }

    public void ObjectDestroyed(float Power, Color color)
    {
        StartCoroutine(ObjectDestroyedCour(Power, color));
    }
    private IEnumerator ObjectDestroyedCour(float Power, Color color)
    {
        Rig.bodyType = RigidbodyType2D.Kinematic;
        transform.position += Vector3.forward * -2;
        Vector3 ExplosivePos = transform.position + Vector3.forward * -1;
        ParticleSystem ThisFire = Instantiate(GameData.ObjectDestoryed, ExplosivePos, transform.rotation, transform);
        var Main = ThisFire.main;
        Main.startSize = new ParticleSystem.MinMaxCurve(0.5f * Mathf.Sqrt(Power), 1.25f * Mathf.Sqrt(Power));
        Main.startSpeed = new ParticleSystem.MinMaxCurve(0, 10f * Power);
        Main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.35f, 0.35f, 0.35f), color);
        Main.startLifetime = Mathf.Sqrt(Power);
        var Emission = ThisFire.emission;
        float count = Power * 50;
        Emission.SetBurst(0, new ParticleSystem.Burst(0, count, 1, 0.25f));

        ThisFire.Play();
        yield return new WaitForSeconds(Main.startLifetime.constantMax);
        Destroy(gameObject);
        yield break;
    }
}
