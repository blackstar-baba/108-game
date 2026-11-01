using System.Collections.Generic;
using UnityEngine;

namespace Liangshan
{
    /// <summary>Additive Euler pose for the procedural humanoid. Angles are degrees around each bone's bind rotation.</summary>
    public sealed class Pose
    {
        public Vector3 Spine;
        public Vector3 Chest;
        public Vector3 Neck;
        public Vector3 Head;
        public Vector3 ShoulderL;
        public Vector3 ElbowL;
        public Vector3 HandL;
        public Vector3 ShoulderR;
        public Vector3 ElbowR;
        public Vector3 HandR;
        public Vector3 HipL;
        public Vector3 KneeL;
        public Vector3 FootL;
        public Vector3 HipR;
        public Vector3 KneeR;
        public Vector3 FootR;
        public Vector3 WeaponR;
        public Vector3 WeaponL;
        public Vector3 VisualOffset;

        public static Pose Idle(WeaponStyle style)
        {
            var p = new Pose
            {
                Spine = new Vector3(1.5f, 0f, 0f),
                Chest = new Vector3(3f, 0f, 0f),
                Neck = new Vector3(-2f, 0f, 0f),
                Head = new Vector3(-2f, 0f, 0f),
                ShoulderL = new Vector3(-6f, 0f, -13f),
                ElbowL = new Vector3(-22f, 0f, 0f),
                HandL = new Vector3(0f, 0f, 0f),
                ShoulderR = new Vector3(-6f, 0f, 13f),
                ElbowR = new Vector3(-22f, 0f, 0f),
                HandR = new Vector3(0f, 0f, 0f),
                HipL = new Vector3(0f, 4f, -5f),
                KneeL = new Vector3(7f, 0f, 0f),
                FootL = new Vector3(-4f, 0f, 0f),
                HipR = new Vector3(0f, -4f, 5f),
                KneeR = new Vector3(7f, 0f, 0f),
                FootR = new Vector3(-4f, 0f, 0f),
                WeaponR = Vector3.zero,
                WeaponL = Vector3.zero
            };
            switch (style)
            {
                case WeaponStyle.Spear:
                    p.ShoulderR = new Vector3(-10f, 0f, 18f);
                    p.ElbowR = new Vector3(-38f, 0f, 0f);
                    p.WeaponR = new Vector3(2f, 0f, -4f);
                    p.ShoulderL = new Vector3(-14f, 0f, -20f);
                    p.ElbowL = new Vector3(-46f, 0f, 0f);
                    break;
                case WeaponStyle.DualBlades:
                    p.ShoulderR = new Vector3(-16f, 0f, 24f);
                    p.ElbowR = new Vector3(-30f, 0f, 0f);
                    p.ShoulderL = new Vector3(-16f, 0f, -24f);
                    p.ElbowL = new Vector3(-30f, 0f, 0f);
                    p.WeaponR = new Vector3(155f, 0f, -14f);
                    p.WeaponL = new Vector3(155f, 0f, 14f);
                    break;
                case WeaponStyle.Staff:
                    p.ShoulderR = new Vector3(-18f, 0f, 22f);
                    p.ElbowR = new Vector3(-52f, 0f, 0f);
                    p.ShoulderL = new Vector3(-8f, 0f, -18f);
                    p.ElbowL = new Vector3(-30f, 0f, 0f);
                    p.WeaponR = new Vector3(0f, 0f, -3f);
                    break;
                case WeaponStyle.Dao:
                    p.ShoulderR = new Vector3(-10f, 0f, 20f);
                    p.ElbowR = new Vector3(-30f, 0f, 0f);
                    p.WeaponR = new Vector3(30f, 0f, -8f);
                    break;
                case WeaponStyle.Shield:
                    p.ShoulderL = new Vector3(-50f, 0f, -18f);
                    p.ElbowL = new Vector3(-60f, 0f, 0f);
                    p.ShoulderR = new Vector3(-12f, 0f, 20f);
                    p.ElbowR = new Vector3(-34f, 0f, 0f);
                    p.WeaponR = new Vector3(36f, 0f, -8f);
                    break;
                case WeaponStyle.Bow:
                    p.ShoulderL = new Vector3(-72f, 0f, -24f);
                    p.ElbowL = new Vector3(-8f, 0f, 0f);
                    p.ShoulderR = new Vector3(-26f, 0f, 16f);
                    p.ElbowR = new Vector3(-82f, 0f, 0f);
                    p.WeaponL = new Vector3(0f, 0f, 80f);
                    p.WeaponR = new Vector3(0f, 0f, 0f);
                    break;
                case WeaponStyle.Halberd:
                    p.ShoulderR = new Vector3(-22f, 0f, 26f);
                    p.ElbowR = new Vector3(-54f, 0f, 0f);
                    p.ShoulderL = new Vector3(-10f, 0f, -18f);
                    p.ElbowL = new Vector3(-34f, 0f, 0f);
                    p.WeaponR = new Vector3(-8f, 0f, -5f);
                    break;
            }
            return p;
        }

