using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Informer
{
    public static string TankInfo(Tank tank)
    {
        string Level = tank.Level.ToString();
        string Name = tank.Name;
        string Hp = tank.Hp.ToString() + "/" + tank.CalculateMaxHp();

        return Name + "\n" + "Level " + Level + "\n" + "Health " + Hp;
    }
}
