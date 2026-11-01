using System.Collections.Generic;
using UnityEngine;

namespace Liangshan
{
    /// <summary>Builds the low-poly, ink-toon cast entirely from primitives and generated meshes.</summary>
    public static class CharacterFactory
    {
        static readonly Dictionary<string, Material> MaterialCache = new Dictionary<string, Material>();
        static Material _vertexMaterial;
        static Material _capeMaterial;

        public static ProceduralRig BuildHero(Transform parent, HeroDefinition hero, Vector3 position, Quaternion rotation)
        {
            var root = WorldFactory.Empty("Hero " + hero.Name, parent, position, rotation);
            var rig = root.AddComponent<ProceduralRig>();
            rig.Root = root.transform;
            rig.Style = hero.Weapon;
            rig.Clips = ProceduralRig.BuildClips(hero.Weapon);
            rig.Visual = WorldFactory.Empty("Visual", root.transform, Vector3.zero).transform;

            Color skin = GameConfig.Hex("D0A37A");
            Color hair = GameConfig.Hex("1C1A19");
            var body = BuildBody(rig, hero.Primary, hero.Secondary, hero.Cloth, hero.Trim, skin, hair, false);
            BuildFace(body.Head, skin, hair, hero.Id == "luzhishen");

            switch (hero.Id)
            {
                case "linchong":
                    BuildLinchongHead(body.Head, hero, skin, hair);
                    BuildCape(body.Chest, GameConfig.Hex("D9D2C3"), hero.Primary, 1.05f, 0.58f);
                    break;
                case "wusong":
                    BuildWusongHead(body.Head, hero, skin, hair);
                    AddTigerPelt(body.Chest, hero);
                    break;
                case "luzhishen":
                    BuildLuzhishenHead(body.Head, body.Chest, hero, skin, hair);
                    BuildRobeSkirt(body.Hips, hero.Cloth, hero.Trim);
                    break;
            }

            BuildWeapon(rig, hero.Weapon, hero.Trim);
            rig.Height = 1.9f;
            rig.CaptureBindPose();

            var flash = root.AddComponent<HitFlash>();
            flash.Refresh();
            return rig;
        }

        public static ProceduralRig BuildEnemy(Transform parent, EnemyDefinition def, Vector3 position, Quaternion rotation)
        {
            var root = WorldFactory.Empty("Enemy " + def.Name, parent, position, rotation);
            var rig = root.AddComponent<ProceduralRig>();
            rig.Root = root.transform;
            rig.Style = def.Weapon;
            rig.IsBoss = def.IsBoss;
            rig.Clips = ProceduralRig.BuildClips(def.Weapon);
            rig.Visual = WorldFactory.Empty("Visual", root.transform, Vector3.zero).transform;

            Color skin = GameConfig.Hex("B98A64");
            Color hair = GameConfig.Hex("171615");
            var body = BuildBody(rig, def.Primary, def.Secondary, def.Cloth, def.Trim, skin, hair, def.IsBoss || def.Type == EnemyType.Captain);
            BuildFace(body.Head, skin, hair, false);

            switch (def.Type)
            {
                case EnemyType.Blade:
                    BuildBladeHead(body.Head, def);
                    break;
                case EnemyType.Spear:
                    BuildSpearHead(body.Head, def);
                    break;
                case EnemyType.Shield:
                    BuildShieldHead(body.Head, def);
                    break;
                case EnemyType.Archer:
                    BuildArcherHead(body.Head, def);
                    break;
                case EnemyType.Captain:
                    BuildCaptainHead(body.Head, def);
                    break;
                case EnemyType.Boss:
                    BuildBossHead(body.Head, def);
                    BuildCape(body.Chest, GameConfig.Hex("241C26"), def.Trim, 1.45f, 0.78f);
                    BuildBossPauldrons(body.Chest, def);
                    break;
            }

            BuildWeapon(rig, def.Weapon, def.Trim);
            if (def.Type == EnemyType.Archer)
                BuildQuiver(body.Chest, def);

            rig.Height = 1.9f * def.Scale;
            root.transform.localScale = Vector3.one * def.Scale;
            rig.CaptureBindPose();

            var flash = root.AddComponent<HitFlash>();
            flash.Refresh();
            return rig;
        }

        sealed class BodyRefs
        {
            public Transform Hips, Spine, Chest, Neck, Head;
            public Transform ShoulderL, UpperArmL, LowerArmL, HandL;
            public Transform ShoulderR, UpperArmR, LowerArmR, HandR;
            public Transform ThighL, ShinL, FootL, ThighR, ShinR, FootR;
        }

