using System.Collections.Generic;
using UnityEngine;

namespace Liangshan
{
    /// <summary>All sound is synthesized at boot: taiko, gongs, wind, plucked strings.</summary>
    public sealed class AudioDirector : MonoBehaviour
    {
        const int SampleRate = 44100;
        readonly List<AudioSource> _pool = new List<AudioSource>();
        int _cursor;
        AudioSource _ambient;
        AudioClip _drum, _drumLight, _whoosh, _hit, _hurt, _pluck, _gong, _bell, _arrow, _select, _victory, _defeat;

        public void Initialize()
        {
            for (int i = 0; i < 14; i++)
            {
                var go = new GameObject("SFX " + i);
                go.transform.SetParent(transform, false);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0.55f;
                src.rolloffMode = AudioRolloffMode.Linear;
                src.minDistance = 3f;
                src.maxDistance = 55f;
                _pool.Add(src);
            }

            _drum = MakeDrum("Taiko", 0.75f, 54f, 0.9f);
            _drumLight = MakeDrum("Taiko Light", 0.28f, 92f, 0.55f);
            _whoosh = MakeWhoosh("Whoosh", 0.30f, 0.55f);
            _hit = MakeHit("Hit", 0.20f, 120f);
            _hurt = MakeHurt("Hurt", 0.42f);
            _pluck = MakePluck("Pluck", 0.9f, 196f);
            _gong = MakeGong("Gong", 2.2f, 82f);
            _bell = MakeBell("Bell", 1.4f, 540f);
            _arrow = MakeWhoosh("Arrow", 0.18f, 0.35f);
            _select = MakePluck("Select", 0.25f, 420f);
            _victory = MakeVictory();
            _defeat = MakeDefeat();

            var ambientGo = new GameObject("Ambient");
            ambientGo.transform.SetParent(transform, false);
            _ambient = ambientGo.AddComponent<AudioSource>();
            _ambient.clip = MakeAmbient();
            _ambient.loop = true;
            _ambient.volume = 0.32f;
            _ambient.spatialBlend = 0f;
            _ambient.Play();
        }

        AudioSource Next()
        {
            var src = _pool[_cursor];
            _cursor = (_cursor + 1) % _pool.Count;
            return src;
        }

        void Play(AudioClip clip, float volume, float pitch, Vector3 position)
        {
            if (clip == null) return;
            var src = Next();
            src.Stop();
            src.clip = clip;
            src.volume = volume;
            src.pitch = pitch;
            src.transform.position = position;
            src.Play();
        }

        public void PlayFootstep(Vector3 p) => Play(_drumLight, 0.16f, Random.Range(0.85f, 1.1f), p);
        public void PlayWhoosh(Vector3 p, bool heavy) => Play(_whoosh, heavy ? 0.55f : 0.32f, heavy ? 0.7f : Random.Range(0.95f, 1.25f), p);
        public void PlayHit(Vector3 p, bool heavy) => Play(heavy ? _drum : _hit, heavy ? 0.8f : 0.5f, Random.Range(0.9f, 1.12f), p);
        public void PlayHurt(Vector3 p) => Play(_hurt, 0.7f, Random.Range(0.9f, 1.05f), p);
        public void PlayDodge(Vector3 p) => Play(_whoosh, 0.28f, 1.5f, p);
        public void PlaySkill(Vector3 p) => Play(_bell, 0.35f, 0.85f, p);
        public void PlayUltimate(Vector3 p) { Play(_gong, 0.7f, 0.8f, p); Play(_pluck, 0.4f, 0.5f, p); }
        public void PlayHeavyImpact(Vector3 p) => Play(_drum, 0.9f, 0.72f, p);
        public void PlayEnemyWindup(Vector3 p, bool boss) => Play(boss ? _drum : _drumLight, boss ? 0.45f : 0.18f, 0.8f, p);
        public void PlayEnemySwing(Vector3 p, bool boss) => Play(_whoosh, boss ? 0.5f : 0.22f, boss ? 0.7f : 1f, p);
        public void PlayBow(Vector3 p) => Play(_arrow, 0.35f, 1.2f, p);
        public void PlayArrowHit(Vector3 p) => Play(_hit, 0.35f, 1.3f, p);
        public void PlayBossPhase(Vector3 p) => Play(_gong, 0.8f, 0.65f, p);
        public void PlayClick() => Play(_select, 0.25f, 1f, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        public void PlayBoon() => Play(_bell, 0.5f, 1.15f, Vector3.zero);
        public void PlayWave() => Play(_gong, 0.55f, 0.9f, Vector3.zero);
        public void PlayVictory() => Play(_victory, 0.8f, 1f, Vector3.zero);
        public void PlayDefeat() => Play(_defeat, 0.8f, 1f, Vector3.zero);
        public void PlayPause() => Play(_select, 0.3f, 0.8f, Vector3.zero);
        public void PlaySpawn(Vector3 p) => Play(_drumLight, 0.25f, 1.4f, p);

        static AudioClip MakeDrum(string name, float duration, float freq, float noise)
        {
            int n = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[n];
            var random = new System.Random(name.GetHashCode());
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SampleRate;
                float env = Mathf.Exp(-t * (5.5f / duration));
                float body = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.9f;
                body += Mathf.Sin(2f * Mathf.PI * freq * 2.02f * t) * 0.35f * Mathf.Exp(-t * 12f);
                float nz = ((float)random.NextDouble() * 2f - 1f) * noise * Mathf.Exp(-t * 26f);
                data[i] = (body * 0.7f + nz) * env * 0.8f;
            }
            return Clip(name, data);
        }

