using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour
{
    public enum MoveType {Track, Wheel, Hover}
    public MoveType Move;
    [Range(0, 1f)]
    public float MaxSpeed;
    [Range(0, 100f)]
    public float MaxMass;
    [Range(0, 1f)]
    public float Acceleration;
    [Range(0, 2f)]
    public float RotationSpeed;
    [Range(0, 1f)]
    public float Clutch;

    public static float GetMassOverLoad(float MaxMass, float Mass) //1 - NoLoad 0 - OverLoad
    {
        return MaxMass / Mass < 1 ? MaxMass / Mass : 1;
    }
}