        static BodyRefs BuildBody(ProceduralRig rig, Color primary, Color secondary, Color cloth, Color trim,
            Color skin, Color hair, bool bulky)
        {
            Material mCloth = Mat(cloth);
            Material mPrimary = Mat(primary);
            Material mSecondary = Mat(secondary);
            Material mTrim = Mat(trim);
            Material mSkin = Mat(skin);
            Material mLeather = Mat(GameConfig.Hex("3C2C22"));
            Material mMetal = Mat(GameConfig.Hex("8A9195"));

            Transform visual = rig.Visual;
            var refs = new BodyRefs();
            refs.Hips = Bone("Hips", visual, new Vector3(0f, 0.92f, 0f)).transform;
            refs.Spine = Bone("Spine", refs.Hips, new Vector3(0f, 0.12f, 0f)).transform;
            refs.Chest = Bone("Chest", refs.Spine, new Vector3(0f, 0.24f, 0f)).transform;
            refs.Neck = Bone("Neck", refs.Chest, new Vector3(0f, 0.24f, 0f)).transform;
            refs.Head = Bone("Head", refs.Neck, new Vector3(0f, 0.07f, 0f)).transform;

            rig.Hips = refs.Hips;
            rig.Spine = refs.Spine;
            rig.Chest = refs.Chest;
            rig.Neck = refs.Neck;
            rig.Head = refs.Head;

            // Pelvis, belt and sash.
            Part("pelvis", refs.Hips, PrimitiveType.Cylinder, new Vector3(0f, 0.01f, 0f), new Vector3(0.20f, 0.075f, 0.16f), mSecondary);
            Part("belt", refs.Hips, PrimitiveType.Cylinder, new Vector3(0f, 0.075f, 0f), new Vector3(0.225f, 0.045f, 0.185f), mTrim);
            Part("belt knot", refs.Hips, PrimitiveType.Sphere, new Vector3(0f, 0.065f, -0.19f), new Vector3(0.10f, 0.08f, 0.06f), mTrim);

            // Side cloth panels and a back panel: the front stays open so the legs read in motion.
            for (int i = 0; i < 2; i++)
            {
                float sx = i == 0 ? -1f : 1f;
                var panel = Part("sash panel", refs.Hips, PrimitiveType.Cube, new Vector3(sx * 0.20f, -0.22f, 0.03f),
                    new Vector3(0.11f, 0.46f, 0.28f), mCloth, Quaternion.Euler(0f, 0f, sx * 7f));
                Part("sash trim", refs.Hips, PrimitiveType.Cube, new Vector3(sx * 0.20f, -0.44f, 0.03f),
                    new Vector3(0.115f, 0.035f, 0.285f), mTrim, Quaternion.Euler(0f, 0f, sx * 7f));
            }
            Part("back skirt", refs.Hips, PrimitiveType.Cube, new Vector3(0f, -0.24f, -0.15f),
                new Vector3(0.34f, 0.48f, 0.05f), mCloth, Quaternion.Euler(-7f, 0f, 0f));
            Part("back skirt trim", refs.Hips, PrimitiveType.Cube, new Vector3(0f, -0.46f, -0.17f),
                new Vector3(0.345f, 0.035f, 0.055f), mTrim, Quaternion.Euler(-7f, 0f, 0f));

            // Torso: an octagonal tapered breastplate over a cloth barrel.
            TaperPart("torso cloth", refs.Chest, new Vector3(0f, -0.17f, 0f), 0.185f, 0.245f, 0.46f, 8,
                cloth * 0.78f, cloth, mCloth, new Vector3(1f, 1f, 0.82f));
            TaperPart("breastplate", refs.Chest, new Vector3(0f, -0.10f, 0.025f), 0.20f, 0.265f, 0.31f, 8,
                primary * 0.75f, primary, mPrimary, new Vector3(1.04f, 1.03f, 1.14f));
            Part("chest trim", refs.Chest, PrimitiveType.Cube, new Vector3(0f, 0.04f, 0.225f),
                new Vector3(0.36f, 0.035f, 0.035f), mTrim);
            Part("chest emblem", refs.Chest, PrimitiveType.Cube, new Vector3(0f, -0.10f, 0.245f),
                new Vector3(0.13f, 0.13f, 0.02f), mTrim, Quaternion.Euler(0f, 0f, 45f));
            Part("abdomen", refs.Spine, PrimitiveType.Cylinder, new Vector3(0f, 0.04f, 0f),
                new Vector3(0.185f, 0.10f, 0.15f), mCloth);

            // Shoulder pauldrons.
            for (int i = 0; i < 2; i++)
            {
                float sx = i == 0 ? -1f : 1f;
                Part("pauldron", refs.Chest, PrimitiveType.Cube, new Vector3(sx * 0.28f, 0.15f, 0f),
                    new Vector3(bulky ? 0.24f : 0.19f, 0.10f, 0.28f), mSecondary, Quaternion.Euler(0f, 0f, sx * -12f));
                Part("pauldron trim", refs.Chest, PrimitiveType.Cube, new Vector3(sx * 0.29f, 0.10f, 0f),
                    new Vector3(bulky ? 0.25f : 0.20f, 0.025f, 0.29f), mTrim, Quaternion.Euler(0f, 0f, sx * -12f));
            }

            // Neck.
            Part("neck", refs.Neck, PrimitiveType.Cylinder, new Vector3(0f, 0.015f, 0f), new Vector3(0.095f, 0.065f, 0.095f), mSkin);
            Part("collar", refs.Chest, PrimitiveType.Cylinder, new Vector3(0f, 0.205f, 0f), new Vector3(0.16f, 0.035f, 0.13f), mTrim);

            // Arms.
            refs.ShoulderL = Bone("ShoulderL", refs.Chest, new Vector3(-0.26f, 0.16f, 0f)).transform;
            refs.UpperArmL = Bone("UpperArmL", refs.ShoulderL, Vector3.zero).transform;
            refs.LowerArmL = Bone("LowerArmL", refs.UpperArmL, new Vector3(0f, -0.28f, 0f)).transform;
            refs.HandL = Bone("HandL", refs.LowerArmL, new Vector3(0f, -0.26f, 0f)).transform;
            refs.ShoulderR = Bone("ShoulderR", refs.Chest, new Vector3(0.26f, 0.16f, 0f)).transform;
            refs.UpperArmR = Bone("UpperArmR", refs.ShoulderR, Vector3.zero).transform;
            refs.LowerArmR = Bone("LowerArmR", refs.UpperArmR, new Vector3(0f, -0.28f, 0f)).transform;
            refs.HandR = Bone("HandR", refs.LowerArmR, new Vector3(0f, -0.26f, 0f)).transform;

            rig.ShoulderL = refs.ShoulderL;
            rig.UpperArmL = refs.UpperArmL;
            rig.LowerArmL = refs.LowerArmL;
            rig.HandL = refs.HandL;
            rig.ShoulderR = refs.ShoulderR;
            rig.UpperArmR = refs.UpperArmR;
            rig.LowerArmR = refs.LowerArmR;
            rig.HandR = refs.HandR;

            BuildArm(refs.UpperArmL, refs.LowerArmL, refs.HandL, mCloth, mSecondary, mSkin, mTrim, -1f);
            BuildArm(refs.UpperArmR, refs.LowerArmR, refs.HandR, mCloth, mSecondary, mSkin, mTrim, 1f);

            // Legs.
            refs.ThighL = Bone("ThighL", refs.Hips, new Vector3(-0.12f, -0.02f, 0f)).transform;
            refs.ShinL = Bone("ShinL", refs.ThighL, new Vector3(0f, -0.42f, 0f)).transform;
            refs.FootL = Bone("FootL", refs.ShinL, new Vector3(0f, -0.40f, 0f)).transform;
            refs.ThighR = Bone("ThighR", refs.Hips, new Vector3(0.12f, -0.02f, 0f)).transform;
            refs.ShinR = Bone("ShinR", refs.ThighR, new Vector3(0f, -0.42f, 0f)).transform;
            refs.FootR = Bone("FootR", refs.ShinR, new Vector3(0f, -0.40f, 0f)).transform;

            rig.ThighL = refs.ThighL;
            rig.ShinL = refs.ShinL;
            rig.FootL = refs.FootL;
            rig.ThighR = refs.ThighR;
            rig.ShinR = refs.ShinR;
            rig.FootR = refs.FootR;

            BuildLeg(refs.ThighL, refs.ShinL, refs.FootL, mSecondary, mCloth, mLeather, mTrim);
            BuildLeg(refs.ThighR, refs.ShinR, refs.FootR, mSecondary, mCloth, mLeather, mTrim);

            return refs;
        }

