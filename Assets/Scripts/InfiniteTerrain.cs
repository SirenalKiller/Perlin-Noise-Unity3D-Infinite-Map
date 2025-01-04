using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    public GameObject chunkPrefab; // ���ο�Ԥ����
    public int chunkSize = 1024;   // ���ο�߳�
    public int renderDistance = 2; // ��Ⱦ��Χ���Ե��ο�Ϊ��λ��

    private Transform player; 
    private Dictionary<Vector2Int, GameObject> activeChunks; // ��ǰ���ο�

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
                    LoadChunk(chunkCoord);
                }
            }
        }

        // ж����Ұ��Χ��ĵ��ο�
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
        // ��������
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
