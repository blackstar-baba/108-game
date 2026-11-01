using UnityEngine;

namespace Liangshan
{
    public sealed class FlickerLight : MonoBehaviour
    {
        public float baseIntensity = 2.2f;
        public float amplitude = 0.6f;
        public float speed = 5.5f;
        public float phase;
        Light _light;

        void Awake()
        {
            _light = GetComponent<Light>();
            if (phase == 0f) phase = Random.Range(0f, 20f);
        }

        void Update()
        {
            if (_light == null) return;
            float t = Time.time * speed + phase;
            float noise = Mathf.PerlinNoise(t, 0.4f) * 0.65f + Mathf.PerlinNoise(t * 2.7f, 0.9f) * 0.35f;
            _light.intensity = baseIntensity + (noise - 0.5f) * 2f * amplitude;
        }
    }
}