        static void BuildArm(Transform upper, Transform lower, Transform hand, Material cloth, Material bracer,
            Material skin, Material trim, float side)
        {
            Part("shoulder", upper, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.15f, 0.14f, 0.15f), bracer);
            Part("upper sleeve", upper, PrimitiveType.Cylinder, new Vector3(0f, -0.14f, 0f),
                new Vector3(0.073f, 0.15f, 0.073f), cloth);
            Part("arm wrap", lower, PrimitiveType.Cylinder, new Vector3(0f, -0.05f, 0f),
                new Vector3(0.068f, 0.055f, 0.068f), trim);
            Part("forearm", lower, PrimitiveType.Cylinder, new Vector3(0f, -0.14f, 0f),
                new Vector3(0.062f, 0.13f, 0.062f), bracer);
            Part("hand", hand, PrimitiveType.Sphere, new Vector3(0f, 0f, 0.01f),
                new Vector3(0.075f, 0.085f, 0.065f), skin);
            Part("thumb", hand, PrimitiveType.Sphere, new Vector3(side * 0.045f, 0.02f, 0.045f),
                new Vector3(0.026f, 0.045f, 0.026f), skin);
        }

        static void BuildLeg(Transform thigh, Transform shin, Transform foot, Material pants, Material cloth,
            Material leather, Material trim)
        {
            Part("thigh", thigh, PrimitiveType.Cylinder, new Vector3(0f, -0.20f, 0f),
                new Vector3(0.098f, 0.21f, 0.098f), pants);
            Part("thigh wrap", thigh, PrimitiveType.Cylinder, new Vector3(0f, -0.34f, 0f),
                new Vector3(0.102f, 0.035f, 0.102f), trim);
            Part("shin", shin, PrimitiveType.Cylinder, new Vector3(0f, -0.19f, 0f),
                new Vector3(0.084f, 0.20f, 0.084f), cloth);
            Part("boot cuff", shin, PrimitiveType.Cylinder, new Vector3(0f, -0.34f, 0f),
                new Vector3(0.096f, 0.055f, 0.096f), leather);
            Part("boot", foot, PrimitiveType.Cube, new Vector3(0f, -0.04f, 0.045f),
                new Vector3(0.155f, 0.10f, 0.27f), leather);
            Part("sole", foot, PrimitiveType.Cube, new Vector3(0f, -0.085f, 0.045f),
                new Vector3(0.16f, 0.025f, 0.275f), Mat(GameConfig.Hex("231B16")));
        }

        static void BuildFace(Transform head, Color skin, Color hair, bool fullBeard)
        {
            Material mSkin = Mat(skin);
            Material mHair = Mat(hair);
            Material mDark = Mat(GameConfig.Hex("241B16"));
            Material mWhite = Mat(GameConfig.Hex("D9CFC0"));
            Part("head", head, PrimitiveType.Sphere, new Vector3(0f, 0.10f, 0f), new Vector3(0.30f, 0.34f, 0.29f), mSkin);
            Part("ear L", head, PrimitiveType.Sphere, new Vector3(-0.15f, 0.10f, 0f), new Vector3(0.05f, 0.09f, 0.04f), mSkin);
            Part("ear R", head, PrimitiveType.Sphere, new Vector3(0.15f, 0.10f, 0f), new Vector3(0.05f, 0.09f, 0.04f), mSkin);
            Part("nose", head, PrimitiveType.Sphere, new Vector3(0f, 0.075f, 0.155f), new Vector3(0.045f, 0.065f, 0.05f), mSkin);
            for (int i = 0; i < 2; i++)
            {
                float sx = i == 0 ? -1f : 1f;
                Part("eye", head, PrimitiveType.Sphere, new Vector3(sx * 0.067f, 0.105f, 0.143f),
                    new Vector3(0.047f, 0.023f, 0.024f), mWhite);
                Part("pupil", head, PrimitiveType.Sphere, new Vector3(sx * 0.067f, 0.105f, 0.158f),
                    new Vector3(0.019f, 0.018f, 0.012f), mDark);
                Part("brow", head, PrimitiveType.Cube, new Vector3(sx * 0.070f, 0.148f, 0.147f),
                    new Vector3(0.095f, 0.018f, 0.018f), mHair, Quaternion.Euler(0f, 0f, sx * -8f));
                Part("cheek", head, PrimitiveType.Cube, new Vector3(sx * 0.10f, 0.045f, 0.135f),
                    new Vector3(0.055f, 0.07f, 0.02f), mSkin, Quaternion.Euler(0f, 0f, sx * 8f));
            }
            Part("mouth", head, PrimitiveType.Cube, new Vector3(0f, 0.018f, 0.153f),
                new Vector3(0.075f, 0.012f, 0.015f), Mat(GameConfig.Hex("6D3B32")));

            if (fullBeard)
            {
                Part("beard", head, PrimitiveType.Cube, new Vector3(0f, -0.06f, 0.135f),
                    new Vector3(0.22f, 0.18f, 0.11f), mHair, Quaternion.Euler(-5f, 0f, 0f));
                Part("beard tip", head, PrimitiveType.Cube, new Vector3(0f, -0.17f, 0.12f),
                    new Vector3(0.14f, 0.12f, 0.08f), mHair, Quaternion.Euler(-8f, 0f, 0f));
                Part("mustache L", head, PrimitiveType.Cube, new Vector3(-0.055f, 0.03f, 0.16f),
                    new Vector3(0.075f, 0.018f, 0.02f), mHair, Quaternion.Euler(0f, 0f, -12f));
                Part("mustache R", head, PrimitiveType.Cube, new Vector3(0.055f, 0.03f, 0.16f),
                    new Vector3(0.075f, 0.018f, 0.02f), mHair, Quaternion.Euler(0f, 0f, 12f));
            }
            else
            {
                Part("beard shadow", head, PrimitiveType.Cube, new Vector3(0f, -0.015f, 0.135f),
                    new Vector3(0.11f, 0.05f, 0.06f), mHair);
            }
        }

        static void BuildLinchongHead(Transform head, HeroDefinition hero, Color skin, Color hair)
        {
            Material mMetal = Mat(GameConfig.Hex("9CA4A8"));
            Material mTrim = Mat(hero.Trim);
            Material mDark = Mat(hero.Secondary);
            Material mHair = Mat(hair);

            Part("hair cap", head, PrimitiveType.Sphere, new Vector3(0f, 0.17f, -0.025f),
                new Vector3(0.325f, 0.22f, 0.315f), mHair);
            Part("helmet bowl", head, PrimitiveType.Cylinder, new Vector3(0f, 0.245f, 0f),
                new Vector3(0.21f, 0.10f, 0.20f), mDark);
            Part("helmet rim", head, PrimitiveType.Cylinder, new Vector3(0f, 0.185f, 0f),
                new Vector3(0.235f, 0.025f, 0.225f), mTrim);
            Part("helmet ridge", head, PrimitiveType.Cube, new Vector3(0f, 0.30f, 0f),
                new Vector3(0.05f, 0.055f, 0.33f), mTrim);
            Part("helmet crest", head, PrimitiveType.Sphere, new Vector3(0f, 0.335f, 0.02f),
                new Vector3(0.10f, 0.10f, 0.10f), mTrim);
            for (int i = 0; i < 3; i++)
            {
                Part("plume", head, PrimitiveType.Sphere, new Vector3(0f, 0.43f + i * 0.06f, -0.04f - i * 0.03f),
                    new Vector3(0.11f - i * 0.02f, 0.075f, 0.09f - i * 0.015f), Mat(GameConfig.Hex("A83C30")));
            }
            Part("forehead guard", head, PrimitiveType.Cube, new Vector3(0f, 0.17f, 0.135f),
                new Vector3(0.17f, 0.035f, 0.035f), mTrim);
            Part("side hair", head, PrimitiveType.Cube, new Vector3(0f, 0.06f, -0.12f),
                new Vector3(0.30f, 0.22f, 0.10f), mHair);
            Part("goatee", head, PrimitiveType.Cube, new Vector3(0f, -0.11f, 0.13f),
                new Vector3(0.10f, 0.12f, 0.06f), mHair, Quaternion.Euler(-6f, 0f, 0f));
            Part("white scarf", head, PrimitiveType.Cylinder, new Vector3(0f, -0.04f, 0f),
                new Vector3(0.14f, 0.07f, 0.14f), Mat(GameConfig.Hex("D9D3C4")));
        }

