using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.AStar
{
    public class PathRequestManager : MonoBehaviour
    {
        Queue<PathResult> results = new Queue<PathResult>();
        Queue<PathRequest> requests = new Queue<PathRequest>();
        static PathRequestManager instance;
        PathFinding pathFinding;

        private void Awake()
        {
            Thread thread = new Thread(ProcessRequest);
            thread.Start();
            instance = this;
            pathFinding = GetComponent<PathFinding>();
        }

        private void Update()
        {
            if (results.Count == 0)
                return;

            int length = results.Count;

            lock (results)
            {
                for (int i = 0; i < length; ++i)
                {
                    PathResult result = results.Dequeue();
                    result.callback(result.path, result.success);
                }
            }
        }

        public static void RequestPath(PathRequest request)
        {
            lock (instance.requests)
            {
                instance.requests.Enqueue(request);
            }
        }

        void ProcessRequest()
        {
            while (true)
            {
                int length = requests.Count;

                lock (requests)
                {
                    for (int i = 0; i < length; ++i)
                    {
                        pathFinding.FindPath(requests.Dequeue(), instance.FinishedProcessingPath);
                    }
                }
            }
        }
        public void FinishedProcessingPath(PathResult result)
        {
            lock (results)
            {
                results.Enqueue(result);
            }
        }

    }
    public struct PathResult
    {
        public Vector3[] path;
        public bool success;
        public Action<Vector3[], bool> callback;

        public PathResult(Vector3[] path, bool success, Action<Vector3[], bool> callback)
        {
            this.path = path;
            this.success = success;
            this.callback = callback;
        }
    }

    public struct PathRequest
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