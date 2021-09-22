using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialPart : MonoBehaviour
{
    public enum Type {ArtHelp}
    public Type SpecialType;
    public float ReloadTime;
    public bool Reloaded;
    public bool Reloading;
    public bool ActionOn;
    public Cannon Art;
    public int ArtNum;

    public Part main;

    public void Action()
    {
        if (ActionOn)
            return;
        if(Reloaded)
        {
            Reloaded = false;
            switch (SpecialType)
            {
                case Type.ArtHelp:
                    StartCoroutine(ArtHelpAction());
                    break;
            }
        }
        else
        {
            StartCoroutine(MakeReload());
        }
        
    }
    private void Start()
    {
        StartCoroutine(MakeReload());    
    }

    private IEnumerator MakeReload()
    {
        if (Reloading)
            yield break;
        if(!Reloaded)
        {
            Reloading = true;
            yield return new WaitForSeconds(ReloadTime);
            Reloaded = true;
        }
        Reloading = false;
        yield break;
    }
    private IEnumerator ArtHelpAction()
    {
        ActionOn = true;
        Vector2 Mouse = Controller.MousePosition;
        float Lenght = EndlessTerrain.MaxDistanceView * 2;

        yield return new WaitForSeconds(1f);
        for(int i = 0; i < ArtNum; i++)
        {
            Vector2 Pos = Mouse + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * Lenght;
            Vector2 Dir = (Mouse - Pos).normalized;

            Cannon cannon = Instantiate(Art.gameObject, Pos, Quaternion.identity, null).GetComponent<Cannon>();
            cannon.OnStart();
            cannon.transform.up = Dir;
            cannon.Fire(Vector2.zero, Mouse, null, SideOwn.type.Netral);
            yield return new WaitForSeconds(0.5f);
            Destroy(cannon.gameObject);
        }
        ActionOn = false;
        StartCoroutine(MakeReload());
        yield break;
    }
}
