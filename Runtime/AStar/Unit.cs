using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreScript.AStar;

public class Unit : MonoBehaviour
{
    const float pathUpdateMoveThreshold = .5f;
    const float minPathUpdateTime = .2f;
    [SerializeField] bool debug = false;

    public Transform target;
    public float speed = 5;
    public float turnSpeed = 5;
    public float turnDst = 5;
    public float stoppingDst = 10;
    Path path;

    void Start()
    {
        StartCoroutine(UpdatePath());
    }

    public void OnPathFound(Vector3[] waypoints, bool Success)
    {
        if (!Success)
            return;

        path = new Path(waypoints, transform.position, turnDst, stoppingDst);
        StopCoroutine(FollowPath());
        StartCoroutine(FollowPath());
    }

    IEnumerator UpdatePath()
    {
        if (Time.timeSinceLevelLoad < .3f)
            yield return new WaitForSeconds(.3f);

        PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));

        float sqrMovethreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target.position;
        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            if ((target.position - targetPosOld).sqrMagnitude > sqrMovethreshold)
            {
                PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
                targetPosOld = target.position;
            }
        }

    }

    IEnumerator FollowPath()
    {
        bool followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);

        float speedPercent = 1;

        while (followingPath)
        {
            Vector2 pos2d = CoreScript.Utility.UtilityClass.V3ToV2(transform.position, CoreScript.PositionSpace.xz);
            while (path.turnBoundaries.Length > pathIndex && path.turnBoundaries[pathIndex].HasCrossedLine(pos2d))
            {
                if (pathIndex == path.finishIndex)
                {
                    followingPath = false;
                    break;
                }
                else
                    ++pathIndex;
            }
            if (followingPath)
            {
                if (pathIndex >= path.slowDownIndex && stoppingDst > 0)
                {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishIndex].DistanceFromPoint(pos2d) / stoppingDst);
                    if (speedPercent < 0.01f)
                        followingPath = false;
                }
                if (path.lookPoints.Length <= pathIndex)
                {
                    followingPath = false;
                    continue;
                }
                Quaternion targetRot = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);

            }
            yield return null;
        }
    }

    public void OnDrawGizmos()
    {
        if (!debug || path == null)
            return;

        path.DrawWithGizmos();
    }
}
