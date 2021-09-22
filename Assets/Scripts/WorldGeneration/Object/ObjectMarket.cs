using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectMarket : SpecialObject
{
    public GameObject UI;
    public UIIcon[] Icons;
    public Transform Spawner;

    public override void Start()
    {
        UpdateAllIcons();
        UI.SetActive(false);
    }
    public override void OnEnter()
    {
        PlayerOn = true;
        UI.SetActive(true);
    }
    public override void OnExit()
    {
        PlayerOn = false;
        UI.SetActive(false);
    }
    
    private Part[] GetPart(int i)
    {
        return NoiseMap.GenerateStaticPart(MapGenerator.GlobalSeed, i, Icons[i].Bought, Obj.MapPos);
    }
    public void UpdateAllIcons()
    {
        if (!UI.activeSelf)
            UI.SetActive(true);
        for(int i = 0; i < Icons.Length; i++)
        {
            UpdateIcon(i);
        }
    }
    public void UpdateIcon(int i)
    {
        Part[] temppart = GetPart(i);
        Icons[i].part = new Part[temppart.Length];
        for (int a = 0; a < temppart.Length; a++)
        {
            Part thisPart = Instantiate(GetPart(i)[a].gameObject, Icons[i].Icon.position, Quaternion.identity, Icons[i].Icon).GetComponent<Part>();
            thisPart.transform.up = transform.up;
            thisPart.NotInteractive = true;
            thisPart.SetColor(GameData.GetRandColor());
            Icons[i].part[a] = thisPart;
            thisPart.transform.localScale = Vector3.one * 2 / thisPart.Size();
            if (thisPart.Rig != null)
                thisPart.Rig.simulated = false;
            thisPart.enabled = false;
        }
    }
    public void OnButtonClick(int i)
    {
        if(Icons[i].part == null)
        {
            return;
        }

        Icons[i].Bought++;
        for(int a = 0; a < Icons[i].part.Length; a++)
        {
            Part nowPart = Icons[i].part[a];
            Icons[i].part[a] = null;
            nowPart.LikeNewPart();
            nowPart.transform.position = Spawner.position;
            nowPart.AddForce(Spawner.transform.up * Random.Range(5f, 20f), Random.Range(500, 1000f));
        }
        UpdateIcon(i);
    }
}
[System.Serializable]
public struct UIIcon
{
    public Transform Icon;
    public TextMeshProUGUI Text;
    public Part[] part;
    public int Bought;
}
