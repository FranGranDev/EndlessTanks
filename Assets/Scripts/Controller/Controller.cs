using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [HideInInspector]
    public Tank tank;
    public static Tank Player;
    public static Controller active;
    public Tank bot;
    public enum ControllTypes {Player, Watching, ControllBot, InBuild};
    public ControllTypes ControllType;
    public Camera mainCamera;
    public static Vector2 MousePosition;
    private Vector2 PrevAimPos;
    public PartManipulation Manipulation;
    public Scene Level;
    public float ManipulateOffset = 1f;
    private float Move;
    private float Rotation;
    private bool Fire;
    private bool Special;
    public bool isManipulation()
    {
        return true;
        //return mainCamera.orthographicSize < ManipulateSize || GameData.InBuild;
    }
    private bool Informed;
    public Marker marker;

    private CameraMovement CameraMove;

    public void Awake()
    {
        active = this;
        Player = tank;
    }
    public void Start()
    {
        tank = GetComponent<Tank>();
        Manipulation = GetComponent<PartManipulation>();
        if (mainCamera != null && mainCamera.GetComponent<CameraMovement>() != null)
        {
            CameraMove = mainCamera.GetComponent<CameraMovement>();
        }
        StartCoroutine(CheckLife());
        SetControll(ControllTypes.Player);
    }

    private void ControllTank()
    {
        MoveInput movement = Movement();
        tank.Drive(movement);

        CameraDeltaSize();
        CameraScope();
        CrossHair();
    }
    private void Watching()
    {
        if(Input.GetKeyDown(KeyCode.D))
        {
            Level.ScrollBot(true);
        }
        if(Input.GetKeyDown(KeyCode.A))
        {
            Level.ScrollBot(false);
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Level.TakeBot();
        }
    }
    private void DoNull()
    {

    }
    private void Build()
    {
        tank.transform.position = Vector2.zero;
        tank.Rig.velocity = Vector2.zero;
        tank.Rig.angularVelocity = 0f;
    }
    public void SetControll(ControllTypes Type)
    {
        ControllType = Type;
        switch(Type)
        {
            case ControllTypes.Player:
                Action = ControllTank;
                Manipulation.Enable = true;
                break;
            case ControllTypes.Watching:
                Action = Watching;
                Manipulation.Enable = false;
                break;
            case ControllTypes.ControllBot:
                Action = DoNull;
                Manipulation.Enable = false;
                break;
            case ControllTypes.InBuild:
                Action = Build;
                Manipulation.Enable = true;
                break;
        }
    }

    public void StartWatching()
    {
        SetControll(ControllTypes.Watching);
    }

    private IEnumerator CheckLife()
    {
        while(!tank.Destroyed)
        {
            yield return new WaitForSeconds(0.25f);
        }
        tank.Rig.velocity *= 0.25f;
        Level.OnPlayerKilled();
        yield break;
    }
    private bool CanFire()
    {
        Vector3 MousePos = new Vector3(MousePosition.x, MousePosition.y, -3f);

        Collider2D hit = Physics2D.OverlapPoint(MousePos, 1 << 9 | 1 << 5);
        return hit == null && Manipulation.NowPart == null ||
            (hit.transform.root.GetComponent<Tank>() != null &&
            !hit.transform.root.GetComponent<Tank>().Destroyed
            && hit.transform.root != transform);
    }
    public MoveInput Movement()
    {
        if(Input.GetKey(KeyCode.W))
        {
            Move = 1f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Move = -1f;
        }
        else
        {
            Move = 0f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            Rotation = Mathf.Lerp(Rotation, -1f, 10f * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Rotation = Mathf.Lerp(Rotation, 1f, 10f * Time.deltaTime);
        }
        else
        {
            Rotation = Mathf.Lerp(Rotation, 0f, 10f * Time.deltaTime);
        }
        if(((Vector2)transform.position - MousePosition).magnitude > ManipulateOffset)
            PrevAimPos = MousePosition;
       
        if(!Fire && Input.GetKeyDown(KeyCode.Mouse0) && CanFire())
        {
            Fire = true;
        }
        if(Fire && Input.GetKeyUp(KeyCode.Mouse0) || Manipulation.NowPart != null)
        {
            Fire = false;
        }

        if (!Special && Input.GetKeyDown(KeyCode.F))
        {
            Special = true;
        }
        if (Special && Input.GetKeyUp(KeyCode.F))
        {
            Special = false;
        }

        return new MoveInput(!isManipulation(), Move, Rotation, PrevAimPos, Fire, Special);
    }

    private void CameraDeltaSize()
    {
        CameraMove.UpdateSize(-Input.mouseScrollDelta.y);
    }
    private void CameraScope()
    {

    }

    private void CrossHair()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && !Informed)
        {

        }
        if (Input.GetKeyUp(KeyCode.Mouse1) && Informed)
        {

        }
        marker.transform.position = MousePosition;
        marker.SetSize(CameraMove.Size / CameraMove.MaxSize);
    }
    public void HaveFired()
    {
        marker.Fire();
    }

    private delegate void ActionType();
    private ActionType Action;

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Scene.active.ExitTest();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
           Debug.Log(Informer.TankInfo(tank));
        }
        MousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Action();
    }

    public struct MoveInput
    {
        public bool isManipulation;
        public float Move;
        public float Rotation;
        public Vector2 MousePos;
        public bool Fire;
        public bool Special;

        public MoveInput(bool isManipulation, float Move, float Rotation, Vector2 MousePos, bool Fire, bool Special)
        {
            this.Move = Move;
            this.Rotation = Rotation;
            this.MousePos = MousePos;
            this.Fire = Fire;
            this.isManipulation = isManipulation;
            this.Special = Special;
        }
    }
}