        static void BuildWusongHead(Transform head, HeroDefinition hero, Color skin, Color hair)
        {
            Material mHair = Mat(hair);
            Material mRed = Mat(GameConfig.Hex("8E312B"));
            Part("hair cap", head, PrimitiveType.Sphere, new Vector3(0f, 0.16f, -0.02f),
                new Vector3(0.33f, 0.26f, 0.32f), mHair);
            Part("headband", head, PrimitiveType.Cylinder, new Vector3(0f, 0.175f, 0f),
                new Vector3(0.175f, 0.032f, 0.168f), mRed);
            Part("headband knot", head, PrimitiveType.Sphere, new Vector3(0.05f, 0.19f, -0.17f),
                new Vector3(0.07f, 0.055f, 0.05f), mRed);
            Part("headband tail", head, PrimitiveType.Cube, new Vector3(0.13f, 0.10f, -0.19f),
                new Vector3(0.035f, 0.20f, 0.025f), mRed, Quaternion.Euler(8f, 0f, 20f));
            Part("topknot", head, PrimitiveType.Sphere, new Vector3(0f, 0.28f, -0.02f),
                new Vector3(0.12f, 0.12f, 0.12f), mHair);
            Part("sideburn L", head, PrimitiveType.Cube, new Vector3(-0.135f, 0.055f, 0.04f),
                new Vector3(0.045f, 0.16f, 0.08f), mHair);
            Part("sideburn R", head, PrimitiveType.Cube, new Vector3(0.135f, 0.055f, 0.04f),
                new Vector3(0.045f, 0.16f, 0.08f), mHair);
            Part("short beard", head, PrimitiveType.Cube, new Vector3(0f, -0.055f, 0.13f),
                new Vector3(0.16f, 0.10f, 0.07f), mHair);
            Part("scar", head, PrimitiveType.Cube, new Vector3(-0.085f, 0.12f, 0.153f),
                new Vector3(0.012f, 0.07f, 0.015f), Mat(GameConfig.Hex("8C4A3A")), Quaternion.Euler(0f, 0f, 28f));
        }

        static void BuildLuzhishenHead(Transform head, Transform chest, HeroDefinition hero, Color skin, Color hair)
        {
            Material mHair = Mat(hair);
            Material mBead = Mat(GameConfig.Hex("6B4A2B"));
            Part("brow ridge", head, PrimitiveType.Cube, new Vector3(0f, 0.17f, 0.12f),
                new Vector3(0.26f, 0.035f, 0.055f), Mat(skin * 0.9f));
            Part("nose bridge", head, PrimitiveType.Cube, new Vector3(0f, 0.10f, 0.16f),
                new Vector3(0.05f, 0.11f, 0.045f), Mat(skin * 0.92f));
            Part("ear ring L", head, PrimitiveType.Cylinder, new Vector3(-0.17f, 0.055f, 0f),
                new Vector3(0.045f, 0.012f, 0.045f), Mat(hero.Trim), Quaternion.Euler(0f, 0f, 90f));
            Part("ear ring R", head, PrimitiveType.Cylinder, new Vector3(0.17f, 0.055f, 0f),
                new Vector3(0.045f, 0.012f, 0.045f), Mat(hero.Trim), Quaternion.Euler(0f, 0f, 90f));
            for (int i = 0; i < 9; i++)
            {
                float a = i / 9f * Mathf.PI * 2f;
                Part("prayer bead", chest, PrimitiveType.Sphere,
                    new Vector3(Mathf.Cos(a) * 0.17f, 0.205f + Mathf.Sin(a) * 0.035f, Mathf.Sin(a) * 0.12f),
                    Vector3.one * 0.045f, mBead);
            }
            Part("monk collar", chest, PrimitiveType.Cylinder, new Vector3(0f, 0.18f, 0f),
                new Vector3(0.17f, 0.035f, 0.14f), Mat(hero.Cloth));
        }

        static void BuildBladeHead(Transform head, EnemyDefinition def)
        {
            Part("leather cap", head, PrimitiveType.Cylinder, new Vector3(0f, 0.235f, 0f),
                new Vector3(0.17f, 0.075f, 0.16f), Mat(def.Trim));
            Part("cap spike", head, PrimitiveType.Cylinder, new Vector3(0f, 0.32f, 0f),
                new Vector3(0.035f, 0.06f, 0.035f), Mat(def.Secondary));
            Part("face wrap", head, PrimitiveType.Cube, new Vector3(0f, 0.015f, 0.15f),
                new Vector3(0.24f, 0.09f, 0.045f), Mat(def.Cloth));
            Part("hair tail", head, PrimitiveType.Cube, new Vector3(0f, 0.14f, -0.17f),
                new Vector3(0.06f, 0.16f, 0.04f), Mat(GameConfig.Hex("1B1815")));
        }

        static void BuildSpearHead(Transform head, EnemyDefinition def)
        {
            Part("helmet", head, PrimitiveType.Cylinder, new Vector3(0f, 0.24f, 0f),
                new Vector3(0.18f, 0.085f, 0.17f), Mat(GameConfig.Hex("70787B")));
            Part("helmet rim", head, PrimitiveType.Cylinder, new Vector3(0f, 0.185f, 0f),
                new Vector3(0.195f, 0.022f, 0.185f), Mat(def.Trim));
            Part("tassel", head, PrimitiveType.Cylinder, new Vector3(0f, 0.335f, 0f),
                new Vector3(0.045f, 0.075f, 0.045f), Mat(GameConfig.Hex("A33A2E")));
            Part("cheek guard L", head, PrimitiveType.Cube, new Vector3(-0.145f, 0.07f, 0.03f),
                new Vector3(0.05f, 0.14f, 0.12f), Mat(GameConfig.Hex("5D6467")));
            Part("cheek guard R", head, PrimitiveType.Cube, new Vector3(0.145f, 0.07f, 0.03f),
                new Vector3(0.05f, 0.14f, 0.12f), Mat(GameConfig.Hex("5D6467")));
        }

