using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public GameObject chunkPrefab;  // 地形块预制体
    public int chunkSize;    // 地形块边长
    public int renderDistance;  // 渲染范围（以地形块为单位）

    public Transform player;  
    //public GameObject[] treePrefabs; 

    private Dictionary<Vector2Int, GameObject> activeChunks; // 当前地形块
    private Dictionary<Vector2Int, MapData> chunkDataCache = new Dictionary<Vector2Int, MapData>(); // 本地缓存字典
    void Start()
    {
        player = Camera.main.transform; 
        activeChunks = new Dictionary<Vector2Int, GameObject>();
        // 清空本地缓存
        chunkDataCache.Clear();
        UpdateChunks(); // 初始化加载
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
                    RequestChunkFromServer(chunkCoord);
                }
            }
        }

        // 卸载视野范围外的地形块
       // List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunkCoord in activeChunks.Keys)
        {
            if (!chunksToKeep.Contains(chunkCoord))
            {
                UnloadChunk(chunkCoord);
            }
        }
    }

    Vector2Int GetChunkCoord(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / chunkSize),
            Mathf.FloorToInt(position.z / chunkSize)
        );
    }

    void RequestChunkFromServer(Vector2Int coord)
    {
        // 先检查本地缓存
        if (chunkDataCache.TryGetValue(coord, out MapData cachedMapData))
        {
            Debug.Log($"Chunk data for coord {coord} found in local cache.");
            // 如果缓存中存在，则直接使用
            OnChunkDataReceived(coord, cachedMapData);
        }
        else
        {
            // 缓存中不存在，向服务器请求
            Debug.Log($"Requesting chunk data for coord {coord} from server...");
            ServerManager.Instance.GenerateChunkData(chunkSize, coord, (receivedCoord, mapData) =>
            {
                // 接收服务器返回的数据后，存入本地缓存
                chunkDataCache[receivedCoord] = mapData;
                OnChunkDataReceived(coord,mapData);
            });
        }
    }

    void OnChunkDataReceived(Vector2Int coord, MapData mapData)
    {
        Debug.Log($"Received chunk data for coord {coord}");

        // 世界坐标
        Vector3 chunkPosition = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);

        GameObject chunk = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity);
        chunk.name = $"Chunk {coord.x}, {coord.y}";
        activeChunks[coord] = chunk;

        ApplyMapData(mapData, chunk);
    }

    void UnloadChunk(Vector2Int coord)
    {
        if (activeChunks.TryGetValue(coord, out GameObject chunk))
        {
            Destroy(chunk);
            activeChunks.Remove(coord);
        }
    }

    void ApplyMapData(MapData mapData, GameObject chunk)
    {
        // 获取地形组件
        Terrain terrain = chunk.GetComponent<Terrain>();
        TerrainCollider terrainCollider = chunk.GetComponent<TerrainCollider>();

        if (terrain == null || terrainCollider == null)
        {
            Debug.LogError("Missing Terrain or TerrainCollider on chunk!");
            return;
        }
        
        terrain.terrainData = mapData.terrainData;
        terrainCollider.terrainData = mapData.terrainData;

        // 实例化树
        //foreach (var tree in mapData.trees)
        //{
        //    GameObject treePrefab = treePrefabs[tree.treeType];
        //    GameObject treeInstance = Instantiate(treePrefab);

        //    treeInstance.transform.position = tree.position;
        //    treeInstance.transform.localScale = Vector3.one * tree.scale;

        //    treeInstance.transform.parent = chunk.transform;
        //}
    }

}
