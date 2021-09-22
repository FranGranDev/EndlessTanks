using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.SceneManagement;

public class Scene : MonoBehaviour
{
    public enum GameTypes {Build, EndLess, Levels, Teams, BattleRoylate, Null}
    public GameTypes GameType;
    public static Scene active;
    public GameData DataGame;
    public static Tank Player;
    private Controller PlayerController;
    public Transform Position;
    public Camera MainCamera;
    public CameraMovement CameraMove;
    public TerrainSurface terrain;
    public MapGenerator LandGenerator;
    public Tank Boss;
    public Tank[] Enemy;
    public Tank[] RedTeam;
    public Tank[] BlueTeam;
    public bool BossDefeated;
    public bool EnemyDefeated;
    private int NowAiNum;
    private int[] CalculateTeams()
    {
        int[] Teams = {BlueTeam.Length, RedTeam.Length};
        for (int i = 0; i < BlueTeam.Length; i++)
        {
            if (BlueTeam[i].Destroyed)
            {
                Teams[0]--;
            }
        }
        for (int i = 0; i < RedTeam.Length; i++)
        {
            if(RedTeam[i].Destroyed)
            {
                Teams[1]--;
            }
        }
        return Teams;
    }
    public int EnemyCount;
    public int EnemyRemain;

    public void OnStart()
    {
        switch(GameType)
        {
            case GameTypes.EndLess:
                StartCoroutine(OnEndlessStart());
                break;
            case GameTypes.Build:
                StartCoroutine(OnBuildStart());
                break;
            case GameTypes.Levels:
                StartCoroutine(OnLevelsStart());
                break;
            case GameTypes.Teams:
                StartCoroutine(OnTeamsStart());
                break;
        }
    }

    public void ReloadTest()
    {
        SceneManager.LoadScene(0);
    }
    public void ExitTest()
    {
        Application.Quit();
    }

    public IEnumerator OnBuildStart()
    {
        yield return new WaitForFixedUpdate();
        Player = CreatePlayer(DataGame.PlayersTank, Position.position);
        Player.GetComponent<Controller>().SetControll(Controller.ControllTypes.InBuild);
        GameData.InBuild = true;
        yield break;
    }
    public IEnumerator OnEndlessStart()
    {
        yield return new WaitForSeconds(1f);
        GameData.InBuild = false;
        Player = CreatePlayer(DataGame.PlayersTank, Position.position);
        Player.Side.Team = SideOwn.type.BlueTeam;
        Player.GetComponent<Controller>().SetControll(Controller.ControllTypes.Player);
        yield break;
    }
    public IEnumerator OnLevelsStart()
    {
        GameData.InBuild = false;
        BossDefeated = !DataGame.Play.Levels[0].HaveBoss;
        Player = CreatePlayer(DataGame.PlayersTank, Position.position);
        Player.Side.Team = SideOwn.type.BlueTeam;
        Player.GetComponent<Controller>().SetControll(Controller.ControllTypes.Player);
        CameraMove.Target = Player.transform;

        CreateLevelEnemies(GameType, 0);
        yield break;
    }
    public IEnumerator OnTeamsStart()
    {
        GameData.InBuild = false;
        Player = CreatePlayer(DataGame.PlayersTank, Position.position);
        Player.Side.Team = SideOwn.type.BlueTeam;
        CreateLevelEnemies(GameType, 0);
        yield break;
    }
    public IEnumerator OnBattleRoylateStart()
    {
        yield break;
    }

