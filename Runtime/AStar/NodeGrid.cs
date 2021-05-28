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
        public Vector2 gridWorldSize = Vector2.one;
        public float nodeRadius;
        Node[,] grid;

        float nodeDiameter;
        Vector2Int gridSize;
        int minWeight = int.MaxValue;
        int maxWeight = int.MinValue;

        public int MaxSize
        {
            get { return gridSize.x * gridSize.y; }
        }

        void Awake()
        {
            nodeDiameter = nodeRadius * 2;
            gridSize.x = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
            gridSize.y = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
            //gridSize.z = Mathf.RoundToInt(gridWorldSize.z / nodeDiameter);

            foreach (var item in walkableRegions)
            {
                walkableMask.value += item.terrainMask.value;
                walkableRegionsDictionary.Add((int)Mathf.Log(item.terrainMask.value, 2), item.terrainWeights);
            }

            CreateGrid();
        }


        void CreateGrid()
        {
            grid = new Node[gridSize.x, gridSize.y];

            Vector3 worldBtmLeft = transform.position - Vector3.right * gridWorldSize.x * .5f - Vector3.forward * gridWorldSize.y * .5f;

            for (int x = 0; x < gridSize.x; ++x)
            {
                for (int y = 0; y < gridSize.y; ++y)
                {
                    Vector3 worldPoint = worldBtmLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                    int weights = 0;
                    bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask);

                    Ray ray = new Ray(worldPoint + Vector3.up * nodeDiameter, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, nodeDiameter + nodeRadius, walkableMask))
                        walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out weights);

                    if (!walkable)
                        weights += obsticalPenalty;

                    grid[x, y] = new Node(walkable, worldPoint, x, y, weights);
                }
            }
            BlurPenaltyMap(3);
        }

        void BlurPenaltyMap(int blurSize)
        {
            int kernelSize = blurSize * 2 + 1;
            int kernalExtents = (int)((kernelSize - 1) * .5f);

            int[,] penaltiesHorizontalPass = new int[gridSize.x, gridSize.y];
            int[,] penaltiesVerticalPass = new int[gridSize.x, gridSize.y];

            for (int y = 0; y < gridSize.y; ++y)
            {
                for (int x = -kernalExtents; x <= kernalExtents; ++x)
                {
                    int sampleX = Mathf.Clamp(x, 0, kernalExtents);
                    penaltiesHorizontalPass[0, y] += grid[sampleX, y].weights;
                }

                for (int x = 1; x < gridSize.x; ++x)
                {
                    int removeIndex = Mathf.Clamp(x - kernalExtents - 1, 0, gridSize.x);
                    int addIndex = Mathf.Clamp(x + kernalExtents, 0, gridSize.x - 1);
                    penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].weights + grid[addIndex, y].weights;
                }
            }

            for (int x = 0; x < gridSize.x; ++x)
            {
                for (int y = -kernalExtents; y <= kernalExtents; ++y)
                {
                    int sampleY = Mathf.Clamp(y, 0, kernalExtents);
                    penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
                }

                int blurredWeights = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
                grid[x, 0].weights = blurredWeights;

                for (int y = 1; y < gridSize.y; ++y)
                {
                    int removeIndex = Mathf.Clamp(y - kernalExtents - 1, 0, gridSize.y);
                    int addIndex = Mathf.Clamp(y + kernalExtents, 0, gridSize.y - 1);
                    penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                    blurredWeights = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
                    grid[x, y].weights = blurredWeights;

                    if (blurredWeights < minWeight)
                        minWeight = blurredWeights;
                    if (blurredWeights > maxWeight)
                        maxWeight = blurredWeights;
                }
            }
        }

        public Node NodeFromWorldPoint(Vector3 worldPosition)
        {
            return grid[Mathf.RoundToInt((gridSize.x - 1) * Mathf.Clamp01((worldPosition.x + gridWorldSize.x * .5f) / gridWorldSize.x)), Mathf.RoundToInt((gridSize.y - 1) * Mathf.Clamp01((worldPosition.z + gridWorldSize.y * .5f) / gridWorldSize.y))];
        }

        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbourNode = new List<Node>();

            for (int x = -1; x <= 1; ++x)
            {
                for (int y = -1; y <= 1; ++y)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int nextX = node.gridX + x;
                    int nextY = node.gridY + y;

                    if (nextX < 0 || nextX >= gridSize.x || nextY < 0 || nextY >= gridSize.y)
                        continue;

                    neighbourNode.Add(grid[nextX, nextY]);
                }
            }
            return neighbourNode;
        }
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

            if (!debugMode)
                return;

            if (grid != null)
            {
                foreach (Node item in grid)
                {
                    Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(minWeight, maxWeight, item.weights));
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