        static AudioClip MakeWhoosh(string name, float duration, float tone)
        {
            int n = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[n];
            var random = new System.Random(name.GetHashCode());
            float last = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                float nz = (float)random.NextDouble() * 2f - 1f;
                last = Mathf.Lerp(last, nz, 0.18f + tone * 0.2f);
                float env = Mathf.Sin(Mathf.PI * t) * (1f - t * 0.4f);
                data[i] = last * env * 0.5f;
            }
            return Clip(name, data);
        }

        static AudioClip MakeHit(string name, float duration, float freq)
        {
            int n = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[n];
            var random = new System.Random(name.GetHashCode());
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SampleRate;
                float env = Mathf.Exp(-t * 26f);
                float body = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f + Mathf.Sin(2f * Mathf.PI * 430f * t) * 0.25f;
                float nz = ((float)random.NextDouble() * 2f - 1f) * 0.8f;
                data[i] = (body + nz * Mathf.Exp(-t * 45f)) * env * 0.7f;
            }
            return Clip(name, data);
        }

        static AudioClip MakeHurt(string name, float duration)
        {
            int n = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SampleRate;
                float env = Mathf.Exp(-t * 5.5f);
                float freq = Mathf.Lerp(240f, 96f, t / duration);
                data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.45f + Mathf.Sin(2f * Mathf.PI * freq * 0.5f * t) * 0.3f) * env;
            }
            return Clip(name, data);
        }

        static AudioClip MakePluck(string name, float duration, float freq)
        {
            int n = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SampleRate;
                float env = Mathf.Exp(-t * 4.2f);
                float v = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f;
                v += Mathf.Sin(2f * Mathf.PI * freq * 2.01f * t) * 0.25f * Mathf.Exp(-t * 8f);
                v += Mathf.Sin(2f * Mathf.PI * freq * 3.02f * t) * 0.14f * Mathf.Exp(-t * 11f);
                data[i] = v * env * 0.55f;
            }
            return Clip(name, data);
        }

        static AudioClip MakeGong(string name, float duration, float freq)
        {
            int n = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[n];
            var random = new System.Random(name.GetHashCode());
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SampleRate;
                float env = Mathf.Exp(-t * 1.6f);
                float v = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.55f;
                v += Mathf.Sin(2f * Mathf.PI * freq * 1.51f * t + 0.4f) * 0.3f;
                v += Mathf.Sin(2f * Mathf.PI * freq * 2.73f * t) * 0.18f;
                v += ((float)random.NextDouble() * 2f - 1f) * 0.06f;
                data[i] = v * env * 0.6f;
            }
            return Clip(name, data);
        }

        static AudioClip MakeBell(string name, float duration, float freq)
        {
            int n = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SampleRate;
                float env = Mathf.Exp(-t * 3.2f);
                data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f + Mathf.Sin(2f * Mathf.PI * freq * 2.77f * t) * 0.2f) * env * 0.55f;
            }
            return Clip(name, data);
        }

        static AudioClip MakeVictory()
        {
            float duration = 2.6f;
            int n = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[n];
            float[] notes = { 196f, 246.94f, 293.66f, 392f, 293.66f, 392f, 493.88f };
            for (int k = 0; k < notes.Length; k++)
            {
                int start = Mathf.CeilToInt(k * 0.22f * SampleRate);
                for (int i = start; i < n; i++)
                {
                    float t = (i - start) / (float)SampleRate;
                    float env = Mathf.Exp(-t * 2.2f);
                    data[i] += Mathf.Sin(2f * Mathf.PI * notes[k] * t) * env * 0.16f;
                }
            }
            return Clip("Victory", data);
        }

        static AudioClip MakeDefeat()
        {
            float duration = 3.0f;
            int n = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SampleRate;
                float env = Mathf.Exp(-t * 1.2f);
                float freq = Mathf.Lerp(110f, 55f, t / duration);
                data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.3f + Mathf.Sin(2f * Mathf.PI * freq * 1.49f * t) * 0.18f) * env;
            }
            return Clip("Defeat", data);
        }

        static AudioClip MakeAmbient()
        {
            float duration = 10f;
            int n = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[n];
            var random = new System.Random(4417);
            float wind = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SampleRate;
                float u = i / (float)n;
                float nz = (float)random.NextDouble() * 2f - 1f;
                wind = Mathf.Lerp(wind, nz, 0.015f);
                float swell = 0.5f + 0.5f * Mathf.Sin(t * 0.35f + Mathf.Sin(t * 0.11f) * 2f);
                float drone = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.035f + Mathf.Sin(2f * Mathf.PI * 82.5f * t) * 0.02f;
                float fade = Mathf.Min(1f, Mathf.Min(u, 1f - u) * 12f);
                data[i] = (wind * 0.35f * swell + drone * (0.5f + swell * 0.5f)) * fade * 0.5f;
            }
            return Clip("Ambient", data);
        }

        static AudioClip Clip(string name, float[] data)
        {
            var clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
