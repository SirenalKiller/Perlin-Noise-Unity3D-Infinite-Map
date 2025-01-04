using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public GameObject chunkPrefab;  // ���ο�Ԥ����
    public int chunkSize;    // ���ο�߳�
    public int renderDistance;  // ��Ⱦ��Χ���Ե��ο�Ϊ��λ��

    public Transform player;  
    //public GameObject[] treePrefabs; 

    private Dictionary<Vector2Int, GameObject> activeChunks; // ��ǰ���ο�
    private Dictionary<Vector2Int, MapData> chunkDataCache = new Dictionary<Vector2Int, MapData>(); // ���ػ����ֵ�
    void Start()
    {
        player = Camera.main.transform; 
        activeChunks = new Dictionary<Vector2Int, GameObject>();
        // ��ձ��ػ���
        chunkDataCache.Clear();
        UpdateChunks(); // ��ʼ������
    }

    void Update()
    {
        UpdateChunks(); 
    }

    void UpdateChunks()
    {
        // ��ȡ������ڵ��ο������
        Vector2Int playerChunkCoord = GetChunkCoord(player.position);
        HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();

        // ������Ұ��Χ�ڵĵ��ο�
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

        // ж����Ұ��Χ��ĵ��ο�
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
        // �ȼ�鱾�ػ���
        if (chunkDataCache.TryGetValue(coord, out MapData cachedMapData))
        {
            Debug.Log($"Chunk data for coord {coord} found in local cache.");
            // ��������д��ڣ���ֱ��ʹ��
            OnChunkDataReceived(coord, cachedMapData);
        }
        else
        {
            // �����в����ڣ������������
            Debug.Log($"Requesting chunk data for coord {coord} from server...");
            ServerManager.Instance.GenerateChunkData(chunkSize, coord, (receivedCoord, mapData) =>
            {
                // ���շ��������ص����ݺ󣬴��뱾�ػ���
                chunkDataCache[receivedCoord] = mapData;
                OnChunkDataReceived(coord,mapData);
            });
        }
    }

    void OnChunkDataReceived(Vector2Int coord, MapData mapData)
    {
        Debug.Log($"Received chunk data for coord {coord}");

        // ��������
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
        // ��ȡ�������
        Terrain terrain = chunk.GetComponent<Terrain>();
        TerrainCollider terrainCollider = chunk.GetComponent<TerrainCollider>();

        if (terrain == null || terrainCollider == null)
        {
            Debug.LogError("Missing Terrain or TerrainCollider on chunk!");
            return;
        }
        
        terrain.terrainData = mapData.terrainData;
        terrainCollider.terrainData = mapData.terrainData;

        // ʵ������
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
