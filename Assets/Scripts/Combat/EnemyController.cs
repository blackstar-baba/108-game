using System.Collections;
using UnityEngine;

namespace Liangshan
{
    public sealed class EnemyController : MonoBehaviour
    {
        public EnemyDefinition Definition;
        public Combatant Combat;
        public ProceduralRig Rig;
        public bool IsBoss => Definition != null && Definition.IsBoss;
        public bool IsDead => Combat != null && Combat.IsDead;
        public float Health01 => Combat != null ? Combat.Health01 : 0f;

        enum AIState
        {
            Spawning,
            Chase,
            Windup,
            Recover,
            Special,
            Dead
        }

        Game _game;
        AIState _state;
        float _stateTimer;
        float _cooldown;
        float _specialCooldown;
        float _speed;
        float _damage;
        float _range;
        float _windup;
        float _cooldownTime;
        Vector3 _knockback;
        Vector3 _finalScale;
        float _spawnTime;
        bool _phaseTwo;
        bool _summoned1;
        bool _summoned2;
        float _strafe;
        bool _deathReported;

        public void Initialize(EnemyDefinition def, int wave)
        {
            Definition = def;
            _game = Game.Instance;
            _speed = def.Speed;
            _damage = def.Damage * (1f + Mathf.Max(0, wave - 1) * 0.075f);
            _range = def.AttackRange;
            _windup = def.Windup;
            _cooldownTime = def.Cooldown;
            _cooldown = Random.Range(0.2f, 0.8f);
            _specialCooldown = 4.5f;
            _state = AIState.Spawning;
            _stateTimer = 0.65f;
            _finalScale = transform.localScale;
            transform.localScale = _finalScale * 0.02f;
            _spawnTime = 0f;

            Rig = GetComponent<ProceduralRig>();
            Combat = gameObject.AddComponent<Combatant>();
            float health = def.Health * (1f + Mathf.Max(0, wave - 1) * 0.13f);
            if (def.IsBoss) health = def.Health;
            Combat.Initialize(Team.Enemy, def.Name, health, Rig);
            Combat.Shielded = def.Shielded;
            Combat.IsBoss = def.IsBoss;
            Combat.Damaged += OnDamaged;
            Combat.Died += OnDied;

            if (!def.IsBoss)
                WorldHealthBar.Attach(transform, Combat, def.Primary);

            _game.Vfx.InkBurst(transform.position, def.Secondary, def.IsBoss ? 3f : 1.2f);
            if (def.IsBoss)
                _game.OnBossSpawned(this);
        }

        void Update()
        {
            if (Combat == null || Combat.IsDead || _game == null) return;
            if (_game.State != GameState.Playing) return;

            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            if (_state == AIState.Spawning)
            {
                _spawnTime += dt;
                float t = Mathf.Clamp01(_spawnTime / 0.55f);
                transform.localScale = Vector3.Lerp(_finalScale * 0.02f, _finalScale, t);
                if (t >= 1f)
                    _state = AIState.Chase;
                return;
            }

            _cooldown = Mathf.Max(0f, _cooldown - dt);
            _specialCooldown = Mathf.Max(0f, _specialCooldown - dt);
            _knockback = Vector3.Lerp(_knockback, Vector3.zero, 1f - Mathf.Exp(-9f * dt));
            if (_knockback.sqrMagnitude > 0.0001f)
            {
                transform.position += _knockback * dt;
                Clamp();
            }

            if (IsBoss)
            {
                if (!_phaseTwo && Combat.Health01 <= 0.5f)
                    EnterPhaseTwo();
                if (_specialCooldown <= 0f && _state != AIState.Windup && _state != AIState.Special)
                {
                    StartCoroutine(BossSpecial());
                    return;
                }
            }

            PlayerController player = _game.Player;
            if (player == null || player.Combat.IsDead)
            {
                Rig.SetLocomotion(0f);
                return;
            }

            if (_state == AIState.Recover)
            {
                _stateTimer -= dt;
                Rig.SetLocomotion(0f);
                if (_stateTimer <= 0f)
                    _state = AIState.Chase;
                return;
            }

            if (_state == AIState.Windup)
            {
                _stateTimer -= dt;
                Rig.SetLocomotion(0f);
                Face(player.transform.position, 8f);
                if (_stateTimer <= 0f)
                {
                    Strike(player);
                    _state = AIState.Recover;
                    _stateTimer = 0.32f;
                    _cooldown = _cooldownTime;
                }
                return;
            }

            if (_state == AIState.Special)
                return;

            if (Definition.Ranged)
                UpdateArcher(player, dt);
            else
                UpdateMelee(player, dt);

            ApplySeparation(dt);
            Clamp();
        }

