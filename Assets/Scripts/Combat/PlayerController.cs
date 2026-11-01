using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Liangshan
{
    public sealed class PlayerController : MonoBehaviour
    {
        public HeroDefinition Hero;
        public ProceduralRig Rig;
        public Combatant Combat;
        public PlayerBoons Boons = new PlayerBoons();
        public float UltimateMeter; // 0..100
        public int Combo;
        public float ComboTimer;

        public float SkillCooldownRemaining => Mathf.Max(0f, _skillCooldown);
        public float DodgeCooldownRemaining => Mathf.Max(0f, _dodgeCooldown);
        public float AttackRange => Hero.AttackRange * Boons.RangeMult;
        public bool IsDead => Combat != null && Combat.IsDead;

        Game _game;
        CharacterController _controller;
        float _verticalVelocity;
        Vector3 _moveVelocity;
        Vector3 _forcedVelocity;
        float _forcedMoveTimer;

        float _attackTimer = -1f;
        float _attackDuration;
        float _attackBuffer;
        int _comboIndex;
        int _comboMax;
        bool _hitDone;
        bool _queuedAttack;
        bool _heavyAttack;
        string _attackClipName;
        float _comboReset;

        float _dodgeCooldown;
        float _dodgeTimer;
        Vector3 _dodgeDirection;
        float _invulnTimer;
        float _skillCooldown;
        float _castingTimer;
        bool _casting;
        float _stepTimer;
        int _hitCounter;
        float _lastHurtTime;

        public void Initialize(Game game, HeroDefinition hero)
        {
            _game = game;
            Hero = hero;
            _comboMax = hero.ComboCount;

            transform.position = game.World.PlayerSpawn + Vector3.up * 0.02f;
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            _controller = gameObject.AddComponent<CharacterController>();
            _controller.height = 1.78f;
            _controller.radius = 0.36f;
            _controller.center = new Vector3(0f, 0.91f, 0f);
            _controller.slopeLimit = 55f;
            _controller.stepOffset = 0.35f;
            _controller.skinWidth = 0.04f;
            _controller.minMoveDistance = 0f;

            Rig = CharacterFactory.BuildHero(transform, hero, Vector3.zero, Quaternion.identity);
            Rig.transform.localPosition = Vector3.zero;
            Rig.transform.localRotation = Quaternion.identity;

            Combat = gameObject.AddComponent<Combatant>();
            Combat.Initialize(Team.Player, hero.Name, hero.MaxHealth, Rig);
            Combat.Damaged += OnDamaged;
            Combat.Died += OnDied;
            Combat.DamageReduction = Boons.DamageReduction;

            _comboMax = hero.ComboCount + Boons.ExtraCombo;
            game.Hud?.BindPlayer(this);
        }

        void Update()
        {
            if (Combat == null || Combat.IsDead) return;

            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            _skillCooldown = Mathf.Max(0f, _skillCooldown - dt);
            _dodgeCooldown = Mathf.Max(0f, _dodgeCooldown - dt);
            _comboReset = Mathf.Max(0f, _comboReset - dt);
            if (_comboReset <= 0f && _attackTimer < 0f && !_casting)
            {
                Combo = 0;
                _comboIndex = 0;
            }
            if (Combo > 0)
                ComboTimer = Mathf.Max(0f, ComboTimer - dt);

            if (_game == null || _game.State != GameState.Playing)
                return;

            Vector2 look = InputReader.MouseDelta;
            if (look.sqrMagnitude > 0.0001f)
                _game.CameraRig.AddLook(look);

            if (_forcedMoveTimer > 0f)
            {
                _forcedMoveTimer -= dt;
                _controller.Move(_forcedVelocity * dt + Vector3.up * _verticalVelocity * dt);
                ClampToIsland();
                Rig.SetLocomotion(1f);
                return;
            }

            if (_casting)
            {
                _castingTimer -= dt;
                UpdateGravity(dt, Vector3.zero, 0f);
                Rig.SetLocomotion(0f);
                return;
            }

            HandleDodge(dt);
            HandleAttack(dt);

            Vector3 input = CameraRelativeInput();
            float speed = Hero.MoveSpeed * Boons.SpeedMult;
            if (_dodgeTimer > 0f)
            {
                _moveVelocity = _dodgeDirection * 12.5f;
            }
            else if (_attackTimer >= 0f)
            {
                _moveVelocity = Vector3.MoveTowards(_moveVelocity, Vector3.zero, 24f * dt);
                speed *= 0.12f;
            }
            else
            {
                Vector3 target = input * speed;
                float accel = input.sqrMagnitude > 0.01f ? 38f : 26f;
                _moveVelocity = Vector3.MoveTowards(_moveVelocity, target, accel * dt);
                if (input.sqrMagnitude > 0.02f)
                    FaceDirection(input, 14f);
            }

            UpdateGravity(dt, _moveVelocity, _attackTimer >= 0f ? 0.35f : 1f);
            ClampToIsland();
            Rig.SetLocomotion(new Vector2(_moveVelocity.x, _moveVelocity.z).magnitude / Mathf.Max(0.1f, speed));

            if (_moveVelocity.sqrMagnitude > 2f && _attackTimer < 0f && _dodgeTimer <= 0f)
            {
                _stepTimer -= dt;
                if (_stepTimer <= 0f)
                {
                    _stepTimer = 0.38f;
                    _game.Audio.PlayFootstep(transform.position);
                }
            }

            if (InputReader.PauseDown)
                _game.TogglePause();
        }

        Vector3 CameraRelativeInput()
        {
            Vector2 raw = InputReader.Move;
            if (raw.sqrMagnitude < 0.01f) return Vector3.zero;
            Transform cam = _game.GameCamera.transform;
            Vector3 forward = cam.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = cam.right;
            right.y = 0f;
            right.Normalize();
            Vector3 dir = right * raw.x + forward * raw.y;
            return dir.sqrMagnitude > 1f ? dir.normalized : dir;
        }

        void UpdateGravity(float dt, Vector3 horizontal, float control)
        {
            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            else
                _verticalVelocity -= 24f * dt;
            Vector3 motion = horizontal * dt * control;
            motion.y = _verticalVelocity * dt;
            _controller.Move(motion);
        }

        void ClampToIsland()
        {
            Vector3 p = transform.position;
            Vector2 flat = new Vector2(p.x, p.z);
            if (flat.magnitude > GameConfig.PlayerBoundary)
            {
                flat = flat.normalized * GameConfig.PlayerBoundary;
                p.x = flat.x;
                p.z = flat.y;
                transform.position = p;
            }
            if (p.y < -1f)
            {
                p.y = 0.02f;
                transform.position = p;
                _verticalVelocity = 0f;
            }
        }

        void FaceDirection(Vector3 dir, float speed)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return;
            Quaternion target = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, 1f - Mathf.Exp(-speed * Time.deltaTime));
        }

        void HandleDodge(float dt)
        {
            if (_dodgeTimer > 0f)
            {
                _dodgeTimer -= dt;
                if (_dodgeTimer <= 0f)
                    Rig.CancelAttack();
                return;
            }
            if (!InputReader.DodgeDown || _dodgeCooldown > 0f || _casting) return;

            Vector3 dir = CameraRelativeInput();
            if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
            _dodgeDirection = dir.normalized;
            _dodgeTimer = 0.30f;
            _dodgeCooldown = 0.95f * Boons.DodgeCooldownMult;
            _invulnTimer = 0.38f;
            Combat.GrantInvulnerability(0.38f);
            _attackTimer = -1f;
            Rig.PlayDodge(0.30f);
            FaceDirection(_dodgeDirection, 30f);
            _game.Vfx.Dust(transform.position + Vector3.up * 0.05f, _dodgeDirection, GameConfig.Paper * 0.8f);
            _game.Audio.PlayDodge(transform.position);
            _game.CameraRig.Shake(0.12f);
        }

        void HandleAttack(float dt)
        {
            if (InputReader.AttackDown)
                _attackBuffer = 0.28f;
            if (InputReader.HeavyDown && _attackTimer < 0f && !_casting && _dodgeTimer <= 0f)
            {
                _attackBuffer = 0f;
                StartAttack(_comboIndex, true);
                return;
            }

            if (_attackBuffer > 0f)
                _attackBuffer -= dt;

            if (_attackTimer >= 0f)
            {
                _attackTimer += dt;
                if (!_hitDone && _attackTimer / _attackDuration >= 0.45f)
                {
                    _hitDone = true;
                    PerformHit(_heavyAttack);
                }
                if (_attackBuffer > 0f && _attackTimer / _attackDuration > 0.32f)
                    _queuedAttack = true;
                if (_attackTimer >= _attackDuration)
                {
                    _attackTimer = -1f;
                    if (_queuedAttack && _comboIndex < _comboMax - 1)
                    {
                        _queuedAttack = false;
                        _attackBuffer = 0f;
                        StartAttack(_comboIndex + 1, false);
                    }
                    else
                    {
                        _comboReset = 0.55f;
                    }
                }
            }
            else if (_attackBuffer > 0f && !_casting && _dodgeTimer <= 0f)
            {
                _attackBuffer = 0f;
                StartAttack(0, false);
            }

            if (InputReader.SkillDown && _skillCooldown <= 0f && !_casting && _dodgeTimer <= 0f)
                StartCoroutine(SkillRoutine());
            if (InputReader.UltimateDown && UltimateMeter >= 99.5f && !_casting && _dodgeTimer <= 0f)
                StartCoroutine(UltimateRoutine());
        }

        void StartAttack(int index, bool heavy)
        {
            _comboIndex = Mathf.Clamp(index, 0, Mathf.Max(0, _comboMax - 1));
            _heavyAttack = heavy;
            _hitDone = false;
            _queuedAttack = false;
            _attackBuffer = 0f;

            float speed = Boons.AttackSpeedMult * (1f + Mathf.Max(0, _comboIndex) * 0.035f);
            _attackDuration = (heavy ? Hero.AttackDuration * 1.55f : Hero.AttackDuration) / Mathf.Max(0.4f, speed);
            _attackTimer = 0f;

            Transform target = FindTarget(11f, 80f);
            if (target != null)
                FaceDirection(target.position - transform.position, 26f);
            else
                FaceDirection(CameraForward(), 18f);

            int clip = AttackClipFor(index, heavy);
            Rig.PlayAttack(clip, _attackDuration);
            _attackClipName = heavy ? "重击" : "连击";
            _game.Audio.PlayWhoosh(transform.position, heavy);
            _game.CameraRig.Shake(heavy ? 0.10f : 0.035f);
        }

        int AttackClipFor(int index, bool heavy)
        {
            if (heavy) return 2;
            switch (Hero.Weapon)
            {
                case WeaponStyle.Spear:
                    return (index % 3 == 0) ? 0 : (index % 3 == 1 ? 1 : 0);
                case WeaponStyle.DualBlades:
                    return (index % 3 == 2) ? 2 : 1;
                case WeaponStyle.Staff:
                    return (index % 3 == 0) ? 2 : 1;
                default:
                    return (index % 3 == 1) ? 2 : 1;
            }
        }

        void PerformHit(bool heavy)
        {
            float range = AttackRange;
            float arc = Hero.Weapon == WeaponStyle.Staff ? 150f : (Hero.Weapon == WeaponStyle.DualBlades ? 120f : 78f);
            float mult = (heavy ? 1.65f : 1f) * (1f + _comboIndex * 0.07f);
            int hits = DamageArea(transform.position, transform.forward, range, arc, mult, heavy, true);

            Vector3 effectPos = transform.position + transform.forward * (range * 0.65f) + Vector3.up * 1.18f;
            Color slashColor = Color.Lerp(GameConfig.Paper, Hero.Primary, 0.55f);
            _game.Vfx.Slash(effectPos, transform.rotation, slashColor, heavy ? 1.5f : 1f, arc, range);

            if (hits > 0)
            {
                _hitCounter += hits;
                _game.HitStop(heavy ? 0.07f : 0.035f);
                _game.CameraRig.Shake(heavy ? 0.30f : 0.16f);
                _game.Audio.PlayHit(transform.position, heavy);
                Combo += hits;
                ComboTimer = 2.2f;
                _game.Hud?.PulseCombo(Combo);
            }
        }

        int DamageArea(Vector3 origin, Vector3 forward, float range, float arc, float mult, bool heavy, bool grantUltimate)
        {
            int hits = 0;
            var enemies = _game.Enemies;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.Combat == null || enemy.Combat.IsDead) continue;
                Vector3 to = enemy.transform.position - origin;
                to.y = 0f;
                float distance = to.magnitude;
                if (distance > range + 0.5f) continue;
                if (distance > 0.05f)
                {
                    float dot = Vector3.Dot(forward, to / distance);
                    if (dot < Mathf.Cos(arc * 0.5f * Mathf.Deg2Rad)) continue;
                }

                float falloff = Mathf.Lerp(1f, 0.72f, Mathf.Clamp01(distance / Mathf.Max(0.5f, range)));
                bool crit = Random.value < Boons.CritChance;
                float lowHealth = Combat.Health01 < 0.4f ? 1.25f : 1f;
                float amount = Hero.Damage * Boons.DamageMult * mult * falloff * lowHealth * (crit ? Boons.CritMult : 1f);
                var info = new DamageInfo
                {
                    Amount = amount,
                    Point = enemy.transform.position + Vector3.up * 1.1f,
                    Direction = (enemy.transform.position - transform.position).normalized,
                    Source = Combat,
                    Heavy = heavy,
                    Critical = crit
                };
                if (enemy.Combat.TakeDamage(info))
                {
                    hits++;
                    if (Boons.Lifesteal > 0f)
                        Combat.Heal(amount * Boons.Lifesteal);
                    _game.Vfx.Impact(info.Point, crit ? GameConfig.Gold : Hero.Primary, heavy ? 1.5f : 1f);
                    _game.Vfx.DamageNumber(info.Point + Vector3.up * 0.25f, amount, crit);
                    if (grantUltimate)
                        AddUltimate((heavy ? 11f : 7f) * Boons.UltimateGainMult);
                }
            }
            return hits;
        }

        public void AddUltimate(float amount)
        {
            UltimateMeter = Mathf.Clamp(UltimateMeter + amount, 0f, 100f);
            _game.Hud?.SetUltimate(UltimateMeter / 100f);
        }

        Transform FindTarget(float maxDistance, float maxAngle)
        {
            Transform best = null;
            float bestScore = float.MaxValue;
            Vector3 forward = CameraForward();
            for (int i = 0; i < _game.Enemies.Count; i++)
            {
                var enemy = _game.Enemies[i];
                if (enemy == null || enemy.Combat == null || enemy.Combat.IsDead) continue;
                Vector3 to = enemy.transform.position - transform.position;
                to.y = 0f;
                float distance = to.magnitude;
                if (distance > maxDistance || distance < 0.01f) continue;
                float angle = Vector3.Angle(forward, to);
                if (angle > maxAngle) continue;
                float score = distance + angle * 0.09f;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = enemy.transform;
                }
            }
            return best;
        }

        Vector3 CameraForward()
        {
            Vector3 forward = _game.GameCamera.transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.001f ? forward.normalized : transform.forward;
        }

        IEnumerator SkillRoutine()
        {
            _casting = true;
            _castingTimer = 1.2f;
            _attackTimer = -1f;
            _skillCooldown = Hero.SkillCooldown;
            _game.Hud?.ShowSkillName(Hero.SkillName);
            _game.Audio.PlaySkill(transform.position);
            Combat.GrantInvulnerability(0.35f);
            Rig.PlayAttack(2, 0.85f);

            switch (Hero.Id)
            {
                case "linchong":
                {
                    yield return new WaitForSeconds(0.18f);
                    for (int i = 0; i < 3; i++)
                    {
                        Transform target = FindTarget(9f, 70f);
                        Vector3 dir = target != null ? (target.position - transform.position).normalized : transform.forward;
                        FaceDirection(dir, 30f);
                        Vector3 at = transform.position + dir * 1.9f + Vector3.up * 1.2f;
                        _game.Vfx.Slash(at, Quaternion.LookRotation(dir), GameConfig.Hex("9FD3E0"), 1.35f, 45f, 7f);
                        DamageArea(transform.position, dir, 8.2f, 58f, 0.85f, false, true);
                        _game.Audio.PlayHit(transform.position, false);
                        _game.CameraRig.Shake(0.16f);
                        yield return new WaitForSeconds(0.18f);
                    }
                    break;
                }
                case "wusong":
                {
                    yield return new WaitForSeconds(0.08f);
                    Vector3 dir = CameraRelativeInput();
                    if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
                    _dodgeDirection = dir.normalized;
                    FaceDirection(dir, 40f);
                    _forcedVelocity = dir.normalized * 15f;
                    _forcedMoveTimer = 0.24f;
                    Combat.GrantInvulnerability(0.4f);
                    _game.Vfx.Dust(transform.position, -dir, GameConfig.Vermilion);
                    yield return new WaitForSeconds(0.24f);
                    _forcedMoveTimer = 0f;
                    _game.Vfx.Ring(transform.position + Vector3.up * 0.1f, 1f, 4.2f, GameConfig.Vermilion, 0.4f, 0.18f);
                    DamageArea(transform.position, transform.forward, 4.6f, 360f, 2.3f, true, true);
                    _game.CameraRig.Shake(0.5f);
                    _game.Audio.PlayHeavyImpact(transform.position);
                    break;
                }
                default:
                {
                    yield return new WaitForSeconds(0.25f);
                    Vector3 at = transform.position + transform.forward * 1.2f;
                    _game.Vfx.Ring(at + Vector3.up * 0.12f, 1f, 6.8f, GameConfig.Gold, 0.6f, 0.35f);
                    _game.Vfx.Shockwave(at, GameConfig.Gold, 6.5f);
                    DamageArea(transform.position, transform.forward, 5.6f, 360f, 2.5f, true, true);
                    _game.CameraRig.Shake(0.8f);
                    _game.Audio.PlayHeavyImpact(at);
                    break;
                }
            }

            yield return new WaitForSeconds(0.28f);
            _casting = false;
            _castingTimer = 0f;
        }

        IEnumerator UltimateRoutine()
        {
            _casting = true;
            _castingTimer = 3.5f;
            _attackTimer = -1f;
            UltimateMeter = 0f;
            _game.Hud?.SetUltimate(0f);
            _game.Hud?.ShowUltimateName(Hero.UltimateName);
            _game.Audio.PlayUltimate(transform.position);
            Combat.GrantInvulnerability(1.8f);
            _game.CameraRig.Cinematic(2.4f, 1.1f);
            _game.Vfx.Kick(0.35f);

            if (Hero.Id == "linchong")
            {
                for (int i = 0; i < 4; i++)
                {
                    Rig.PlayAttack(i % 2, 0.5f);
                    _game.Vfx.SnowBurst(transform.position, 9f);
                    _game.Vfx.Slash(transform.position + Vector3.up * 1.1f, Quaternion.Euler(0f, i * 90f, 0f),
                        GameConfig.Hex("BFE4EE"), 2.4f, 360f, 6.5f);
                    DamageArea(transform.position, Quaternion.Euler(0f, i * 90f, 0f) * Vector3.forward, 6.5f, 150f, 1.5f, true, false);
                    _game.CameraRig.Shake(0.45f);
                    _game.Audio.PlayHeavyImpact(transform.position);
                    yield return new WaitForSeconds(0.24f);
                }
            }
            else if (Hero.Id == "wusong")
            {
                for (int i = 0; i < 8; i++)
                {
                    float a = i * 45f;
                    Rig.PlayAttack(1, 0.3f);
                    Vector3 dir = Quaternion.Euler(0f, a, 0f) * Vector3.forward;
                    FaceDirection(dir, 60f);
                    _game.Vfx.Slash(transform.position + dir * 1.4f + Vector3.up * 1.1f, Quaternion.LookRotation(dir),
                        GameConfig.Hex("D65A4A"), 1.6f, 55f, 4f);
                    DamageArea(transform.position, dir, 4.5f, 70f, 0.85f, false, false);
                    _game.Audio.PlayHit(transform.position, false);
                    yield return new WaitForSeconds(0.11f);
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    Rig.PlayAttack(2, 0.65f);
                    _game.Vfx.Ring(transform.position + Vector3.up * 0.1f, 1f, 7.5f + i, GameConfig.Gold, 0.55f, 0.3f);
                    _game.Vfx.Shockwave(transform.position, GameConfig.Hex("D9B45F"), 7.5f + i);
                    DamageArea(transform.position, transform.forward, 6.2f + i * 0.6f, 360f, 1.9f, true, false);
                    _game.CameraRig.Shake(0.8f);
                    _game.Audio.PlayHeavyImpact(transform.position);
                    yield return new WaitForSeconds(0.3f);
                }
            }

            yield return new WaitForSeconds(0.8f);
            _casting = false;
            _castingTimer = 0f;
        }

        void OnDamaged(Combatant combatant, DamageInfo info)
        {
            if (Time.time - _lastHurtTime < 0.08f) return;
            _lastHurtTime = Time.time;
            Combat.GrantInvulnerability(0.42f);
            _game.OnPlayerDamaged(info.Amount);
            _game.CameraRig.Shake(0.42f);
            _game.Audio.PlayHurt(transform.position);
            _game.Vfx.Impact(transform.position + Vector3.up * 1.1f, GameConfig.Vermilion, 1.2f);
            if (_attackTimer >= 0f && Random.value < 0.35f)
            {
                _attackTimer = -1f;
                Rig.CancelAttack();
            }
        }

        void OnDied(Combatant combatant)
        {
            _moveVelocity = Vector3.zero;
            _casting = false;
            _game.OnPlayerDeath();
        }

        public void ApplyBoons(bool healBonus)
        {
            float oldMax = Combat.MaxHealth;
            float ratio = oldMax > 0f ? Combat.Health / oldMax : 1f;
            Combat.MaxHealth = Hero.MaxHealth * Boons.HealthMult;
            Combat.Health = Mathf.Clamp(Combat.MaxHealth * Mathf.Max(ratio, 0f), 1f, Combat.MaxHealth);
            Combat.DamageReduction = Boons.DamageReduction;
            _comboMax = Hero.ComboCount + Boons.ExtraCombo;
            if (healBonus)
                Combat.Heal(Combat.MaxHealth * 0.30f);
            _game.Hud?.BindPlayer(this);
        }

        public string CurrentAttackName => _attackClipName;

        public void DebugAttack() { if (_attackTimer < 0f) StartAttack(0, false); }
        public void DebugSkill() { if (_skillCooldown <= 0f) StartCoroutine(SkillRoutine()); }
        public void DebugUltimate() { UltimateMeter = 100f; if (!_casting) StartCoroutine(UltimateRoutine()); }
    }
}
