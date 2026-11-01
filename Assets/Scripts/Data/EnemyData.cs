using UnityEngine;

namespace Liangshan
{
    public enum EnemyType
    {
        Blade,
        Spear,
        Shield,
        Archer,
        Captain,
        Boss
    }

    public sealed class EnemyDefinition
    {
        public EnemyType Type;
        public string Name;
        public string Title;
        public float Health;
        public float Damage;
        public float Speed;
        public float AttackRange;
        public float Windup;
        public float Cooldown;
        public float Scale;
        public WeaponStyle Weapon;
        public Color Primary;
        public Color Secondary;
        public Color Cloth;
        public Color Trim;
        public bool Ranged;
        public bool Shielded;
        public bool IsBoss;

        public static EnemyDefinition Get(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.Spear:
                    return new EnemyDefinition
                    {
                        Type = type, Name = "枪兵", Title = "高俅府枪手",
                        Health = 66f, Damage = 11f, Speed = 3.15f, AttackRange = 2.6f,
                        Windup = 0.62f, Cooldown = 1.35f, Scale = 0.98f, Weapon = WeaponStyle.Spear,
                        Primary = GameConfig.Hex("3D4A3E"), Secondary = GameConfig.Hex("222B25"),
                        Cloth = GameConfig.Hex("5C5545"), Trim = GameConfig.Hex("9A7A44")
                    };
                case EnemyType.Shield:
                    return new EnemyDefinition
                    {
                        Type = type, Name = "盾兵", Title = "殿前司刀盾手",
                        Health = 142f, Damage = 13f, Speed = 2.55f, AttackRange = 1.95f,
                        Windup = 0.82f, Cooldown = 1.7f, Scale = 1.04f, Weapon = WeaponStyle.Shield,
                        Primary = GameConfig.Hex("4C4F52"), Secondary = GameConfig.Hex("282A2C"),
                        Cloth = GameConfig.Hex("5E4A38"), Trim = GameConfig.Hex("B08A4E"),
                        Shielded = true
                    };
                case EnemyType.Archer:
                    return new EnemyDefinition
                    {
                        Type = type, Name = "弓手", Title = "禁军神射手",
                        Health = 48f, Damage = 8f, Speed = 3.3f, AttackRange = 10.5f,
                        Windup = 0.9f, Cooldown = 2.15f, Scale = 0.95f, Weapon = WeaponStyle.Bow,
                        Primary = GameConfig.Hex("544936"), Secondary = GameConfig.Hex("2B261D"),
                        Cloth = GameConfig.Hex("6B6249"), Trim = GameConfig.Hex("A88448"),
                        Ranged = true
                    };
                case EnemyType.Captain:
                    return new EnemyDefinition
                    {
                        Type = type, Name = "教头", Title = "殿前司都教头",
                        Health = 330f, Damage = 20f, Speed = 2.95f, AttackRange = 2.7f,
                        Windup = 0.85f, Cooldown = 1.75f, Scale = 1.16f, Weapon = WeaponStyle.Halberd,
                        Primary = GameConfig.Hex("5C2C2A"), Secondary = GameConfig.Hex("2A1B1A"),
                        Cloth = GameConfig.Hex("3E3430"), Trim = GameConfig.Hex("C6A15B")
                    };
                case EnemyType.Boss:
                    return new EnemyDefinition
                    {
                        Type = type, Name = "高俅", Title = "殿帅府太尉",
                        Health = 1650f, Damage = 28f, Speed = 3.35f, AttackRange = 3.6f,
                        Windup = 0.95f, Cooldown = 1.55f, Scale = 1.58f, Weapon = WeaponStyle.Halberd,
                        Primary = GameConfig.Hex("3A2030"), Secondary = GameConfig.Hex("160F16"),
                        Cloth = GameConfig.Hex("22202A"), Trim = GameConfig.Hex("D9B45F"),
                        IsBoss = true
                    };
                default:
                    return new EnemyDefinition
                    {
                        Type = EnemyType.Blade, Name = "刀兵", Title = "高俅府家丁",
                        Health = 82f, Damage = 10f, Speed = 3.45f, AttackRange = 1.9f,
                        Windup = 0.55f, Cooldown = 1.15f, Scale = 1f, Weapon = WeaponStyle.Dao,
                        Primary = GameConfig.Hex("4B3B31"), Secondary = GameConfig.Hex("251D19"),
                        Cloth = GameConfig.Hex("6A4A34"), Trim = GameConfig.Hex("9E7A4A")
                    };
            }
        }
    }
}
