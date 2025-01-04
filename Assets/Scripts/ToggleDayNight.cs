using UnityEngine;

public class ToggleDayNight : MonoBehaviour
{
    public Material daySkybox; // 白天的 Skybox 材质
    public Material nightSkybox; // 夜晚的 Skybox 材质
    public Light playerLight; // Player 的 Light 组件
    public Light sunLight;
    public Light moonLight;
    public MonoBehaviour particleFollowPlayerScript; 
    private bool isDay = true; 

    void Update()
    {
        // Q键切换
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleSkyboxAndLight();
        }
    }

    void ToggleSkyboxAndLight()
    {
        if (isDay)
        {
            // 切换到夜晚
            RenderSettings.skybox = nightSkybox;
            if (playerLight != null) playerLight.enabled = true; // 启用 Player 的光源
            if (sunLight != null) sunLight.enabled = false;
            if (moonLight != null) moonLight.enabled = true;
            particleFollowPlayerScript.enabled = true; 
        }
        else
        {
            // 切换到白天
            RenderSettings.skybox = daySkybox;
            if (playerLight != null) playerLight.enabled = false; // 禁用 Player 的光源
            if (sunLight != null) sunLight.enabled = true;
            if (moonLight != null) moonLight.enabled = false;
            particleFollowPlayerScript.enabled = false; 
        }

        // 刷新 Skybox 更改
        DynamicGI.UpdateEnvironment();

        isDay = !isDay;
    }
}
