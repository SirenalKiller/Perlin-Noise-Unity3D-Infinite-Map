using System.Collections.Generic;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance;
    public TerrainLayer[] detailTextureLayers; 
    public Texture2D[] grassTextures; 
    public GameObject[] treePrefabs; 
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 初始化静态资源
        Chunk.InitializeResources(detailTextureLayers, grassTextures, treePrefabs);
    }

    public void GenerateChunkData(int chunksize, Vector2Int coord, System.Action<Vector2Int, MapData> callback)
    {
        // 创建地形块对象并调用 Generate
        GameObject chunkObject = new GameObject("ServerChunk");
        Chunk chunk = chunkObject.AddComponent<Chunk>();

        chunk.chunkSize = chunksize;
        chunk.terrainDepth = 500;
        chunk.resolution = 513;
        chunk.scale = 0.07f;
        chunk.treeDensity = 20;

        MapData mapData = chunk.GenerateMapData(coord);

        // 销毁临时 Chunk 对象
        Destroy(chunkObject);

        // 返回地图数据给客户端
        callback?.Invoke(coord, mapData);
    }
}
