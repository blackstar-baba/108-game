using UnityEngine;

namespace Liangshan
{
    public sealed class BirdFlock : MonoBehaviour
    {
        struct Bird
        {
            public Transform Transform;
            public float Angle;
            public float Radius;
            public float Height;
            public float Speed;
            public float Phase;
        }

        Bird[] _birds;

        void Start()
        {
            var builder = new MeshBuilder(true);
            Color ink = new Color(0.09f, 0.10f, 0.11f, 1f);
            builder.FlatTriangle(new Vector3(-0.55f, 0f, 0f), new Vector3(0f, 0.12f, -0.12f), new Vector3(0f, 0f, 0.06f), ink);
            builder.FlatTriangle(new Vector3(0.55f, 0f, 0f), new Vector3(0f, 0.12f, -0.12f), new Vector3(0f, 0f, 0.06f), ink);
            Mesh mesh = builder.Build("bird");
            Material material = RuntimeAssets.Toon(ink, ink * 0.7f, 0f, false, true);

            int count = 7;
            _birds = new Bird[count];
            for (int i = 0; i < count; i++)
            {
                var go = WorldFactory.MeshObject("bird", transform, mesh, material, Vector3.zero, null, Vector3.one * (0.65f + i * 0.04f), false, false);
                _birds[i] = new Bird
                {
                    Transform = go.transform,
                    Angle = i * 0.35f,
                    Radius = 34f + i * 3f,
                    Height = 20f + i * 1.8f,
                    Speed = 0.035f + i * 0.002f,
                    Phase = i * 1.3f
                };
            }
        }

        void Update()
        {
            if (_birds == null) return;
            float time = Time.time;
            for (int i = 0; i < _birds.Length; i++)
            {
                Bird bird = _birds[i];
                float angle = bird.Angle + time * bird.Speed;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * bird.Radius, bird.Height + Mathf.Sin(time * 0.3f + bird.Phase) * 1.2f, Mathf.Sin(angle) * bird.Radius);
                Vector3 next = new Vector3(Mathf.Cos(angle + 0.01f) * bird.Radius, pos.y, Mathf.Sin(angle + 0.01f) * bird.Radius);
                bird.Transform.position = pos;
                bird.Transform.rotation = Quaternion.LookRotation(next - pos, Vector3.up);
                bird.Transform.Rotate(0f, 0f, Mathf.Sin(time * 3.5f + bird.Phase) * 20f, Space.Self);
            }
        }
    }
}
