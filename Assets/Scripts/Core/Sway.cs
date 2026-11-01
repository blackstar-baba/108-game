using UnityEngine;

namespace Liangshan
{
    public sealed class Sway : MonoBehaviour
    {
        public float amplitude = 2.2f;
        public float speed = 0.55f;
        public float phase;
        public bool swayX = true;
        public bool swayZ = true;
        public float scale = 1f;

        Quaternion _baseRotation;

        void Awake()
        {
            _baseRotation = transform.localRotation;
            if (phase == 0f)
                phase = Random.Range(0f, 100f);
        }

        void Update()
        {
            float t = Time.time * speed + phase;
            float x = swayX ? (Mathf.PerlinNoise(t, 0.31f) - 0.5f) * 2f * amplitude : 0f;
            float z = swayZ ? (Mathf.PerlinNoise(0.73f, t) - 0.5f) * 2f * amplitude * 0.7f : 0f;
            transform.localRotation = _baseRotation * Quaternion.Euler(x * scale, 0f, z * scale);
        }
    }
}
