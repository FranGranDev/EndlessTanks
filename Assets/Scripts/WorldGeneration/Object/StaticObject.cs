using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticObject : MonoBehaviour
{
    public enum Type {Plant, House, Special, Other};
    public Type ObjectType;
    public int MaxHp;
    public float StrongMM;
    public float Mass;
    public float Size;
    public bool Obstacle;
    public float StartHeight;
    public Color color;
    public SpriteRenderer[] Renderer;
    public int Hp;
    public Vector2 PrevHit;
    public Collider2D Collider;
    public ObjectComponent[] Parts;
    public SpriteRenderer[] Floor;
    public Vector2 MapPos;
    private int Index;

    public void Start()
    {
        //OnStart(Vector2.zero, 0, new bool[0], Random.Range(0.75f, 1.25f), Color.green);
        
    }
    public void OnStart(Vector2 MapPos, int Index, bool[] PartsDestroyed, float Size, Color color)
    {
        this.MapPos = MapPos;
        this.Index = Index;

        switch(ObjectType)
        {
            case Type.House:
                {
                    this.color = color;
                    this.Size = Size;
                    
                    for (int i = 0; i < Renderer.Length; i++)
                    {
                        Color thisColor = new Color(color.r, color.g, color.b, Renderer[i].color.a);
                        Renderer[i].color = thisColor;
                    }
                    Hp = MaxHp;

                    SetParts(PartsDestroyed);
                }
                break;
            case Type.Plant:
                {
                    this.color = color;
                    this.Size = Size;
                    for(int i = 0; i < Renderer.Length; i++)
                    {
                        Color thisColor = new Color(color.r, color.g, color.b, Renderer[i].color.a);
                        Renderer[i].color = thisColor;
                    }
                    transform.localScale = Vector3.one * Size;
                    transform.position = transform.position + Vector3.forward * -StartHeight;
                    MaxHp = Mathf.FloorToInt(MaxHp * Size * Size);
                    StrongMM *= Size;
                    Mass *= Size * Size;
                    Hp = MaxHp;

                    Randomize();
                    SetParts(PartsDestroyed);
                }
                break;
            case Type.Other:
                {
                    this.Size = Size;
                    transform.localScale = Vector3.one * Size;
                    transform.position = transform.position + Vector3.forward * -StartHeight;
                    MaxHp = Mathf.FloorToInt(MaxHp * Size * Size);
                    StrongMM *= Size;
                    Mass *= Size * Size;
                    Hp = MaxHp;

                    Randomize();
                    SetParts(PartsDestroyed);
                }
                break;
            case Type.Special:
                {
                    this.color = color;
                    this.Size = Size;

                    for (int i = 0; i < Renderer.Length; i++)
                    {
                        Color thisColor = new Color(color.r, color.g, color.b, Renderer[i].color.a);
                        Renderer[i].color = thisColor;
                    }
                    Hp = MaxHp;

                    SetParts(PartsDestroyed);
                }
                break;
        }
    }

    private void Randomize()
    {
        for(int i = 0; i < Parts.Length; i++)
        {
            Parts[i].Main.transform.position += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
        }
    }
    private void SetParts(bool[] PartsDestroyed)
    {
        for(int i = 0; i < Parts.Length; i++)
        {
            if(i < PartsDestroyed.Length - 1)
            {
                Parts[i].Main.gameObject.SetActive(!PartsDestroyed[i]);
            }
            Parts[i].Main.Main = this;
            Parts[i].Main.Index = i;
            Parts[i].Main.SetColor(color);
        }
        for(int i = 0; i < Floor.Length; i++)
        {
            Floor[i].color = color;
        }
    }

    public void GetHit(int Damage, Vector2 Dir)
    {
        if(!Obstacle)
            return;
        PrevHit = Dir;
        Hp -= Damage;
        if(Hp <= 0)
        {
            DestroySelf();
        }
    }
    public void OnPartDestroyed(int PartIndex)
    {
        Parts[PartIndex].Destroyed = true;
        switch(ObjectType)
        {
            case Type.House:
                EndlessTerrain.OnHousePartDestroyed(MapPos, Index, PartIndex);
                break;
            case Type.Other:
                EndlessTerrain.OnOtherPartDestroyed(MapPos, Index, PartIndex);
                break;
            case Type.Plant:
                EndlessTerrain.OnPlantPartDestroyed(MapPos, Index, PartIndex);
                break;
            case Type.Special:

                break;

        }
        SpriteRenderer Render = Parts[PartIndex].Main.Renderer;
        float Power = Render.bounds.size.magnitude / 10;
        Vector2 Pos = Parts[PartIndex].Main.transform.position + new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0);
        ParticleEffect This = Instantiate(GameData.Particle.gameObject, Pos, transform.rotation, null).GetComponent<ParticleEffect>();
        This.ObjectDestroyed(Power, color);
    }
    public void DestroySelf()
    {
        StartCoroutine(DestroyCour());
    }
    private IEnumerator DestroyCour()
    {
        float Power = 0;
        Collider2D Collider = GetComponent<Collider2D>();
        if (Collider != null)
        {
            Collider.enabled = false;
        }
        if (Renderer.Length > 0)
        {
            Vector2 Dir = ((Vector2)transform.position - PrevHit).normalized;
            Rigidbody2D Rig = gameObject.AddComponent<Rigidbody2D>();
            Rig.gravityScale = 0;
            Rig.drag = 2;
            Rig.velocity += Dir * Random.Range(5, 10);
            Power = Renderer[0].bounds.size.magnitude / 10;
            Texture2D texture = Renderer[0].sprite.texture;
            ParticleEffect This = Instantiate(GameData.Particle.gameObject, transform.position, transform.rotation, transform).GetComponent<ParticleEffect>();
            This.ObjectDestroyed(Power, color);
        }
        else
        {
            for(int i = 0; i < Parts.Length; i++)
            {
                Collider2D col = Parts[i].Main.GetComponent<Collider2D>();
                if (col != null)
                {
                    col.enabled = false;
                }
                Vector2 Dir = ((Vector2)Parts[i].Main.transform.position - PrevHit).normalized;
                Rigidbody2D Rig = Parts[i].Main.gameObject.AddComponent<Rigidbody2D>();
                Rig.gravityScale = 0;
                Rig.drag = 2;
                Rig.velocity += Dir * Random.Range(5, 10);
                SpriteRenderer Render = Parts[i].Main.Renderer;
                Power = Render.bounds.size.magnitude / 5;
                ParticleEffect This = Instantiate(GameData.Particle.gameObject, Parts[i].Main.transform.position, transform.rotation, Rig.transform).GetComponent<ParticleEffect>();
                This.ObjectDestroyed(Power, color);
            }
        }

        switch (ObjectType)
        {
            case Type.House:
                EndlessTerrain.OnHouseDestroyed(MapPos, Index);
                break;
            case Type.Plant:
                EndlessTerrain.OnPlantDestroyed(MapPos, Index);
                break;
            case Type.Other:
                EndlessTerrain.OnOtherDestroyed(MapPos, Index);
                break;
        }
        if (Renderer.Length > 0)
        {
            for (int i = 0; i < 30; i++)
            {
                float Size = (30 - i) / 30;
                transform.localScale = new Vector3(Size, Size, Size);
                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            for (int i = 0; i < 30; i++)
            {
                for(int a = 0; a < Parts.Length; a++)
                {
                    float Size = (30 - a) / 30;
                    Parts[a].Main.transform.localScale = new Vector3(Size, Size, Size);
                }
                
                yield return new WaitForFixedUpdate();
            }
        }
        float time = Mathf.Sqrt(Power) - Time.fixedDeltaTime * 30;
        Destroy(gameObject, time);
        yield break;
    }

}
[System.Serializable]
public struct ObjectComponent
{
    
    public ObjectPart Main;
    public bool Destroyed;
}