using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    public GameObject chunkPrefab; // 地形块预制体
    public int chunkSize = 1024;   // 地形块边长
    public int renderDistance = 2; // 渲染范围（以地形块为单位）

    private Transform player; 
    private Dictionary<Vector2Int, GameObject> activeChunks; // 当前地形块

    void Start()
    {
        player = Camera.main.transform; 
        activeChunks = new Dictionary<Vector2Int, GameObject>();
        UpdateChunks();
    }

    void Update()
    {
        UpdateChunks(); 
    }

    void UpdateChunks()
    {
        // 获取玩家所在地形块的坐标
        Vector2Int playerChunkCoord = GetChunkCoord(player.position);
        HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();

        // 加载视野范围内的地形块
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                Vector2Int chunkCoord = playerChunkCoord + new Vector2Int(x, z);
                chunksToKeep.Add(chunkCoord);

                if (!activeChunks.ContainsKey(chunkCoord))
                {
                    LoadChunk(chunkCoord);
                }
            }
        }

        // 卸载视野范围外的地形块
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunkCoord in activeChunks.Keys)
        {
            if (!chunksToKeep.Contains(chunkCoord))
            {
                chunksToRemove.Add(chunkCoord);
            }
        }

        foreach (var chunkCoord in chunksToRemove)
        {
            UnloadChunk(chunkCoord);
        }
    }

    Vector2Int GetChunkCoord(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / chunkSize),
            Mathf.FloorToInt(position.z / chunkSize)
        );
    }

    void LoadChunk(Vector2Int coord)
    {
        // 世界坐标
        Vector3 chunkPosition = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);

        GameObject chunk = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity);
        chunk.name = $"Chunk {coord.x}, {coord.y}";

        Chunk chunkScript = chunk.GetComponent<Chunk>();
        if (chunkScript != null)
        {
            chunkScript.chunkSize = chunkSize;
            chunkScript.Generate(coord); 
        }

        activeChunks.Add(coord, chunk);
    }

    void UnloadChunk(Vector2Int coord)
    {
        if (activeChunks.TryGetValue(coord, out GameObject chunk))
        {
            Destroy(chunk);
            activeChunks.Remove(coord);
        }
    }
}
