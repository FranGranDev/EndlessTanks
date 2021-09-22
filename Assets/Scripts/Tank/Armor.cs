using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Armor : MonoBehaviour
{
    public int ArmorMM;
    [Range(0.2f, 5f)]
    public float DamageCoefficient;
    public int[] LinkToDamage;

    public Part Main;
    private void Start()
    {
        Main = transform.parent.parent.GetComponent<Part>();
    }

    public void Hit(int Damage, float LinkDamage, Vector2 Position, float Force, Vector2 Direction, Tank Enemy, Action callback)
    {
        int CurrenteDamage = Mathf.RoundToInt(Damage * DamageCoefficient * GameData.DamageC);
        Main.GetHitFromArmor(CurrenteDamage, LinkDamage, LinkToDamage, Enemy, callback);
        if(Main.Parent != null)
        {
            Main.Parent.GetForce(Position, Direction, Force);
        }
        else
        {
            Main.GetForce(Position, Direction, Force);
        }
    }


}
