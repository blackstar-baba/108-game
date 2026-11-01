using System;
using System.Collections;
using UnityEngine;

namespace Liangshan
{
    public static class SimpleTween
    {
        public static IEnumerator Run(float duration, Action<float> onUpdate, Action onComplete = null, Func<float, float> ease = null)
        {
            float time = 0f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / duration);
                onUpdate?.Invoke(ease != null ? ease(t) : t);
                yield return null;
            }
            onUpdate?.Invoke(1f);
            onComplete?.Invoke();
        }

        public static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
        public static float EaseInCubic(float t) => t * t * t;
        public static float EaseInOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
