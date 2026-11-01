using System.Collections.Generic;
using UnityEngine;

namespace Liangshan
{
    public enum WeaponStyle
    {
        Spear,
        DualBlades,
        Staff,
        Dao,
        Shield,
        Bow,
        Halberd
    }

    public sealed class HeroDefinition
    {
        public string Id;
        public string Name;
        public string Nickname;
        public string Star;
        public string WeaponName;
        public string Role;
        public string SkillName;
        public string SkillDesc;
        public string UltimateName;
        public string UltimateDesc;
        public string Lore;
        public string Quote;
        public Color Primary;
        public Color Secondary;
        public Color Cloth;
        public Color Trim;
        public WeaponStyle Weapon;
        public float MaxHealth;
        public float Damage;
        public float MoveSpeed;
        public float AttackRange;
        public float AttackDuration;
        public int ComboCount;
        public float SkillCooldown;
        public int Seat;
    }

    public static class HeroDatabase
    {
        public static readonly List<HeroDefinition> Playable = new List<HeroDefinition>
        {
            new HeroDefinition
            {
                Id = "linchong",
                Name = "林冲",
                Nickname = "豹子头",
                Star = "天雄星",
                WeaponName = "丈八蛇矛",
                Role = "枪术宗师 · 中距突刺",
                SkillName = "寒星点点",
                SkillDesc = "连刺三枪，枪尖如寒星，贯穿身前敌阵。",
                UltimateName = "风雪山神庙",
                UltimateDesc = "风雪骤起，长枪横扫八方，冻彻梁山。",
                Lore = "原东京八十万禁军枪棒教头，遭高俅陷害，雪夜上梁山。一杆蛇矛，枪出如龙。",
                Quote = "风雪压我两三年，我笑风轻雪如棉。",
                Primary = GameConfig.Hex("4F8FA0"),
                Secondary = GameConfig.Hex("24404A"),
                Cloth = GameConfig.Hex("E7DFD0"),
                Trim = GameConfig.Hex("C9A45C"),
                Weapon = WeaponStyle.Spear,
                MaxHealth = 200f,
                Damage = 24f,
                MoveSpeed = 6.2f,
                AttackRange = 3.8f,
                AttackDuration = 0.47f,
                ComboCount = 4,
                SkillCooldown = 7f,
                Seat = 6
            },
            new HeroDefinition
            {
                Id = "wusong",
                Name = "武松",
                Nickname = "行者",
                Star = "天伤星",
                WeaponName = "鸳鸯双刀",
                Role = "近身快攻 · 连斩爆发",
                SkillName = "玉环步·鸳鸯脚",
                SkillDesc = "飞身踢击，破阵而入，踹翻沿途敌兵。",
                UltimateName = "血溅鸳鸯楼",
                UltimateDesc = "双刀化作风暴，一瞬斩尽周身仇敌。",
                Lore = "景阳冈打虎，狮子楼除奸，血溅鸳鸯楼。行者武松，快意恩仇，双刀之下不留行。",
                Quote = "平生只打不平事，管他天高地厚。",
                Primary = GameConfig.Hex("B23A36"),
                Secondary = GameConfig.Hex("2B2226"),
                Cloth = GameConfig.Hex("423A3C"),
                Trim = GameConfig.Hex("D0A85C"),
                Weapon = WeaponStyle.DualBlades,
                MaxHealth = 180f,
                Damage = 18f,
                MoveSpeed = 6.9f,
                AttackRange = 2.8f,
                AttackDuration = 0.33f,
                ComboCount = 5,
                SkillCooldown = 6f,
                Seat = 14
            },
            new HeroDefinition
            {
                Id = "luzhishen",
                Name = "鲁智深",
                Nickname = "花和尚",
                Star = "天孤星",
                WeaponName = "水磨禅杖",
                Role = "重击破阵 · 范围压制",
                SkillName = "倒拔垂杨柳",
                SkillDesc = "禅杖顿地，气浪裂石，将周围敌兵掀翻。",
                UltimateName = "钱塘潮信",
                UltimateDesc = "禅杖三震，潮信如雷，层层气浪撼动山门。",
                Lore = "拳打镇关西，倒拔垂杨柳，大相国寺倒拔垂杨。花和尚鲁智深，酒肉穿肠，佛在心头。",
                Quote = "洒家平生，只好打抱不平。",
                Primary = GameConfig.Hex("B07A3E"),
                Secondary = GameConfig.Hex("4E3A28"),
                Cloth = GameConfig.Hex("6B5138"),
                Trim = GameConfig.Hex("B8914E"),
                Weapon = WeaponStyle.Staff,
                MaxHealth = 240f,
                Damage = 34f,
                MoveSpeed = 5.6f,
                AttackRange = 3.4f,
                AttackDuration = 0.66f,
                ComboCount = 3,
                SkillCooldown = 9f,
                Seat = 13
            }
        };

        public static HeroDefinition Get(string id)
        {
            for (int i = 0; i < Playable.Count; i++)
                if (Playable[i].Id == id)
                    return Playable[i];
            return Playable[0];
        }

        public static HeroDefinition GetBySeat(int seat)
        {
            for (int i = 0; i < Playable.Count; i++)
                if (Playable[i].Seat == seat)
                    return Playable[i];
            return null;
        }
    }
}
