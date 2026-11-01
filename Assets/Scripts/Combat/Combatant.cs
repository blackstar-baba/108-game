using System;
using UnityEngine;

namespace Liangshan
{
    public struct DamageInfo
    {
        public float Amount;
        public Vector3 Point;
        public Vector3 Direction;
        public Combatant Source;
        public bool Heavy;
        public bool Critical;
    }

    public sealed class Combatant : MonoBehaviour
    {
        public Team Team;
        public string DisplayName;
        public float MaxHealth = 100f;
        public float Health = 100f;
        public bool IsDead;
        public bool Shielded;
        public bool IsBoss;
        public float DamageReduction;
        public float InvulnerableUntil;
        public ProceduralRig Rig;
        public HitFlash Flash;

        public event Action<Combatant, DamageInfo> Damaged;
        public event Action<Combatant> Died;

        public float Health01 => MaxHealth > 0f ? Mathf.Clamp01(Health / MaxHealth) : 0f;
        public bool Invulnerable => Time.time < InvulnerableUntil;

        public void Initialize(Team team, string displayName, float maxHealth, ProceduralRig rig)
        {
            Team = team;
            DisplayName = displayName;
            MaxHealth = maxHealth;
            Health = maxHealth;
            Rig = rig;
            Flash = GetComponent<HitFlash>();
            if (Flash == null)
                Flash = gameObject.AddComponent<HitFlash>();
            Flash.Refresh();
        }

        public void SetHealth(float value, bool heal)
        {
            if (IsDead) return;
            MaxHealth = Mathf.Max(1f, MaxHealth);
            Health = Mathf.Clamp(value, 0f, MaxHealth);
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            Health = Mathf.Min(MaxHealth, Health + Mathf.Max(0f, amount));
        }

        public void GrantInvulnerability(float duration)
        {
            InvulnerableUntil = Mathf.Max(InvulnerableUntil, Time.time + duration);
        }

        public bool TakeDamage(DamageInfo info)
        {
            if (IsDead || info.Amount <= 0f || Invulnerable) return false;

            float amount = info.Amount * (1f - Mathf.Clamp01(DamageReduction));
            if (Shielded && info.Source != null)
            {
                Vector3 toAttacker = info.Source.transform.position - transform.position;
                toAttacker.y = 0f;
                if (toAttacker.sqrMagnitude > 0.0001f && Vector3.Dot(transform.forward, toAttacker.normalized) > 0.12f)
                    amount *= info.Heavy ? 0.62f : 0.28f;
            }

            Health -= amount;
            if (Flash != null)
                Flash.Flash(info.Heavy ? 0.16f : 0.09f);
            Damaged?.Invoke(this, info);
            if (Health <= 0f)
            {
                Health = 0f;
                Die();
            }
            return true;
        }

        void Die()
        {
            if (IsDead) return;
            IsDead = true;
            if (Rig != null)
                Rig.Die();
            Died?.Invoke(this);
        }
    }
}
