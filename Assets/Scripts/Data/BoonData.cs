using System;
using System.Collections.Generic;
using UnityEngine;

namespace Liangshan
{
    public sealed class PlayerBoons
    {
        public float DamageMult = 1f;
        public float HealthMult = 1f;
        public float SpeedMult = 1f;
        public float AttackSpeedMult = 1f;
        public float RangeMult = 1f;
        public float UltimateGainMult = 1f;
        public float DodgeCooldownMult = 1f;
        public float Lifesteal;
        public float CritChance = 0.06f;
        public float CritMult = 1.65f;
        public float DamageReduction;
        public int ExtraCombo;
    }

    public sealed class BoonDefinition
    {
        public string Id;
        public string Name;
        public string Title;
        public string Description;
        public string Flavor;
        public Color Accent;
        public Action<PlayerBoons> Apply;

        public static readonly List<BoonDefinition> All = new List<BoonDefinition>
        {
            new BoonDefinition
            {
                Id = "leopard", Name = "豹子胆", Title = "天雄星余威",
                Description = "伤害提高 18%", Flavor = "枪尖所向，肝胆俱寒。",
                Accent = GameConfig.Hex("4F8FA0"),
                Apply = b => b.DamageMult *= 1.18f
            },
            new BoonDefinition
            {
                Id = "iron", Name = "铁布衫", Title = "罗汉金身",
                Description = "最大生命提高 30%，并立刻恢复 30% 生命",
                Flavor = "任你刀斧加身，我自岿然不动。", Accent = GameConfig.Hex("B08A4E"),
                Apply = b => b.HealthMult *= 1.30f
            },
            new BoonDefinition
            {
                Id = "wind", Name = "神行太保", Title = "踏雪无痕",
                Description = "移动速度提高 14%，闪避冷却缩短 18%",
                Flavor = "日行八百，踏雪无痕。", Accent = GameConfig.Hex("7FA9A0"),
                Apply = b => { b.SpeedMult *= 1.14f; b.DodgeCooldownMult *= 0.82f; }
            },
            new BoonDefinition
            {
                Id = "storm", Name = "疾风骤雨", Title = "快枪九式",
                Description = "攻击速度提高 20%", Flavor = "枪出如雨，密不透风。",
                Accent = GameConfig.Hex("C7A15B"),
                Apply = b => b.AttackSpeedMult *= 1.20f
            },
            new BoonDefinition
            {
                Id = "blood", Name = "血溅鸳鸯楼", Title = "以血还血",
                Description = "造成伤害的 9% 转化为生命", Flavor = "仇人的血，最是解渴。",
                Accent = GameConfig.Hex("A83232"),
                Apply = b => b.Lifesteal += 0.09f
            },
            new BoonDefinition
            {
                Id = "crit", Name = "破甲", Title = "枪挑连环",
                Description = "暴击率提高 18%，暴击伤害提高 20%",
                Flavor = "甲胄再厚，也挡不住这一枪。", Accent = GameConfig.Hex("D06A4A"),
                Apply = b => { b.CritChance += 0.18f; b.CritMult += 0.20f; }
            },
            new BoonDefinition
            {
                Id = "rage", Name = "怒发冲冠", Title = "替天行道",
                Description = "怒气获取提高 40%", Flavor = "不平之气，直冲斗牛。",
                Accent = GameConfig.Hex("C04532"),
                Apply = b => b.UltimateGainMult *= 1.40f
            },
            new BoonDefinition
            {
                Id = "monk", Name = "花和尚", Title = "酒肉穿肠",
                Description = "受到的伤害降低 12%，最大生命提高 10%",
                Flavor = "酒肉穿肠过，佛祖心中留。", Accent = GameConfig.Hex("B07A3E"),
                Apply = b => { b.DamageReduction += 0.12f; b.HealthMult *= 1.10f; }
            },
            new BoonDefinition
            {
                Id = "reach", Name = "丈八蛇矛", Title = "一寸长一寸强",
                Description = "攻击距离提高 16%，范围伤害略微扩大",
                Flavor = "蛇矛在手，丈八之内皆是死地。", Accent = GameConfig.Hex("718C7B"),
                Apply = b => b.RangeMult *= 1.16f
            },
            new BoonDefinition
            {
                Id = "combo", Name = "连环", Title = "拳脚无影",
                Description = "连招上限 +1，连招伤害额外提高", Flavor = "一招未老，一招又至。",
                Accent = GameConfig.Hex("C8B27A"),
                Apply = b => b.ExtraCombo += 1
            },
            new BoonDefinition
            {
                Id = "tiger", Name = "打虎", Title = "景阳冈上",
                Description = "生命低于 40% 时，伤害提高 25%", Flavor = "景阳冈上，三拳打死吊睛白额虎。",
                Accent = GameConfig.Hex("C98A45"),
                Apply = b => b.DamageMult *= 1.06f
            },
            new BoonDefinition
            {
                Id = "snow", Name = "风雪山神", Title = "雪夜上梁山",
                Description = "闪避后 2 秒内，下一次攻击必定暴击",
                Flavor = "那夜风雪很大，回不去的，便不回了。", Accent = GameConfig.Hex("9FB6C4"),
                Apply = b => b.CritChance += 0.10f
            }
        };

        public static List<BoonDefinition> Roll(int count, System.Random random)
        {
            var pool = new List<BoonDefinition>(All);
            var result = new List<BoonDefinition>();
            count = Mathf.Clamp(count, 1, pool.Count);
            for (int i = 0; i < count; i++)
            {
                int index = random.Next(0, pool.Count);
                result.Add(pool[index]);
                pool.RemoveAt(index);
            }
            return result;
        }
    }
}
