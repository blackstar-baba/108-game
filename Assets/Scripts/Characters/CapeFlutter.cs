using UnityEngine;

namespace Liangshan
{
    /// <summary>Cheap vertex flutter for a cape / robe tail. No cloth simulation, just wind and motion.</summary>
    public sealed class CapeFlutter : MonoBehaviour
    {
        public float Amplitude = 0.07f;
        public float Frequency = 2.3f;
        public float Speed;

        Mesh _mesh;
        Vector3[] _base;
        Vector3[] _verts;
        float _seed;
        Vector3 _lastWorld;

        void Awake()
        {
            var filter = GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null) { enabled = false; return; }
            _mesh = filter.mesh;
            _base = _mesh.vertices;
            _verts = new Vector3[_base.Length];
            _seed = Random.value * 10f;
            _lastWorld = transform.position;
        }

        void LateUpdate()
        {
            if (_mesh == null || _base == null) return;
            Vector3 world = transform.position;
            Vector3 velocity = (world - _lastWorld) / Mathf.Max(0.001f, Time.deltaTime);
            _lastWorld = world;
            Speed = Mathf.Lerp(Speed, velocity.magnitude, 0.18f);

            for (int i = 0; i < _base.Length; i++)
            {
                Vector3 v = _base[i];
                float hang = Mathf.Clamp01(-v.y);
                float wave = Mathf.Sin(Time.time * Frequency + _seed + v.y * 4.8f + v.x * 3.2f);
                float flap = Mathf.Sin(Time.time * 1.1f + _seed) * 0.3f;
                v.z += (wave * Amplitude + flap * Amplitude) * hang;
                v.x += Mathf.Sin(Time.time * 1.7f + _seed + v.y * 2.1f) * Amplitude * 0.35f * hang;
                v.z -= Mathf.Clamp(Speed * 0.02f, 0f, 0.20f) * hang;
                _verts[i] = v;
            }
            _mesh.vertices = _verts;
            _mesh.RecalculateNormals();
        }
    }
}