        public static Pose Lerp(Pose a, Pose b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Pose
            {
                Spine = Vector3.LerpUnclamped(a.Spine, b.Spine, t),
                Chest = Vector3.LerpUnclamped(a.Chest, b.Chest, t),
                Neck = Vector3.LerpUnclamped(a.Neck, b.Neck, t),
                Head = Vector3.LerpUnclamped(a.Head, b.Head, t),
                ShoulderL = Vector3.LerpUnclamped(a.ShoulderL, b.ShoulderL, t),
                ElbowL = Vector3.LerpUnclamped(a.ElbowL, b.ElbowL, t),
                HandL = Vector3.LerpUnclamped(a.HandL, b.HandL, t),
                ShoulderR = Vector3.LerpUnclamped(a.ShoulderR, b.ShoulderR, t),
                ElbowR = Vector3.LerpUnclamped(a.ElbowR, b.ElbowR, t),
                HandR = Vector3.LerpUnclamped(a.HandR, b.HandR, t),
                HipL = Vector3.LerpUnclamped(a.HipL, b.HipL, t),
                KneeL = Vector3.LerpUnclamped(a.KneeL, b.KneeL, t),
                FootL = Vector3.LerpUnclamped(a.FootL, b.FootL, t),
                HipR = Vector3.LerpUnclamped(a.HipR, b.HipR, t),
                KneeR = Vector3.LerpUnclamped(a.KneeR, b.KneeR, t),
                FootR = Vector3.LerpUnclamped(a.FootR, b.FootR, t),
                WeaponR = Vector3.LerpUnclamped(a.WeaponR, b.WeaponR, t),
                WeaponL = Vector3.LerpUnclamped(a.WeaponL, b.WeaponL, t),
                VisualOffset = Vector3.LerpUnclamped(a.VisualOffset, b.VisualOffset, t)
            };
        }

        public Pose Copy()
        {
            return (Pose)MemberwiseClone();
        }
    }

    public sealed class AttackClip
    {
        public string Name;
        public Pose[] Poses;
        public float[] Times;

        public Pose Sample(float normalized)
        {
            normalized = Mathf.Clamp01(normalized);
            for (int i = 0; i < Times.Length - 1; i++)
            {
                if (normalized <= Times[i + 1])
                {
                    float span = Mathf.Max(0.0001f, Times[i + 1] - Times[i]);
                    float t = (normalized - Times[i]) / span;
                    return Pose.Lerp(Poses[i], Poses[i + 1], t);
                }
            }
            return Poses[Poses.Length - 1];
        }
    }

