using System.Collections.Generic;
using UnityEngine;

namespace Liangshan
{
    /// <summary>Per-renderer emission flash without breaking shared materials.</summary>
    public sealed class HitFlash : MonoBehaviour
    {
        readonly List<Renderer> _renderers = new List<Renderer>();
        MaterialPropertyBlock _block;
        float _timer;
        float _duration;

        public void Refresh()
        {
            _renderers.Clear();
            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                if (r is MeshRenderer || r is SkinnedMeshRenderer)
                    _renderers.Add(r);
            }
            _block = new MaterialPropertyBlock();
        }

        public void Flash(float duration = 0.11f)
        {
            _duration = Mathf.Max(0.03f, duration);
            _timer = _duration;
        }

        void Update()
        {
            if (_timer <= 0f) return;
            _timer -= Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(_timer / _duration);
            Color emission = new Color(0.85f * k, 0.72f * k, 0.55f * k, 1f);
            for (int i = 0; i < _renderers.Count; i++)
            {
                var r = _renderers[i];
                if (r == null) continue;
                r.GetPropertyBlock(_block);
                _block.SetColor("_Emission", emission);
                r.SetPropertyBlock(_block);
            }
            if (_timer <= 0f)
                Clear();
        }

        void Clear()
        {
            for (int i = 0; i < _renderers.Count; i++)
            {
                var r = _renderers[i];
                if (r == null) continue;
                r.GetPropertyBlock(_block);
                _block.SetColor("_Emission", Color.black);
                r.SetPropertyBlock(_block);
            }
        }
    }
}
