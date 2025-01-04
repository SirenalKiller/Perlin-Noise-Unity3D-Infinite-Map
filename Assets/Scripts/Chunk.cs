using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class MapData
{
    public TerrainData terrainData; 
    //public List<TreeData> trees; 
}

[System.Serializable]
public class TreeData
{
    public Vector3 position;
    public float scale;
    public int treeType; 
}
public class Chunk : MonoBehaviour
{
    // ȫ����Դ
    public static TerrainLayer[] detailTextureLayers;
    public static Texture2D[] grassTextures;
    public static GameObject[] treePrefabs;
    public int chunkSize;
    public float terrainDepth;
    public int resolution;
    public float scale = 0.08f;
    private float freqeuncyScale;
    private float maxHeightScale;
    // TerrainData �����ֵ�
    // private static Dictionary<Vector2Int, TerrainData> terrainDataCache = new Dictionary<Vector2Int, TerrainData>();
    public float[] frequencyScales = { 300f, 80f, 2f };
    public float[] maxHeights = { 300f, 150f, 5f };
    //public TerrainLayer[] detailTextureLayers; // �� Inspector ������һ�� TerrainLayer
    //public Texture2D[] grassTextures;         // �ݵ���ͼ
    //public GameObject[] treePrefabs; // ����Ԥ�������飬֧�ֶ�����
    public int treeDensity = 30;    // �����ܶȣ��������ɵ�������
    public int minGrassDensity = 240;
    public int maxGrassDensity = 250;
    public int detailResolution = 1024;
    public int mountainHeight = 300;
    public int mountainDelta = 100;
    public int landHeight = 50;