    /// <summary>
    /// Procedurally animated puppet. No imported rig, no Animator: each limb is a transform
    /// hierarchy and every pose is authored in code, which keeps the project self-contained.
    /// </summary>
    public sealed class ProceduralRig : MonoBehaviour
    {
        public Transform Root;
        public Transform Visual;
        public Transform Hips;
        public Transform Spine;
        public Transform Chest;
        public Transform Neck;
        public Transform Head;
        public Transform ShoulderL;
        public Transform UpperArmL;
        public Transform LowerArmL;
        public Transform HandL;
        public Transform ShoulderR;
        public Transform UpperArmR;
        public Transform LowerArmR;
        public Transform HandR;
        public Transform ThighL;
        public Transform ShinL;
        public Transform FootL;
        public Transform ThighR;
        public Transform ShinR;
        public Transform FootR;
        public Transform WeaponGripR;
        public Transform WeaponGripL;
        public Transform Cape;

        public WeaponStyle Style;
        public bool IsBoss;
        public float Height = 1.8f;
        public AttackClip[] Clips;

        readonly Dictionary<Transform, Quaternion> _bindRotation = new Dictionary<Transform, Quaternion>();
        readonly Dictionary<Transform, Vector3> _bindPosition = new Dictionary<Transform, Vector3>();
        Pose _pose;
        Pose _target;
        float _phase;
        float _speed;
        float _attackTimer = -1f;
        float _attackDuration = 0.5f;
        int _attackClip = -1;
        float _hitTimer;
        float _dodgeTimer = -1f;
        float _dodgeDuration = 0.32f;
        float _deathTimer = -1f;
        float _victoryTimer = -1f;
        float _bob;
        Vector3 _visualBase;
        Quaternion _visualBaseRot;
        float _attackWeight;

        public bool IsAttacking => _attackTimer >= 0f;
        public bool IsDead => _deathTimer >= 0f;
        public float AttackProgress => _attackDuration > 0f ? Mathf.Clamp01(_attackTimer / _attackDuration) : 0f;

        void Awake()
        {
            if (Visual != null)
            {
                _visualBase = Visual.localPosition;
                _visualBaseRot = Visual.localRotation;
            }
        }

        public void CaptureBindPose()
        {
            _bindRotation.Clear();
            _bindPosition.Clear();
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                _bindRotation[t] = t.localRotation;
                _bindPosition[t] = t.localPosition;
            }
            if (Visual != null)
            {
                _visualBase = Visual.localPosition;
                _visualBaseRot = Visual.localRotation;
            }
            _pose = Pose.Idle(Style);
            _target = _pose.Copy();
        }

        public void SetLocomotion(float normalizedSpeed)
        {
            _speed = Mathf.Clamp01(normalizedSpeed);
            _phase += Time.deltaTime * (5.2f + _speed * 7.5f);
        }

        public void PlayAttack(int clip, float duration)
        {
            if (Clips == null || Clips.Length == 0) return;
            _attackClip = Mathf.Clamp(clip, 0, Clips.Length - 1);
            _attackDuration = Mathf.Max(0.12f, duration);
            _attackTimer = 0f;
            _attackWeight = 0f;
        }

        public void CancelAttack()
        {
            _attackTimer = -1f;
            _attackWeight = 0f;
        }

        public void PlayHit()
        {
            _hitTimer = 0.22f;
        }

        public void PlayDodge(float duration)
        {
            _dodgeTimer = 0f;
            _dodgeDuration = Mathf.Max(0.15f, duration);
        }

        public void PlayVictory()
        {
            _victoryTimer = 0f;
        }

        public void Die()
        {
            if (_deathTimer >= 0f) return;
            _deathTimer = 0f;
            CancelAttack();
        }

        void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            if (Visual == null) return;

