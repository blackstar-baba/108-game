using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Liangshan.Tests
{
    public sealed class LiangshanDataTests
    {
        [Test]
        public void CodexContainsAllOneHundredAndEightHeroes()
        {
            Assert.AreEqual(108, Heroes108.All.Count, "The codex must contain the full 108-star roster.");
            CollectionAssert.AreEquivalent(Enumerable.Range(1, 108).ToList(),
                Heroes108.All.Select(h => h.Rank).ToList(), "Every rank must appear exactly once.");
        }

        [Test]
        public void CodexNamesAndStarsAreComplete()
        {
            foreach (var hero in Heroes108.All)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(hero.Name), "Missing name at rank " + hero.Rank);
                Assert.IsFalse(string.IsNullOrWhiteSpace(hero.Nickname), "Missing nickname at rank " + hero.Rank);
                Assert.IsTrue(hero.Star.Contains("星"), "Rank " + hero.Rank + " has an invalid star name.");
            }
        }

        [Test]
        public void ThreePlayableHeroesHaveDistinctRoles()
        {
            Assert.AreEqual(3, HeroDatabase.Playable.Count);
            var ids = new HashSet<string>();
            foreach (var hero in HeroDatabase.Playable)
            {
                Assert.IsTrue(ids.Add(hero.Id), "Duplicate playable hero id: " + hero.Id);
                Assert.Greater(hero.MaxHealth, 0f);
                Assert.Greater(hero.Damage, 0f);
                Assert.Greater(hero.ComboCount, 1);
                Assert.IsFalse(string.IsNullOrWhiteSpace(hero.SkillName));
                Assert.IsFalse(string.IsNullOrWhiteSpace(hero.UltimateName));
            }
        }

        [Test]
        public void BoonRollsAreUniqueAndApplyable()
        {
            var random = new System.Random(108);
            for (int i = 0; i < 42; i++)
            {
                var roll = BoonDefinition.Roll(3, random);
                Assert.AreEqual(3, roll.Count);
                Assert.AreEqual(3, roll.Select(b => b.Id).Distinct().Count(), "Boon roll contained duplicates.");
                var boons = new PlayerBoons();
                foreach (var boon in roll)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(boon.Name));
                    Assert.IsFalse(string.IsNullOrWhiteSpace(boon.Description));
                    boon.Apply(boons);
                }
            }
        }

        [Test]
        public void ArenaConstantsStayInsideTheIsland()
        {
            Assert.Less(GameConfig.ArenaRadius, GameConfig.IslandRadius);
            Assert.Less(GameConfig.PlayerBoundary, GameConfig.IslandRadius);
            Assert.Greater(GameConfig.PlayerSpawnZ, 0f);
            Assert.AreEqual(5, GameConfig.TotalWaves);
        }

        [Test]
        public void PaletteParsesHexColors()
        {
            Assert.AreEqual(new Color32(184, 65, 50, 255), (Color32)GameConfig.Hex("B84132"));
            Assert.AreEqual(Color.white, GameConfig.Hex("FFFFFF"));
        }
    }
}
