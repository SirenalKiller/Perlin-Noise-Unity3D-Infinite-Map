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
    // 全局资源
    public static TerrainLayer[] detailTextureLayers;
    public static Texture2D[] grassTextures;
    public static GameObject[] treePrefabs;
    public int chunkSize;
    public float terrainDepth;
    public int resolution;
    public float scale = 0.08f;
    private float freqeuncyScale;
    private float maxHeightScale;
    // TerrainData 缓存字典
    // private static Dictionary<Vector2Int, TerrainData> terrainDataCache = new Dictionary<Vector2Int, TerrainData>();
    public float[] frequencyScales = { 300f, 80f, 2f };
    public float[] maxHeights = { 300f, 150f, 5f };
    //public TerrainLayer[] detailTextureLayers; // 在 Inspector 中拖入一个 TerrainLayer
    //public Texture2D[] grassTextures;         // 草的贴图
    //public GameObject[] treePrefabs; // 树的预制体数组，支持多种树
    public int treeDensity = 30;    // 树的密度，决定生成的树数量
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

        //// 如果缓存中已存在该 coord 对应的 TerrainData，则直接使用
        //if (terrainDataCache.TryGetValue(coord, out terrainData))
        //{
        //    Debug.Log($"Using cached TerrainData for chunk {coord}");
        //}
        //else
        //{
        //    Debug.Log($"Creating new TerrainData for chunk {coord}");
        //    // 创建新的 TerrainData
        //    terrainData = new TerrainData();
        //    freqeuncyScale = GenerateRandomNumber(coord, 0.5f, 5f);
        //    maxHeightScale = GenerateRandomNumber(coord, 0.3f, 2f);

        //    // 设置地形大小，确保与 chunkSize 匹配
        //    terrainData.size = new Vector3(chunkSize * scale, terrainDepth * maxHeightScale, chunkSize * scale);
        //    terrainData.heightmapResolution = resolution;

        //    // 生成高度图
        //    float[,] heights = GenerateHeights(coord);


        //    // 应用高度数据
        //    terrainData.SetHeights(0, 0, heights);

        //    // 添加 Detail Texture
        //    AddDetailTexture(terrainData);

        //    // 添加立体草
        //    AddGrassDetails(terrainData);

        //    // 缓存生成的 TerrainData
        //    terrainDataCache[coord] = terrainData;
        //    // 调用 AddTrees 方法
        //    AddTrees(terrainData, transform.position);
        //}

        //// 应用到地形和地形碰撞器
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
        int bufferWidth = Mathf.CeilToInt(0.3f * RemapMaxHeightScale() * (resolution - 1)); // 缓冲区宽度，占Chunk边缘的40%

        for (int x = 0; x < resolution; x++)
        {
            for (int z = 0; z < resolution; z++)
            {
                // 计算绝对坐标
                float worldX = (coord.x + (float)x / (resolution - 1)) * chunkSize+GenerateRandomNumber(coord, 0, 100);
                float worldZ = (coord.y + (float)z / (resolution - 1)) * chunkSize+GenerateRandomNumber(coord, 0, 100);

                float finalHeight = 0f;

                // 多层噪声叠加
                for (int i = 0; i < frequencyScales.Length; i++)
                {
                    float frequency = frequencyScales[i]* freqeuncyScale;
                    float noiseValue = Mathf.PerlinNoise(worldX / frequency, worldZ / frequency);
                    float mappedHeight = noiseValue * maxHeights[i] * maxHeightScale;
                    finalHeight += mappedHeight;
                }

                // 边缘缓和弯折逻辑
                float edgeFactor = 1f;

                // 下边界
                if (z < bufferWidth)
                {
                    float t = (float)z / bufferWidth; // 插值比例 [0, 1]
                    edgeFactor = Mathf.SmoothStep(0.0f, 1.0f, t); 
                }

                // 上边界
                if (z >= resolution - bufferWidth)
                {
                    float t = (float)(resolution - 1 - z) / bufferWidth; // 插值比例 [0, 1]
                    edgeFactor = Mathf.SmoothStep(0.0f, 1.0f, t); 
                }

                // 左边界
                if (x < bufferWidth)
                {
                    float t = (float)x / bufferWidth; // 插值比例 [0, 1]
                    edgeFactor = Mathf.Min(edgeFactor, Mathf.SmoothStep(0.0f, 1.0f, t));
                }

                // 右边界
                if (x >= resolution - bufferWidth)
                {
                    float t = (float)(resolution - 1 - x) / bufferWidth; // 插值比例 [0, 1]
                    edgeFactor = Mathf.Min(edgeFactor, Mathf.SmoothStep(0.0f, 1.0f, t));
                }

                // 应用边缘弯折并叠加高频噪声纹理
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

    //    // 获取地形的宽度和深度
    //    float terrainWidth = terrainData.size.x;
    //    float terrainDepth = terrainData.size.z;

    //    // 随机生成树的位置
    //    for (int i = 0; i < treeDensity; i++)
    //    {
    //        // 随机生成树的坐标
    //        float normalizedX = Random.Range(0f, 1f); // 归一化的 X 坐标
    //        float normalizedZ = Random.Range(0f, 1f); // 归一化的 Z 坐标

    //        // 获取该点的世界高度
    //        float worldHeight = terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);

    //        // 判断是否高于 mountainHeight
    //        if (worldHeight > mountainHeight && worldHeight< landHeight) continue;

    //        // 转换为世界坐标
    //        float worldX = chunkPosition.x + normalizedX * terrainWidth;
    //        float worldZ = chunkPosition.z + normalizedZ * terrainDepth;
    //        float worldY = worldHeight;

    //        // 随机选择一种树的预制体
    //        GameObject selectedTreePrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];

    //        // 实例化树
    //        GameObject tree = Instantiate(selectedTreePrefab);
    //        tree.transform.position = new Vector3(worldX, worldY, worldZ);
    //        tree.transform.localScale *= Random.Range(0.5f, 1.5f); // 随机调整树的大小
    //        tree.transform.parent = this.transform; // 将树设置为当前 Chunk 的子对象
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

        // 定义草的细节原型
        for (int i = 0; i < grassTextures.Length; i++)
        {
            grassDetails[i] = new DetailPrototype
            {
                prototypeTexture = grassTextures[i],         // 草的贴图
                renderMode = DetailRenderMode.Grass, // 面片草
                minWidth = 19f,                              // 草的最小宽度
                maxWidth = 20f,                              // 草的最大宽度
                minHeight = 19f,                             // 草的最小高度
                maxHeight = 20f,                             // 草的最大高度
                noiseSpread = 0.8f,                          // 草的随机分布
                healthyColor = Color.white,                  // 健康状态颜色
                dryColor = Color.white,                      // 干燥状态颜色
            };
        }
        terrainData.detailPrototypes = grassDetails;

        // 生成草的地图
        terrainData.SetDetailResolution(detailResolution, 64);

        // 为每种草生成地图
        for (int layer = 0; layer < grassTextures.Length; layer++)
        {
            int[,] grassMap = new int[detailResolution, detailResolution];

            for (int x = 0; x < detailResolution; x++)
            {
                for (int z = 0; z < detailResolution; z++)
                {
                    // 计算世界高度
                    float normalizedX = (float)x / (detailResolution - 1);
                    float normalizedZ = (float)z / (detailResolution - 1);
                    float worldHeight = terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);

                    // 根据海拔调整草的密度
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

        // 生成Alpha Map
        int alphaMapResolution = terrainData.alphamapResolution;
        float[,,] alphaMap = new float[alphaMapResolution, alphaMapResolution, 3]; // 3 层

        for (int x = 0; x < alphaMapResolution; x++)
        {
            for (int z = 0; z < alphaMapResolution; z++)
            {
                // 计算当前高度世界坐标
                float normalizedX = (float)x / (alphaMapResolution - 1);
                float normalizedZ = (float)z / (alphaMapResolution - 1);
                float height = terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);

                float snowStartHeight = mountainHeight; // 草地向雪顶过渡开始的高度
                float snowEndHeight = mountainHeight + mountainDelta; // 完全雪覆盖的高度
                float rockStartHeight = mountainHeight + mountainDelta; // 雪顶向岩石过渡开始的高度
                float rockEndHeight = mountainHeight + 1.5f * mountainDelta; // 完全岩石覆盖的高度

                // 计算权重
                float snowWeight = Mathf.Clamp01(Mathf.InverseLerp(snowStartHeight, snowEndHeight, height));
                float grassWeight = Mathf.Clamp01(1f - snowWeight);
                float rockWeight = Mathf.Clamp01(Mathf.InverseLerp(rockStartHeight, rockEndHeight, height));

                // 防止多个材质叠加过多
                if (height > snowEndHeight && height < rockStartHeight)
                {
                    rockWeight = 0f; // 在雪顶与岩石之间清除岩石权重
                }

                // 分配权重到Alpha Map
                alphaMap[x, z, 0] = grassWeight; // 草地
                alphaMap[x, z, 1] = snowWeight; // 雪顶
                alphaMap[x, z, 2] = rockWeight; // 岩石
            }
        }

        // 应用 Alpha Map
        terrainData.SetAlphamaps(0, 0, alphaMap);
    }

    List<TreeData> GenerateTreeData(TerrainData terrainData, Vector2Int coord)
    {
        List<TreeData> treeDataList = new List<TreeData>();

        // 地形尺寸
        float treeTerrainWidth = terrainData.size.x;
        float treeTerrainDepth = terrainData.size.z;

        for (int i = 0; i < treeDensity; i++)
        {
            // 随机位置
            float normalizedX = Random.Range(0f, 1f);
            float normalizedZ = Random.Range(0f, 1f);

            // 获取高度
            float worldHeight = terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);

            // 高度限制
            if (worldHeight > mountainHeight || worldHeight < landHeight)
            {
                continue;
            }

            // 转换为世界坐标
            float worldX = coord.x * treeTerrainWidth + normalizedX * treeTerrainWidth;
            float worldZ = coord.y * treeTerrainDepth + normalizedZ * treeTerrainDepth;

            // 树数据
            TreeData treeData = new TreeData
            {
                position = new Vector3(worldX, worldHeight, worldZ),
                scale = Random.Range(0.8f, 1.5f),
                treeType = Random.Range(0, treePrefabs.Length) // 随机树类型
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
        // 设置地形大小和分辨率
        terrainData.size = new Vector3(chunkSize * scale, terrainDepth*maxHeightScale, chunkSize * scale);
        Debug.Log("size:" + terrainData.size);
        terrainData.heightmapResolution = resolution;

        // 生成高度图
        float[,] heights = GenerateHeights(coord);
        terrainData.SetHeights(0, 0, heights);

        // 添加 Detail Texture
        AddDetailTexture(terrainData);

        // 添加立体草
        AddGrassDetails(terrainData);
        // 生成树的数据
        //List<TreeData> treeDataList = GenerateTreeData(terrainData, coord);
        return new MapData
        {
            terrainData = terrainData,
            //trees = treeDataList
        };
    }

}