            if (_deathTimer >= 0f)
            {
                _deathTimer += dt;
                float fall = Mathf.Clamp01(_deathTimer / 0.55f);
                float ease = 1f - (1f - fall) * (1f - fall);
                Visual.localRotation = Quaternion.Slerp(_visualBaseRot, _visualBaseRot * Quaternion.Euler(-86f, 0f, 0f), ease);
                Visual.localPosition = _visualBase + Vector3.down * (0.12f * ease);
                Pose dead = Pose.Idle(Style);
                dead.Spine = new Vector3(22f, 0f, 0f);
                dead.Chest = new Vector3(12f, 0f, 0f);
                dead.Head = new Vector3(18f, 12f, 0f);
                dead.ShoulderL = new Vector3(-40f, 0f, -50f);
                dead.ElbowL = new Vector3(-12f, 0f, 0f);
                dead.ShoulderR = new Vector3(-30f, 0f, 48f);
                dead.ElbowR = new Vector3(-10f, 0f, 0f);
                dead.HipL = new Vector3(-20f, 0f, -12f);
                dead.KneeL = new Vector3(28f, 0f, 0f);
                dead.HipR = new Vector3(12f, 0f, 10f);
                dead.KneeR = new Vector3(20f, 0f, 0f);
                _target = dead;
                SmoothPose(6f, dt);
                return;
            }

            if (Visual.localRotation != _visualBaseRot)
                Visual.localRotation = Quaternion.Slerp(Visual.localRotation, _visualBaseRot, 1f - Mathf.Exp(-12f * dt));
            Visual.localPosition = _visualBase;

            Pose basePose = Pose.Idle(Style);
            float move = _speed;
            float swing = Mathf.Sin(_phase) * (22f + 14f * move) * move;
            float bounce = Mathf.Abs(Mathf.Sin(_phase)) * 0.055f * move;

            basePose.HipL += new Vector3(swing, 0f, 0f);
            basePose.HipR += new Vector3(-swing, 0f, 0f);
            basePose.KneeL += new Vector3(Mathf.Max(0f, -swing) * 0.9f, 0f, 0f);
            basePose.KneeR += new Vector3(Mathf.Max(0f, swing) * 0.9f, 0f, 0f);
            basePose.ShoulderL += new Vector3(swing * 0.55f, 0f, 0f);
            basePose.ShoulderR += new Vector3(-swing * (Style == WeaponStyle.DualBlades ? 0.5f : 0.28f), 0f, 0f);
            basePose.Chest += new Vector3(2f + move * 3.5f, Mathf.Sin(_phase) * 1.5f * move, 0f);
            basePose.Spine += new Vector3(move * 2.2f, -Mathf.Sin(_phase) * 1.8f * move, 0f);
            basePose.Head += new Vector3(-move * 2.5f, Mathf.Sin(_phase * 1.1f) * 2.2f * move, 0f);
            basePose.VisualOffset = new Vector3(0f, bounce, 0f);

            // Idle breathing.
            float breath = Mathf.Sin(Time.time * 1.55f);
            basePose.Chest += new Vector3(breath * 0.6f, 0f, 0f);
            basePose.ShoulderR += new Vector3(0f, 0f, breath * 0.8f);
            basePose.ShoulderL += new Vector3(0f, 0f, -breath * 0.8f);

            if (_dodgeTimer >= 0f)
            {
                _dodgeTimer += dt;
                float t = Mathf.Clamp01(_dodgeTimer / _dodgeDuration);
                float bend = Mathf.Sin(t * Mathf.PI);
                basePose.Chest += new Vector3(18f * bend, 0f, 0f);
                basePose.Spine += new Vector3(14f * bend, 0f, 0f);
                basePose.Head += new Vector3(-18f * bend, 0f, 0f);
                basePose.HipL += new Vector3(-24f * bend, 0f, 0f);
                basePose.HipR += new Vector3(24f * bend, 0f, 0f);
                basePose.KneeL += new Vector3(28f * bend, 0f, 0f);
                basePose.KneeR += new Vector3(28f * bend, 0f, 0f);
                basePose.WeaponR += new Vector3(-30f * bend, 0f, 0f);
                basePose.VisualOffset += new Vector3(0f, -0.10f * bend, 0f);
                if (_dodgeTimer >= _dodgeDuration)
                    _dodgeTimer = -1f;
            }

