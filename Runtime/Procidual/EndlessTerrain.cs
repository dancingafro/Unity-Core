using CoreScript.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Procidual
{
    public class EndlessTerrain : MonoBehaviour
    {
        public const float maxViewDst = 300f;
        public Transform viewer;
        public Material mapMaterial;

        public static Vector2 viewerPos;
        int chunkSize;
        int chunkVisibleInViewDst;
        static MapGenerator mapGenerator;
        Dictionary<Vector2, TerrainChunk> terrainChunks = new Dictionary<Vector2, TerrainChunk>();
        List<TerrainChunk> lastVisibleChunk = new List<TerrainChunk>();

        void Start()
        {
            mapGenerator = FindObjectOfType<MapGenerator>();
            chunkSize = MapGenerator.mapChunkSize;
            chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
        }

        void Update()
        {
            viewerPos = new Vector2(viewer.position.x, viewer.position.z);
            UpdateVisibleChunks();
        }

        void UpdateVisibleChunks()
        {
            for (int i = 0; i < lastVisibleChunk.Count; ++i)
                lastVisibleChunk[i].SetVisible(false);

            lastVisibleChunk.Clear();

            Vector2Int currentChunkCoord = new Vector2Int(Mathf.RoundToInt(viewerPos.x / chunkSize), Mathf.RoundToInt(viewerPos.y / chunkSize));
            for (int xOffset = -chunkVisibleInViewDst; xOffset <= chunkVisibleInViewDst; ++xOffset)
            {
                for (int yOffset = -chunkVisibleInViewDst; yOffset <= chunkVisibleInViewDst; ++yOffset)
                {
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoord.x + xOffset, currentChunkCoord.y + yOffset);

                    if (terrainChunks.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunks[viewedChunkCoord].UpdateTerrainChunk();

                        if (terrainChunks[viewedChunkCoord].IsVisible)
                            lastVisibleChunk.Add(terrainChunks[viewedChunkCoord]);

                        continue;
                    }
                    terrainChunks.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, mapMaterial));
                }
            }
        }

        public class TerrainChunk
        {
            GameObject meshObject;
            Vector2 position;
            Bounds bounds;

            MeshRenderer meshRenderer;
            MeshFilter meshFilter;

            public bool IsVisible { get { return meshObject.activeSelf; } }

            public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
            {
                position = coord * size;
                bounds = new Bounds(position, Vector3.one * size);
                Vector3 positionV3 = new Vector3(position.x, 0, position.y);

                meshObject = new GameObject("Terrain Chunk");
                meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshFilter = meshObject.AddComponent<MeshFilter>();

                meshRenderer.material = material;

                meshObject.transform.position = positionV3;
                meshObject.transform.parent = parent;
                SetVisible(false);

                mapGenerator.RequestMapData(OnMapDataRecieved);
            }

            void OnMapDataRecieved(MapData mapData)
            {
                mapGenerator.RequestMeshData(mapData, OnMeshDataRecieved);
            }

            void OnMeshDataRecieved(MeshData meshData)
            {
                meshFilter.mesh = meshData.CreateMesh();
            }

            public void UpdateTerrainChunk()
            {
                float viewerDstFromNearestEdge = bounds.SqrDistance(viewerPos);
                bool visible = viewerDstFromNearestEdge <= maxViewDst * maxViewDst;
                SetVisible(visible);
            }

            public void SetVisible(bool visible)
            {
                meshObject.SetActive(visible);
            }

        }
    }
}