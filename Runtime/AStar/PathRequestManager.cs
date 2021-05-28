using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.AStar
{
    public class PathRequestManager : MonoBehaviour
    {
        Queue<PathRequest> pathRequestsQueue = new Queue<PathRequest>();
        PathRequest currentPathRequest;

        static PathRequestManager instance;
        PathFinding pathFinding;

        bool isProcessing;

        private void Awake()
        {
            instance = this;
            pathFinding = GetComponent<PathFinding>();
        }

        public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
        {
            instance.pathRequestsQueue.Enqueue(new PathRequest(pathStart, pathEnd, callback));
            instance.TryProcessNext();
        }

        void TryProcessNext()
        {
            if (!isProcessing && pathRequestsQueue.Count > 0)
            {
                currentPathRequest = pathRequestsQueue.Dequeue();
                isProcessing = true;
                pathFinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
            }
        }

        public void FinishedProcessingPath(Vector3[] path, bool success)
        {
            currentPathRequest.callback(path, success);
            isProcessing = false;
            TryProcessNext();
        }

        struct PathRequest
        {
            public Vector3 pathStart;
            public Vector3 pathEnd;
            public Action<Vector3[], bool> callback;

            public PathRequest(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
            {
                this.pathStart = pathStart;
                this.pathEnd = pathEnd;
                this.callback = callback;
            }
        }
    }
}