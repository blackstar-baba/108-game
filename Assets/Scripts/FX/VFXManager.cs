using System.Collections.Generic;
using UnityEngine;

namespace Liangshan
{
    public sealed class VFXManager : MonoBehaviour
    {
        Transform _root;
        Material _inkMat;
        Material _sparkMat;
        Material _decalMat;
        Material _meshMat;
        ParticleSystem _snow;
        Texture2D _white;

        public void Initialize(Transform parent)
        {
            _root = WorldFactory.Empty("Generated FX", parent, Vector3.zero).transform;
            _white = Texture2D.whiteTexture;

            _inkMat = new Material(RuntimeAssets.ParticleShader) { name = "FX Ink" };
            _inkMat.SetTexture("_MainTex", RuntimeAssets.SoftCircle);
            _inkMat.SetColor("_Color", Color.white);
            _inkMat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _inkMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _inkMat.SetFloat("_ZWrite", 0f);

            _sparkMat = new Material(RuntimeAssets.ParticleShader) { name = "FX Spark" };
            _sparkMat.SetTexture("_MainTex", RuntimeAssets.GlowTexture);
            _sparkMat.SetColor("_Color", Color.white);
            _sparkMat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _sparkMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            _sparkMat.SetFloat("_ZWrite", 0f);

            _decalMat = new Material(RuntimeAssets.ParticleShader) { name = "FX Decal" };
            _decalMat.SetTexture("_MainTex", RuntimeAssets.InkSplatB);
            _decalMat.SetColor("_Color", Color.white);
            _decalMat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _decalMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _decalMat.SetFloat("_ZWrite", 0f);

            _meshMat = new Material(RuntimeAssets.ParticleShader) { name = "FX Mesh" };
            _meshMat.SetTexture("_MainTex", _white);
            _meshMat.SetColor("_Color", Color.white);
            _meshMat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _meshMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            _meshMat.SetFloat("_ZWrite", 0f);

            BuildSnow();
        }

        public void Clear()
        {
            if (_root == null) return;
            for (int i = _root.childCount - 1; i >= 0; i--)
            {
                var child = _root.GetChild(i).gameObject;
                if (child.name == "Snow Field") continue;
                Destroy(child);
            }
        }

        public void SetSnow(bool enabled)
        {
            if (_snow == null) return;
            if (enabled) _snow.Play(); else _snow.Stop();
        }

        void BuildSnow()
        {
            var go = new GameObject("Snow Field");
            go.transform.SetParent(_root, false);
            _snow = go.AddComponent<ParticleSystem>();
            var main = _snow.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.06f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.9f, 0.94f, 1f, 0.55f), new Color(0.7f, 0.8f, 0.9f, 0.25f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 700;
            main.gravityModifier = 0.015f;
            var emission = _snow.emission;
            emission.rateOverTime = 55f;
            var shape = _snow.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(32f, 0.5f, 32f);
            shape.position = new Vector3(0f, 14f, 0f);
            var velocity = _snow.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.8f, 0.4f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.8f, -0.3f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = _inkMat;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            var follow = go.AddComponent<SnowFollower>();
            follow.CameraTransform = Camera.main != null ? Camera.main.transform : null;
        }

        public void InkBurst(Vector3 position, Color color, float size)
        {
            Burst(position + Vector3.up * 0.2f, color, Mathf.RoundToInt(14f + size * 7f), 2.5f * size, 0.25f * size, 0.7f, false, 0.5f);
            Burst(position + Vector3.up * 0.55f, color * 1.2f, 8, 3.5f * size, 0.12f * size, 0.5f, true, 0.2f);
            if (size > 0.9f)
            {
                var go = WorldFactory.Primitive("ink stain", _root, PrimitiveType.Quad, position + Vector3.up * 0.09f,
                    new Vector3(size * 2.6f, size * 2.6f, 1f), Quaternion.Euler(90f, Random.Range(0f, 360f), 0f), null);
                var renderer = go.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = new Material(_decalMat);
                renderer.sharedMaterial.SetColor("_Color", new Color(color.r * 0.5f, color.g * 0.45f, color.b * 0.4f, 0.55f));
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                var fade = go.AddComponent<FadeAndDestroy>();
                fade.Setup(8f, 0.55f, false);
            }
        }

        public void Dust(Vector3 position, Vector3 direction, Color color)
        {
            Burst(position + Vector3.up * 0.12f, color, 12, 1.1f, 0.35f, 0.7f, false, -0.04f);
        }

