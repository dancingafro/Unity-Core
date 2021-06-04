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
        Thread thread;
        bool started = true;
        private void Awake()
        {
            thread = new Thread(ProcessRequest);
            instance = this;
            pathFinding = GetComponent<PathFinding>();
        }

        void OnEnable()
        {
            thread.Start();
            started = true;
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
            while (started)
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

        private void OnDisable()
        {
            started = false;
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
        public bool needLand;
        public Action<Vector3[], bool> callback;

        public PathRequest(Vector3 pathStart, Vector3 pathEnd, bool needLand, Action<Vector3[], bool> callback)
        {
            this.pathStart = pathStart;
            this.pathEnd = pathEnd;
            this.needLand = needLand;
            this.callback = callback;
        }
    }
}