using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AiController : MonoBehaviour
{
    public enum ControllerType { Battle, Static, Coward }
    public ControllerType AiControllerType;
    public enum AiTactic {Simple, LongRange, FlankAttack}
    public AiTactic Tactic;
    public enum AiDriveType {Tank, Wheel};
    public AiDriveType AiDrive;
    public enum AiOnFree {Static, RandomRide, GoToPoint}
    public AiOnFree OnFree;
    public enum AiAction { Static, Fire, FlankFire, GoToEnemy, GoTo, Taran, RandomRide, RunAway, RunAndFire }
    public AiAction NowAction;
    [Range(0, 1)]
    public float FireScatter;
    public float RotateToAccuracy;
    public Tank Enemy;
    [HideInInspector]
    public Vector2 Target;
    [HideInInspector]
    public Vector2 AimTarget;
    private bool HavePoint;
    public Vector2 TacticalPoint;
    private Coroutine LooseEnemyCour;
    private float Rotation;
    private float Move;
    private bool Fire;
    private bool Aiming;
    public float DotToTarget;
    private Coroutine FireCour;
    private Coroutine RandRideCoroutine;
    private Coroutine FindingCour;
    private Coroutine CheckLifeCour;
    private Coroutine BuildPathCour;
    public delegate void ControllerAction();
    public ControllerAction Action;
    private Vector2 TowerTargetPos;
    public Tank tank;
    public bool PlayerControlled;
    public bool AiOff;
    private bool SAM;
    private Seeker seeker;
    private Path path;
    private int NowWaypoint;
    private float NextWaypointDistance;
    private const int StartWaypoint = 0;
    private bool Reached;
    private bool PathReady;
    private Transform Finding;
    [Range(0.1f, 1f)]
    public float FindingSpeed;
    [Range(0, 4)]
    public float FindingRange;
    [Range(0, 4)]
    public float OptimalBattleRange;
    [Range(0, 5)]
    public float LooseEnemyRange;
    [Range(0, 10)]
    public float LooseEnemyDelay;

    private bool Rand;
    private float GetBulletSpeed()
    {
        if (tank.Gun.Length == 0)
            return 1;
        return tank.Gun[0].cannon.BulletSpeed * GameData.BulletSpeedC;
    }
    private float EnemyDistance(Vector2 EnemyTarget)
    {
        return ((Vector2)transform.position - EnemyTarget).magnitude;
    }
    private float Speed()
    {
        return tank.Rig.velocity.magnitude / Tank.RigAcceleration;
    }
    private Vector2 DirectionEnemy()
    {
        return ((Vector2)Enemy.transform.position - (Vector2)transform.position).normalized;
    }
    private bool Loaded;
    private EndlessAiStuff Endless;
    private Scene Level;

    private void Awake()
    {
        tank = GetComponent<Tank>();
        seeker = GetComponent<Seeker>();
        Finding = transform.GetChild(1);
        //SAM = tank.Tower[0].tower.Static;
    }
    private void Start()
    {
        Level = Scene.active;
        TryGetComponent(out Endless);
        ChangeControllerType(AiControllerType);
        Rand = Random.Range(0, 2) == 0;
        FindingRange *= MapGenerator.ChunckSize;
        OptimalBattleRange *= MapGenerator.ChunckSize;
        LooseEnemyRange *= MapGenerator.ChunckSize;
        switch (AiDrive)
        {
            case AiDriveType.Tank:
                DriveTo = TankGoByPath;
                break;
            case AiDriveType.Wheel:
                DriveTo = WheelGoByPath;
                break;
        }
    }
    public delegate float[] GoToDelegate(float DistanceToStop);
    public GoToDelegate DriveTo;

    public void ChangeControllerType(ControllerType Type)
    {
        AiControllerType = Type;
        switch (Type)
        {
            case ControllerType.Battle:
                Action = DoBattle;
                break;
            case ControllerType.Static:
                Action = DoStatic;
                break;
            case ControllerType.Coward:
                Action = DoCoward;
                break;
        }
        OnStart(AiControllerType);
    }
    public void OnStart(ControllerType Type)
    {
        switch(Type)
        {
            case ControllerType.Battle:
                if(CheckLifeCour == null)
                    CheckLifeCour = StartCoroutine(CheckLife());
                if (FindingCour == null)
                    FindingCour = StartCoroutine(FindingRotate());
                if (BuildPathCour == null)
                    BuildPathCour = StartCoroutine(BuildingPath());
                break;
            case ControllerType.Coward:
                if (CheckLifeCour == null)
                    CheckLifeCour = StartCoroutine(CheckLife());
                if (FindingCour == null)
                    FindingCour = StartCoroutine(FindingRotate());
                if (BuildPathCour == null)
                    BuildPathCour = StartCoroutine(BuildingPath());
                break;
            case ControllerType.Static:
                if (CheckLifeCour == null)
                    CheckLifeCour = StartCoroutine(CheckLife());
                break;
        }
    }

    private float[] TankGoByPath(float DistanceToStop)
    {
        float Move = 0;
        float Turn = 0;
        if (path == null)
        {
            return new float[2] { Move, Turn };
        }
        if (NowWaypoint > path.vectorPath.Count - 3)
            return new float[2] { Move, Turn };
        if ((Target - (Vector2)transform.position).magnitude < DistanceToStop * (Speed() + 1))
        {
            return new float[2] { Move, Turn };
        }

        Vector2 Dir = (path.vectorPath[NowWaypoint + 2] - path.vectorPath[NowWaypoint]);
        Vector2 NextDir = (path.vectorPath[NowWaypoint + 2] - path.vectorPath[NowWaypoint + 1]);
        float DotX = Vector2.Dot(Dir.normalized, transform.right);
        float DotY = Vector2.Dot(Dir.normalized, transform.up);
        float StopDirDot = Mathf.Pow(Vector2.Dot(transform.up, NextDir.normalized), 4);

        if (DotY > 0)
        {
            DotX = -DotX;
        }
        else
        {
            float RoundedDot = -Mathf.Ceil(Mathf.Abs(DotX));
            DotX = DotX > 0 ? RoundedDot : -RoundedDot;
        }
        Turn = DotX;
        Move = StopDirDot * (1 - DotX) * TankAhead();
        return new float[2] { Move, Turn };
    }
    private float[] WheelGoByPath(float DistanceToStop)
    {
        float Move = 0;
        float Turn = 0;
        if (path == null)
        {
            return new float[2] { Move, Turn };
        }
        if (NowWaypoint > path.vectorPath.Count - 3)
            return new float[2] { Move, Turn };
        if ((Target - (Vector2)transform.position).magnitude < DistanceToStop * (Speed() + 1))
        {
            return new float[2] { Move, Turn };
        }

        Vector2 Dir = (path.vectorPath[NowWaypoint + 2] - path.vectorPath[NowWaypoint]);
        Vector2 NextDir = (path.vectorPath[NowWaypoint + 2] - path.vectorPath[NowWaypoint + 1]);
        float DotX = Vector2.Dot(Dir.normalized, transform.right);
        float DotY = Vector2.Dot(Dir.normalized, transform.up);
        float StopDirDot = Mathf.Pow(Vector2.Dot(transform.up, NextDir.normalized), 2);

        if (DotY > 0)
        {
            DotX = -DotX;
        }
        else
        {
            float RoundedDot = -Mathf.Ceil(Mathf.Abs(DotX));
            DotX = DotX > 0 ? RoundedDot : -RoundedDot;
        }
        Turn = DotX;
        Move = (StopDirDot + 1f) * (1 - DotX * 0.5f) * TankAhead() * 0.5f;
        return new float[2] { Move, Turn };
    }

    private float[] TankGoTo(Vector2 Target, Vector2 FinalTarget, float DistanceToStop)
    {
        float Move = 0;
        float Turn = 0;
        if (EnemyDistance(FinalTarget) < DistanceToStop * (Speed() + 1))
        {
            return new float[2] { Move, Turn };
        }

        Vector2 Dir = (Target - (Vector2)transform.position);
        float DotX = Vector2.Dot(Dir.normalized, transform.right);
        float DotY = Vector2.Dot(Dir.normalized, transform.up);
        if(DotY > 0)
        {
            DotX = -Mathf.Abs(DotX) * DotX;
        }
        else
        {
            float RoundedDot = -Mathf.Ceil(Mathf.Abs(DotX));
            DotX = DotX > 0 ? RoundedDot : -RoundedDot;
        }
        Debug.Log("DotY: " + DotY + " DotX: " + DotX);
        Turn = DotX;
        Move = (1 - Mathf.Abs(DotX)) * TankAhead();
        return new float[2]{ Move, Turn };
    }
    private float[] WheelGoTo(Vector2 Target, Vector2 FinalTarget, float DistanceToStop)
    {
        float Move = 0;
        float Turn = 0;
        if (EnemyDistance(FinalTarget) < DistanceToStop)
        {
            return new float[2] { Move, Turn };
        }
        Vector2 Dir = ((Vector2)transform.position - Target);
        float DotX = Vector2.Dot(Dir.normalized, transform.right);
        float DotY = Vector2.Dot(Dir.normalized, transform.up);
        float MaxDotX = DotX > 0 ? 0.3f : -0.3f;
        Turn = DotY > 0f ? Mathf.Abs(DotX) * DotX : MaxDotX;
        Move = (1 - Mathf.Abs(DotX)) * TankAhead();
        return new float[2] { Move, Turn };
    }
    private float TankAhead()
    {
        if (NowAction == AiAction.Taran)
            return 1f;
        Vector2 Pos = tank.Main.part.TopPoint.position;
        RaycastHit2D hit = Physics2D.Raycast(Pos, transform.up, Speed() * 50, 1 << 10);
        if(hit.collider != null && hit.transform.root != transform.root)
        {
            return (hit.distance - Speed() * 50) / (Speed() * 50f);
        }
        return 1f;
    }
    private float RotateTo(Vector2 Target, float Do)
    {
        Vector2 Dir = ((Vector2)transform.position - Target);
        float DotX = Vector2.Dot(Dir.normalized, transform.right);
        bool Yea = Mathf.Abs(DotX) > Do;
        DotX = DotX > 0 ? Mathf.Sqrt(Mathf.Abs(DotX)) : -Mathf.Sqrt(Mathf.Abs(DotX));
        
        return Yea ? DotX : 0;
    }
    private bool AimToReady(Vector2 Target, float Accurancy)
    {
        if (tank.Tower.Length == 0)
            return false;
        Vector2 Dir = ((Vector2)tank.Tower[0].part.transform.position - Target);
        float DotX = Vector2.Dot(Dir.normalized, tank.Tower[0].part.transform.up);
        DotToTarget = Mathf.Abs(DotX);
        return DotToTarget > Accurancy + Mathf.Sqrt(Dir.magnitude / OptimalBattleRange) * 0.1f;
    }
    private Vector2 EnemyFlank()
    {
        if(Enemy.Tower.Length > 0)
        {
            float Offset = OptimalBattleRange * 0.25f;
            if (Rand)
            {
                return Enemy.transform.position + Enemy.Tower[0].part.transform.right * Offset;
            }
            else
            {
                return Enemy.transform.position - Enemy.Tower[0].part.transform.right * Offset;
            }
        }
        else
        {
            return Enemy.transform.position;
        }
    }
    private Vector2 AimTo(Vector2 TargetPoint)
    {
        float Dot = 1 - Mathf.Abs(Vector2.Dot(((Vector2)transform.position - TargetPoint).normalized, Enemy.Rig.velocity.normalized));
        Vector2 RealAim = TargetPoint + Enemy.Rig.velocity * Dot * EnemyDistance(Enemy.transform.position) / GetBulletSpeed() * 1f * (1 + Random.Range(-FireScatter, FireScatter) * 0.5f);
        return RealAim;
    }
    private Vector2 StartPathPoint()
    {
        return transform.position;
    }

    private IEnumerator BuildingPath()
    {
        yield return new WaitForSeconds(0.5f);
        while(!tank.Destroyed)
        {
            while (!InPathfinding())
            {
                OnOutOfPathfinding();
                yield return new WaitForFixedUpdate();
            }

            PathReady = false;
            seeker.StartPath(StartPathPoint(), Target, OnPathComplite);
            while (!PathReady)
            {
                yield return new WaitForFixedUpdate();
            }
            if (path != null)
            {
                float distance = ((Vector2)transform.position - (Vector2)path.vectorPath[NowWaypoint]).magnitude;
                if (distance < NextWaypointDistance)
                {
                    NowWaypoint++;
                }
            }
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }
    private bool InPathfinding()
    {
        Bounds Box = ProceduralGridMover.bounds;
        return (transform.position.x < Box.max.x && transform.position.x > Box.min.x &&
                transform.position.y < Box.max.y && transform.position.y > Box.min.y);
    }
    private void OnPathComplite(Path p)
    {
        if(!p.error)
        {
            path = p;
            NowWaypoint = StartWaypoint;
            PathReady = true;
        }
        else
        {
            path = null;
        }
    }
    private void OnOutOfPathfinding()
    {
        path = null;
    }

    public void SetEnable(bool On)
    {
        AiOff = !On;
    }

    public void DoBattle()
    {
        #region SolveAboutAction
        FindEnemy();
        if(Enemy == null || Enemy.Destroyed)
        {
            StartLooseEnemy(2f, Enemy);
            switch(OnFree)
            {
                case AiOnFree.GoToPoint:
                    NowAction = AiAction.GoTo;
                    Target = TacticalPoint;
                    break;
                case AiOnFree.RandomRide:
                    NowAction = AiAction.RandomRide;
                    break;
                case AiOnFree.Static:
                    NowAction = AiAction.Static;
                    break;
            }
            
        }
        else if(!CanSeeEnemy())
        {
            NowAction = AiAction.GoToEnemy;
            Target = Enemy.transform.position;
        }
        else if(tank.Gun.Length > 0)
        {
            switch(Tactic)
            {
                case AiTactic.FlankAttack:
                    NowAction = AiAction.FlankFire;
                    Target = EnemyFlank();
                    break;
                case AiTactic.LongRange:
                    NowAction = AiAction.Fire;
                    Target = Enemy.transform.position;
                    break;
                case AiTactic.Simple:
                    NowAction = AiAction.Fire;
                    Target = Enemy.transform.position;
                    break;
            }
            
        }
        else
        {
            NowAction = AiAction.Taran;
            Target = Enemy.transform.position;
        }
        #endregion
        #region DoThatAction
        switch(NowAction)
        {
            case AiAction.RandomRide:
                {
                    if (RandRideCoroutine == null)
                    {
                        RandRideCoroutine = StartCoroutine(RandomRide());
                    }
                }
                break;
            case AiAction.GoTo:
                if(EnemyDistance(TacticalPoint) > 3f)
                {
                    Rotation = Mathf.Lerp(Rotation, DriveTo(1f)[1], 0.025f);
                    Move = Mathf.Lerp(Move, DriveTo(1f)[0], 0.05f);
                }
                else
                {
                    Rotation = 0;
                    Move = 0;
                }
                AimTarget = (Vector2)transform.position + (Vector2)transform.up;
                break;
            case AiAction.Fire:
                {
                    if(DotToTarget > 0.3f || SAM)
                    {
                        Rotation = RotateTo(Target, RotateToAccuracy);
                    }
                    else
                    {
                        Rotation = 0;
                    }
                    Move = 0;
                    AimTarget = AimTo(Target);
                    if(!Aiming && FireCour == null)
                    {
                        FireCour = StartCoroutine(MakeFire(Target, 0.9f));
                    }
                    break;
                }
            case AiAction.FlankFire:
                {
                    Rotation = Mathf.Lerp(Rotation, DriveTo(2f)[1], 1f);
                    Move = Mathf.Lerp(Move, DriveTo(4f)[0], 2f);
                    AimTarget = AimTo(Enemy.transform.position);
                    
                    if (!Aiming && FireCour == null)
                    {
                        FireCour = StartCoroutine(MakeFire(Enemy.transform.position, 0.9f));
                    }
                    break;
                }
            case AiAction.GoToEnemy:
                {
                    Rotation = Mathf.Lerp(Rotation, DriveTo(1f)[1], 0.025f);
                    Move = Mathf.Lerp(Move, DriveTo(1f)[0], 0.05f);
                    AimTarget = AimTo(Enemy.transform.position);
                    break;
                }
            case AiAction.Taran:
                {
                    Rotation = Mathf.Lerp(Rotation, DriveTo(0f)[1], 0.1f);
                    Move = Mathf.Lerp(Move, DriveTo(0f)[0], 0.1f);
                    AimTarget = (Vector2)transform.position + (Vector2)transform.up;
                    break;
                }
            case AiAction.Static:
                Rotation = 0;
                Move = 0;
                break;
        }
        #endregion


        Controller.MoveInput input = new Controller.MoveInput(false, Move, Rotation, AimTarget, Fire, false);
        tank.Drive(input);
    }
    public void DoStatic()
    {
        NowAction = AiAction.Static;
        TowerTargetPos = transform.position + transform.up;
        Controller.MoveInput input = new Controller.MoveInput(false, 0, 0, TowerTargetPos, false, false);
        tank.Drive(input);
    }
    public void DoCoward()
    {
        #region SolveAboutAction
        FindEnemy();
        if (Enemy == null || Enemy.Destroyed)
        {
            if(Enemy != null && Enemy.Destroyed)
            {
                StartLooseEnemy(LooseEnemyDelay, Enemy);
            }
            switch (OnFree)
            {
                case AiOnFree.GoToPoint:
                    NowAction = AiAction.GoTo;
                    Target = TacticalPoint;
                    break;
                case AiOnFree.RandomRide:
                    NowAction = AiAction.RandomRide;
                    break;
                case AiOnFree.Static:
                    NowAction = AiAction.Static;
                    break;
            }

        }
        else if (!CanSeeEnemy())
        {
            NowAction = AiAction.Static;
        }
        else if(tank.Gun.Length > 0)
        {
            NowAction = AiAction.RunAndFire;
            Target = FindCowardPath();
        }
        else
        {
            NowAction = AiAction.RunAway;
            Target = FindCowardPath();
        }
        #endregion
        #region DoThatAction
        switch (NowAction)
        {
            case AiAction.RandomRide:
                {
                    if (RandRideCoroutine == null)
                    {
                        RandRideCoroutine = StartCoroutine(RandomRide());
                    }
                }
                break;
            case AiAction.RunAndFire:
                Rotation = Mathf.Lerp(Rotation, DriveTo(1f)[1], 0.025f);
                Move = Mathf.Lerp(Move, DriveTo(1f)[0], 0.05f);
                AimTarget = AimTo(Enemy.transform.position);
                if (!Aiming && FireCour == null)
                {
                    FireCour = StartCoroutine(MakeFire(Enemy.transform.position, 0.75f));
                }
                break;
            case AiAction.RunAway:
                {
                    Rotation = Mathf.Lerp(Rotation, DriveTo(1f)[1], 0.025f);
                    Move = Mathf.Lerp(Move, DriveTo(1f)[0], 0.05f);
                    AimTarget = (Vector2)transform.position + (Vector2)transform.up;
                    break;
                }
            case AiAction.Static:
                Rotation = 0;
                Move = 0;
                break;
        }
        #endregion


        Controller.MoveInput input = new Controller.MoveInput(false, Move, Rotation, AimTarget, Fire, false);
        tank.Drive(input);
    }

    public void PlayerTake()
    {
        if(FireCour != null)
            StopCoroutine(FireCour);
        if(LooseEnemyCour != null)
            StopCoroutine(LooseEnemyCour);
        if(RandRideCoroutine != null)
            StopCoroutine(RandRideCoroutine);

        PlayerControlled = true;
        Debug.Log(PlayerControlled);
    }

    public IEnumerator RandomRide()
    {
        while(Enemy == null)
        {
            Target = FindRoad();
            while (((Vector2)transform.position - Target).magnitude > 5)
            {
                Rotation = Mathf.Lerp(Rotation, DriveTo(1f)[1], 0.25f);
                Move = Mathf.Lerp(Move, DriveTo(1f)[0], 0.25f);
                AimTarget = (Vector2)transform.position + (Vector2)transform.up;
                yield return new WaitForFixedUpdate();
                if (Enemy != null)
                    break;
            }
            yield return new WaitForFixedUpdate();
        }
        RandRideCoroutine = null;
        yield break;
    }
    private Vector2 FindRoad()
    {
        float Dst = OptimalBattleRange  ;

        float LimitMaxX = 50;
        float LimitMinX = -50;
        float MinX = transform.position.x - Dst > LimitMinX ? transform.position.x - Dst : LimitMinX;
        float MaxX = transform.position.x + Dst < LimitMaxX ? transform.position.x + Dst : LimitMaxX;
        float X = Random.Range(MinX, MaxX);

        float LimitMaxY = 50;
        float LimitMinY = -50;
        float MinY = transform.position.y - Dst > LimitMinY ? transform.position.y - Dst : LimitMinY;
        float MaxY = transform.position.y + Dst < LimitMaxY ? transform.position.y + Dst : LimitMaxY;
        float Y = Random.Range(MinY, MaxY);

        return new Vector2(X, Y);
    }
    private Vector2 FindCowardPath()
    {
        if (Enemy == null)
            return transform.position;
        Vector2 Dir = (transform.position - Enemy.transform.position).normalized;
        return (Vector2)transform.position + Dir * OptimalBattleRange * 0.5f; 
    }

    public void FindEnemy()
    {
        float LooseRange = LooseEnemyRange;
        if (Enemy != null && EnemyDistance(Enemy.transform.position) > LooseRange)
        {
            StartLooseEnemy(LooseEnemyDelay, Enemy);
            return;
        }

        float Range = FindingRange;
        RaycastHit2D hit = Physics2D.Raycast(Finding.position, Finding.transform.up, Range, 1 << 15 | 1 << 12);
        if (hit.collider == null)
            return;
        if(hit.collider.gameObject.layer == 15)
        {
            Tank thisEnemy = hit.collider.GetComponent<Tank>();
            if (tank.Side.isEnemy(thisEnemy.Side.Team))
            {
                GetEnemy(thisEnemy);
            }
        }

    }
    public IEnumerator FindingRotate()
    {
        while(!tank.Destroyed)
        {
            Finding.transform.Rotate(Vector3.forward, FindingSpeed * 5f);
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }

    private IEnumerator CheckLife()
    {
        while(!tank.Destroyed)
        {
            yield return new WaitForSeconds(0.25f);
        }
        OnAiKilled();
        yield break;
    }
    public void OnAiKilled()
    {
        if (Endless != null)
            Endless.OnAiKilled();
        Level.OnEnemyKilled(tank);
    }

    public bool CanSeeEnemy()
    {
        float Range = OptimalBattleRange  ;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, DirectionEnemy(), Range, 1 << 15 | 1 << 12);

        return (hit.collider != null && hit.transform.gameObject.layer == 15);
        
    }
    public void GetEnemy(Tank enemy)
    {
        if(CanGetEnemy(enemy))
        {
            Enemy = enemy;
            if (OnFree == AiOnFree.GoToPoint)
                OnFree = AiOnFree.RandomRide;
        }
    }
    public bool CanGetEnemy(Tank enemy)
    {
        return Enemy == null || EnemyDistance(enemy.transform.position) - 25 > (transform.position - enemy.transform.position).magnitude;
    }
    public void StartLooseEnemy(float Delay, Tank enemy)
    {
        if (LooseEnemyCour == null)
        {
            LooseEnemyCour = StartCoroutine(LoosingEnemy(Delay, enemy));
        }
    }
    private IEnumerator LoosingEnemy(float Delay, Tank enemy)
    {
        yield return new WaitForSeconds(Delay);
        if(enemy == Enemy)
        {
            LooseEnemy();
        }
        LooseEnemyCour = null;
        yield break;
    }
    public void LooseEnemy()
    {
        Enemy = null;
    }

    public void GetTactiacalPoint(Vector2 Point)
    {
        HavePoint = true;
        TacticalPoint = Point;
    }

    private IEnumerator MakeFire(Vector2 TargetPoint, float Accurancy)
    {
        Aiming = true;
        Fire = false;
        while (!AimToReady(TargetPoint, Accurancy))
        {
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(0.1f);
        Fire = true;
        yield return new WaitForFixedUpdate();
        Fire = false;
        Aiming = false;
        FireCour = null;
        yield break;
    }

    public void Controll()
    {
        Action();
    }

    private void FixedUpdate()
    {
        if (AiOff)
            return;
        if(!tank.Destroyed && !PlayerControlled)
        {
            Controll();
        }

    }
}
