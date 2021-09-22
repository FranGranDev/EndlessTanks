using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    [Range(0, 10000)]
    public float HorsePower;
    [Range(0, 5f)]
    public float MaxSpeed;
    [Range(0, 100f)]
    public float MaxMass;
}
