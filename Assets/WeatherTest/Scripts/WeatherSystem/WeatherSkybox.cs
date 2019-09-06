using UnityEngine;

namespace WeatherSystem
{
    [System.Serializable]
    public struct WeatherSkyboxData
    {
        public float SunSize;
        public Color SkyTint;
        public float Exposure;
        public Color TintColor;
        public Color AddColor;
        public float CloudFactor;
        public float CloudSpeed;

        public WeatherSkyboxData GetDataByBlendWeight(float blendWeight)
        {
            WeatherSkyboxData data = new WeatherSkyboxData();
            data.SunSize = SunSize * blendWeight;
            data.SkyTint = SkyTint * blendWeight;
            data.Exposure = Exposure * blendWeight;
            data.TintColor = TintColor * blendWeight;
            data.AddColor = AddColor * blendWeight;
            data.CloudFactor = CloudFactor * blendWeight;
            data.CloudSpeed = CloudSpeed * blendWeight;
            return data;
        }

        public static WeatherSkyboxData operator +(WeatherSkyboxData data1, WeatherSkyboxData data2)
        {
            WeatherSkyboxData data = new WeatherSkyboxData();
            data.SunSize = data1.SunSize + data2.SunSize;
            data.SkyTint = data1.SkyTint + data2.SkyTint;
            data.Exposure = data1.Exposure + data2.Exposure;
            data.TintColor = data1.TintColor + data2.TintColor;
            data.AddColor = data1.AddColor + data2.AddColor;
            data.CloudFactor = data1.CloudFactor + data2.CloudFactor;
            data.CloudSpeed = data1.CloudSpeed + data2.CloudSpeed;
            return data;
        }
    }

    public class WeatherSkybox
    {
        public ShaderPropertyFloat SunDisk;
        public ShaderPropertyFloat SunSize;
        public ShaderPropertyFloat SunSizeConvergence;
        public ShaderPropertyFloat AtmosphereThickness;
        public ShaderPropertyColor SkyTint;
        public ShaderPropertyColor Ground;
        public ShaderPropertyFloat Exposure;
        public ShaderPropertyColor TintColor;
        public ShaderPropertyColor AddColor;
        public ShaderPropertyFloat CloudFactor;
        public ShaderPropertyFloat CloudSpeed;

        private Material m_skyboxMat;

        public WeatherSkybox()
        {
            m_skyboxMat = Material.Instantiate<Material>(RenderSettings.skybox);
            RenderSettings.skybox = m_skyboxMat;
            //m_skyboxMat = RenderSettings.skybox;

            SunDisk = new ShaderPropertyFloat("_SunDisk", m_skyboxMat);
            SunSize = new ShaderPropertyFloat("_SunSize", m_skyboxMat);
            SunSizeConvergence = new ShaderPropertyFloat("_SunSizeConvergence", m_skyboxMat);
            AtmosphereThickness = new ShaderPropertyFloat("_AtmosphereThickness", m_skyboxMat);
            SkyTint = new ShaderPropertyColor("_SkyTint", m_skyboxMat);
            Ground = new ShaderPropertyColor("_GroundColor", m_skyboxMat);
            Exposure = new ShaderPropertyFloat("_Exposure", m_skyboxMat);
            TintColor = new ShaderPropertyColor("_TintColor", m_skyboxMat);
            AddColor = new ShaderPropertyColor("_AddColor", m_skyboxMat);
            CloudFactor = new ShaderPropertyFloat("_CloudFactor", m_skyboxMat);
            CloudSpeed = new ShaderPropertyFloat("_CloudSpeed", m_skyboxMat);
        }

        public void InitData(WeatherSkyboxData skyboxData)
        {
            SunSize.Init(skyboxData.SunSize);
            SkyTint.Init(skyboxData.SkyTint);
            Exposure.Init(skyboxData.Exposure);
            TintColor.Init(skyboxData.TintColor);
            AddColor.Init(skyboxData.AddColor);
            CloudFactor.Init(skyboxData.CloudFactor);
            CloudSpeed.Init(skyboxData.CloudSpeed);
        }

        public void UpdateByTime(float time)
        {
            SunSize.Update(time);
            SkyTint.Update(time);
            Exposure.Update(time);
            TintColor.Update(time);
            AddColor.Update(time);
            CloudFactor.Update(time);
            CloudSpeed.Update(time);
        }

        public void SetData(WeatherSkyboxData data)
        {
            SunSize.SetFloat(data.SunSize);
            SkyTint.SetColor(data.SkyTint);
            Exposure.SetFloat(data.Exposure);
            TintColor.SetColor(data.TintColor);
            AddColor.SetColor(data.AddColor);
            CloudFactor.SetFloat(data.CloudFactor);
            CloudSpeed.SetFloat(data.CloudSpeed);
        }
    }
}