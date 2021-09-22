using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class GridMover : MonoBehaviour
{
    public float updateDistance = 10;
    public Transform target;
    GridGraph graph;

    public void Start()
    {
        graph = AstarPath.active.data.FindGraphWhichInheritsFrom(typeof(GridGraph)) as GridGraph;
    }
    public void Update()
    {
        var graphCenterInGraphSpace = PointToGraphSpace(graph.center);
        var targetPositionInGraphSpace = PointToGraphSpace(target.position);

        if (VectorMath.SqrDistanceXZ(graphCenterInGraphSpace, targetPositionInGraphSpace) > updateDistance * updateDistance)
        {
            //UpdateGraph();
        }
    }

    Vector3 PointToGraphSpace(Vector3 p)
    {
        return graph.transform.InverseTransform(p);
    }
}
