using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPart : MonoBehaviour
{
    public float ChanseTowerHit;
    [Range(0, 100)]
    public int BoomChanse;
    public TowerLimit[] TowersLimit; 
    public float MaxTowerMass;
    public float MaxEngineMass;
    public float MaxEnginePower;
    public float MaxTrackMass;
}
[System.Serializable]
public struct TowerLimit
{
    public float MinAngle;
    public float MaxAngle;
}

