using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialObject : MonoBehaviour
{
    public enum Types {Market, Task, Boss}
    public Types Type;
    public bool PlayerOn;
    public StaticObject Obj;

    public void Awake()
    {
    }
    virtual public void Start()
    {

    }
    virtual public void OnEnter()
    {
        
    }
    virtual public void OnExit()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.root == Scene.Player.transform)
        {
            OnEnter();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.root == Scene.Player.transform)
        {
            OnExit();
        }
    }
}