    public static void InitializeResources(TerrainLayer[] DetailTextureLayers, Texture2D[] GrassTextures, GameObject[] TreePrefabs)
    {
        detailTextureLayers = DetailTextureLayers;
        grassTextures = GrassTextures;
        treePrefabs = TreePrefabs;
    }
    public void Generate(Vector2Int coord)
    {
        //Terrain terrain = GetComponent<Terrain>();
        //if (terrain == null)
        //{
        //    Debug.LogError("Missing Terrain component on Chunk.");
        //    return;
        //}

        //TerrainData terrainData;

        //// ����������Ѵ��ڸ� coord ��Ӧ�� TerrainData����ֱ��ʹ��
        //if (terrainDataCache.TryGetValue(coord, out terrainData))
        //{
        //    Debug.Log($"Using cached TerrainData for chunk {coord}");
        //}
        //else
        //{
        //    Debug.Log($"Creating new TerrainData for chunk {coord}");
        //    // �����µ� TerrainData
        //    terrainData = new TerrainData();
        //    freqeuncyScale = GenerateRandomNumber(coord, 0.5f, 5f);
        //    maxHeightScale = GenerateRandomNumber(coord, 0.3f, 2f);

        //    // ���õ��δ�С��ȷ���� chunkSize ƥ��
        //    terrainData.size = new Vector3(chunkSize * scale, terrainDepth * maxHeightScale, chunkSize * scale);
        //    terrainData.heightmapResolution = resolution;

        //    // ���ɸ߶�ͼ
        //    float[,] heights = GenerateHeights(coord);


        //    // Ӧ�ø߶�����
        //    terrainData.SetHeights(0, 0, heights);

        //    // ��� Detail Texture
        //    AddDetailTexture(terrainData);

        //    // ��������
        //    AddGrassDetails(terrainData);

        //    // �������ɵ� TerrainData
        //    terrainDataCache[coord] = terrainData;
        //    // ���� AddTrees ����
        //    AddTrees(terrainData, transform.position);
        //}

        //// Ӧ�õ����κ͵�����ײ��
        //terrain.terrainData = terrainData;
        //TerrainCollider terrainCollider = GetComponent<TerrainCollider>();
        //terrainCollider.terrainData = terrainData;

        //Debug.Log($"Chunk {coord} generated at {transform.position}");
    }
    float GenerateRandomNumber(Vector2Int coord, float max, float min)
    {
        int seed = coord.x * 73856093 ^ coord.y * 19349663; 
        UnityEngine.Random.InitState(seed);

        float randomValue = UnityEngine.Random.value; 
        return Mathf.Lerp(min, max, randomValue);
    }
    float RemapMaxHeightScale()
    {
        if (maxHeightScale < 1f)
        {
            return Mathf.Lerp(1f, maxHeightScale, 0.1f); 
        }
        else
        {
            return Mathf.Lerp(1f, 1.5f, Mathf.InverseLerp(1f, 1.5f, maxHeightScale));
        }
    }
    float[,] GenerateHeights(Vector2Int coord)
    {
        float[,] heights = new float[resolution, resolution];
        int bufferWidth = Mathf.CeilToInt(0.3f * RemapMaxHeightScale() * (resolution - 1)); // ��������ȣ�ռChunk��Ե��40%

        for (int x = 0; x < resolution; x++)
        {
            for (int z = 0; z < resolution; z++)
            {
                // �����������
                float worldX = (coord.x + (float)x / (resolution - 1)) * chunkSize+GenerateRandomNumber(coord, 0, 100);
                float worldZ = (coord.y + (float)z / (resolution - 1)) * chunkSize+GenerateRandomNumber(coord, 0, 100);

                float finalHeight = 0f;

                // �����������
                for (int i = 0; i < frequencyScales.Length; i++)
                {
                    float frequency = frequencyScales[i]* freqeuncyScale;
                    float noiseValue = Mathf.PerlinNoise(worldX / frequency, worldZ / frequency);
                    float mappedHeight = noiseValue * maxHeights[i] * maxHeightScale;
                    finalHeight += mappedHeight;
                }

                // ��Ե���������߼�
                float edgeFactor = 1f;

                // �±߽�
                if (z < bufferWidth)
                {
                    float t = (float)z / bufferWidth; // ��ֵ���� [0, 1]
                    edgeFactor = Mathf.SmoothStep(0.0f, 1.0f, t); 
                }

                // �ϱ߽�
                if (z >= resolution - bufferWidth)
                {
                    float t = (float)(resolution - 1 - z) / bufferWidth; // ��ֵ���� [0, 1]
                    edgeFactor = Mathf.SmoothStep(0.0f, 1.0f, t); 
                }

                // ��߽�
                if (x < bufferWidth)
                {
                    float t = (float)x / bufferWidth; // ��ֵ���� [0, 1]
                    edgeFactor = Mathf.Min(edgeFactor, Mathf.SmoothStep(0.0f, 1.0f, t));
                }

                // �ұ߽�
                if (x >= resolution - bufferWidth)
                {
                    float t = (float)(resolution - 1 - x) / bufferWidth; // ��ֵ���� [0, 1]
                    edgeFactor = Mathf.Min(edgeFactor, Mathf.SmoothStep(0.0f, 1.0f, t));
                }

                // Ӧ�ñ�Ե���۲����Ӹ�Ƶ��������
                float highFrequencyNoise = Mathf.PerlinNoise(worldX / 10f, worldZ / 10f) * 0.005f; 
                heights[x, z] = (finalHeight / terrainDepth * edgeFactor) + highFrequencyNoise; 
            }
        }

        return heights;
    }

    //void AddTrees(TerrainData terrainData, Vector3 chunkPosition)
    //{
    //    if (treePrefabs == null || treePrefabs.Length == 0)
    //    {
    //        Debug.LogWarning("Tree prefabs are not assigned!");
    //        return;
    //    }

    //    // ��ȡ���εĿ�Ⱥ����
    //    float terrainWidth = terrainData.size.x;
    //    float terrainDepth = terrainData.size.z;

    //    // �����������λ��
    //    for (int i = 0; i < treeDensity; i++)
    //    {
    //        // ���������������
    //        float normalizedX = Random.Range(0f, 1f); // ��һ���� X ����
    //        float normalizedZ = Random.Range(0f, 1f); // ��һ���� Z ����

    //        // ��ȡ�õ������߶�
    //        float worldHeight = terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);

    //        // �ж��Ƿ���� mountainHeight
    //        if (worldHeight > mountainHeight && worldHeight< landHeight) continue;

    //        // ת��Ϊ��������
    //        float worldX = chunkPosition.x + normalizedX * terrainWidth;
    //        float worldZ = chunkPosition.z + normalizedZ * terrainDepth;
    //        float worldY = worldHeight;

