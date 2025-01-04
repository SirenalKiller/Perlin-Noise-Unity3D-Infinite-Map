using UnityEngine;

public class ToggleDayNight : MonoBehaviour
{
    public Material daySkybox; // ����� Skybox ����
    public Material nightSkybox; // ҹ��� Skybox ����
    public Light playerLight; // Player �� Light ���
    public Light sunLight;
    public Light moonLight;
    public MonoBehaviour particleFollowPlayerScript; 
    private bool isDay = true; 

    void Update()
    {
        // Q���л�
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleSkyboxAndLight();
        }
    }

    void ToggleSkyboxAndLight()
    {
        if (isDay)
        {
            // �л���ҹ��
            RenderSettings.skybox = nightSkybox;
            if (playerLight != null) playerLight.enabled = true; // ���� Player �Ĺ�Դ
            if (sunLight != null) sunLight.enabled = false;
            if (moonLight != null) moonLight.enabled = true;
            particleFollowPlayerScript.enabled = true; 
        }
        else
        {
            // �л�������
            RenderSettings.skybox = daySkybox;
            if (playerLight != null) playerLight.enabled = false; // ���� Player �Ĺ�Դ
            if (sunLight != null) sunLight.enabled = true;
            if (moonLight != null) moonLight.enabled = false;
            particleFollowPlayerScript.enabled = false; 
        }

        // ˢ�� Skybox ����
        DynamicGI.UpdateEnvironment();

        isDay = !isDay;
    }
}