        public void Impact(Vector3 position, Color color, float strength)
        {
            Burst(position, color, Mathf.RoundToInt(12f + strength * 10f), 4.5f * strength, 0.13f * strength, 0.42f, true, 0.3f);
            Burst(position, Color.white, 6, 3.2f, 0.09f, 0.32f, true, 0f);
        }

        public void Slash(Vector3 position, Quaternion rotation, Color color, float scale, float arc, float range)
        {
            float radius = Mathf.Clamp(range * 0.62f, 1.2f, 5.2f);
            var mesh = BuildArc(radius * 0.55f, radius, arc, 18);
            var go = WorldFactory.MeshObject("slash", _root, mesh, new Material(_meshMat), position, rotation, Vector3.one * scale);
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial.SetColor("_Color", color * 1.25f);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var fade = go.AddComponent<FadeAndDestroy>();
            fade.Setup(0.24f, 0.9f, true);
            Burst(position, color, 9, 2.4f * scale, 0.16f, 0.33f, true, 0f);
        }

        public void Ring(Vector3 center, float innerRadius, float outerRadius, Color color, float duration, float height = 0.05f)
        {
            var builder = new MeshBuilder(false);
            builder.Ring(Vector3.zero, innerRadius, outerRadius, 48, Color.white, Color.white, 0f);
            var go = WorldFactory.MeshObject("ring", _root, builder.Build("ring"), new Material(_meshMat),
                center + Vector3.up * height, null, Vector3.one);
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial.SetColor("_Color", color);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var fade = go.AddComponent<FadeAndDestroy>();
            fade.Setup(duration, 1.4f, true);
        }

        public void Shockwave(Vector3 center, Color color, float radius)
        {
            Ring(center, radius * 0.78f, radius, color, 0.55f, 0.12f);
            Ring(center, radius * 0.42f, radius * 0.55f, color * 0.8f, 0.4f, 0.18f);
            Burst(center + Vector3.up * 0.15f, color, 30, 5.5f, 0.38f, 0.8f, false, 0.1f);
        }

        public void Telegraph(Vector3 center, float radius, float duration, Color color)
        {
            var builder = new MeshBuilder(false);
            builder.Ring(Vector3.zero, Mathf.Max(0.2f, radius - 0.22f), radius, 48, Color.white, Color.white, 0f);
            var go = WorldFactory.MeshObject("telegraph", _root, builder.Build("telegraph"), new Material(_decalMat),
                center + Vector3.up * 0.08f, null, Vector3.one);
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial.SetTexture("_MainTex", RuntimeAssets.White);
            renderer.sharedMaterial.SetColor("_Color", new Color(color.r, color.g, color.b, 0.35f));
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var fade = go.AddComponent<FadeAndDestroy>();
            fade.Setup(duration, 0.8f, true);
            fade.Pulse = true;
            Burst(center + Vector3.up * 0.2f, color, 8, 0.8f, 0.3f, Mathf.Min(duration, 0.5f), false, 0f);
        }

        public void SnowBurst(Vector3 center, float radius)
        {
            Burst(center + Vector3.up * 1.2f, new Color(0.9f, 0.96f, 1f, 0.9f), 55, 5.5f, 0.12f, 1.3f, true, 0.2f);
            Burst(center + Vector3.up * 0.8f, new Color(0.8f, 0.9f, 1f, 0.6f), 30, 3.2f, 0.3f, 1.6f, false, 0.05f);
        }

        public void Kick(float strength)
        {
            if (Game.Instance != null)
            {
                Game.Instance.CameraRig.Shake(strength);
                Game.Instance.Hud?.FlashInk(strength * 0.7f);
            }
        }

