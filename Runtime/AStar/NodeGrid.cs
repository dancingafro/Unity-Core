using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.AStar
{
    public class NodeGrid : MonoBehaviour
    {
        [SerializeField] bool debugMode;

        public LayerMask unwalkableMask;
        public int obsticalPenalty = 10;
        public TerrainType[] walkableRegions;
        LayerMask walkableMask;
        Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
        public Vector3 gridWorldSize;
        public float nodeRadius;
        Node[,,] grid;
        Queue<MinMax3DInt> requests = new Queue<MinMax3DInt>();

        static NodeGrid instance;

        float nodeDiameter;
        Vector3Int gridSize;
        Vector3 gridWorldBtmLeft;
        MinMaxInt minMaxWeight = new MinMaxInt();

        int blurBrushSize = 3;
        bool gridModified = false;
        bool threadIsAlive = false;
        public bool DebugMode { get { return debugMode; } }
        bool GotRequests { get { return debugMode; } }

        public int MaxSize { get { return gridSize.x * gridSize.y * gridSize.z; } }

        private void Awake()
        {
            if (grid == null)
                Init();
        }

        public void Init()
        {
            instance = this;
            nodeDiameter = nodeRadius * 2;
            gridSize = new Vector3Int(Mathf.RoundToInt(gridWorldSize.x / nodeDiameter), Mathf.RoundToInt(gridWorldSize.y / nodeDiameter), Mathf.RoundToInt(gridWorldSize.z / nodeDiameter));

            foreach (var item in walkableRegions)
            {
                walkableMask |= item.terrainMask;
                walkableRegionsDictionary.Add((int)Mathf.Log(item.terrainMask.value, 2), item.terrainWeights);
            }

            gridWorldBtmLeft = transform.position - new Vector3(gridWorldSize.x * .5f, gridWorldSize.y * .5f, gridWorldSize.z * .5f);

            CreateGrid();
        }

        void LoopGrid(Vector3Int min, Vector3Int max)
        {
            for (int y = min.y; y < max.y; ++y)
                for (int x = min.x; x < max.x; ++x)
                    for (int z = min.z; z < max.z; z++)
                        UpdateNodeGrid(x, y, z);
        }

        void CreateGrid()
        {
            grid = new Node[gridSize.x, gridSize.y, gridSize.z];
            LoopGrid(Vector3Int.zero, gridSize);
            BlurPenaltyMap();
        }

        private void LateUpdate()
        {
            for (int i = 0; i < requests.Count; ++i)
            {
                MinMax3DInt request = requests.Dequeue();
                LoopGrid(request.Min - Vector3Int.one, request.Max + Vector3Int.one);
                gridModified = true;
            }
            if (gridModified)
            {
                gridModified = false;
                BlurPenaltyMap();
            }
        }

        public void UpdateNodeGrid(int x, int y, int z)
        {
            Vector3 worldPoint = gridWorldBtmLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius) + Vector3.forward * (z * nodeDiameter + nodeRadius);
            int weights = 0;
            bool walkable = CheckIsWalkable(worldPoint, x, y, z);

            Ray ray = new Ray(worldPoint + Vector3.up * nodeRadius, Vector3.down);

            bool gotLand = Physics.Raycast(ray, out RaycastHit hit, nodeDiameter, walkableMask);

            if (gotLand)
                walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out weights);

            if (!walkable)
                weights += obsticalPenalty;

            grid[x, y, z] = new Node(walkable, gotLand, worldPoint, new Vector3Int(x, y, z), weights);
        }

        public static void ModifyGridNode(MinMax3DInt request)
        {
            instance.requests.Enqueue(request);
        }

        bool CheckIsWalkable(Vector3 worldPoint, int x, int y, int z)
        {
            bool isWalkable = !Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask);

            if (!isWalkable)
            {
                Collider[] temp = Physics.OverlapSphere(worldPoint, nodeRadius - .001f, unwalkableMask);

                foreach (var item in temp)
                {
                    if (!item.TryGetComponent(out Destructible destructible))
                        continue;

                    destructible.AddCoord(new Vector3Int(x, y, z));
                }
            }

            return isWalkable;
        }

        void BlurPenaltyMap()
        {
            lock (grid)
            {
                int kernelSize = blurBrushSize * 2 + 1;
                int kernalExtents = (int)((kernelSize - 1) * .5f);

                int[,] penaltiesHorizontalPass = new int[gridSize.x, gridSize.z];
                int[,] penaltiesForwardPass = new int[gridSize.x, gridSize.z];

                for (int y = 0; y < gridSize.y; ++y)
                {
                    for (int z = 0; z < gridSize.z; ++z)
                    {
                        for (int x = -kernalExtents; x <= kernalExtents; ++x)
                        {
                            int sampleX = Mathf.Clamp(x, 0, kernalExtents);
                            penaltiesHorizontalPass[0, z] += grid[sampleX, y, z].weights;
                        }

                        for (int x = 1; x < gridSize.x; ++x)
                        {
                            int removeIndex = Mathf.Clamp(x - kernalExtents - 1, 0, gridSize.x);
                            int addIndex = Mathf.Clamp(x + kernalExtents, 0, gridSize.x - 1);
                            penaltiesHorizontalPass[x, z] = penaltiesHorizontalPass[x - 1, z] - grid[removeIndex, y, z].weights + grid[addIndex, y, z].weights;
                        }
                    }

                    for (int x = 0; x < gridSize.x; ++x)
                    {
                        for (int z = -kernalExtents; z <= kernalExtents; ++z)
                        {
                            int sampleY = Mathf.Clamp(z, 0, kernalExtents);
                            penaltiesForwardPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
                        }

                        int blurredWeights = Mathf.RoundToInt((float)penaltiesForwardPass[x, 0] / (kernelSize * kernelSize));
                        grid[x, y, 0].weights = blurredWeights;

                        for (int z = 1; z < gridSize.z; ++z)
                        {
                            int removeIndex = Mathf.Clamp(z - kernalExtents - 1, 0, gridSize.z);
                            int addIndex = Mathf.Clamp(z + kernalExtents, 0, gridSize.z - 1);
                            penaltiesForwardPass[x, z] = penaltiesForwardPass[x, z - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                            blurredWeights = Mathf.RoundToInt((float)penaltiesForwardPass[x, z] / (kernelSize * kernelSize));
                            grid[x, y, z].weights = blurredWeights;

                            minMaxWeight.AddValue(blurredWeights);
                        }
                    }
                }
                gridModified = false;
                threadIsAlive = false;
            }
        }

        public Node NodeFromWorldPoint(Vector3 worldPosition)
        {
            return grid[Mathf.RoundToInt((gridSize.x - 1) * Mathf.Clamp01((worldPosition.x + gridWorldSize.x * .5f) / gridWorldSize.x)), Mathf.RoundToInt((gridSize.y - 1) * Mathf.Clamp01((worldPosition.y + gridWorldSize.y * .5f) / gridWorldSize.y)), Mathf.RoundToInt((gridSize.z - 1) * Mathf.Clamp01((worldPosition.z + gridWorldSize.z * .5f) / gridWorldSize.z))];
        }

        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbourNode = new List<Node>();

            for (int x = -1; x <= 1; ++x)
            {
                for (int y = -1; y <= 1; ++y)
                {
                    for (int z = -1; z <= 1; ++z)
                    {
                        if (x == 0 && y == 0 && z == 0)
                            continue;

                        Vector3Int next = node.grid + new Vector3Int(x, y, z);

                        if (next.x < 0 || next.x >= gridSize.x || next.y < 0 || next.y >= gridSize.y || next.z < 0 || next.z >= gridSize.z)
                            continue;

                        neighbourNode.Add(grid[next.x, next.y, next.z]);
                    }
                }
            }
            return neighbourNode;
        }
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, gridWorldSize.z));

            if (!debugMode)
                return;

            if (grid != null)
            {
                foreach (Node item in grid)
                {
                    if (!item.gotLand)
                        continue;

                    Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(minMaxWeight.Min, minMaxWeight.Max, item.weights));
                    Gizmos.color = item.isWalkable ? Gizmos.color : Color.red;
                    Gizmos.DrawCube(item.worldPos, Vector3.one * (nodeDiameter - .1f));
                }
            }
        }
    }

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainWeights;
    }
}