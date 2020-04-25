using CoreScript.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreScript.Factories;

namespace CoreScript.Procidual
{
    public class EndlessTerrain : MonoBehaviour
    {
        const float scale = 1f;
        const float viewerMoveThresholdForChunkUpdate = 25f;
        const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
        public LODInfo[] detailLevels;
        public static float maxViewDst = 450f;

        public Transform viewer;
        public Material mapMaterial;

        public static Vector2 viewerPos;
        Vector2 viewerPosOld = Vector2.zero;
        int chunkSize;
        int chunkVisibleInViewDst;
        static MapGenerator mapGenerator;
        Dictionary<Vector2, TerrainChunk> terrainChunks = new Dictionary<Vector2, TerrainChunk>();
        static List<TerrainChunk> lastVisibleChunk = new List<TerrainChunk>();

        void Start()
        {
            mapGenerator = FindObjectOfType<MapGenerator>();
            maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
            chunkSize = MapGenerator.mapChunkSize;
            chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
            UpdateVisibleChunks();
        }

        void Update()
        {
            viewerPos = new Vector2(viewer.position.x, viewer.position.z) / scale;
            if ((viewerPosOld - viewerPos).sqrMagnitude >= sqrViewerMoveThresholdForChunkUpdate)
            {
                viewerPosOld = viewerPos;
                UpdateVisibleChunks();
            }
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
                        continue;
                    }
                    terrainChunks.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
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
            LODInfo[] detailLevels;
            LODMesh[] lodMeshes;

            MapData mapData;
            bool mapDataRecieved = false;
            int previousLODIndex = -1;

            public bool IsVisible { get { return meshObject.activeSelf; } }

            public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
            {
                position = coord * size;
                bounds = new Bounds(position, Vector3.one * size);
                Vector3 positionV3 = new Vector3(coord.x * (size - 1), 0, coord.y * (size - 1));

                this.detailLevels = detailLevels;
                lodMeshes = new LODMesh[detailLevels.Length];

                for (int i = 0; i < lodMeshes.Length; ++i)
                    lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);

                meshObject = new GameObject("Terrain Chunk");
                meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshFilter = meshObject.AddComponent<MeshFilter>();

                meshRenderer.material = material;

                meshObject.transform.position = positionV3 * scale;
                meshObject.transform.parent = parent;
                meshObject.transform.localScale = Vector3.one * scale;

                SetVisible(false);

                mapGenerator.RequestMapData(position, OnMapDataRecieved);
            }

            void OnMapDataRecieved(MapData mapData)
            {
                this.mapData = mapData;
                mapDataRecieved = true;

                Texture2D texture = UtilityClass.TextureFromColors(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
                meshRenderer.material.mainTexture = texture;

                UpdateTerrainChunk();
            }

            void OnMeshDataRecieved(MeshData meshData)
            {
                meshFilter.mesh = meshData.CreateMesh();
            }

            public void UpdateTerrainChunk()
            {
                if (!mapDataRecieved)
                    return;

                float viewerDstFromNearestEdgeSqr = bounds.SqrDistance(viewerPos);
                bool visible = viewerDstFromNearestEdgeSqr <= maxViewDst * maxViewDst;

                if (visible)
                {
                    int lodIndex = detailLevels.Length - 1;
                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (DstanceCheck(viewerDstFromNearestEdgeSqr, detailLevels[i].visibleDstThreshold))
                            continue;

                        lodIndex = i;
                        break;
                    }
                    SwapMesh(lodIndex);

                    lastVisibleChunk.Add(this);
                }


                SetVisible(visible);
            }

            void SwapMesh(int lodIndex)
            {
                if (previousLODIndex == lodIndex)
                    return;

                LODMesh lodMesh = lodMeshes[lodIndex];
                if (lodMesh.hasMesh)
                {
                    previousLODIndex = lodIndex;
                    meshFilter.mesh = lodMesh.mesh;
                }
                else if (!lodMesh.hasRequested)
                    lodMesh.RequestMesh(mapData);
            }

            bool DstanceCheck(float viewerDstFromNearestEdgeSqr, float maxViewDst)
            {
                return viewerDstFromNearestEdgeSqr > maxViewDst * maxViewDst;
            }

            public void SetVisible(bool visible)
            {
                meshObject.SetActive(visible);
            }

        }

        class LODMesh
        {
            public Mesh mesh;
            public bool hasRequested;
            public bool hasMesh;
            int lod;
            System.Action updateCallback;

            public LODMesh(int lod, System.Action updateCallback)
            {
                this.lod = lod;
                this.updateCallback = updateCallback;
            }

            void OnMeshDataRequested(MeshData meshData)
            {
                mesh = meshData.CreateMesh();
                hasMesh = true;
                updateCallback();
            }

            public void RequestMesh(MapData mapData)
            {
                hasRequested = true;
                mapGenerator.RequestMeshData(mapData, lod, OnMeshDataRequested);
            }
        }

        [System.Serializable]
        public struct LODInfo
        {
            public int lod;
            public float visibleDstThreshold;
        }
    }
}