    //        // ���ѡ��һ������Ԥ����
    //        GameObject selectedTreePrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];

    //        // ʵ������
    //        GameObject tree = Instantiate(selectedTreePrefab);
    //        tree.transform.position = new Vector3(worldX, worldY, worldZ);
    //        tree.transform.localScale *= Random.Range(0.5f, 1.5f); // ����������Ĵ�С
    //        tree.transform.parent = this.transform; // ��������Ϊ��ǰ Chunk ���Ӷ���
    //    }
    //}
    void AddGrassDetails(TerrainData terrainData)
    {
        if (grassTextures == null || grassTextures.Length == 0)
        {
            Debug.LogWarning("Grass texture is not assigned!");
            return;
        }

        DetailPrototype[] grassDetails = new DetailPrototype[grassTextures.Length];

        // ����ݵ�ϸ��ԭ��
        for (int i = 0; i < grassTextures.Length; i++)
        {
            grassDetails[i] = new DetailPrototype
            {
                prototypeTexture = grassTextures[i],         // �ݵ���ͼ
                renderMode = DetailRenderMode.Grass, // ��Ƭ��
                minWidth = 19f,                              // �ݵ���С���
                maxWidth = 20f,                              // �ݵ������
                minHeight = 19f,                             // �ݵ���С�߶�
                maxHeight = 20f,                             // �ݵ����߶�
                noiseSpread = 0.8f,                          // �ݵ�����ֲ�
                healthyColor = Color.white,                  // ����״̬��ɫ
                dryColor = Color.white,                      // ����״̬��ɫ
            };
        }
        terrainData.detailPrototypes = grassDetails;

        // ���ɲݵĵ�ͼ
        terrainData.SetDetailResolution(detailResolution, 64);

        // Ϊÿ�ֲ����ɵ�ͼ
        for (int layer = 0; layer < grassTextures.Length; layer++)
        {
            int[,] grassMap = new int[detailResolution, detailResolution];

            for (int x = 0; x < detailResolution; x++)
            {
                for (int z = 0; z < detailResolution; z++)
                {
                    // ��������߶�
                    float normalizedX = (float)x / (detailResolution - 1);
                    float normalizedZ = (float)z / (detailResolution - 1);
                    float worldHeight = terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);

                    // ���ݺ��ε����ݵ��ܶ�
                    if (worldHeight >= mountainHeight + mountainDelta)
                    {
                        grassMap[x, z] = 0; 
                    }
                    else if (worldHeight >= mountainHeight)
                    {
                        float t = Mathf.InverseLerp(mountainHeight + mountainDelta, mountainHeight, worldHeight);
                        grassMap[x, z] = Mathf.RoundToInt(Random.Range(minGrassDensity * t, maxGrassDensity * t)); 
                    }
                    else
                    {
                        grassMap[x, z] = Random.Range(minGrassDensity, maxGrassDensity); 
                    }
                }
            }

            terrainData.SetDetailLayer(0, 0, layer, grassMap);
        }
    }


    void AddDetailTexture(TerrainData terrainData)
    {
        if (detailTextureLayers == null || detailTextureLayers.Length < 3)
        {
            Debug.LogWarning("Detail texture layers are not correctly assigned! Make sure to have at least 3 layers: grass, snow, and rock.");
            return;
        }

        TerrainLayer grassLayer = new TerrainLayer
        {
            diffuseTexture = detailTextureLayers[0].diffuseTexture, 
            tileSize = new Vector2(15, 15)
        };

        TerrainLayer snowLayer = new TerrainLayer
        {
            diffuseTexture = detailTextureLayers[1].diffuseTexture, 
            tileSize = new Vector2(15, 15)
        };

        TerrainLayer rockLayer = new TerrainLayer
        {
            diffuseTexture = detailTextureLayers[2].diffuseTexture,
            tileSize = new Vector2(15, 15)
        };

        terrainData.terrainLayers = new TerrainLayer[] { grassLayer, snowLayer, rockLayer };

        // ����Alpha Map
        int alphaMapResolution = terrainData.alphamapResolution;
        float[,,] alphaMap = new float[alphaMapResolution, alphaMapResolution, 3]; // 3 ��

        for (int x = 0; x < alphaMapResolution; x++)
        {
            for (int z = 0; z < alphaMapResolution; z++)
            {
                // ���㵱ǰ�߶���������
                float normalizedX = (float)x / (alphaMapResolution - 1);
                float normalizedZ = (float)z / (alphaMapResolution - 1);
                float height = terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);

                float snowStartHeight = mountainHeight; // �ݵ���ѩ�����ɿ�ʼ�ĸ߶�
                float snowEndHeight = mountainHeight + mountainDelta; // ��ȫѩ���ǵĸ߶�
                float rockStartHeight = mountainHeight + mountainDelta; // ѩ������ʯ���ɿ�ʼ�ĸ߶�
                float rockEndHeight = mountainHeight + 1.5f * mountainDelta; // ��ȫ��ʯ���ǵĸ߶�

                // ����Ȩ��
                float snowWeight = Mathf.Clamp01(Mathf.InverseLerp(snowStartHeight, snowEndHeight, height));
                float grassWeight = Mathf.Clamp01(1f - snowWeight);
                float rockWeight = Mathf.Clamp01(Mathf.InverseLerp(rockStartHeight, rockEndHeight, height));

                // ��ֹ������ʵ��ӹ���
                if (height > snowEndHeight && height < rockStartHeight)
                {
                    rockWeight = 0f; // ��ѩ������ʯ֮�������ʯȨ��
                }

                // ����Ȩ�ص�Alpha Map
                alphaMap[x, z, 0] = grassWeight; // �ݵ�
                alphaMap[x, z, 1] = snowWeight; // ѩ��
                alphaMap[x, z, 2] = rockWeight; // ��ʯ
            }
        }

        // Ӧ�� Alpha Map
        terrainData.SetAlphamaps(0, 0, alphaMap);
    }

    List<TreeData> GenerateTreeData(TerrainData terrainData, Vector2Int coord)
    {
        List<TreeData> treeDataList = new List<TreeData>();

        // ���γߴ�
        float treeTerrainWidth = terrainData.size.x;
        float treeTerrainDepth = terrainData.size.z;

        for (int i = 0; i < treeDensity; i++)
        {
            // ���λ��
            float normalizedX = Random.Range(0f, 1f);
            float normalizedZ = Random.Range(0f, 1f);

            // ��ȡ�߶�
            float worldHeight = terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);

            // �߶�����
            if (worldHeight > mountainHeight || worldHeight < landHeight)
            {
                continue;
            }

            // ת��Ϊ��������
            float worldX = coord.x * treeTerrainWidth + normalizedX * treeTerrainWidth;
            float worldZ = coord.y * treeTerrainDepth + normalizedZ * treeTerrainDepth;

            // ������
            TreeData treeData = new TreeData
            {
                position = new Vector3(worldX, worldHeight, worldZ),
                scale = Random.Range(0.8f, 1.5f),
                treeType = Random.Range(0, treePrefabs.Length) // ���������
            };
            treeDataList.Add(treeData);
        }

        return treeDataList;
    }

    public MapData GenerateMapData(Vector2Int coord)
    {
        maxHeightScale = GenerateRandomNumber(coord, 0.3f, 2f);
        freqeuncyScale = GenerateRandomNumber(coord, 0.5f, 1.5f);
        TerrainData terrainData = new TerrainData();
        // ���õ��δ�С�ͷֱ���
        terrainData.size = new Vector3(chunkSize * scale, terrainDepth*maxHeightScale, chunkSize * scale);
        Debug.Log("size:" + terrainData.size);
        terrainData.heightmapResolution = resolution;

        // ���ɸ߶�ͼ
        float[,] heights = GenerateHeights(coord);
        terrainData.SetHeights(0, 0, heights);

        // ��� Detail Texture
        AddDetailTexture(terrainData);

        // ��������
        AddGrassDetails(terrainData);
        // ������������
        //List<TreeData> treeDataList = GenerateTreeData(terrainData, coord);
        return new MapData
        {
            terrainData = terrainData,
            //trees = treeDataList
        };
    }

}
