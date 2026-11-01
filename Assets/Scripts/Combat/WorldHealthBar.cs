using UnityEngine;

namespace Liangshan
{
    /// <summary>Small billboarded health strip that keeps crowds readable at a glance.</summary>
    public sealed class WorldHealthBar : MonoBehaviour
    {
        Combatant _combat;
        Transform _fill;
        Renderer _fillRenderer;
        Renderer _backRenderer;
        float _width = 1.22f;
        float _height = 0.095f;

        public static WorldHealthBar Attach(Transform parent, Combatant combat, Color tint)
        {
            var go = new GameObject("Health Bar");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 2.10f, 0f);
            var bar = go.AddComponent<WorldHealthBar>();
            bar.Build(combat, tint);
            return bar;
        }

        void Build(Combatant combat, Color tint)
        {
            _combat = combat;
            var back = WorldFactory.Primitive("back", transform, PrimitiveType.Cube, Vector3.zero,
                new Vector3(_width + 0.06f, _height + 0.035f, 0.018f), null,
                RuntimeAssets.Unlit(new Color(0.035f, 0.03f, 0.026f, 0.9f)), false, false);
            _backRenderer = back.GetComponent<Renderer>();
            _backRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _backRenderer.receiveShadows = false;

            var fill = WorldFactory.Primitive("fill", transform, PrimitiveType.Cube, new Vector3(0f, 0f, -0.02f),
                new Vector3(_width, _height, 0.02f), null,
                RuntimeAssets.Unlit(Color.Lerp(tint, GameConfig.Gold, 0.55f)), false, false);
            _fill = fill.transform;
            _fillRenderer = fill.GetComponent<Renderer>();
            _fillRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _fillRenderer.receiveShadows = false;
        }

        void LateUpdate()
        {
            if (_combat == null || _combat.IsDead)
            {
                if (gameObject.activeSelf) gameObject.SetActive(false);
                return;
            }
            var cam = Game.Instance != null ? Game.Instance.GameCamera : Camera.main;
            if (cam != null) transform.rotation = cam.transform.rotation;
            float f = Mathf.Clamp01(_combat.Health01);
            Vector3 scale = _fill.localScale;
            scale.x = _width * Mathf.Max(0.001f, f);
            _fill.localScale = scale;
            _fill.localPosition = new Vector3(-_width * (1f - f) * 0.5f, 0f, -0.02f);
            if (_fillRenderer != null && _backRenderer != null)
            {
                bool show = Game.Instance != null && Game.Instance.State == GameState.Playing;
                _fillRenderer.enabled = show;
                _backRenderer.enabled = show;
            }
        }
    }
}
