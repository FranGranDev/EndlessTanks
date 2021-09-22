using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionCulling : MonoBehaviour
{
    public CircleCollider2D ColliderDisable;
    public CircleCollider2D ColliderDelete;
    private bool Work;

    private void Start()
    {
        StartCoroutine(OnStart());
    }
    private IEnumerator OnStart()
    {
        if ((Work = MapGenerator.BuildType == MapGenerator.BuildTypes.Endless))
        {
            yield return new WaitForFixedUpdate();

            ColliderDisable.radius = EndlessTerrain.MaxDistanceView + 1;
            ColliderDelete.radius = EndlessTerrain.DistanceDeleteRange[0] + 5;
        }
        yield break;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Work)
            return;
        if (collision.tag == "Ai")
        {
            collision.gameObject.GetComponent<EndlessAiStuff>().SetObjectActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!Work)
            return;
        if ((collision.transform.position - transform.position).magnitude > EndlessTerrain.DistanceDeleteRange[0])
        {
            if(collision.tag == "Ai")
            {
                Destroy(collision.gameObject);
            }
            else if(collision.tag == "Part" && collision.transform.parent == null)
            {
                Destroy(collision.gameObject);
            }
        }
        else if ((collision.transform.position - transform.position).magnitude > EndlessTerrain.MaxDistanceView)
        {
            if (collision.tag == "Ai")
            {
                collision.gameObject.GetComponent<EndlessAiStuff>().SetObjectActive(false);
            }
        }
    }
}
