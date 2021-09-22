using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessAiStuff : MonoBehaviour
{
    public AiController aiController;
    public Tank tank;
    public Rigidbody2D Rig;

    private Vector2 Key;
    private int Index;

    public void SetOnStart(bool SetOn, Vector2 Key, int Index)
    {
        aiController = GetComponent<AiController>();
        tank = aiController.tank;
        Rig = tank.Rig;

        this.Key = Key;
        this.Index = Index;

        SetObjectActive(SetOn);
    }

    public void SetObjectActive(bool On)
    {
        if(On)
        {
            SetAiOn();
        }
        else
        {
            SetAiOff();
        }
    }
    private void SetAiOff()
    {
        aiController.SetEnable(false);
        tank.enabled = false;
        Rig.velocity = Vector2.zero;
        Rig.angularVelocity = 0f;
    }
    private void SetAiOn()
    {
        aiController.SetEnable(true);
        tank.enabled = true;

    }

    public void OnAiKilled()
    {
        EndlessTerrain.OnEnemyDestroyed(Key, Index);
    }
}
