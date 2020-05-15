using UnityEngine;

namespace CoreScript.PathCreation.Examples
{
    [ExecuteInEditMode]
    public abstract class PathSceneTool : MonoBehaviour
    {
        public event System.Action OnDestroyed;
        public PathCreator pathCreator;
        public bool autoUpdate = true;

        protected VertexPath VertexPath { get { return pathCreator.VertexPath; } }
        public void TriggerUpdate() { PathUpdated(); }
        protected virtual void OnDestroy() { OnDestroyed?.Invoke(); }

        protected abstract void PathUpdated();
    }
}
