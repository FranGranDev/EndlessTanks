using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform Target;
    public enum Type {Smooth, HalfMouse}
    public Type FollowType;
    public float Size;
    public float MinSize;
    public float MaxSize;
    public float Offset;
    [Range(0f, 1f)]
    public float Speed;
    [Range(0f, 1f)]
    private float PositionSpeed;
    public float RotationSpeed;
    public float ScrollSpeed;
    private Camera MainCamera;
    private Vector3 NextPos;
    private bool HaveBound;
    private Bounds bounds;

    public void Start()
    {
        MainCamera = GetComponent<Camera>();
    }

    public void SetBound(bool HaveBound, Bounds bounds)
    {
        this.HaveBound = HaveBound;
        this.bounds = bounds;
    }

    public void ShowTargetForTime(Transform Enemy,float FlySpeed, float time)
    {
        StartCoroutine(ShowTargetForTimeCour(Enemy, FlySpeed, time));
    }
    private IEnumerator ShowTargetForTimeCour(Transform Enemy, float FlySpeed, float time)
    {
        Transform TempTarget = Target;
        float tempSpeed = Speed;
        Target = Enemy;
        Speed = FlySpeed;
        yield return new WaitForSeconds(time);
        Target = TempTarget;
        Speed = tempSpeed;
        yield break;
    }

    public void UpdateSize(float DeltaSize)
    {
        float CurrentDeltaSize = DeltaSize * ScrollSpeed * Size / MaxSize;
        if (Size + CurrentDeltaSize > MinSize && Size + CurrentDeltaSize < MaxSize)
        {
            Size += CurrentDeltaSize;
        }
        else if(Size + CurrentDeltaSize < MinSize)
        {
            Size = MinSize;
        }
        else if(Size + CurrentDeltaSize > MaxSize)
        {
            Size = MaxSize;
        }
        switch(FollowType)
        {
            case Type.Smooth:
                MainCamera.orthographicSize = Mathf.Lerp(MainCamera.orthographicSize, Size, 5f * Time.deltaTime);
                break;
            case Type.HalfMouse:
                MainCamera.orthographicSize = Mathf.Lerp(MainCamera.orthographicSize, Size, 10f * Time.deltaTime);
                break;
        }
        PositionSpeed = Speed * (Mathf.Abs(MainCamera.orthographicSize - Size) + 1);
    }
    public void UpdatePosition()
    {
        Vector2 Dir = Target.up; //((Vector2)MainCamera.ScreenToWorldPoint(Input.mousePosition) - (Vector2)transform.position).normalized;
        Vector3 CurranteOffset = Dir * Offset * Mathf.Pow(Size / MaxSize, 4);
        NextPos = CurranteOffset + new Vector3(Target.position.x, Target.position.y, -10f);
        CheckForBounds(NextPos);
        transform.position = Vector3.Lerp(transform.position, NextPos, PositionSpeed);
        transform.up = Vector2.Lerp(transform.up, Target.transform.up, RotationSpeed);
    }
    public void UpdatePositionMouse()
    {
        Vector2 CamDeltaPosition = MainCamera.ScreenToViewportPoint(Input.mousePosition);
        float Size = MainCamera.orthographicSize * 2f;
        Vector3 position = new Vector3(Target.position.x + (CamDeltaPosition.x - 0.5f) * Size, Target.position.y + (CamDeltaPosition.y - 0.5f) * Size, -10f);
        transform.position = position;
    }
    public void UpdateRotation(Vector2 Direction)
    {
        transform.up = Vector2.Lerp(transform.up, Direction, 0.1f);
    }
    private void CheckForBounds(Vector2 Pos)
    {
        if (!HaveBound)
            return;
        float Offset = 1.2f;
        float SizeY = Offset * MainCamera.orthographicSize;
        float SizeX = SizeY * MainCamera.aspect;
        Vector2 Min = new Vector2(SizeX, SizeY);
        Vector2 Max = new Vector2(bounds.max.x - SizeX, bounds.max.y - SizeY);
        if(Pos.x > Max.x)
        {
            NextPos = new Vector3(Max.x, NextPos.y, -10f);
        }
        else if(Pos.x < Min.x)
        {
            NextPos = new Vector3(Min.x, NextPos.y, -10f);
        }
        if (Pos.y > Max.y)
        {
            NextPos = new Vector3(NextPos.x, Max.y, -10f);
        }
        else if (Pos.y < Min.y)
        {
            NextPos = new Vector3(NextPos.x, Min.y, -10f);
        }
    }
    private void FixedUpdate()
    {
        if (Target == null)
            return;
        switch(FollowType)
        {
            case Type.HalfMouse:
                UpdatePositionMouse();
                break;
            case Type.Smooth:
                UpdatePosition();
                break;
        }
    }
}
