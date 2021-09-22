using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuildUi : MonoBehaviour
{
    public enum NavigationType { Tanks, Bodies, Towers, Cannons, Engines, Tracks };
    public NavigationType Navigation;
    public int NowNavigation;
    public bool MainClosed;
    public bool InfoClosed = true;
    public int SelectedPage;
    private int CountOnPage = 15;
    private int Selected;
    public Transform SpawnPos;
    public TankInfo[] Tanks;
    public PartInfo[] Parts;
    public PartSelect[] PartsSelect;
    public TextMeshProUGUI PartSelectText;
    public GameObject NowView;
    public Tank NowTank;
    public Part NowPart;
    public Transform TankPosition;
    public Animator anim;
    public GameData gameData;
    public Scene Level;

    public void Start()
    {
        GameData.InBuild = true;
        InfoClosed = true;
        MainClosed = true;

        OpenPartSelect();
    }

    public void ClosePartInfo()
    {
        anim.SetTrigger("ClosePartInfo");
    }

    public void SwipePartSelect(bool Right)
    {
        if (Right)
        {
            NowNavigation++;
            if (NowNavigation >= 6)
            {
                NowNavigation = 0;
            }
        }
        else
        {
            NowNavigation--;
            if (NowNavigation < 0)
            {
                NowNavigation = 5;
            }
        }
        OpenPartSelect();
    }
    private void OpenPartSelect()
    {
        Navigation = (NavigationType)NowNavigation;
        PartSelectText.text = Navigation.ToString();
        UpdatePartsInfo(Navigation);
    }
    private void UpdatePartsInfo(NavigationType type)
    {
        switch (type)
        {
            case NavigationType.Tanks:
                Tanks = gameData.GetSortedTanks();
                for (int i = SelectedPage; i < SelectedPage + 15; i++)
                {
                    if (i + SelectedPage < Tanks.Length)
                    {
                        UpdatePart(i, new PartSelectInfo(Tanks[i]));
                    }
                    else
                    {
                        HidePart(i);
                    }
                }
                break;
            case NavigationType.Bodies:
                Parts = gameData.GetSortedParts(Part.Type.Body);
                for (int i = SelectedPage * CountOnPage; i < (SelectedPage + 1) * CountOnPage; i++)
                {
                    if (i < Parts.Length)
                    {
                        UpdatePart(i, new PartSelectInfo(Parts[i]));
                    }
                    else
                    {
                        HidePart(i);
                    }
                }
                break;
            case NavigationType.Cannons:
                Parts = gameData.GetSortedParts(Part.Type.Gun);
                for (int i = SelectedPage * CountOnPage; i < (SelectedPage + 1) * CountOnPage; i++)
                {
                    if (i < Parts.Length)
                    {
                        UpdatePart(i, new PartSelectInfo(Parts[i]));
                    }
                    else
                    {
                        HidePart(i);
                    }
                }
                break;
            case NavigationType.Engines:
                Parts = gameData.GetSortedParts(Part.Type.Engine);
                for (int i = SelectedPage * CountOnPage; i < (SelectedPage + 1) * CountOnPage; i++)
                {
                    if (i < Parts.Length)
                    {
                        UpdatePart(i, new PartSelectInfo(Parts[i]));
                    }
                    else
                    {
                        HidePart(i);
                    }
                }
                break;
            case NavigationType.Towers:
                Parts = gameData.GetSortedParts(Part.Type.Tower);
                for (int i = SelectedPage * CountOnPage; i < (SelectedPage + 1) * CountOnPage; i++)
                {
                    if (i < Parts.Length)
                    {
                        UpdatePart(i, new PartSelectInfo(Parts[i]));
                    }
                    else
                    {
                        HidePart(i);
                    }
                }
                break;
            case NavigationType.Tracks:
                Parts = gameData.GetSortedParts(Part.Type.Track);
                for (int i = SelectedPage * CountOnPage; i < (SelectedPage + 1) * CountOnPage; i++)
                {
                    if (i < Parts.Length)
                    {
                        UpdatePart(i, new PartSelectInfo(Parts[i]));
                    }
                    else
                    {
                        HidePart(i);
                    }
                }
                break;
        }
    }

    private void UpdatePart(int part, PartSelectInfo info)
    {
        PartsSelect[part].Rect.gameObject.SetActive(true);
        PartsSelect[part].Name.text = info.Name;
        PartsSelect[part].Nation.text = info.Nation;
        PartsSelect[part].Level.text = info.Level.ToString();
        PartsSelect[part].Type.text = info.Type;
        if(info.CanTake)
        {
            PartsSelect[part].Icon.color = Color.white;
        }
        else
        {
            PartsSelect[part].Icon.color = Color.black;
        }

    }
    private void HidePart(int part)
    {
        PartsSelect[part].Rect.gameObject.SetActive(false);
    }

    public void OnPartSelected(int part)
    {
        Selected = part + SelectedPage * CountOnPage;
        OpenInfo();
        if(Navigation == NavigationType.Tanks)
        {
            CreateViewTank(part);
        }
        else
        {
            CreateViewPart(part);
        }
        
    }
    public void SpawnSelected()
    {
        switch(Navigation)
        {
            case NavigationType.Tanks:
                break;
            case NavigationType.Bodies:
                PartInfo body = gameData.GetPartInfo(Selected, Part.Type.Body);
                if(body.buildInfo.CanSpawn())
                {
                    gameData.Bodies[Selected].buildInfo.Spawn();
                    SpawnPart(body.part);
                }
                break;
        }
    }
    private void SpawnPart(Part part)
    {
        Vector3 Pos = new Vector3(SpawnPos.position.x, SpawnPos.position.y, -1);
        Part NowPart = Instantiate(part, Pos, SpawnPos.rotation, null);
        NowPart.AddForce(new Vector2(10f, 0), 30f);
    }

    public void CreateViewTank(int tank)
    {
        if(NowView != null)
        {
            Destroy(NowView);
            NowView = null;
            NowTank = null;
            NowPart = null;
        }
        NowTank = Instantiate(Tanks[tank].tank);
        NowTank.MakeLikeImage(TankPosition);
        NowTank.SetColor(Tanks[tank].color);
        NowView = NowTank.gameObject;
    }
    public void CreateViewPart(int part)
    {
        if (NowView != null)
        {
            Destroy(NowView);
            NowView = null;
            NowTank = null;
            NowPart = null;
        }
        NowPart = Instantiate(Parts[part].part);
        NowPart.MakeLikeImage(TankPosition);
        NowPart.SetColor(Parts[part].color);
        NowView = NowPart.gameObject;
    }

    public void OpenCloseMain()
    {
        if (MainClosed)
        {
            OpenMain();
        }
        else
        {
            CloseMain();
        }
    }
    private void CloseMain()
    {
        if(!MainClosed)
            anim.SetTrigger("CloseMain");
        MainClosed = true;
    }
    private void OpenMain()
    {
        if(MainClosed)
            anim.SetTrigger("OpenMain");
        MainClosed = false;
    }

    public void OpenCloseInfo()
    {
        if (InfoClosed)
        {
            OpenInfo();
        }
        else
        {
            CloseInfo();
        }
    }
    private void CloseInfo()
    {
        if(!InfoClosed)
            anim.SetTrigger("ClosePartInfo");
        InfoClosed = true;
    }
    private void OpenInfo()
    {
        if(InfoClosed)
            anim.SetTrigger("OpenPartInfo");
        InfoClosed = false;
    }
}

[System.Serializable]
public struct PartSelect
{
    public Transform Rect;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Level;
    public TextMeshProUGUI Nation;
    public TextMeshProUGUI Type;
    public Image Icon;
}
[System.Serializable]
public struct PartInfoBar
{
    public Transform Rect;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Level;
    public TextMeshProUGUI Nation;
    public TextMeshProUGUI Type;

    public Transform BodyInfo;
    public Transform CannonInfo;
    public Transform TowerInfo;
    public Transform TrackInfo;
    public Transform EngineInfo;
}

[System.Serializable]
public struct PartSelectInfo
{
    public string Name;
    public string Nation;
    public string Type;
    public int Level;
    public bool CanTake;

    public PartSelectInfo(PartInfo part)
    {
        Name = part.Name;
        Nation = "Soviet";
        Level = part.level;
        Type = part.part.PartType.ToString();
        CanTake = part.buildInfo.CanSpawn();
    }
    public PartSelectInfo(TankInfo tank)
    {
        Name = tank.Name;
        Nation = "Soviet";
        Level = (int)tank.level;
        Type = "Tank";
        CanTake = tank.buildInfo.CanSpawn();
    }
}
