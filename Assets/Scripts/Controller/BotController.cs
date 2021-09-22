using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{
    public Tank Bot;
    public CameraMovement CameraMove;
    public Controller Controller;

    private void Start()
    {
        Bot = GetComponent<Tank>();
        StartCoroutine(CheckLife());
    }
    private IEnumerator CheckLife()
    {
        while (!Bot.Destroyed)
        {
            yield return new WaitForSeconds(0.25f);
        }
        Controller.Level.OnPlayerBotKilled();
        yield break;
    }
    private void CameraDeltaSize()
    {
        CameraMove.UpdateSize(-Input.mouseScrollDelta.y);
    }
    private void CameraScope()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            CameraMove.FollowType = CameraMovement.Type.HalfMouse;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            CameraMove.FollowType = CameraMovement.Type.Smooth;
        }
    }


    private void Update()
    {
        Controller.MoveInput movement = Controller.Movement();
        Bot.Drive(movement);

        CameraDeltaSize();
        CameraScope();
    }
}