            if (_hitTimer > 0f)
            {
                _hitTimer -= dt;
                float hit = Mathf.Clamp01(_hitTimer / 0.22f);
                basePose.Chest += new Vector3(-10f * hit, 5f * hit, 0f);
                basePose.Head += new Vector3(-14f * hit, 9f * hit, 0f);
                basePose.ShoulderL += new Vector3(-6f * hit, 0f, -8f * hit);
                basePose.ShoulderR += new Vector3(-6f * hit, 0f, 8f * hit);
            }

            if (_victoryTimer >= 0f)
            {
                _victoryTimer += dt;
                float cheer = Mathf.Clamp01(_victoryTimer / 0.45f);
                basePose.ShoulderR = new Vector3(-150f * cheer, 0f, 30f);
                basePose.ElbowR = new Vector3(-20f, 0f, 0f);
                basePose.WeaponR = new Vector3(8f, 0f, 0f);
                basePose.Head = new Vector3(-16f * cheer, 0f, 0f);
                basePose.Chest = new Vector3(-8f * cheer, 0f, 0f);
            }

            if (_attackTimer >= 0f && _attackClip >= 0)
            {
                _attackTimer += dt;
                float normalized = Mathf.Clamp01(_attackTimer / _attackDuration);
                Pose clipPose = Clips[_attackClip].Sample(normalized);
                // Blend into the attack quickly, then hold the clip entirely.
                _attackWeight = Mathf.MoveTowards(_attackWeight, 1f, dt * 14f);
                _target = Pose.Lerp(basePose, clipPose, _attackWeight);
                if (_attackTimer >= _attackDuration)
                {
                    _attackTimer = -1f;
                    _attackWeight = 0f;
                }
            }
            else
            {
                _target = basePose;
            }