    public Tank CreatePlayer(TankBuildTemp TankBuild, Vector2 Position)
    {
        GameObject Player = new GameObject(TankBuild.Name);
        Player.tag = "Player";
        Player.layer = 15;
        Player.transform.position = new Vector3(Position.x, Position.y, -1f);
        Tank PlayerTank = Player.AddComponent<Tank>();
        PlayerTank.Builded = true;
        if (MainCamera.GetComponent<CameraMovement>() != null)
        {
            MainCamera.GetComponent<CameraMovement>().Target = Player.transform;
        }
        PlayerController = Player.AddComponent<Controller>();
        PartManipulation PlayerPartManipulator = Player.AddComponent<PartManipulation>();
        SideOwn Side = Player.AddComponent<SideOwn>();
        PlayerController.Manipulation = PlayerPartManipulator;
        PlayerController.Level = this;
        PlayerController.marker = Instantiate(GameData.CrossHair).GetComponent<Marker>();

        PlayerTank.AddRig();
        PlayerTank.AddPlayerTrigger();
        PlayerTank.Terrain = terrain;
        PlayerTank.Side = Side;
        PlayerController.tank = PlayerTank;
        PlayerController.mainCamera = MainCamera;
        PlayerPartManipulator.tank = PlayerTank;

        PlayerTank.Tower = new Tank.TankPart[0];
        PlayerTank.LeftTrack = new Tank.TankPart[0];
        PlayerTank.RightTrack = new Tank.TankPart[0];
        PlayerTank.Gun = new Tank.TankPart[0];
        PlayerTank.Engine = new Tank.TankPart[0];

        Part Body = Instantiate(DataGame.GetPart(TankBuild.Body.PartIndex, Part.Type.Body), Player.transform.position, Player.transform.rotation, Player.transform).GetComponent<Part>();
        PlayerPartManipulator.SetBody(Body);

        for (int i = 0; i < TankBuild.Towers.Length; i++)
        {
            if (!TankBuild.Towers[i].Static)
            {
                Part tower = Instantiate(DataGame.GetPart(TankBuild.Towers[i].PartIndex, Part.Type.Tower)).GetComponent<Part>();
                PlayerPartManipulator.InstallPart(tower, TankBuild.Towers[i].ParentLinkIndex, TankBuild.Towers[i].SubLinks, TankBuild.Towers[i].Position);
                tower.SetColor(TankBuild.Towers[i].color);
            }
        }
        for (int i = 0; i < TankBuild.LeftTracks.Length; i++)
        {
            Part Track = Instantiate(DataGame.GetPart(TankBuild.LeftTracks[i].PartIndex, Part.Type.Track)).GetComponent<Part>();
            PlayerPartManipulator.InstallPart(Track, TankBuild.LeftTracks[i].ParentLinkIndex, TankBuild.LeftTracks[i].SubLinks, TankBuild.LeftTracks[i].Position);
            Track.SetColor(TankBuild.LeftTracks[i].color);
        }
        
        for (int i = 0; i < TankBuild.RightTracks.Length; i++)
        {
            Part Track = Instantiate(DataGame.GetPart(TankBuild.RightTracks[i].PartIndex, Part.Type.Track)).GetComponent<Part>();
            PlayerPartManipulator.InstallPart(Track, TankBuild.RightTracks[i].ParentLinkIndex, TankBuild.RightTracks[i].SubLinks, TankBuild.RightTracks[i].Position);
            Track.SetColor(TankBuild.RightTracks[i].color);
        }
        for (int i = 0; i < TankBuild.Guns.Length; i++)
        {
            Part Gun = Instantiate(DataGame.GetPart(TankBuild.Guns[i].PartIndex, Part.Type.Gun)).GetComponent<Part>();
            PlayerPartManipulator.InstallPart(Gun, TankBuild.Guns[i].ParentLinkIndex, TankBuild.Guns[i].SubLinks, TankBuild.Guns[i].Position);
            Gun.SetColor(TankBuild.Guns[i].color);
        }
        for (int i = 0; i < TankBuild.Engines.Length; i++)
        {
            Part Engine = Instantiate(DataGame.GetPart(TankBuild.Engines[i].PartIndex, Part.Type.Engine)).GetComponent<Part>();
            PlayerPartManipulator.InstallPart(Engine, TankBuild.Engines[i].ParentLinkIndex, TankBuild.Engines[i].SubLinks, TankBuild.Engines[i].Position);
            Engine.SetColor(TankBuild.Engines[i].color);
        }
        //PlayerTank.TankColor = new Color(TankBuild.color[0], TankBuild.color[1], TankBuild.color[2]);
        PlayerTank.Name = "Player";
        PlayerTank.SetColor();
        PlayerTank.RecalculateTank();
        return PlayerTank;
    }
    public Tank CreateEnemy(Enemy enemy, Vector2 Pos, Quaternion Rotation, SideOwn.type Side, AiController.ControllerType action)
    {
        Tank tank = Instantiate(enemy.tank, new Vector3(Pos.x, Pos.y, -1f), Rotation, null);
        tank.transform.tag = "Ai";
        tank.name = enemy.Name;
        tank.Name = enemy.Name;
        tank.Terrain = terrain;
        tank.GetComponent<AiController>().ChangeControllerType(action);
        tank.Side = tank.GetComponent<SideOwn>();
        tank.Side.Team = Side;

        tank.RecalculateTank();
        return tank;
    }
    public void CreateLevelEnemies(GameTypes GameType, int level)
    {
        switch(GameType)
        {
            case GameTypes.Levels:
                {
                    Player.transform.up = new Vector2(1, 0);
                    Player.SetColor(new Color(0f, 0.55f, 0.18f));

                    Enemy[] enemytemp = DataGame.Play.Levels[level].EnemyOwn;
                    Enemy = new Tank[enemytemp.Length];
                    float Increase = LandGenerator.ySize / (Enemy.Length + 1);
                    float StartY = 16 - LandGenerator.ySize / 2;
                    float MinX = LandGenerator.xSize / 4;
                    float MaxX = LandGenerator.xSize / 2 - 8;
                    for (int i = 0; i < enemytemp.Length; i++)
                    {
                        Vector2 Position = new Vector2(Random.Range(MinX, MaxX), StartY + Increase * i);
                        Enemy[i] = CreateEnemy(enemytemp[i], Position, Quaternion.identity, SideOwn.type.RedTeam, AiController.ControllerType.Battle);
                        Enemy[i].SetColor(Color.red);
                        Enemy[i].transform.up = new Vector2(-1, 0);
                    }
                    EnemyCount = Enemy.Length;
                    EnemyRemain = Enemy.Length;
                }
                break;
            case GameTypes.Teams:
                {
                    Player.SetColor(new Color(0f, 0.55f, 0.18f));

                    Enemy[] Red = DataGame.Play.Teams[level].RedTeam;
                    Enemy[] Blue = DataGame.Play.Teams[level].BlueTeam;

                    if (Player.Side.Team == SideOwn.type.BlueTeam)
                    {
                        RedTeam = new Tank[Red.Length];
                        BlueTeam = new Tank[Blue.Length - 1];
                    }
                    else if (Player.Side.Team == SideOwn.type.RedTeam)
                    {
                        RedTeam = new Tank[Red.Length - 1];
                        BlueTeam = new Tank[Blue.Length];
                    }
                    float StartX = 16 - LandGenerator.xSize / 2;
                    float StartY = 16 - LandGenerator.ySize / 2;
                    float Lenght = 0;
                    float Increase = 0;
                    Vector2[,] Positions = new Vector2[0, 0];
                    if (Red.Length >= Blue.Length)
                    {
                        Lenght = Red.Length;
                        Increase = LandGenerator.ySize / Red.Length;
                        Positions = new Vector2[Red.Length, 2];
                    }
                    else
                    {
                        Lenght = Blue.Length;
                        Increase = LandGenerator.ySize / Blue.Length;
                        Positions = new Vector2[Blue.Length, 2];
                    }
                    for(int i = 0; i < Lenght; i++)
                    {
                        Positions[i, 0] = new Vector2(StartX, StartY + Increase * i);
                        Positions[i, 1] = new Vector2(-StartX, StartY + Increase * i);
                    }
                    for (int i = 0; i < BlueTeam.Length; i++)
                    {
                        BlueTeam[i] = CreateEnemy(Blue[i], Positions[i, 0], Quaternion.identity, SideOwn.type.BlueTeam, AiController.ControllerType.Battle);
                        BlueTeam[i].transform.up = new Vector2(1, 0);
                        BlueTeam[i].GetComponent<AiController>().GetTactiacalPoint(Positions[i, 1]);
                        BlueTeam[i].SetColor(Color.blue);
                    }
                    if (Player.Side.Team == SideOwn.type.BlueTeam)
                    {
                        Player.transform.position = new Vector2(StartX, StartY + Increase * (Blue.Length - 1));
                        Player.transform.up = new Vector2(1, 0);
                    }
                    for (int i = 0; i < RedTeam.Length; i++)
                    {
                        RedTeam[i] = CreateEnemy(Red[i], Positions[i, 1], Quaternion.identity, SideOwn.type.RedTeam, AiController.ControllerType.Battle);
                        RedTeam[i].transform.up = new Vector2(-1, 0);
                        RedTeam[i].GetComponent<AiController>().GetTactiacalPoint(Positions[i, 0]);
                        RedTeam[i].SetColor(Color.red);
                    }
                    if (Player.Side.Team == SideOwn.type.RedTeam)
                    {
                        Player.transform.position = new Vector2(StartX, StartY + Increase * (Red.Length - 1));
                        Player.transform.up = new Vector2(-1, 0);
                    }
                }
                break;
        }
        
    }
    public IEnumerator CreateLevelBoss(int level)
    {
        yield return new WaitForSeconds(1f);
        Enemy NowBoos = DataGame.Play.Levels[level].Boss;
        Boss = CreateEnemy(NowBoos, Vector2.zero, Quaternion.identity, SideOwn.type.RedTeam, AiController.ControllerType.Static);
        CameraMove.ShowTargetForTime(Boss.transform, 0.01f, 5f);
        yield return new WaitForSeconds(5f);
        Boss.GetComponent<AiController>().ChangeControllerType(AiController.ControllerType.Battle);
        yield break;
    }

