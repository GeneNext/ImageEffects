using UnityEngine;

namespace WeatherSystem
{
    public class ShaderPropertyFloat
    {
        private int m_propertyID;
        private float m_from;
        private float m_to;

        private Material m_mat;

        public ShaderPropertyFloat(string propertyName, Material mat = null)
        {
            m_propertyID = Shader.PropertyToID(propertyName);
            m_mat = mat;
        }

        public void Init(float to)
        {
            m_from = GetFloat();
            m_to = to;
        }

        public void Update(float time)
        {
            float value = Mathf.Lerp(m_from, m_to, time);
            SetFloat(value);
        }

        public float GetFloat()
        {
            float data = 0;
            if (m_mat == null)
            {
                data = Shader.GetGlobalFloat(m_propertyID);
            }
            else
            {
                data = m_mat.GetFloat(m_propertyID);
            }
            return data;
        }

        public void SetFloat(float data)
        {
            if (m_mat == null)
            {
                Shader.SetGlobalFloat(m_propertyID, data);
            }
            else
            {
                m_mat.SetFloat(m_propertyID, data);
            }
        }
    }

    public class ShaderPropertyColor
    {
        private int m_propertyID;
        private Color m_from;
        private Color m_to;

        private Material m_mat;

        public ShaderPropertyColor(string propertyName, Material mat = null)
        {
            m_propertyID = Shader.PropertyToID(propertyName);
            m_mat = mat;
        }

        public void Init(Color to)
        {
            m_from = GetColor();
            m_to = to;
        }

        public void Update(float time)
        {
            Color color = Color.Lerp(m_from, m_to, time);
            SetColor(color);
        }

        public Color GetColor()
        {
            Color color;
            if (m_mat == null)
            {
                color = Shader.GetGlobalColor(m_propertyID);
            }
            else
            {
                color = m_mat.GetColor(m_propertyID);
            }
            return color;
        }

        public void SetColor(Color color)
        {
            if (m_mat == null)
            {
                Shader.SetGlobalColor(m_propertyID, color);
            }
            else
            {
                m_mat.SetColor(m_propertyID, color);
            }
        }
    }
}