            SmoothPose(_attackTimer >= 0f ? 26f : 13f, dt);
            if (Visual != null)
                Visual.localPosition = _visualBase + _pose.VisualOffset;
        }

        void SmoothPose(float rate, float dt)
        {
            float a = 1f - Mathf.Exp(-rate * dt);
            _pose = Pose.Lerp(_pose, _target, a);
            Apply(Spine, _pose.Spine, a);
            Apply(Chest, _pose.Chest, a);
            Apply(Neck, _pose.Neck, a);
            Apply(Head, _pose.Head, a);
            Apply(ShoulderL, _pose.ShoulderL, a);
            Apply(LowerArmL, _pose.ElbowL, a);
            Apply(HandL, _pose.HandL, a);
            Apply(ShoulderR, _pose.ShoulderR, a);
            Apply(LowerArmR, _pose.ElbowR, a);
            Apply(HandR, _pose.HandR, a);
            Apply(ThighL, _pose.HipL, a);
            Apply(ShinL, _pose.KneeL, a);
            Apply(FootL, _pose.FootL, a);
            Apply(ThighR, _pose.HipR, a);
            Apply(ShinR, _pose.KneeR, a);
            Apply(FootR, _pose.FootR, a);
            Apply(WeaponGripR, _pose.WeaponR, a);
            Apply(WeaponGripL, _pose.WeaponL, a);
        }

        void Apply(Transform bone, Vector3 euler, float a)
        {
            if (bone == null) return;
            if (!_bindRotation.TryGetValue(bone, out Quaternion bind)) return;
            Quaternion target = bind * Quaternion.Euler(euler);
            bone.localRotation = Quaternion.Slerp(bone.localRotation, target, a);
        }

        public static AttackClip[] BuildClips(WeaponStyle style)
        {
            var clips = new AttackClip[3];
            Pose idle = Pose.Idle(style);

            // 0: forward thrust
            Pose thrustWind = idle.Copy();
            thrustWind.Chest = new Vector3(-4f, -34f, 0f);
            thrustWind.Spine = new Vector3(2f, -16f, 0f);
            thrustWind.ShoulderR = new Vector3(24f, -20f, 16f);
            thrustWind.ElbowR = new Vector3(-96f, 0f, 0f);
            thrustWind.ShoulderL = new Vector3(-16f, 0f, -22f);
            thrustWind.ElbowL = new Vector3(-38f, 0f, 0f);
            thrustWind.HipL = new Vector3(-16f, 0f, -8f);
            thrustWind.KneeL = new Vector3(16f, 0f, 0f);
            thrustWind.HipR = new Vector3(10f, 0f, 8f);
            thrustWind.KneeR = new Vector3(10f, 0f, 0f);
            thrustWind.WeaponR = new Vector3(42f, 0f, 0f);
            thrustWind.VisualOffset = new Vector3(0f, -0.04f, -0.07f);

            Pose thrustHit = idle.Copy();
            thrustHit.Chest = new Vector3(14f, 20f, 0f);
            thrustHit.Spine = new Vector3(8f, 10f, 0f);
            thrustHit.ShoulderR = new Vector3(-88f, 2f, 8f);
            thrustHit.ElbowR = new Vector3(-6f, 0f, 0f);
            thrustHit.ShoulderL = new Vector3(22f, 0f, -36f);
            thrustHit.ElbowL = new Vector3(-16f, 0f, 0f);
            thrustHit.HipL = new Vector3(-34f, 0f, -8f);
            thrustHit.KneeL = new Vector3(18f, 0f, 0f);
            thrustHit.HipR = new Vector3(24f, 0f, 8f);
            thrustHit.KneeR = new Vector3(24f, 0f, 0f);
            thrustHit.Head = new Vector3(6f, 0f, 0f);
            thrustHit.WeaponR = new Vector3(90f, 0f, 0f);
            thrustHit.VisualOffset = new Vector3(0f, -0.02f, 0.10f);

            Pose thrustRecover = idle.Copy();
            thrustRecover.Chest = new Vector3(8f, 6f, 0f);
            thrustRecover.ShoulderR = new Vector3(-48f, 0f, 12f);
            thrustRecover.ElbowR = new Vector3(-28f, 0f, 0f);
            thrustRecover.HipL = new Vector3(-16f, 0f, -8f);
            thrustRecover.KneeL = new Vector3(12f, 0f, 0f);
            thrustRecover.WeaponR = new Vector3(70f, 0f, 0f);

            clips[0] = new AttackClip
            {
                Name = "thrust",
                Poses = new[] { idle, thrustWind, thrustHit, thrustRecover, idle },
                Times = new[] { 0f, 0.28f, 0.52f, 0.8f, 1f }
            };

            // 1: horizontal slash
            Pose slashWind = idle.Copy();
            slashWind.Chest = new Vector3(-6f, -48f, -4f);
            slashWind.Spine = new Vector3(0f, -22f, 0f);
            slashWind.ShoulderR = new Vector3(-70f, -12f, 54f);
            slashWind.ElbowR = new Vector3(-24f, 0f, 0f);
            slashWind.ShoulderL = new Vector3(-34f, 0f, -36f);
            slashWind.ElbowL = new Vector3(-52f, 0f, 0f);
            slashWind.HipL = new Vector3(-10f, 0f, -10f);
            slashWind.KneeL = new Vector3(16f, 0f, 0f);
            slashWind.HipR = new Vector3(8f, 0f, 10f);
            slashWind.KneeR = new Vector3(10f, 0f, 0f);
            slashWind.WeaponR = new Vector3(76f, 0f, 56f);
            slashWind.WeaponL = new Vector3(150f, 0f, 38f);
            slashWind.VisualOffset = new Vector3(0f, -0.03f, -0.05f);

            Pose slashHit = idle.Copy();
            slashHit.Chest = new Vector3(8f, 46f, 5f);
            slashHit.Spine = new Vector3(4f, 24f, 0f);
            slashHit.ShoulderR = new Vector3(-82f, 22f, 10f);
            slashHit.ElbowR = new Vector3(-12f, 0f, 0f);
            slashHit.ShoulderL = new Vector3(18f, 0f, -40f);
            slashHit.ElbowL = new Vector3(-22f, 0f, 0f);
            slashHit.HipL = new Vector3(-26f, 0f, -8f);
            slashHit.KneeL = new Vector3(20f, 0f, 0f);
            slashHit.HipR = new Vector3(20f, 0f, 8f);
            slashHit.KneeR = new Vector3(22f, 0f, 0f);
            slashHit.Head = new Vector3(2f, 22f, 0f);
            slashHit.WeaponR = new Vector3(92f, 0f, -52f);
            slashHit.WeaponL = new Vector3(168f, 0f, -28f);
            slashHit.VisualOffset = new Vector3(0f, -0.01f, 0.10f);

            Pose slashRecover = idle.Copy();
            slashRecover.Chest = new Vector3(4f, 22f, 0f);
            slashRecover.ShoulderR = new Vector3(-36f, 0f, 20f);
            slashRecover.ElbowR = new Vector3(-30f, 0f, 0f);
            slashRecover.WeaponR = new Vector3(60f, 0f, -24f);
            slashRecover.WeaponL = new Vector3(158f, 0f, 0f);

            clips[1] = new AttackClip
            {
                Name = "slash",
                Poses = new[] { idle, slashWind, slashHit, slashRecover, idle },
                Times = new[] { 0f, 0.26f, 0.5f, 0.78f, 1f }
            };

            // 2: overhead heavy slam
            Pose heavyWind = idle.Copy();
            heavyWind.Chest = new Vector3(-18f, -8f, 0f);
            heavyWind.Spine = new Vector3(-8f, 0f, 0f);
            heavyWind.ShoulderR = new Vector3(154f, 0f, 28f);
            heavyWind.ElbowR = new Vector3(-28f, 0f, 0f);
            heavyWind.ShoulderL = new Vector3(130f, 0f, -34f);
            heavyWind.ElbowL = new Vector3(-34f, 0f, 0f);
            heavyWind.HipL = new Vector3(-12f, 0f, -10f);
            heavyWind.KneeL = new Vector3(14f, 0f, 0f);
            heavyWind.HipR = new Vector3(12f, 0f, 10f);
            heavyWind.KneeR = new Vector3(14f, 0f, 0f);
            heavyWind.Head = new Vector3(-18f, 0f, 0f);
            heavyWind.WeaponR = new Vector3(-46f, 0f, 0f);
            heavyWind.WeaponL = new Vector3(-46f, 0f, 0f);
            heavyWind.VisualOffset = new Vector3(0f, 0.02f, -0.04f);

            Pose heavyHit = idle.Copy();
            heavyHit.Chest = new Vector3(34f, 0f, 0f);
            heavyHit.Spine = new Vector3(20f, 0f, 0f);
            heavyHit.ShoulderR = new Vector3(-42f, 0f, 8f);
            heavyHit.ElbowR = new Vector3(-4f, 0f, 0f);
            heavyHit.ShoulderL = new Vector3(-42f, 0f, -8f);
            heavyHit.ElbowL = new Vector3(-4f, 0f, 0f);
            heavyHit.HipL = new Vector3(-42f, 0f, -16f);
            heavyHit.KneeL = new Vector3(48f, 0f, 0f);
            heavyHit.HipR = new Vector3(-36f, 0f, 16f);
            heavyHit.KneeR = new Vector3(52f, 0f, 0f);
            heavyHit.Head = new Vector3(24f, 0f, 0f);
            heavyHit.WeaponR = new Vector3(138f, 0f, 0f);
            heavyHit.WeaponL = new Vector3(138f, 0f, 0f);
            heavyHit.VisualOffset = new Vector3(0f, -0.12f, 0.08f);

            clips[2] = new AttackClip
            {
                Name = "heavy",
                Poses = new[] { idle, heavyWind, heavyHit, idle },
                Times = new[] { 0f, 0.4f, 0.58f, 1f }
            };
            return clips;
        }
    }
}
