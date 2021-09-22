using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseBuilder : MonoBehaviour
{
    public int xSize;
    public int ySize;
    public int Index;
    public StaticObject Obj;
    public Transform Parts;
    public Transform Floor;

    void Start() => Build();

    public void Build()
    {
        HouseBuildData data = GameData.Builds[Index];
        int xLeftCenter = Mathf.FloorToInt(xSize / 2);
        int xRightCenter = xLeftCenter + 1;
        for (int y = 0, i = 0; y < ySize; y++)
        {
            for(int x = 0; x < xSize; x++, i++)
            {
                Vector2 Pos = (Vector2)transform.position + new Vector2(x * 2, y * 2);
                ObjectPart This = null;
                if(y == 0)
                {
                    This = data.Bottom;
                }
                else if(y == ySize - 1)
                {
                    This = data.Top;
                }
                else if(x == 0)
                {
                    This = data.LeftTop;
                }
                else if(x == xSize - 1)
                {
                    This = data.RightTop;
                }
                else if(x == xLeftCenter)
                {
                    This = data.CenterLeft;
                }
                else if(x == xRightCenter)
                {
                    This = data.CenterRight;
                }
                else if(x < xLeftCenter)
                {
                    This = data.Left;
                }
                else if(x > xRightCenter)
                {
                    This = data.Right;
                }
                ObjectPart part = Instantiate(This, Pos, Quaternion.identity, Parts);
            }
        }
    }
}