        static void BuildShieldHead(Transform head, EnemyDefinition def)
        {
            Part("full helm", head, PrimitiveType.Sphere, new Vector3(0f, 0.14f, 0f),
                new Vector3(0.335f, 0.36f, 0.33f), Mat(GameConfig.Hex("626A6D")));
            Part("visor", head, PrimitiveType.Cube, new Vector3(0f, 0.105f, 0.155f),
                new Vector3(0.25f, 0.045f, 0.04f), Mat(GameConfig.Hex("1A1C1E")));
            Part("helm ridge", head, PrimitiveType.Cube, new Vector3(0f, 0.30f, 0f),
                new Vector3(0.055f, 0.07f, 0.31f), Mat(def.Trim));
            Part("breath holes", head, PrimitiveType.Cube, new Vector3(0f, 0.025f, 0.16f),
                new Vector3(0.14f, 0.07f, 0.035f), Mat(GameConfig.Hex("353B3D")));
        }

        static void BuildArcherHead(Transform head, EnemyDefinition def)
        {
            Part("hood", head, PrimitiveType.Sphere, new Vector3(0f, 0.16f, -0.03f),
                new Vector3(0.36f, 0.32f, 0.35f), Mat(def.Cloth));
            Part("hood point", head, PrimitiveType.Cube, new Vector3(0f, 0.30f, -0.07f),
                new Vector3(0.08f, 0.15f, 0.08f), Mat(def.Cloth), Quaternion.Euler(-15f, 0f, 0f));
            Part("face wrap", head, PrimitiveType.Cube, new Vector3(0f, 0.04f, 0.15f),
                new Vector3(0.22f, 0.12f, 0.06f), Mat(def.Secondary));
            Part("eye band", head, PrimitiveType.Cube, new Vector3(0f, 0.105f, 0.18f),
                new Vector3(0.22f, 0.025f, 0.02f), Mat(GameConfig.Hex("1B1A18")));
        }

        static void BuildCaptainHead(Transform head, EnemyDefinition def)
        {
            Material mSteel = Mat(GameConfig.Hex("7A8285"));
            Material mGold = Mat(def.Trim);
            Part("helmet", head, PrimitiveType.Cylinder, new Vector3(0f, 0.26f, 0f),
                new Vector3(0.20f, 0.105f, 0.19f), mSteel);
            Part("helmet band", head, PrimitiveType.Cylinder, new Vector3(0f, 0.20f, 0f),
                new Vector3(0.22f, 0.028f, 0.21f), mGold);
            Part("mask", head, PrimitiveType.Cube, new Vector3(0f, 0.065f, 0.155f),
                new Vector3(0.26f, 0.13f, 0.055f), Mat(GameConfig.Hex("3B3E40")));
            Part("mask slit", head, PrimitiveType.Cube, new Vector3(0f, 0.09f, 0.185f),
                new Vector3(0.19f, 0.018f, 0.012f), Mat(GameConfig.Hex("0F1010")));
            for (int i = 0; i < 2; i++)
            {
                float sx = i == 0 ? -1f : 1f;
                Part("horn", head, PrimitiveType.Cylinder, new Vector3(sx * 0.26f, 0.38f, -0.01f),
                    new Vector3(0.045f, 0.22f, 0.045f), mGold, Quaternion.Euler(0f, 0f, sx * -38f));
                Part("horn tip", head, PrimitiveType.Sphere, new Vector3(sx * 0.38f, 0.43f, -0.01f),
                    Vector3.one * 0.055f, mGold);
            }
            Part("crest", head, PrimitiveType.Cube, new Vector3(0f, 0.40f, -0.02f),
                new Vector3(0.055f, 0.14f, 0.22f), Mat(GameConfig.Hex("9A2E27")));
            Part("neck guard", head, PrimitiveType.Cube, new Vector3(0f, -0.02f, -0.10f),
                new Vector3(0.31f, 0.10f, 0.12f), mSteel);
        }

        static void BuildBossHead(Transform head, EnemyDefinition def)
        {
            Material mBlack = Mat(GameConfig.Hex("17151B"));
            Material mGold = Mat(def.Trim);
            Part("official brim", head, PrimitiveType.Cylinder, new Vector3(0f, 0.315f, 0f),
                new Vector3(0.36f, 0.018f, 0.34f), mBlack);
            Part("official hat", head, PrimitiveType.Cylinder, new Vector3(0f, 0.44f, 0f),
                new Vector3(0.16f, 0.14f, 0.15f), mBlack);
            Part("hat band", head, PrimitiveType.Cylinder, new Vector3(0f, 0.365f, 0f),
                new Vector3(0.22f, 0.025f, 0.21f), mGold);
            for (int i = 0; i < 2; i++)
            {
                float sx = i == 0 ? -1f : 1f;
                Part("hat wing", head, PrimitiveType.Cube, new Vector3(sx * 0.36f, 0.40f, -0.02f),
                    new Vector3(0.42f, 0.035f, 0.10f), mBlack, Quaternion.Euler(0f, 0f, sx * 6f));
                Part("wing tip", head, PrimitiveType.Cube, new Vector3(sx * 0.56f, 0.40f, -0.02f),
                    new Vector3(0.055f, 0.07f, 0.10f), mGold);
            }
            Part("gaunt cheeks", head, PrimitiveType.Cube, new Vector3(-0.12f, 0.02f, 0.13f),
                new Vector3(0.06f, 0.15f, 0.05f), Mat(GameConfig.Hex("A9826A")));
            Part("gaunt cheeks R", head, PrimitiveType.Cube, new Vector3(0.12f, 0.02f, 0.13f),
                new Vector3(0.06f, 0.15f, 0.05f), Mat(GameConfig.Hex("A9826A")));
            Part("long beard", head, PrimitiveType.Cube, new Vector3(0f, -0.16f, 0.12f),
                new Vector3(0.16f, 0.34f, 0.09f), Mat(GameConfig.Hex("3A3533")), Quaternion.Euler(-6f, 0f, 0f));
            Part("mustache L", head, PrimitiveType.Cube, new Vector3(-0.06f, 0.035f, 0.16f),
                new Vector3(0.09f, 0.018f, 0.02f), Mat(GameConfig.Hex("3A3533")), Quaternion.Euler(0f, 0f, -18f));
            Part("mustache R", head, PrimitiveType.Cube, new Vector3(0.06f, 0.035f, 0.16f),
                new Vector3(0.09f, 0.018f, 0.02f), Mat(GameConfig.Hex("3A3533")), Quaternion.Euler(0f, 0f, 18f));
        }

