using UnityEngine;

namespace Liangshan
{
    /// <summary>Perlin-noise camera shake, consumed by CameraDirector.</summary>
    public sealed class CameraShake
    {
        float _trauma;
        float _seed;

        public float Amplitude = 0.32f;
        public float Frequency = 18f;

        public float Trauma => _trauma;

        public CameraShake()
        {
            _seed = 37.19f;
        }

        public void Add(float amount)
        {
            _trauma = Mathf.Clamp01(_trauma + amount);
        }

        public void Update(float unscaledDeltaTime)
        {
            _trauma = Mathf.Max(0f, _trauma - unscaledDeltaTime * 1.7f);
        }

        public Vector3 Offset
        {
            get
            {
                if (_trauma <= 0.001f) return Vector3.zero;
                float strength = _trauma * _trauma * Amplitude;
                float t = Time.unscaledTime * Frequency;
                return new Vector3(
                    (Mathf.PerlinNoise(_seed, t) - 0.5f) * 2f,
                    (Mathf.PerlinNoise(_seed + 4.2f, t) - 0.5f) * 2f,
                    (Mathf.PerlinNoise(_seed + 8.7f, t) - 0.5f) * 2f) * strength;
            }
        }

        public Quaternion RotationOffset
        {
            get
            {
                if (_trauma <= 0.001f) return Quaternion.identity;
                float strength = _trauma * _trauma * Amplitude * 18f;
                float t = Time.unscaledTime * Frequency;
                return Quaternion.Euler(
                    (Mathf.PerlinNoise(_seed + 2f, t) - 0.5f) * 2f * strength,
                    (Mathf.PerlinNoise(_seed + 6f, t) - 0.5f) * 2f * strength,
                    (Mathf.PerlinNoise(_seed + 10f, t) - 0.5f) * 2f * strength);
            }
        }
    }
}
