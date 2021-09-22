using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Marker : MonoBehaviour
{
    private Tank player;
    public Transform[] Crosshair;
    public TextMeshProUGUI Text;
    [Range(0, 1f)]
    public float CrosshairSize;
    public float Size = 0.33f;
    public enum CrosshairType {Simple, OnEnemy}
    private Coroutine FireCoroutine;


    public void Fire()
    {
        if(FireCoroutine == null)
        {
            FireCoroutine = StartCoroutine(FireCour());
        }
        else
        {
            StopCoroutine(FireCoroutine);
            FireCoroutine = StartCoroutine(FireCour());
        }
    }
    private IEnumerator FireCour()
    {
        for(int i = 0; i < 10; i++)
        {
            CrosshairSize = i / 2;
            yield return new WaitForFixedUpdate();
        }
        for (int i = 50; i > 0; i--)
        {
            CrosshairSize = i / 10;
            yield return new WaitForFixedUpdate();
        }
        CrosshairSize = 0;
        FireCoroutine = null;
        yield break;
    }
    private void SetCrosshairSize()
    {
        Crosshair[0].localPosition = Vector2.Lerp(Crosshair[0].localPosition, new Vector2(0, -0.5f - CrosshairSize), 0.1f);
        Crosshair[1].localPosition = Vector2.Lerp(Crosshair[1].localPosition, new Vector2(-0.5f - CrosshairSize, 0), 0.1f);
        Crosshair[2].localPosition = Vector2.Lerp(Crosshair[2].localPosition, new Vector2(0, 0.5f + CrosshairSize), 0.1f);
        Crosshair[3].localPosition = Vector2.Lerp(Crosshair[3].localPosition, new Vector2(0.5f + CrosshairSize, 0), 0.1f);
    }
    public void SetAccuracy(float i)
    {
        CrosshairSize = i;
    }
    public void SetSize(float i)
    {
        Size = (i + 0.33f);
        transform.localScale = Vector3.one * Size;
    }
    public void FixedUpdate()
    {
        SetCrosshairSize();
        transform.up = Camera.main.transform.up;
    }
}
