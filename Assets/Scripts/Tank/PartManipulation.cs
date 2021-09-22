using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller))]
public class PartManipulation : MonoBehaviour
{
    private Vector2 Velocity;
    private Vector2 PrevMouse;
    private Vector2 TakenPosition;
    private Animator[] PrevAnim;
    public Part NowPart;
    private Vector2 PrevPartPos;
    private bool TakenFromTank;
    private bool isNowPartBody;
    public const float PinSetOffset = 1f;
    private Controller controller;
    public Tank tank;
    public bool Enable;

    private void Start()
    {
        PrevAnim = new Animator[0];
        controller = GetComponent<Controller>();
        tank = GetComponent<Tank>();
    }

    public static Part.Linking GetNearestLink(Transform transform, Part part, int Pin, Part.Linking[] Links, bool[] Taken)
    {
        float Min = float.MaxValue;
        int MinIndex = -1;
        bool SideCheck = part.PartType == Part.Type.Track;
        Vector2 PartOffset = part.transform.InverseTransformPoint(transform.position);
        for (int i = 0; i < Links.Length; i++)
        {
            if (Links[i].PartType == part.PartType)
            {
                Vector2 PointOffset = Links[i].Point.transform.InverseTransformPoint(transform.position);
                if (i > 0 && Taken[i] || (SideCheck && PartOffset.x * PointOffset.x < 0))
                    continue;
                float Lenght = ((Vector2)part.Pin[Pin].transform.position - (Vector2)Links[i].Point.position).magnitude;
                if (Lenght > PinSetOffset)
                    continue;
                if (Lenght < Min)
                {
                    Min = Lenght;
                    MinIndex = i;
                }
            }
        }
        if (MinIndex == -1)
            return null;
        return Links[MinIndex];

    }
    public static Part.Linking[] GetLinksForPins(Transform transform, Part part, Part.Linking[] Links)
    {
        Part.Linking[] ThisLinks = new Part.Linking[part.Pin.Length];
        bool[] Taken = new bool[Links.Length];
        for(int i = 0; i < part.Pin.Length; i++)
        {
            ThisLinks[i] = GetNearestLink(transform, part, i, Links, Taken);
            if (ThisLinks[i] != null)
                Taken[ThisLinks[i].Index] = true;
        }
        return ThisLinks;
    }
    public static Vector2 GetSumLinksPosition(Part.Linking[] Links)
    {
        Vector2 Position = Vector2.zero;
        for(int i = 0; i < Links.Length; i++)
        {
            Position += (Vector2)Links[i].Point.position;
        }
        return Position / Links.Length;
    }
    bool PinsFit(Part part, Part.Linking[] Links)
    {
        Part.Linking[] ThisLinks = GetLinksForPins(transform, part, Links);
        for(int i = 0; i < ThisLinks.Length; i++)
        {
            if (ThisLinks[i] == null || ThisLinks[i].Installed)
            {
                return false;
            }
        }
        return true;
    }

    Part.Linking GetLinksOfType(Part.Linking[] Links, Part.Type PartType)
    {
        return Links[0];
    }

    private bool CanTakePart(Collider2D Collider)
    {
        if (Collider == null)
            return false;
        Part part = Collider.GetComponent<Part>();
        return (part != null && !part.NotInteractive && (part.ParentLink == null || !part.ParentLink.Static) && (part.Parent == null || part.Parent == tank || part.Parent.Destroyed) && NowPart == null);
    }
    private bool CanTakeBodyPart(Part part)
    {
        return (part.PartType == Part.Type.Body);
    }
    private void TryTakePart(Collider2D Collider)
    {
        if (CanTakePart(Collider))
        {
            TakenPosition = PrevMouse;
            Part part = Collider.GetComponent<Part>();
            
            if (isNowPartBody = CanTakeBodyPart(part))
            {
                tank.OnReplacingBody(false);
                part.ThrowLinks(3f);
                part.OnTake();

                NowPart = part;
            }
            else
            {
                TakenFromTank = part.OnTank;
                PrevPartPos = part.transform.position;

                if(part.ParentLink != null && part.ParentLink.Parent != null)
                {
                    part.ParentLink.Parent.Links[part.NumInLinks].DeInstallPart(part);
                }
                TakenPosition = PrevMouse;
                NowPart = part;
                NowPart.OnTake();

                NowPart.transform.up = transform.up;
            }
        }
    }
    private bool CanThrowPart()
    {
        Vector2 Cam = Controller.MousePosition;
        Vector3 MousePos = new Vector3(Cam.x, Cam.y, -3f);
        RaycastHit2D[] hit = Physics2D.RaycastAll(MousePos, Vector3.forward, 2f, 1 << 9);
        for(int i = 0; i < hit.Length; i++)
        {
            if (hit[i].collider != null && hit[i].collider.transform.root.GetComponent<Part>() != NowPart)
                return false;
        }
        return true;
    }