        void UpdateMelee(PlayerController player, float dt)
        {
            Vector3 to = player.transform.position - transform.position;
            to.y = 0f;
            float distance = to.magnitude;
            Vector3 dir = distance > 0.01f ? to / distance : transform.forward;

            if (distance > _range * 0.92f)
            {
                Vector3 move = dir;
                Move(move, _speed, dt);
                Rig.SetLocomotion(_speed / Mathf.Max(0.1f, Definition.Speed));
            }
            else
            {
                Face(player.transform.position, 11f);
                Rig.SetLocomotion(0f);
                if (_cooldown <= 0f)
                {
                    _state = AIState.Windup;
                    _stateTimer = _windup;
                    Rig.PlayAttack(0, _windup + 0.35f);
                    _game.Vfx.Telegraph(transform.position, _range * 1.15f, _windup, IsBoss ? GameConfig.Gold : GameConfig.Vermilion);
                    _game.Audio.PlayEnemyWindup(transform.position, Definition.IsBoss);
                }
            }

            // Boss adds pressure by refusing to be circled.
            if (IsBoss && distance < _range * 0.9f)
                Face(player.transform.position, 14f);
        }

        void UpdateArcher(PlayerController player, float dt)
        {
            Vector3 to = player.transform.position - transform.position;
            to.y = 0f;
            float distance = to.magnitude;
            Vector3 dir = distance > 0.01f ? to / distance : transform.forward;

            if (distance < 6.5f)
                Move(-dir, _speed * 1.05f, dt);
            else if (distance > 11f)
                Move(dir, _speed, dt);
            else
            {
                _strafe += dt;
                Vector3 side = Vector3.Cross(Vector3.up, dir) * Mathf.Sin(_strafe * 0.7f);
                Move(side, _speed * 0.55f, dt);
            }
            Face(player.transform.position, 8f);
            Rig.SetLocomotion(0.55f);

            if (_cooldown <= 0f && distance < 16f)
            {
                _state = AIState.Windup;
                _stateTimer = _windup;
                Rig.PlayAttack(1, _windup + 0.25f);
                _game.Vfx.Telegraph(transform.position, 0.9f, _windup, GameConfig.Hex("D0A85C"));
                _game.Audio.PlayEnemyWindup(transform.position, false);
            }
        }

        void Strike(PlayerController player)
        {
            Vector3 to = player.transform.position - transform.position;
            to.y = 0f;
            float distance = to.magnitude;

            if (Definition.Ranged)
            {
                Vector3 origin = transform.position + Vector3.up * 1.35f + transform.forward * 0.5f;
                Vector3 aim = player.transform.position + Vector3.up * 1.0f;
                Vector3 dir = (aim - origin).normalized;
                Projectile.Spawn(origin, dir, 22f, _damage, Combat, _game.Vfx, _game.Audio);
                Rig.SetLocomotion(0f);
                _game.Audio.PlayBow(transform.position);
                return;
            }

            if (distance <= _range * 1.28f && distance > 0.01f)
            {
                float angle = Vector3.Angle(transform.forward, to);
                if (angle < 105f || distance < _range * 0.6f)
                {
                    var info = new DamageInfo
                    {
                        Amount = _damage,
                        Point = player.transform.position + Vector3.up * 1.1f,
                        Direction = to.normalized,
                        Source = Combat,
                        Heavy = IsBoss
                    };
                    player.Combat.TakeDamage(info);
                }
            }

            Vector3 slashAt = transform.position + transform.forward * (_range * 0.65f) + Vector3.up * 1.15f;
            _game.Vfx.Slash(slashAt, transform.rotation, IsBoss ? GameConfig.Gold : GameConfig.Hex("C58A5A"),
                IsBoss ? 1.7f : 0.9f, 100f, _range);
            _game.Audio.PlayEnemySwing(transform.position, IsBoss);
            if (IsBoss)
                _game.CameraRig.Shake(0.3f);
        }

