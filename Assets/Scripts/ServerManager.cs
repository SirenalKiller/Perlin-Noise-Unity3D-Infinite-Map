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

        // ��ʼ����̬��Դ
        Chunk.InitializeResources(detailTextureLayers, grassTextures, treePrefabs);
    }

    public void GenerateChunkData(int chunksize, Vector2Int coord, System.Action<Vector2Int, MapData> callback)
    {
        // �������ο���󲢵��� Generate
        GameObject chunkObject = new GameObject("ServerChunk");
        Chunk chunk = chunkObject.AddComponent<Chunk>();

        chunk.chunkSize = chunksize;
        chunk.terrainDepth = 500;
        chunk.resolution = 513;
        chunk.scale = 0.07f;
        chunk.treeDensity = 20;

        MapData mapData = chunk.GenerateMapData(coord);

        // ������ʱ Chunk ����
        Destroy(chunkObject);

        // ���ص�ͼ���ݸ��ͻ���
        callback?.Invoke(coord, mapData);
    }
}