        static void BuildBossPauldrons(Transform chest, EnemyDefinition def)
        {
            Material mGold = Mat(def.Trim);
            Material mDark = Mat(def.Secondary);
            for (int i = 0; i < 2; i++)
            {
                float sx = i == 0 ? -1f : 1f;
                Part("boss pauldron", chest, PrimitiveType.Sphere, new Vector3(sx * 0.31f, 0.16f, 0f),
                    new Vector3(0.30f, 0.17f, 0.32f), mDark);
                Part("boss pauldron trim", chest, PrimitiveType.Cylinder, new Vector3(sx * 0.31f, 0.09f, 0f),
                    new Vector3(0.16f, 0.035f, 0.17f), mGold, Quaternion.Euler(0f, 0f, 90f));
                for (int k = 0; k < 3; k++)
                {
                    Part("spike", chest, PrimitiveType.Cylinder, new Vector3(sx * (0.20f + k * 0.06f), 0.28f, -0.01f),
                        new Vector3(0.035f, 0.11f, 0.035f), mGold, Quaternion.Euler(0f, 0f, sx * -28f));
                }
            }
        }

        static void BuildWeapon(ProceduralRig rig, WeaponStyle style, Color trim)
        {
            Material mTrim = Mat(trim);
            Material mSteel = Mat(GameConfig.Hex("A7ADB0"));
            Material mDarkSteel = Mat(GameConfig.Hex("555C60"));
            Material mWood = Mat(GameConfig.Hex("4C3322"));
            Material mLeather = Mat(GameConfig.Hex("2B2019"));
            Material mRed = Mat(GameConfig.Hex("8E2D27"));

            var gripR = WorldFactory.Empty("WeaponGripR", rig.HandR, new Vector3(0f, 0f, 0.02f)).transform;
            var gripL = WorldFactory.Empty("WeaponGripL", rig.HandL, new Vector3(0f, 0f, 0.02f)).transform;
            rig.WeaponGripR = gripR;
            rig.WeaponGripL = gripL;

            switch (style)
            {
                case WeaponStyle.Spear:
                    BuildSpear(gripR, mWood, mSteel, mRed, mTrim);
                    break;
                case WeaponStyle.DualBlades:
                    BuildDao(gripR, mLeather, mSteel, mTrim, 1f);
                    BuildDao(gripL, mLeather, mSteel, mTrim, -1f);
                    break;
                case WeaponStyle.Staff:
                    BuildStaff(gripR, mWood, mTrim, mDarkSteel);
                    break;
                case WeaponStyle.Dao:
                    BuildDao(gripR, mLeather, mSteel, mTrim, 1f);
                    break;
                case WeaponStyle.Shield:
                    BuildDao(gripR, mLeather, mSteel, mTrim, 1f);
                    BuildShield(gripL, mTrim, mDarkSteel, mRed);
                    break;
                case WeaponStyle.Bow:
                    BuildBow(gripL, mWood, mTrim, mLeather);
                    break;
                case WeaponStyle.Halberd:
                    BuildHalberd(gripR, mWood, mSteel, mTrim, mRed);
                    break;
            }
        }

        static void BuildSpear(Transform grip, Material wood, Material steel, Material red, Material trim)
        {
            Part("spear shaft", grip, PrimitiveType.Cylinder, new Vector3(0f, 0.35f, 0f),
                new Vector3(0.027f, 0.95f, 0.027f), wood);
            for (int i = 0; i < 4; i++)
                Part("grip wrap", grip, PrimitiveType.Cylinder, new Vector3(0f, -0.12f + i * 0.075f, 0f),
                    new Vector3(0.031f, 0.02f, 0.031f), red);
            Part("spear collar", grip, PrimitiveType.Cylinder, new Vector3(0f, 1.28f, 0f),
                new Vector3(0.045f, 0.035f, 0.045f), trim);
            TaperPart("spear blade", grip, new Vector3(0f, 1.31f, 0f), 0.075f, 0.0f, 0.40f, 4,
                GameConfig.Hex("A7ADB0"), GameConfig.Hex("767C7F"), steel, Vector3.one, Quaternion.Euler(0f, 45f, 0f));
            Part("tassel", grip, PrimitiveType.Sphere, new Vector3(0f, 1.24f, 0f),
                new Vector3(0.10f, 0.13f, 0.10f), red);
            Part("butt cap", grip, PrimitiveType.Cylinder, new Vector3(0f, -0.58f, 0f),
                new Vector3(0.04f, 0.045f, 0.04f), trim);
        }

        static void BuildDao(Transform grip, Material leather, Material steel, Material trim, float side)
        {
            Part("handle", grip, PrimitiveType.Cylinder, new Vector3(0f, 0.02f, 0f),
                new Vector3(0.032f, 0.10f, 0.032f), leather);
            Part("pommel", grip, PrimitiveType.Sphere, new Vector3(0f, -0.085f, 0f),
                new Vector3(0.06f, 0.05f, 0.06f), trim);
            Part("guard", grip, PrimitiveType.Cube, new Vector3(0f, 0.125f, 0f),
                new Vector3(0.19f, 0.028f, 0.075f), trim);
            var blade = Part("blade", grip, PrimitiveType.Cube, new Vector3(side * 0.012f, 0.50f, 0f),
                new Vector3(0.085f, 0.72f, 0.022f), steel, Quaternion.Euler(0f, 0f, side * -2f));
            Part("blade back", grip, PrimitiveType.Cube, new Vector3(side * 0.012f, 0.45f, -0.018f),
                new Vector3(0.055f, 0.60f, 0.015f), Mat(GameConfig.Hex("7F8689")), Quaternion.Euler(0f, 0f, side * -2f));
            TaperPart("blade tip", grip, new Vector3(side * 0.02f, 0.86f, 0f), 0.043f, 0f, 0.17f, 4,
                GameConfig.Hex("A7ADB0"), GameConfig.Hex("767C7F"), steel, new Vector3(1f, 1f, 0.3f), Quaternion.Euler(0f, 45f, 0f));
            Part("blade edge", grip, PrimitiveType.Cube, new Vector3(side * 0.055f, 0.48f, 0f),
                new Vector3(0.012f, 0.68f, 0.018f), Mat(GameConfig.Hex("D8DEDF")));
        }

        static void BuildStaff(Transform grip, Material wood, Material trim, Material steel)
        {
            Part("staff shaft", grip, PrimitiveType.Cylinder, new Vector3(0f, 0.30f, 0f),
                new Vector3(0.036f, 1.05f, 0.036f), wood);
            for (int i = 0; i < 2; i++)
            {
                float y = i == 0 ? 1.30f : -0.76f;
                Part("staff cap", grip, PrimitiveType.Cylinder, new Vector3(0f, y, 0f),
                    new Vector3(0.075f, 0.055f, 0.075f), trim);
                Part("staff ring", grip, PrimitiveType.Cylinder, new Vector3(0f, y - Mathf.Sign(y) * 0.07f, 0f),
                    new Vector3(0.058f, 0.018f, 0.058f), steel);
            }
            Part("staff collar", grip, PrimitiveType.Cylinder, new Vector3(0f, 1.18f, 0f),
                new Vector3(0.05f, 0.04f, 0.05f), trim);
            for (int i = 0; i < 2; i++)
            {
                Part("staff loop", grip, PrimitiveType.Cylinder, new Vector3(0f, 1.10f - i * 0.12f, 0f),
                    new Vector3(0.05f, 0.012f, 0.05f), trim);
            }
        }

