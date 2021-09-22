using UnityEngine;

public class LandGenerator : MonoBehaviour
{
    public int SizeX;
    public int SizeY;

    public NoiseMap.NormalizeNode NormalizeMod;

    public enum ColoredType { Solid, Gradient }
    public ColoredType LandColorType;
    public LandType[] Land;
    public Gradient LandGradient;

    public int Seed;
    public Vector2 Offset;
    public float Scale;
    public TerrainSettings[] Settings;

    public MeshRenderer meshRenderer;
    public AnimationCurve Curve;

    private void Start()
    {
        
    }
    private void FixedUpdate()
    {

    }
}