        void Burst(Vector3 position, Color color, int count, float speed, float size, float lifetime, bool additive, float gravity)
        {
            var go = new GameObject("burst");
            go.transform.SetParent(_root, false);
            go.transform.position = position;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false;
            main.duration = 0.12f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.55f, lifetime);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.35f, speed);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.45f, size * 1.25f);
            main.startColor = color;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = gravity;
            main.maxParticles = Mathf.Max(8, count);
            main.playOnAwake = false;
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.Max(1, count)) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = Mathf.Max(0.05f, size * 1.4f);
            var colorOver = ps.colorOverLifetime;
            colorOver.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.7f, 0.55f), new GradientAlphaKey(0f, 1f) });
            colorOver.color = gradient;
            var sizeOver = ps.sizeOverLifetime;
            sizeOver.enabled = true;
            sizeOver.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f));
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = additive ? _sparkMat : _inkMat;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 3;
            ps.Play();
            var timed = go.AddComponent<TimedDestroy>();
            timed.lifetime = lifetime + 0.4f;
        }

        public void DamageNumber(Vector3 position, float amount, bool highlight)
        {
            var go = new GameObject("damage");
            go.transform.SetParent(_root, false);
            go.transform.position = position + new Vector3(Random.Range(-0.25f, 0.25f), 0.2f, Random.Range(-0.15f, 0.15f));
            var text = go.AddComponent<TextMesh>();
            text.font = RuntimeAssets.NumberFont;
            text.fontSize = 80;
            text.characterSize = 0.38f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = highlight ? GameConfig.Gold : GameConfig.Paper;
            text.text = Mathf.RoundToInt(amount).ToString();
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = text.font.material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.sortingOrder = 10;
            go.AddComponent<Billboard>();
            var fade = go.AddComponent<TextFade>();
            fade.Setup(highlight ? 1.15f : 0.9f, highlight ? 0.45f : 0.28f, highlight ? 0.5f : 0.35f);
        }

        static Mesh BuildArc(float inner, float outer, float arcDegrees, int segments)
        {
            var builder = new MeshBuilder(false);
            float half = arcDegrees * 0.5f * Mathf.Deg2Rad;
            for (int i = 0; i < segments; i++)
            {
                float a0 = Mathf.Lerp(-half, half, i / (float)segments);
                float a1 = Mathf.Lerp(-half, half, (i + 1) / (float)segments);
                Vector3 i0 = new Vector3(Mathf.Sin(a0) * inner, 0f, Mathf.Cos(a0) * inner);
                Vector3 o0 = new Vector3(Mathf.Sin(a0) * outer, 0f, Mathf.Cos(a0) * outer);
                Vector3 i1 = new Vector3(Mathf.Sin(a1) * inner, 0f, Mathf.Cos(a1) * inner);
                Vector3 o1 = new Vector3(Mathf.Sin(a1) * outer, 0f, Mathf.Cos(a1) * outer);
                builder.Quad(i0, i1, o1, o0, Color.white);
            }
            return builder.Build("arc");
        }
    }

    public sealed class FadeAndDestroy : MonoBehaviour
    {
        Renderer[] _renderers;
        Material[] _materials;
        Color[] _base;
        float _duration;
        float _elapsed;
        float _scale;
        bool _expand;
        public bool Pulse;

        public void Setup(float duration, float scaleTo, bool expand)
        {
            _duration = Mathf.Max(0.05f, duration);
            _scale = scaleTo;
            _expand = expand;
            _renderers = GetComponentsInChildren<Renderer>(true);
            _materials = new Material[_renderers.Length];
            _base = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                _materials[i] = _renderers[i].material;
                _base[i] = _materials[i].HasProperty("_Color") ? _materials[i].GetColor("_Color") : Color.white;
            }
        }

        void Update()
        {
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            float alpha = 1f - t;
            if (Pulse)
                alpha *= 0.55f + 0.45f * Mathf.Sin(Time.time * 12f);
            for (int i = 0; i < _materials.Length; i++)
            {
                if (_materials[i] == null) continue;
                Color c = _base[i];
                c.a = alpha * _base[i].a;
                _materials[i].SetColor("_Color", c);
            }
            if (_expand)
            {
                float s = Mathf.Lerp(1f, _scale, t);
                transform.localScale = Vector3.one * s;
            }
            if (t >= 1f)
                Destroy(gameObject);
        }
    }

    public sealed class TextFade : MonoBehaviour
    {
        TextMesh _text;
        float _duration;
        float _elapsed;
        float _speed;
        float _startSize;
        Color _base;
        Camera _camera;

        public void Setup(float duration, float speed, float startSize)
        {
            _duration = duration;
            _speed = speed;
            _startSize = startSize;
            _text = GetComponent<TextMesh>();
            if (_text != null) _base = _text.color;
            _camera = Camera.main;
        }

        void Update()
        {
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            transform.position += Vector3.up * _speed * Time.deltaTime;
            if (_text != null)
            {
                Color c = _base;
                c.a = 1f - t * t;
                _text.color = c;
                _text.characterSize = Mathf.Lerp(_startSize, _startSize * 0.75f, t);
            }
            if (_camera != null)
            {
                Vector3 look = transform.position - _camera.transform.position;
                if (look.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(look, Vector3.up);
            }
            if (t >= 1f) Destroy(gameObject);
        }
    }

    public sealed class SnowFollower : MonoBehaviour
    {
        public Transform CameraTransform;
        public float Height = 13f;

        void LateUpdate()
        {
            if (CameraTransform == null) return;
            Vector3 p = CameraTransform.position;
            transform.position = new Vector3(p.x, p.y + Height, p.z);
        }
    }
}
