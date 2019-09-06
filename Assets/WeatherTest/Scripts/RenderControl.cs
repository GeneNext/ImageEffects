using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WeatherSystem;

public class RenderControl : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Button m_shadowButton = null;
    [SerializeField]
    private UnityEngine.UI.Button m_modelButton = null;
    [SerializeField]
    private UnityEngine.UI.Button m_bloomButton = null;
    [SerializeField]
    private UnityEngine.UI.Button m_bokehButton = null;
    [SerializeField]
    private UnityEngine.UI.Button m_reflectionButton = null;
    [SerializeField]
    private UnityEngine.UI.Button m_rainButton = null;
    [SerializeField]
    private UnityEngine.UI.Button m_snowButton = null;
    [SerializeField]
    private UnityEngine.UI.Button m_sunnyButton = null;
    [SerializeField]
    private UnityEngine.UI.Button m_thunderButton = null;
    [SerializeField]
    private UnityEngine.UI.Button m_weatherEffectButton = null;
    [SerializeField]
    private UnityEngine.UI.Button m_depthControlButton = null;

    [SerializeField]
    private Light m_mainLight = null;
    [SerializeField]
    private GameObject m_modelRoot = null;
    [SerializeField]
    private BloomEffect2 m_bloomEffect = null;
    [SerializeField]
    private BokehEffect m_bokehEffect = null;
    [SerializeField]
    private WeatherEffect m_weatherEffect = null;
    [SerializeField]
    private Reflection m_reflection = null;
    [SerializeField]
    private CameraDepthControl m_depthControl = null;

    void Start()
    {
        m_shadowButton.onClick.AddListener(ClickShadowButton);
        m_modelButton.onClick.AddListener(ClickModelButton);
        m_bloomButton.onClick.AddListener(ClickBloomButton);
        m_bokehButton.onClick.AddListener(ClickBokehButton);
        m_reflectionButton.onClick.AddListener(ClickReflectionButton);
        m_rainButton.onClick.AddListener(ClickRainButton);
        m_snowButton.onClick.AddListener(ClickSnowButton);
        m_sunnyButton.onClick.AddListener(ClickSunnyButton);
        m_thunderButton.onClick.AddListener(ClickThunderButton);
        m_weatherEffectButton.onClick.AddListener(ClickWeatherEffectButton);
        m_depthControlButton.onClick.AddListener(ClickDepthControlButton);

        ClickSunnyButton();
    }

    private void ClickShadowButton()
    {
        m_mainLight.shadows = m_mainLight.shadows == LightShadows.None ? LightShadows.Soft : LightShadows.None;
    }

    private void ClickModelButton()
    {
        m_modelRoot.SetActive(!m_modelRoot.activeSelf);
    }

    private void ClickBloomButton()
    {
        m_bloomEffect.enabled = !m_bloomEffect.enabled;
    }

    private void ClickBokehButton()
    {
        m_bokehEffect.enabled = !m_bokehEffect.enabled;
    }

    private void ClickReflectionButton()
    {
        m_reflection.enabled = !m_reflection.enabled;
    }

    private void ClickRainButton()
    {
        WeatherManager.Control.PlayWeather("Rain");
    }

    private void ClickSnowButton()
    {
        WeatherManager.Control.PlayWeather("Snow");
    }

    private void ClickSunnyButton()
    {
        WeatherManager.Control.PlayWeather("Sunny");
    }

    private void ClickThunderButton()
    {
        WeatherManager.Control.PlayThunder();
    }

    private void ClickWeatherEffectButton()
    {
        m_weatherEffect.enabled = !m_weatherEffect.enabled;
    }

    private void ClickDepthControlButton()
    {
        m_depthControl.SetDepthMode(m_depthControl.DepthMode == DepthTextureMode.None ? DepthTextureMode.Depth : DepthTextureMode.None);
    }
}
