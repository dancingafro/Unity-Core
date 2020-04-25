using UnityEngine;

namespace CoreScript.PathCreation.Examples
{
    [ExecuteInEditMode]
    public class PathPlacer : PathSceneTool
    {

        public GameObject prefab;
        public GameObject holder;
        public float spacing = 3;

        const float minSpacing = .1f;

        void Generate()
        {
            if (pathCreator != null && prefab != null && holder != null)
            {
                DestroyObjects();

                VertexPath VertexPath = pathCreator.VertexPath;

                spacing = Mathf.Max(minSpacing, spacing);
                float dst = 0;

                while (dst < VertexPath.length)
                {
                    Vector3 point = VertexPath.GetPointAtDistance(dst);
                    Quaternion rot = VertexPath.GetRotationAtDistance(dst);
                    Instantiate(prefab, point, rot, holder.transform);
                    dst += spacing;
                }
            }
        }

        void DestroyObjects()
        {
            int numChildren = holder.transform.childCount;
            for (int i = numChildren - 1; i >= 0; i--)
                DestroyImmediate(holder.transform.GetChild(i).gameObject, false);
        }

        protected override void PathUpdated() { if (pathCreator != null) Generate(); }
    }
}