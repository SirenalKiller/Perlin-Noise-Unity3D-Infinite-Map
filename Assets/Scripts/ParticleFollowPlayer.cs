using UnityEngine;

public class ParticleFollowPlayer : MonoBehaviour
{
    public GameObject particlePrefab; 
    public Transform player; 
    public float lifetime = 5f; // ����ϵͳ��������
    public float generatePaulse = 3f; // �������ӵ�ʱ����
    public float offset; // �����������ҵ�Y��ƫ����

    private GameObject currentParticle; // ��ǰʵ��
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
            SpawnParticle(); // ����������
            generatetimer = 0f;
        }
    }

    void SpawnParticle()
    {
        Vector3 spawnPosition = player.position + new Vector3(0, offset, 0);
        currentParticle = Instantiate(particlePrefab, spawnPosition, Quaternion.Euler(90, 0, 0));

        // lifetime + 1 ����Զ�����
        Destroy(currentParticle, lifetime + 1f);
    }
}