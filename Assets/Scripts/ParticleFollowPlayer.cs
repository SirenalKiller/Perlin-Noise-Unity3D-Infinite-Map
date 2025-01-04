using UnityEngine;

public class ParticleFollowPlayer : MonoBehaviour
{
    public GameObject particlePrefab; 
    public Transform player; 
    public float lifetime = 5f; // 粒子系统生命周期
    public float generatePaulse = 3f; // 生成粒子的时间间隔
    public float offset; // 粒子相对于玩家的Y轴偏移量

    private GameObject currentParticle; // 当前实例
    private float generatetimer = 0f;

    void Start()
    {
        if (particlePrefab == null || player == null)
        {
            Debug.LogError("Particle prefab or player is not assigned!");
            return;
        }

        SpawnParticle();
    }

    void Update()
    {
        generatetimer += Time.deltaTime;

        if (generatetimer >= generatePaulse)
        {
            SpawnParticle(); // 生成新粒子
            generatetimer = 0f;
        }
    }

    void SpawnParticle()
    {
        Vector3 spawnPosition = player.position + new Vector3(0, offset, 0);
        currentParticle = Instantiate(particlePrefab, spawnPosition, Quaternion.Euler(90, 0, 0));

        // lifetime + 1 秒后自动销毁
        Destroy(currentParticle, lifetime + 1f);
    }
}