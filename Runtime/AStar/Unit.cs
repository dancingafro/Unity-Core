using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreScript.AStar;

public class Unit : MonoBehaviour
{
    public Transform target;
    [SerializeField] float speed = 5;
    Vector3[] path;
    int targetIndex;

    // Start is called before the first frame update
    void Start()
    {
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    public void OnPathFound(Vector3[] path, bool Success)
    {
        if (!Success)
            return;

        this.path = path;
        targetIndex = 0;
        StopCoroutine("FollowPath");
        StartCoroutine("FollowPath");
    }

    IEnumerator FollowPath()
    {
        Vector3 currentWaypoint = path[0];

        while (true)
        {
            if (transform.position == currentWaypoint)
            {
                ++targetIndex;
                if (targetIndex >= path.Length)
                    yield break;

                currentWaypoint = path[targetIndex];
            }

            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
            yield return null;
        }

    }

    public void OnDrawGizmos()
    {
        if (path == null)
            return;

        for (int i = targetIndex; i < path.Length; ++i)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawCube(path[i], Vector3.one);

            if (i == targetIndex)
                Gizmos.DrawLine(transform.position, path[i]);
            else
                Gizmos.DrawLine(path[i - 1], path[i]);
        }
    }
}