        void Move(Vector3 dir, float speed, float dt)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;
            Vector3 normalized = dir.normalized;
            transform.position += normalized * speed * dt;
            Face(transform.position + normalized, 6f);
        }

        void Face(Vector3 target, float speed)
        {
            Vector3 dir = target - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized, Vector3.up),
                1f - Mathf.Exp(-speed * Time.deltaTime));
        }

        void ApplySeparation(float dt)
        {
            Vector3 push = Vector3.zero;
            var enemies = _game.Enemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                var other = enemies[i];
                if (other == null || other == this || other.Combat == null || other.Combat.IsDead) continue;
                Vector3 delta = transform.position - other.transform.position;
                delta.y = 0f;
                float d = delta.magnitude;
                if (d < 1.05f && d > 0.001f)
                    push += delta.normalized * (1.05f - d) * 3.2f;
            }
            if (push.sqrMagnitude > 0.0001f)
                transform.position += push * dt;
        }

        void Clamp()
        {
            Vector3 p = transform.position;
            Vector2 flat = new Vector2(p.x, p.z);
            float limit = GameConfig.PlayerBoundary - 0.6f;
            if (flat.magnitude > limit)
            {
                flat = flat.normalized * limit;
                p.x = flat.x;
                p.z = flat.y;
            }
            if (p.y < 0f) p.y = 0f;
            transform.position = p;
        }

        void OnDamaged(Combatant combatant, DamageInfo info)
        {
            if (Combat.IsDead) return;
            float resist = IsBoss ? 0.12f : 1f;
            _knockback += info.Direction * (info.Heavy ? 4.5f : 2f) * resist;
            Rig.PlayHit();
            if (IsBoss)
                _game.Hud?.ShakeBossBar();
        }

        void OnDied(Combatant combatant)
        {
            if (_deathReported) return;
            _deathReported = true;
            _state = AIState.Dead;
            StopAllCoroutines();
            Combat.IsDead = true;
            Rig.Die();
            _game.Vfx.InkBurst(transform.position + Vector3.up * 0.8f, Definition.Secondary, IsBoss ? 4f : 1.5f);
            _game.Vfx.Slash(transform.position + Vector3.up * 1f, transform.rotation, GameConfig.Paper * 0.9f,
                IsBoss ? 2.6f : 1f, 360f, 3f);
            _game.OnEnemyKilled(this);
            Destroy(gameObject, IsBoss ? 5f : 2.6f);
        }

        void EnterPhaseTwo()
        {
            _phaseTwo = true;
            _speed *= 1.22f;
            _damage *= 1.18f;
            _cooldownTime *= 0.78f;
            _game.Hud?.ShowBossPhase("高俅 · 怒");
            _game.CameraRig.Shake(0.7f);
            _game.Audio.PlayBossPhase(transform.position);
            _game.Vfx.Shockwave(transform.position, GameConfig.Vermilion, 6f);
        }

        IEnumerator BossSpecial()
        {
            _state = AIState.Special;
            _specialCooldown = Random.Range(6.5f, 8.5f);
            PlayerController player = _game.Player;
            if (player == null) { _state = AIState.Chase; yield break; }

            int choice = Random.Range(0, 3);
            if (choice == 0)
            {
                // Ground slam.
                _game.Hud?.ShowBossMove("裂地");
                _game.Vfx.Telegraph(player.transform.position, 5f, 1.0f, GameConfig.Gold);
                Rig.PlayAttack(2, 1.2f);
                yield return new WaitForSeconds(1.0f);
                Vector3 at = player.transform.position;
                _game.Vfx.Shockwave(at, GameConfig.Gold, 6.5f);
                _game.Vfx.Ring(at + Vector3.up * 0.1f, 1f, 6f, GameConfig.Gold, 0.7f, 0.35f);
                _game.Audio.PlayHeavyImpact(at);
                _game.CameraRig.Shake(0.85f);
                if (Vector3.Distance(player.transform.position, at) < 5.4f)
                {
                    player.Combat.TakeDamage(new DamageInfo
                    {
                        Amount = _damage * 1.35f,
                        Point = player.transform.position,
                        Direction = (player.transform.position - at).normalized,
                        Source = Combat,
                        Heavy = true
                    });
                }
                yield return new WaitForSeconds(0.55f);
            }
            else if (choice == 1)
            {
                // Whirlwind.
                _game.Hud?.ShowBossMove("横扫千军");
                _game.Vfx.Telegraph(transform.position, 5.8f, 0.9f, GameConfig.Gold);
                Rig.PlayAttack(1, 1.35f);
                yield return new WaitForSeconds(0.9f);
                _game.Vfx.Slash(transform.position + Vector3.up * 1.2f, Quaternion.Euler(0f, 180f, 0f),
                    GameConfig.Gold, 2.8f, 360f, 6f);
                _game.Vfx.Shockwave(transform.position, GameConfig.Gold, 7f);
                _game.CameraRig.Shake(0.8f);
                _game.Audio.PlayHeavyImpact(transform.position);
                if (Vector3.Distance(player.transform.position, transform.position) < 6.2f)
                {
                    player.Combat.TakeDamage(new DamageInfo
                    {
                        Amount = _damage * 1.1f,
                        Point = player.transform.position,
                        Direction = (player.transform.position - transform.position).normalized,
                        Source = Combat,
                        Heavy = true
                    });
                }
                yield return new WaitForSeconds(0.6f);
            }
            else
            {
                // Charge.
                _game.Hud?.ShowBossMove("撞阵");
                Face(player.transform.position, 30f);
                _game.Vfx.Telegraph(transform.position, 2f, 0.45f, GameConfig.Vermilion);
                yield return new WaitForSeconds(0.45f);
                Vector3 dir = player.transform.position - transform.position;
                dir.y = 0f;
                dir = dir.normalized;
                Vector3 start = transform.position;
                float t = 0f;
                while (t < 0.42f)
                {
                    t += Time.deltaTime;
                    transform.position += dir * 17f * Time.deltaTime;
                    if (Vector3.Distance(transform.position, player.transform.position) < 2.6f)
                    {
                        player.Combat.TakeDamage(new DamageInfo
                        {
                            Amount = _damage * 1.5f,
                            Point = player.transform.position,
                            Direction = dir,
                            Source = Combat,
                            Heavy = true
                        });
                        break;
                    }
                    yield return null;
                }
                _game.Vfx.Shockwave(transform.position, GameConfig.Vermilion, 4f);
                _game.CameraRig.Shake(0.7f);
                yield return new WaitForSeconds(0.45f);
            }

            if (!_summoned1 && Combat.Health01 < 0.65f)
            {
                _summoned1 = true;
                _game.SpawnAdds(3, transform.position);
            }
            else if (!_summoned2 && Combat.Health01 < 0.35f)
            {
                _summoned2 = true;
                _game.SpawnAdds(4, transform.position);
            }

            _state = AIState.Chase;
            _cooldown = 0.8f;
        }
    }
}