        static void BuildHalberd(Transform grip, Material wood, Material steel, Material trim, Material red)
        {
            Part("halberd shaft", grip, PrimitiveType.Cylinder, new Vector3(0f, 0.40f, 0f),
                new Vector3(0.042f, 1.25f, 0.042f), wood);
            Part("halberd butt", grip, PrimitiveType.Cylinder, new Vector3(0f, -0.80f, 0f),
                new Vector3(0.055f, 0.075f, 0.055f), trim);
            Part("halberd collar", grip, PrimitiveType.Cylinder, new Vector3(0f, 1.60f, 0f),
                new Vector3(0.07f, 0.055f, 0.07f), trim);
            TaperPart("halberd blade", grip, new Vector3(0f, 1.64f, 0f), 0.13f, 0f, 0.65f, 4,
                GameConfig.Hex("A7ADB0"), GameConfig.Hex("767C7F"), steel, new Vector3(0.35f, 1f, 1f), Quaternion.Euler(0f, 45f, 0f));
            Part("halberd crescent", grip, PrimitiveType.Cylinder, new Vector3(0.23f, 1.70f, 0f),
                new Vector3(0.20f, 0.035f, 0.10f), steel, Quaternion.Euler(0f, 0f, 32f));
            Part("halberd spike", grip, PrimitiveType.Cylinder, new Vector3(0f, 2.28f, 0f),
                new Vector3(0.035f, 0.16f, 0.035f), trim);
            Part("halberd tassel", grip, PrimitiveType.Sphere, new Vector3(0f, 1.55f, 0f),
                new Vector3(0.13f, 0.17f, 0.13f), red);
        }

        static void BuildShield(Transform grip, Material trim, Material steel, Material red)
        {
            Part("shield", grip, PrimitiveType.Cylinder, new Vector3(0f, -0.04f, 0.12f),
                new Vector3(0.54f, 0.035f, 0.54f), steel, Quaternion.Euler(90f, 0f, 0f));
            Part("shield rim", grip, PrimitiveType.Cylinder, new Vector3(0f, -0.04f, 0.10f),
                new Vector3(0.58f, 0.025f, 0.58f), trim, Quaternion.Euler(90f, 0f, 0f));
            Part("shield boss", grip, PrimitiveType.Sphere, new Vector3(0f, -0.04f, 0.17f),
                new Vector3(0.16f, 0.16f, 0.10f), trim);
            Part("shield stud L", grip, PrimitiveType.Sphere, new Vector3(-0.30f, 0.04f, 0.16f),
                Vector3.one * 0.045f, trim);
            Part("shield stud R", grip, PrimitiveType.Sphere, new Vector3(0.30f, -0.12f, 0.16f),
                Vector3.one * 0.045f, trim);
            Part("shield stripe", grip, PrimitiveType.Cube, new Vector3(0f, -0.04f, 0.165f),
                new Vector3(0.10f, 0.46f, 0.012f), red);
        }

        static void BuildBow(Transform grip, Material wood, Material trim, Material leather)
        {
            int segments = 7;
            for (int i = 0; i < segments; i++)
            {
                float t0 = i / (float)segments;
                float t1 = (i + 1) / (float)segments;
                Vector3 p0 = BowPoint(t0);
                Vector3 p1 = BowPoint(t1);
                Vector3 mid = (p0 + p1) * 0.5f;
                float len = Vector3.Distance(p0, p1);
                float angle = Mathf.Atan2(p1.y - p0.y, p1.x - p0.x) * Mathf.Rad2Deg;
                Part("bow limb", grip, PrimitiveType.Cylinder, mid, new Vector3(0.028f, len * 0.5f, 0.032f), wood,
                    Quaternion.Euler(0f, 0f, angle - 90f));
            }
            Part("bow grip", grip, PrimitiveType.Cylinder, new Vector3(0.13f, 0f, 0f),
                new Vector3(0.045f, 0.10f, 0.045f), leather);
            Part("bow tip L", grip, PrimitiveType.Sphere, BowPoint(0f), Vector3.one * 0.045f, trim);
            Part("bow tip R", grip, PrimitiveType.Sphere, BowPoint(1f), Vector3.one * 0.045f, trim);
            Vector3 a = BowPoint(0f);
            Vector3 b = BowPoint(1f);
            Part("bowstring", grip, PrimitiveType.Cylinder, (a + b) * 0.5f + Vector3.right * -0.05f,
                new Vector3(0.006f, Vector3.Distance(a, b) * 0.5f, 0.006f), Mat(GameConfig.Hex("D6CCB8")),
                Quaternion.Euler(0f, 0f, 0f));
        }

        static Vector3 BowPoint(float t)
        {
            float y = Mathf.Lerp(-0.62f, 0.62f, t);
            float x = 0.24f * Mathf.Sin(t * Mathf.PI);
            return new Vector3(x, y, 0f);
        }

        static void BuildQuiver(Transform chest, EnemyDefinition def)
        {
            var quiver = Part("quiver", chest, PrimitiveType.Cylinder, new Vector3(0.18f, 0.10f, -0.20f),
                new Vector3(0.09f, 0.30f, 0.09f), Mat(def.Cloth), Quaternion.Euler(20f, 0f, -15f));
            for (int i = 0; i < 4; i++)
            {
                Part("arrow", chest, PrimitiveType.Cylinder, new Vector3(0.16f + i * 0.025f, 0.40f, -0.24f + i * 0.02f),
                    new Vector3(0.008f, 0.22f, 0.008f), Mat(GameConfig.Hex("5B4329")), Quaternion.Euler(20f, 0f, -15f));
            }
        }