    private bool CanInstallPart(Part.Linking link)
    {
        if(link.PartType == NowPart.PartType)
        {
            switch(link.PartType)
            {
                case Part.Type.Body:
                    return true;
                case Part.Type.Engine:
                    if(link.Parent.GetComponent<BodyPart>().MaxEngineMass >= NowPart.Mass)
                    {
                        return true;
                    }
                    else
                    {
                        Debug.Log("Engine too heavy");
                        return false;
                    }
                case Part.Type.Gun:
                    if(link.Parent.GetComponent<Tower>().MaxGunMass < NowPart.Mass)
                    {
                        Debug.Log("Gun too heavy");
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case Part.Type.Tower:
                    if(link.Parent.GetComponent<BodyPart>().MaxTowerMass >= NowPart.Mass)
                    {
                        return true;
                    }
                    else
                    {
                        Debug.Log("Tower too heavy");
                        return false;
                    }
                case Part.Type.Track:
                    if (link.Parent.GetComponent<BodyPart>().MaxTrackMass >= NowPart.Mass)
                    {
                        return true;
                    }
                    else
                    {
                        Debug.Log("Track too heavy");
                        return false;
                    }
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    private void TryInstallPart()
    {
        if (isNowPartBody)
        {
            Vector2 Cam = Controller.MousePosition;
            Vector3 MousePos = new Vector3(Cam.x, Cam.y, -1f);
            Collider2D[] col = Physics2D.OverlapPointAll(MousePos, 1 << 9);
            if (CanReplaceBody(col))
            {
                tank.Main.part.ThrowLinks(1f);
                tank.Main.part.OnDrop(Vector2.zero);
                SetBody(NowPart);
            }
            else if(NowPart == tank.Main.part)
            {
                NowPart.transform.localPosition = Vector2.zero;
            }
            else
            {
                NowPart.transform.position = MousePos;
            }
        }
        else
        {
            Part.Linking[] Links = tank.Main.part.Links;

            if(PinsFit(NowPart, Links))
            {
                if (CanInstallPart(GetLinksForPins(transform, NowPart, Links)[0]))
                {
                    Part.Linking[] LinksToInstall = GetLinksForPins(transform, NowPart, Links);
                    InstallPart(tank, NowPart, LinksToInstall);
                    NowPart = null;
                    return;
                }
                else
                {
                    NowPart.transform.position = TakenPosition;
                }
            }
            else
            {
                for (int i = 0; i < Links.Length; i++)
                {
                    if(Links[i].InstalledPart != null)
                    {
                        Part.Linking[] SubLinks = Links[i].InstalledPart.Links;
                        if (PinsFit(NowPart, SubLinks))
                        {
                            if (CanInstallPart(GetLinksForPins(transform, NowPart, SubLinks)[0]))
                            {
                                Part.Linking[] LinksToInstall = GetLinksForPins(transform, NowPart, SubLinks);
                                InstallPart(tank, NowPart, LinksToInstall);
                                NowPart = null;
                                return;
                            }
                            else
                            {
                                NowPart.transform.position = TakenPosition;
                            }
                        }
                    }
                    
                }
            }
            
            /*
            Part.Linking[] Links = tank.Main.part.Links;
            for (int i = 0; i < Links.Length; i++)
            {
                if (Links[i].Installed)
                {
                    for (int a = 0; a < Links[i].InstalledPart.Links.Length; a++)
                    {
                        Part.Linking link = Links[i].InstalledPart.Links[a];
                        if (((Vector2)link.Point.position - PrevMouse).magnitude < 0.5f)
                        {
                            if (CanInstallPart(link) && !link.Installed)
                            {
                                InstallPart(NowPart, i, a);
                                return;
                            }
                            if(TakenFromTank)
                            {
                                ReinstallPart();
                            }
                            else
                            {
                                NowPart.transform.position = TakenPosition;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    if (((Vector2)Links[i].Point.position - PrevMouse).magnitude < 0.5f)
                    {
                        if (CanInstallPart(Links[i]))
                        {
                            InstallPart(NowPart, i, -1);
                            return;
                        }
                        if (TakenFromTank)
                        {
                            ReinstallPart();
                        }
                        else
                        {
                            NowPart.transform.position = TakenPosition;
                        }
                        break;
                    }
                }
                
            }
            */
        }

        if (NowPart != null)
        {
            if (CanThrowPart() || isNowPartBody)
            {
                NowPart.OnDrop(Velocity);
            }
            else if (TakenFromTank)
            {
                ReinstallPart();
            }
            else
            {
                NowPart.transform.position = TakenPosition;
            }
        }
        NowPart = null;
    }
    private bool CanReplaceBody(Collider2D[] hit)
    {
        if(hit.Length > 1)
        {
            Part part1 = hit[0].transform.GetComponent<Part>();
            Part part2 = hit[1].transform.GetComponent<Part>();
            if (part1.PartType == Part.Type.Body && part2.PartType == Part.Type.Body && ((part1.Parent == tank) ||
                (part2.Parent == tank)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
        
    }
    public void SetBody(Part part)
    {
        if(tank.Main.NotNull)
        {
            tank.Main.part.transform.position = TakenPosition;
            tank.Main.part.OnDeInstalled(false);
        }
        part.ChangeBodyParent(tank);
        tank.ClearParts();
        for (int i = 0; i < part.Links.Length; i++)
        {
            if(part.Links[i].Static)
            {
                part.Links[i].SetBody();
            }
        }
    }

    public static void InstallPart(Tank tank, Part part, Part.Linking[] Links)
    {
        for(int i = 0; i < part.Pin.Length; i++)
        {
            if (i == 0)
            {
                Vector2 TempPos = Links[i].Point.localPosition;
                Links[i].Point.localPosition = (Vector3)TempPos + new Vector3(0, 0, Links[i].ThisHeight);

                Vector2 Pos = GetSumLinksPosition(Links);
                Links[i].Point.localScale = Vector3.one;

                part.transform.position = Pos;
                Links[i].InstallPart(part);
            }
            else
            {
                Links[i].SubInstallPart(part, i);
            }
        }
    }
    public void InstallPart(Part part, int ParentLinkIndex, int[] SubLink, float[] Position)
    {
        if(ParentLinkIndex == -1)
        {
            part.transform.position = (Vector2)transform.position + new Vector2(Position[0], Position[1]);
            Part.Linking[] Links = GetLinksForPins(transform, part, tank.Main.part.Links);

            for (int i = 0; i < part.Pin.Length; i++)
            {
                if (Links[i] == null)
                    continue;
                if (i == 0)
                {
                    Vector2 Pos = GetSumLinksPosition(Links);
                    Links[i].Point.localScale = Vector3.one;

                    part.transform.position = Pos;
                    Links[i].InstallPart(part);
                }
                else
                {
                    Links[i].SubInstallPart(part, i);
                }
            }
        }
        else
        {
            part.transform.position = (Vector2)transform.position + new Vector2(Position[0], Position[1]);
            Part.Linking[] Links = GetLinksForPins(transform, part, tank.Main.part.Links[ParentLinkIndex].InstalledPart.Links);

            for (int i = 0; i < part.Pin.Length; i++)
            {
                if (i == 0)
                {
                    Vector2 Pos = GetSumLinksPosition(Links);
                    Links[i].Point.localScale = Vector3.one;

                    part.transform.position = Pos;
                    Links[i].InstallPart(part);
                }
                else
                {
                    Links[i].SubInstallPart(part, i);
                }
            }
        }
    }

    public void ReinstallPart()
    {
        if (NowPart == null)
            return;
        NowPart.transform.position = PrevPartPos;
        TryInstallPart();
        NowPart = null;
    }
    public void SetTempLinks(Part part)
    {
        PrevPartPos = part.transform.position;
    }

    private void Manipulate()
    {
        if (!controller.isManipulation())
        {
            if(NowPart != null)
            {
                TryInstallPart();
            }
            return;
        }
            
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector2 Cam = Controller.MousePosition;
            Vector3 MousePos = new Vector3(Cam.x, Cam.y, -3f);

            Collider2D col = Physics2D.OverlapPoint(MousePos, 1 << 9);
            TryTakePart(col);
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (NowPart != null)
            {
                TryInstallPart();
            }
        }
    }
    private void CalculateVelocity()
    {
        Velocity = ((Vector2)controller.mainCamera.ScreenToWorldPoint(Input.mousePosition) - PrevMouse) / Time.deltaTime * 0.5f;
        PrevMouse = controller.mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }
    private void FollowPart()
    {
        if(NowPart != null)
        {
            Vector3 Position = (Vector3)Controller.MousePosition + Vector3.back * 2f;
            NowPart.transform.position = Position;
            if ((tank.transform.position - NowPart.transform.position).magnitude > 10f)
            {
                for(int i = 0; i < PrevAnim.Length; i++)
                {
                    PrevAnim[i].SetTrigger("Idle");
                }
                PrevAnim = new Animator[0];

                for (int i = 0; i < tank.Main.part.Links.Length; i++)
                {
                    if (tank.Main.part.Links[i].InstalledPart != null)
                    {
                        for (int a = 0; a < tank.Main.part.Links[i].InstalledPart.Links.Length; a++)
                        {
                            Part.Linking link = tank.Main.part.Links[i].InstalledPart.Links[a];
                            Vector2 Pos = link.Point.localPosition;
                            link.Point.localPosition = (Vector3)Pos + new Vector3(0, 0, link.ThisHeight);
                        }
                    }
                    else
                    {
                        Vector2 Pos = tank.Main.part.Links[i].Point.localPosition;
                        tank.Main.part.Links[i].Point.localPosition = (Vector3)Pos + new Vector3(0, 0, tank.Main.part.Links[i].ThisHeight);
                    }

                }
                return;
            }
            if(PrevAnim.Length == 0)
            {
                Part.Linking[] Links = tank.Main.part.Links;
                for (int i = 0; i < Links.Length; i++)
                {
                    if (Links[i].Installed || Links[i].SubInstalled)
                    {
                        for (int a = 0; a < Links[i].InstalledPart.Links.Length; a++)
                        {
                            Part.Linking link = Links[i].InstalledPart.Links[a];
                            Animator anim = link.Point.GetComponent<Animator>();
                            if (link.PartType == NowPart.PartType && !link.Installed)
                            {
                                Animator[] temp = PrevAnim;
                                PrevAnim = new Animator[temp.Length + 1];
                                for (int b = 0; b < temp.Length; b++)
                                {
                                    PrevAnim[b] = temp[b];
                                }
                                PrevAnim[PrevAnim.Length - 1] = anim;
                                anim.SetTrigger("TrySet");
                                if (link.PointOnUp)
                                {
                                    link.Point.position += Vector3.back * 0.1f;
                                }
                            }
                        }
                    }
                    else
                    {
                        Animator anim = Links[i].Point.GetComponent<Animator>();
                        if (Links[i].PartType == NowPart.PartType && !Links[i].Installed)
                        {
                            Animator[] temp = PrevAnim;
                            PrevAnim = new Animator[temp.Length + 1];
                            for (int b = 0; b < temp.Length; b++)
                            {
                                PrevAnim[b] = temp[b];
                            }
                            PrevAnim[PrevAnim.Length - 1] = anim;
                            if (Links[i].PointOnUp)
                            {
                                Links[i].Point.position += Vector3.back * 0.1f;
                            }
                            anim.SetTrigger("TrySet");
                        }
                    }
                    
                }
            }
            
        }
        else if(PrevAnim.Length > 0)
        {
            for (int i = 0; i < PrevAnim.Length; i++)
            {
                PrevAnim[i].SetTrigger("Idle");
            }
            PrevAnim = new Animator[0];
            for (int i = 0; i < tank.Main.part.Links.Length; i++)
            {
                if (tank.Main.part.Links[i].Installed)
                {
                    for (int a = 0; a < tank.Main.part.Links[i].InstalledPart.Links.Length; a++)
                    {
                        Part.Linking link = tank.Main.part.Links[i].InstalledPart.Links[a];
                        Vector2 Pos = link.Point.localPosition;
                        link.Point.localPosition = (Vector3)Pos + new Vector3(0, 0, link.ThisHeight);
                    }
                }
                else
                {
                    Vector2 Pos = tank.Main.part.Links[i].Point.localPosition;
                    tank.Main.part.Links[i].Point.localPosition = (Vector3)Pos + new Vector3(0, 0, tank.Main.part.Links[i].ThisHeight);
                }

            }
        }
    }


    private void Action()
    {
        if (!Enable)
            return;
        Manipulate();
        FollowPart();
    }

    private void Update()
    {
        Action();
    }
    private void FixedUpdate()
    {
        CalculateVelocity();
    }
}
