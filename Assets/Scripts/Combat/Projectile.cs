using UnityEngine;

namespace Liangshan
{
    public sealed class Projectile : MonoBehaviour
    {
        float _damage;
        float _speed;
        float _life = 5f;
        Vector3 _direction;
        Combatant _source;
        VFXManager _vfx;
        AudioDirector _audio;
        bool _hit;

        public static Projectile Spawn(Vector3 origin, Vector3 direction, float speed, float damage,
            Combatant source, VFXManager vfx, AudioDirector audio)
        {
            var go = new GameObject("Arrow");
            go.transform.position = origin;
            go.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            var proj = go.AddComponent<Projectile>();
            proj._direction = direction.normalized;
            proj._speed = speed;
            proj._damage = damage;
            proj._source = source;
            proj._vfx = vfx;
            proj._audio = audio;

            var shaft = WorldFactory.Primitive("shaft", go.transform, PrimitiveType.Cylinder, Vector3.zero,
                new Vector3(0.018f, 0.36f, 0.018f), Quaternion.Euler(90f, 0f, 0f), RuntimeAssets.Toon(GameConfig.Hex("4A3323"), 0f), false, false);
            var tip = WorldFactory.Primitive("tip", go.transform, PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.38f),
                new Vector3(0.032f, 0.07f, 0.032f), Quaternion.Euler(90f, 0f, 0f), RuntimeAssets.Toon(GameConfig.Hex("8E979A"), 0f), false, false);
            var fletch = WorldFactory.Primitive("fletch", go.transform, PrimitiveType.Cube, new Vector3(0f, 0f, -0.32f),
                new Vector3(0.10f, 0.01f, 0.12f), null, RuntimeAssets.Toon(GameConfig.Vermilion, 0f), false, false);

            var trail = go.AddComponent<TrailRenderer>();
            trail.time = 0.22f;
            trail.minVertexDistance = 0.05f;
            trail.startWidth = 0.07f;
            trail.endWidth = 0f;
            trail.material = RuntimeAssets.Unlit(new Color(1f, 0.82f, 0.55f, 0.8f));
            trail.startColor = new Color(1f, 0.8f, 0.5f, 0.8f);
            trail.endColor = new Color(1f, 0.5f, 0.25f, 0f);
            return proj;
        }

        void Update()
        {
            if (_hit) return;
            float dt = Time.deltaTime;
            _life -= dt;
            if (_life <= 0f) { Destroy(gameObject); return; }
            transform.position += _direction * _speed * dt;

            PlayerController player = Game.Instance != null ? Game.Instance.Player : null;
            if (player != null && !player.Combat.IsDead)
            {
                Vector3 center = player.transform.position + Vector3.up * 1.0f;
                if (Vector3.Distance(transform.position, center) < 0.75f)
                {
                    player.Combat.TakeDamage(new DamageInfo
                    {
                        Amount = _damage,
                        Point = transform.position,
                        Direction = _direction,
                        Source = _source,
                        Heavy = false
                    });
                    _vfx?.Impact(transform.position, GameConfig.Hex("E0B27A"), 0.8f);
                    _audio?.PlayArrowHit(transform.position);
                    _hit = true;
                    Destroy(gameObject);
                    return;
                }
            }

            Vector3 flat = transform.position;
            if (flat.y < 0f || flat.magnitude > 60f)
                Destroy(gameObject);
        }
    }
}