        static void BuildCape(Transform chest, Color cloth, Color trim, float length, float width)
        {
            var go = WorldFactory.Empty("cape", chest, new Vector3(0f, 0.15f, -0.19f));
            int cols = 4, rows = 6;
            var verts = new Vector3[(cols + 1) * (rows + 1)];
            var colors = new Color[verts.Length];
            var uvs = new Vector2[verts.Length];
            for (int y = 0; y <= rows; y++)
            {
                float v = y / (float)rows;
                for (int x = 0; x <= cols; x++)
                {
                    float u = x / (float)cols;
                    int i = y * (cols + 1) + x;
                    float taper = Mathf.Lerp(1f, 0.72f, v);
                    verts[i] = new Vector3((u - 0.5f) * width * taper, -v * length, Mathf.Sin(v * Mathf.PI) * 0.03f);
                    colors[i] = Color.Lerp(cloth, cloth * 0.62f, v);
                    uvs[i] = new Vector2(u, 1f - v);
                }
            }
            var tris = new List<int>();
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    int a = y * (cols + 1) + x;
                    int b = a + 1;
                    int c = a + cols + 1;
                    int d = c + 1;
                    tris.Add(a); tris.Add(b); tris.Add(d);
                    tris.Add(a); tris.Add(d); tris.Add(c);
                }
            }
            var mesh = new Mesh { name = "cape" };
            mesh.vertices = verts;
            mesh.colors = colors;
            mesh.uv = uvs;
            mesh.triangles = tris.ToArray();
            mesh.RecalculateNormals();
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = CapeMaterial();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
            var flutter = go.AddComponent<CapeFlutter>();
            flutter.Amplitude = length * 0.07f;
            flutter.Frequency = 1.8f + length;
            Part("cape clasp", chest, PrimitiveType.Sphere, new Vector3(0f, 0.20f, 0.25f),
                new Vector3(0.09f, 0.07f, 0.05f), Mat(trim));
        }

        static void BuildRobeSkirt(Transform hips, Color cloth, Color trim)
        {
            TaperPart("robe skirt", hips, new Vector3(0f, -0.44f, 0f), 0.34f, 0.20f, 0.56f, 10,
                cloth * 0.75f, cloth, Mat(cloth), new Vector3(1f, 1f, 0.85f));
            Part("robe hem", hips, PrimitiveType.Cylinder, new Vector3(0f, -0.44f, 0f),
                new Vector3(0.345f, 0.035f, 0.295f), Mat(trim));
            for (int i = 0; i < 2; i++)
            {
                float sx = i == 0 ? -1f : 1f;
                Part("robe panel", hips, PrimitiveType.Cube, new Vector3(sx * 0.22f, -0.22f, 0.14f),
                    new Vector3(0.16f, 0.12f, 0.03f), Mat(trim), Quaternion.Euler(0f, 0f, sx * 6f));
            }
        }

        static void AddTigerPelt(Transform chest, HeroDefinition hero)
        {
            Material mPelt = Mat(GameConfig.Hex("A66A34"));
            Material mStripe = Mat(GameConfig.Hex("2B1E15"));
            Part("tiger pelt", chest, PrimitiveType.Cube, new Vector3(-0.08f, 0.02f, 0.275f),
                new Vector3(0.22f, 0.30f, 0.045f), mPelt, Quaternion.Euler(0f, 0f, 14f));
            for (int i = 0; i < 3; i++)
            {
                Part("tiger stripe", chest, PrimitiveType.Cube, new Vector3(-0.16f + i * 0.08f, 0.05f, 0.30f),
                    new Vector3(0.022f, 0.17f, 0.012f), mStripe, Quaternion.Euler(0f, 0f, -8f + i * 16f));
            }
            Part("tiger head", chest, PrimitiveType.Sphere, new Vector3(-0.08f, 0.15f, 0.30f),
                new Vector3(0.13f, 0.11f, 0.055f), mPelt);
        }

        static Transform Bone(string name, Transform parent, Vector3 position)
        {
            return WorldFactory.Empty(name, parent, position).transform;
        }

        static GameObject Part(string name, Transform parent, PrimitiveType type, Vector3 position, Vector3 scale,
            Material material, Quaternion? rotation = null)
        {
            return WorldFactory.Primitive(name, parent, type, position, scale, rotation, material);
        }

        static GameObject TaperPart(string name, Transform parent, Vector3 position, float bottomRadius, float topRadius,
            float height, int sides, Color bottomColor, Color topColor, Material material, Vector3? scale = null,
            Quaternion? rotation = null)
        {
            var builder = new MeshBuilder(true);
            for (int i = 0; i < sides; i++)
            {
                float a0 = i / (float)sides * Mathf.PI * 2f;
                float a1 = (i + 1) / (float)sides * Mathf.PI * 2f;
                Vector3 b0 = new Vector3(Mathf.Cos(a0) * bottomRadius, 0f, Mathf.Sin(a0) * bottomRadius);
                Vector3 b1 = new Vector3(Mathf.Cos(a1) * bottomRadius, 0f, Mathf.Sin(a1) * bottomRadius);
                Vector3 t0 = new Vector3(Mathf.Cos(a0) * topRadius, height, Mathf.Sin(a0) * topRadius);
                Vector3 t1 = new Vector3(Mathf.Cos(a1) * topRadius, height, Mathf.Sin(a1) * topRadius);
                builder.Quad(b0, t0, t1, b1, bottomColor, topColor, topColor, bottomColor);
            }
            if (topRadius > 0.001f)
                for (int i = 0; i < sides; i++)
                {
                    float a0 = i / (float)sides * Mathf.PI * 2f;
                    float a1 = (i + 1) / (float)sides * Mathf.PI * 2f;
                    builder.FlatTriangle(new Vector3(0f, height, 0f),
                        new Vector3(Mathf.Cos(a1) * topRadius, height, Mathf.Sin(a1) * topRadius),
                        new Vector3(Mathf.Cos(a0) * topRadius, height, Mathf.Sin(a0) * topRadius), topColor);
                }
            if (bottomRadius > 0.001f)
                for (int i = 0; i < sides; i++)
                {
                    float a0 = i / (float)sides * Mathf.PI * 2f;
                    float a1 = (i + 1) / (float)sides * Mathf.PI * 2f;
                    builder.FlatTriangle(Vector3.zero,
                        new Vector3(Mathf.Cos(a0) * bottomRadius, 0f, Mathf.Sin(a0) * bottomRadius),
                        new Vector3(Mathf.Cos(a1) * bottomRadius, 0f, Mathf.Sin(a1) * bottomRadius), bottomColor);
                }
            return WorldFactory.MeshObject(name, parent, builder.Build(name), material, position, rotation, scale, true, true);
        }

        static Material Mat(Color color)
        {
            string key = ColorUtility.ToHtmlStringRGBA(color);
            if (MaterialCache.TryGetValue(key, out var mat))
                return mat;
            Color shadow = Color.Lerp(color * 0.42f, GameConfig.Hex("23212A"), 0.28f);
            mat = RuntimeAssets.Toon(color, shadow, 0.008f);
            mat.name = "Char " + key;
            MaterialCache[key] = mat;
            return mat;
        }

        static Material CapeMaterial()
        {
            if (_capeMaterial == null)
            {
                _capeMaterial = RuntimeAssets.VertexToon(0.006f, 0.25f);
                _capeMaterial.name = "Cape";
                _capeMaterial.SetFloat("_Cull", 0f);
            }
            return _capeMaterial;
        }

        // Small partial extensions kept here to avoid another file.
    }
}
