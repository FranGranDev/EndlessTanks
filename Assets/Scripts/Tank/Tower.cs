using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public Transform FirePoint;
    private Vector3 GunPosition;
    public float RecoilLenght;
    public bool Static;
    [Range(0, 1f)]
    public float RotationSpeed;
    public bool Obstacle;
    [Range(0, 1f)]
    public float ChanseBodyHit;
    public float MaxGunMass;
    private Coroutine Cour;

    public void Recoil(float time)
    {
        if(Cour != null)
            StopCoroutine(Cour);
        Cour = StartCoroutine(RecoilCour(time));
    }
    private IEnumerator RecoilCour(float time)
    {
        if (FirePoint == null || FirePoint.transform.childCount == 0)
            yield break;
        float Offset = 0;
        float Frames = (time < 2 ? time : 2) * 0.5f * 30;
        if (Frames < 2)
            yield break;
        float BackLenght = 0.4f;
        GunPosition = FirePoint.transform.GetChild(0).localPosition;
        for (int i = 0; i < Frames; i++)
        {
            if(i < Frames * BackLenght)
            {
                Offset -= (1 / Frames) * RecoilLenght; 
            }
            else
            {
                Offset += (1 / Frames) * RecoilLenght * (1 - BackLenght);
            }
            if(FirePoint.transform.childCount > 0)
            {
                FirePoint.transform.GetChild(0).localPosition = GunPosition + Vector3.up * Offset;
            }
            
            yield return new WaitForFixedUpdate();
        }
        FirePoint.transform.GetChild(0).localPosition = GunPosition;
        yield break;
    }
}
