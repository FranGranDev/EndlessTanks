using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideOwn : MonoBehaviour
{
    public enum type {BlueTeam, RedTeam, Own, Netral}
    public type Team;
    public bool isEnemy(type team)
    {
        return team != Team && team != type.Netral;
    }
}
