using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPart : MonoBehaviour
{
    public int Index;
    public int Hp;
    public int MaxHp;
    public float Mass;
    public float StrongMM;
    public StaticObject Main;
    public SpriteRenderer Renderer;

    public void Start()
    {
        Hp = MaxHp;
    }
    public void SetColor(Color color)
    {
        Renderer.color = color;
    }

    public void GetHit(int Damage)
    {
        Hp -= Damage;
        if(Hp <= 0)
        {
            OnDestroyed();
        }
    }
    public void OnDestroyed()
    {
        Main.OnPartDestroyed(Index);
        gameObject.SetActive(false);
    }
}