    public void WatchBot()
    {
        CameraMove.Target = BlueTeam[NowAiNum].transform;
    }
    public void ScrollBot(bool Right)
    {
        if(Right)
        {
            NowAiNum++;
            if(NowAiNum > BlueTeam.Length - 1)
            {
                NowAiNum = 0;
            }
        }
        else
        {
            NowAiNum--;
            if (NowAiNum < 0)
            {
                NowAiNum = BlueTeam.Length - 1;
            }
        }
        WatchBot();
    }
    public void TakeBot()
    {
        if (BlueTeam[NowAiNum].Destroyed)
            return;
        PlayerController.SetControll(Controller.ControllTypes.ControllBot);
        Tank Bot = BlueTeam[NowAiNum];
        Bot.GetComponent<AiController>().PlayerTake();
        BotController BotControll = BlueTeam[NowAiNum].gameObject.AddComponent<BotController>();
        BotControll.Controller = PlayerController;
        BotControll.CameraMove = CameraMove;
    }


    public void OnEnemyKilled(Tank tank)
    {
        switch(GameType)
        {
            case GameTypes.Levels:
                EnemyRemain--;
                if (EnemyRemain <= 0)
                {
                    if(EnemyDefeated)
                    {
                        BossDefeated = true;
                    }
                    if(BossDefeated)
                    {
                        //StartCoroutine(OnDone());
                        return;
                    }
                    if(!BossDefeated)
                    {
                        StartCoroutine(CreateLevelBoss(0));
                    }
                    EnemyDefeated = true;
                }
                break;
            case GameTypes.Teams:
                int[] Alive = CalculateTeams();
                Debug.Log("Blue: " + Alive[0] + " Red: " + Alive[1]);
                if(Alive[1] == 0)
                {
                    StartCoroutine(OnDone());
                }
                if(Alive[0] == 0 && Player.Destroyed)
                {
                    OnPlayerKilled();
                }
                break;
        }
        
    }
    public void OnPlayerBotKilled()
    {
        BlueTeam[NowAiNum].GetComponent<AiController>().PlayerControlled = false;
        PlayerController.SetControll(Controller.ControllTypes.Watching);
    }
    public void OnPlayerKilled()
    {
        StartCoroutine(OnPlayerKilledCour());
    }
    public IEnumerator OnDone()
    {
        switch (GameType)
        {
            case GameTypes.Levels:
                Debug.Log("You Win");
                break;
            case GameTypes.Teams:
                Debug.Log("You Win");
                break;
                    
        }
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(0);
        yield break;
    }
    public IEnumerator OnPlayerKilledCour()
    {
        switch(GameType)
        {
            case GameTypes.Levels:
                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene(0);
                break;
            case GameTypes.Teams:
                if(CalculateTeams()[0] == 0)
                {
                    yield return new WaitForSeconds(1f);
                    SceneManager.LoadScene(0);
                }
                else
                {
                    PlayerController.StartWatching();
                }
                break;
        }

        yield break;
    }

    public void Awake()
    {
        active = this;
    }
    public void Start()
    {
        OnStart();
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1) && GameType == GameTypes.Build)
        {
            DataGame.SavePlayerTank(Player);
            DataGame.Save();
            SceneManager.LoadScene(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && GameType == GameTypes.Build)
        {
            DataGame.SavePlayerTank(Player);
            DataGame.Save();
            SceneManager.LoadScene(2);
        }
    }
}
