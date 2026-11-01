using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Liangshan
{
    /// <summary>Hand-built props: banners, pines, maples, bamboos, boulders, braziers, watchtowers.</summary>
    public static class WorldProps
    {
        public static void BuildTrees(WorldRefs world)
        {
            Transform parent = world.Props;
            var random = new System.Random(108);
            int pines = 26;
            for (int i = 0; i < pines; i++)
            {
                float angle = (i / (float)pines) * Mathf.PI * 2f + (float)random.NextDouble() * 0.2f;
                float radius = 24.5f + (float)random.NextDouble() * 7.5f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                if (pos.z > 18f && Mathf.Abs(pos.x) < 13f) continue;
                if (pos.z < -23f && Mathf.Abs(pos.x) < 9f) continue;
                BuildPine(parent, pos, i * 37 + 11, 0.85f + (float)random.NextDouble() * 0.45f);
            }

            for (int i = 0; i < 7; i++)
            {
                float angle = 2.1f + i * 0.43f;
                float radius = 24f + (float)random.NextDouble() * 5f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                if (pos.z > 20f) continue;
                BuildMaple(parent, pos, 900 + i, 0.9f + (float)random.NextDouble() * 0.3f);
            }

            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = new Vector3(-13f + i * 6.5f, 0f, 21.5f + (i % 2) * 2.2f);
                BuildBamboo(parent, pos, 400 + i);
            }

            BuildGrassField(parent);
            BuildBoulders(parent);
        }

        public static void BuildPine(Transform parent, Vector3 position, int seed, float scale)
        {
            var random = new System.Random(seed);
            float height = (4.8f + (float)random.NextDouble() * 3.2f) * scale;
            float trunkRadius = 0.17f * scale;
            Material bark = RuntimeAssets.Toon(GameConfig.Hex("4A392B"), 0.004f);
            GameObject root = WorldFactory.Empty("Pine " + seed, parent, position, Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f));
            WorldFactory.Primitive("trunk", root.transform, PrimitiveType.Cylinder, new Vector3(0f, height * 0.5f, 0f),
                new Vector3(trunkRadius, height * 0.5f, trunkRadius), null, bark);

            var colors = new[] { "2E4235", "354A3A", "283A30", "3D5340" };
            var builder = new MeshBuilder(true);
            int layers = 3 + random.Next(2);
            for (int i = 0; i < layers; i++)
            {
                float t = i / (float)layers;
                float y = height * (0.44f + t * 0.40f);
                float radius = (1.9f - t * 0.55f) * scale;
                float layerHeight = (2.0f - t * 0.35f) * scale;
                Color c = GameConfig.Hex(colors[(seed + i) % colors.Length]);
                builder.Cone(new Vector3(0f, y, 0f), radius, layerHeight, 7, c, c * 1.12f, true);
            }
            Mesh mesh = builder.Build("pine canopy " + seed);
            var canopy = WorldFactory.MeshObject("canopy", root.transform, mesh, RuntimeAssets.VertexToon(0.018f, 0.22f), Vector3.zero);
            var sway = canopy.AddComponent<Sway>();
            sway.amplitude = 1.1f;
            sway.speed = 0.34f + (float)random.NextDouble() * 0.18f;
            sway.phase = seed * 0.13f;
        }

        public static void BuildMaple(Transform parent, Vector3 position, int seed, float scale)
        {
            var random = new System.Random(seed);
            float height = 3.6f * scale;
            Material bark = RuntimeAssets.Toon(GameConfig.Hex("52402F"), 0.004f);
            GameObject root = WorldFactory.Empty("Maple " + seed, parent, position, Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f));
            WorldFactory.Primitive("trunk", root.transform, PrimitiveType.Cylinder, new Vector3(0f, height * 0.5f, 0f),
                new Vector3(0.16f * scale, height * 0.5f, 0.16f * scale), null, bark);

            var builder = new MeshBuilder(true);
            Color[] colors = { GameConfig.Hex("9B4B2E"), GameConfig.Hex("B05B32"), GameConfig.Hex("7E3726"), GameConfig.Hex("C26F3E") };
            for (int i = 0; i < 3; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float r = (float)random.NextDouble() * 0.9f * scale;
                Vector3 center = new Vector3(Mathf.Cos(angle) * r, height * (0.9f + (float)random.NextDouble() * 0.35f), Mathf.Sin(angle) * r);
                builder.Rock(center, (1.5f + (float)random.NextDouble() * 0.7f) * scale, (1.5f + (float)random.NextDouble() * 0.7f) * scale, seed + i * 13, colors[i]);
            }
            Mesh mesh = builder.Build("maple canopy " + seed);
            var canopy = WorldFactory.MeshObject("canopy", root.transform, mesh, RuntimeAssets.VertexToon(0.015f, 0.3f), Vector3.zero);
            var sway = canopy.AddComponent<Sway>();
            sway.amplitude = 1.5f;
            sway.speed = 0.6f;
            sway.phase = seed * 0.2f;
        }

        public static void BuildBamboo(Transform parent, Vector3 position, int seed)
        {
            var random = new System.Random(seed);
            GameObject root = WorldFactory.Empty("Bamboo " + seed, parent, position, Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f));
            Material stalk = RuntimeAssets.Toon(GameConfig.Hex("5A6E48"), 0.003f);
            Material leaf = RuntimeAssets.Toon(GameConfig.Hex("485F3A"), 0.006f);
            int count = 5 + random.Next(3);
            for (int i = 0; i < count; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float r = (float)random.NextDouble() * 0.55f;
                float h = 3.3f + (float)random.NextDouble() * 2.2f;
                var stalkGo = WorldFactory.Primitive("stalk", root.transform, PrimitiveType.Cylinder,
                    new Vector3(Mathf.Cos(angle) * r, h * 0.5f, Mathf.Sin(angle) * r),
                    new Vector3(0.035f, h * 0.5f, 0.035f),
                    Quaternion.Euler((float)random.NextDouble() * 5f, 0f, (float)random.NextDouble() * 5f), stalk);
                for (int l = 0; l < 3; l++)
                {
                    float y = h * (0.55f + l * 0.15f);
                    var leaves = WorldFactory.Primitive("leaves", root.transform, PrimitiveType.Cube,
                        new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r),
                        new Vector3(0.55f, 0.035f, 0.2f),
                        Quaternion.Euler(0f, angle * Mathf.Rad2Deg + l * 47f, 18f), leaf);
                    leaves.transform.SetParent(stalkGo.transform, true);
                }
            }
            var sway = root.AddComponent<Sway>();
            sway.amplitude = 1.2f;
            sway.speed = 0.5f;
            sway.phase = seed * 0.31f;
        }

        public static void BuildGrassField(Transform parent)
        {
            var random = new System.Random(777);
            var builder = new MeshBuilder(true);
            for (int i = 0; i < 220; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float radius = 22f + (float)random.NextDouble() * 10f;
                Vector3 center = new Vector3(Mathf.Cos(angle) * radius, 0.01f, Mathf.Sin(angle) * radius);
                if (center.z > 18f && Mathf.Abs(center.x) < 12f) continue;
                if (center.z < -24f && Mathf.Abs(center.x) < 7f) continue;
                int blades = 3 + random.Next(3);
                for (int b = 0; b < blades; b++)
                {
                    float a = (float)random.NextDouble() * Mathf.PI * 2f;
                    float w = 0.035f;
                    float h = 0.35f + (float)random.NextDouble() * 0.55f;
                    Vector3 side = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * w;
                    Vector3 lean = new Vector3(Mathf.Cos(a + 1.7f), 0f, Mathf.Sin(a + 1.7f)) * 0.16f;
                    Color c = Color.Lerp(GameConfig.Hex("4F5E42"), GameConfig.Hex("76805A"), (float)random.NextDouble());
                    builder.FlatTriangle(center - side, center + side, center + Vector3.up * h + lean, c);
                }
            }
            Mesh mesh = builder.Build("grass tufts");
            var go = WorldFactory.MeshObject("grass", parent, mesh, RuntimeAssets.VertexToon(0f, 0.1f), Vector3.zero, null, null, false, false);
            go.isStatic = true;
        }

        public static void BuildBoulders(Transform parent)
        {
            var random = new System.Random(541);
            Color[] colors = { GameConfig.Hex("5D5A50"), GameConfig.Hex("6B665A"), GameConfig.Hex("4B4B45") };
            for (int i = 0; i < 16; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float radius = 23f + (float)random.NextDouble() * 9f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                if (pos.z > 20f && Mathf.Abs(pos.x) < 14f) continue;
                var builder = new MeshBuilder(true);
                float size = 0.7f + (float)random.NextDouble() * 1.5f;
                builder.Rock(Vector3.zero, size, size * (0.65f + (float)random.NextDouble() * 0.5f), i * 71, colors[i % colors.Length]);
                Mesh mesh = builder.Build("boulder " + i);
                WorldFactory.MeshObject("boulder", parent, mesh, RuntimeAssets.VertexToon(0.008f, 0.15f), pos,
                    Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f));
            }

            // Shore rocks at the foot of the cliff.
            for (int i = 0; i < 22; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float radius = 33f + (float)random.NextDouble() * 5f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, GameConfig.WaterHeight + 0.3f, Mathf.Sin(angle) * radius);
                var builder = new MeshBuilder(true);
                float size = 1.2f + (float)random.NextDouble() * 2.7f;
                builder.Rock(Vector3.zero, size, size * 0.9f, i * 31, colors[(i + 1) % colors.Length] * 0.85f);
                WorldFactory.MeshObject("shore rock", parent, builder.Build("shore rock"), RuntimeAssets.VertexToon(0.01f), pos,
                    Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), null, false, true);
            }
        }

        public static GameObject BuildBanner(WorldRefs world, Vector3 position, float yaw, string text, Color cloth, Color textColor, float height = 6.4f)
        {
            var root = WorldFactory.Empty("Banner", world.Props, position, Quaternion.Euler(0f, yaw, 0f));
            Material wood = RuntimeAssets.Toon(GameConfig.Hex("3A2A1D"), 0.004f);
            Material gold = RuntimeAssets.Toon(GameConfig.Gold, 0.004f);
            WorldFactory.Primitive("pole", root.transform, PrimitiveType.Cylinder, new Vector3(0f, height * 0.5f, 0f),
                new Vector3(0.075f, height * 0.5f, 0.075f), null, wood);
            WorldFactory.Primitive("crossbar", root.transform, PrimitiveType.Cylinder, new Vector3(0.55f, height - 0.2f, 0f),
                new Vector3(0.07f, 0.8f, 0.07f), Quaternion.Euler(0f, 0f, 90f), wood);
            WorldFactory.Primitive("finial", root.transform, PrimitiveType.Sphere, new Vector3(0f, height + 0.15f, 0f),
                Vector3.one * 0.16f, null, gold);

            GameObject clothRoot = WorldFactory.Empty("cloth root", root.transform, new Vector3(0.55f, height - 0.25f, 0f));
            var builder = new MeshBuilder(false);
            builder.Banner(1.35f, 3.6f, 3, 7, cloth, cloth * 0.82f, 0.07f);
            Mesh mesh = builder.Build("banner cloth");
            var clothGo = WorldFactory.MeshObject("cloth", clothRoot.transform, mesh, RuntimeAssets.Toon(cloth, cloth * 0.6f, 0f, true, true), Vector3.zero);
            var sway = clothRoot.AddComponent<Sway>();
            sway.amplitude = 1.5f;
            sway.speed = 0.6f;
            sway.phase = position.x * 0.4f + position.z * 0.2f;

            // gold trim
            WorldFactory.Primitive("trim", clothRoot.transform, PrimitiveType.Cube, new Vector3(0f, -3.55f, 0f),
                new Vector3(1.4f, 0.06f, 0.05f), null, gold);

            string vertical = Verticalize(text);
            WorldFactory.Text("banner text front", clothRoot.transform, vertical, new Vector3(0f, -1.8f, -0.06f),
                Quaternion.identity, 0.36f, textColor, RuntimeAssets.TitleFont, TextAnchor.MiddleCenter, TextAlignment.Center, 0);
            WorldFactory.Text("banner text back", clothRoot.transform, vertical, new Vector3(0f, -1.8f, 0.06f),
                Quaternion.Euler(0f, 180f, 0f), 0.36f, textColor, RuntimeAssets.TitleFont, TextAnchor.MiddleCenter, TextAlignment.Center, 0);

            return root;
        }

        public static string Verticalize(string text)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf('\n') >= 0)
                return text;
            return string.Join("\n", text.ToCharArray());
        }

        public static GameObject BuildBrazier(WorldRefs world, Vector3 position, int index)
        {
            var root = WorldFactory.Empty("Brazier " + index, world.Props, position);
            Material stone = RuntimeAssets.Toon(GameConfig.Hex("5E574C"), 0.006f);
            Material metal = RuntimeAssets.Toon(GameConfig.Hex("3E3730"), 0.004f);
            Material ember = RuntimeAssets.Toon(GameConfig.Hex("C05A2C"), GameConfig.Hex("3A1710"), 0f);
            WorldFactory.Primitive("base", root.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.12f, 0f),
                new Vector3(0.55f, 0.12f, 0.55f), null, stone);
            WorldFactory.Primitive("leg", root.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.65f, 0f),
                new Vector3(0.14f, 0.52f, 0.14f), null, metal);
            WorldFactory.Primitive("bowl", root.transform, PrimitiveType.Cylinder, new Vector3(0f, 1.18f, 0f),
                new Vector3(0.62f, 0.18f, 0.62f), null, metal);
            WorldFactory.Primitive("coals", root.transform, PrimitiveType.Sphere, new Vector3(0f, 1.22f, 0f),
                new Vector3(0.42f, 0.16f, 0.42f), null, ember);

            var lightGo = new GameObject("Fire light");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = GameConfig.Hex("FF9B52");
            light.intensity = 3.2f;
            light.range = 9f;
            light.shadows = LightShadows.None;
            var flicker = lightGo.AddComponent<FlickerLight>();
            flicker.baseIntensity = 3.2f;
            flicker.amplitude = 0.7f;
            flicker.speed = 7f;
            flicker.phase = index * 1.7f;
            world.Fires.Add(light);

            var brazier = root.AddComponent<BrazierFire>();
            brazier.FirePoint = new Vector3(0f, 1.35f, 0f);
            brazier.Light = light;
            return root;
        }

        public static GameObject BuildWatchtower(WorldRefs world, Vector3 position, float yaw, int index)
        {
            var root = WorldFactory.Empty("Watchtower " + index, world.Props, position, Quaternion.Euler(0f, yaw, 0f));
            Material wood = RuntimeAssets.Toon(GameConfig.Hex("4A3727"), 0.005f);
            Material darkWood = RuntimeAssets.Toon(GameConfig.Hex("33261C"), 0.005f);
            Material roofMat = RuntimeAssets.Toon(GameConfig.Hex("2C3033"), 0.008f);

            float legHeight = 4.6f;
            for (int i = 0; i < 4; i++)
            {
                float sx = (i % 2 == 0) ? -1f : 1f;
                float sz = (i < 2) ? -1f : 1f;
                WorldFactory.Primitive("leg", root.transform, PrimitiveType.Cylinder,
                    new Vector3(sx * 1.7f, legHeight * 0.5f, sz * 1.7f),
                    new Vector3(0.15f, legHeight * 0.5f, 0.15f), Quaternion.Euler(sz * 5f, 0f, -sx * 5f), wood);
            }
            WorldFactory.Primitive("platform", root.transform, PrimitiveType.Cube, new Vector3(0f, legHeight, 0f),
                new Vector3(4.2f, 0.3f, 4.2f), null, darkWood);
            for (int i = 0; i < 4; i++)
            {
                float sx = (i % 2 == 0) ? -1f : 1f;
                float sz = (i < 2) ? -1f : 1f;
                WorldFactory.Primitive("post", root.transform, PrimitiveType.Cube,
                    new Vector3(sx * 1.9f, legHeight + 1.5f, sz * 1.9f), new Vector3(0.18f, 3f, 0.18f), null, darkWood);
                WorldFactory.Primitive("rail", root.transform, PrimitiveType.Cube,
                    new Vector3(sx * 1.9f, legHeight + 1.3f, 0f), new Vector3(0.12f, 0.12f, 3.8f), null, wood);
                WorldFactory.Primitive("rail", root.transform, PrimitiveType.Cube,
                    new Vector3(0f, legHeight + 1.3f, sz * 1.9f), new Vector3(3.8f, 0.12f, 0.12f), null, wood);
            }
            var roofBuilder = new MeshBuilder(true);
            roofBuilder.Cone(new Vector3(0f, legHeight + 3.1f, 0f), 3.2f, 2.0f, 4, GameConfig.Hex("2B3033"), GameConfig.Hex("3A4043"), false);
            WorldFactory.MeshObject("roof", root.transform, roofBuilder.Build("tower roof"), roofMat, Vector3.zero);
            WorldFactory.Primitive("ladder", root.transform, PrimitiveType.Cube, new Vector3(0f, 2.2f, 2.15f),
                new Vector3(0.9f, 4.2f, 0.1f), Quaternion.Euler(76f, 0f, 0f), wood);

            // guard silhouette
            Material guard = RuntimeAssets.Toon(GameConfig.Hex("30383A"), 0.004f);
            WorldFactory.Primitive("guard", root.transform, PrimitiveType.Capsule, new Vector3(0.9f, legHeight + 1.3f, 0.2f),
                new Vector3(0.38f, 0.95f, 0.38f), null, guard);
            WorldFactory.Primitive("guard head", root.transform, PrimitiveType.Sphere, new Vector3(0.9f, legHeight + 2.25f, 0.2f),
                Vector3.one * 0.32f, null, guard);
            WorldFactory.Primitive("spear", root.transform, PrimitiveType.Cylinder, new Vector3(1.28f, legHeight + 1.5f, 0.2f),
                new Vector3(0.035f, 1.8f, 0.035f), null, wood);
            return root;
        }

        public static void BuildDistantScenery(WorldRefs world)
        {
            var random = new System.Random(2049);
            Color far = GameConfig.Hex("59636D");
            Color mid = GameConfig.Hex("424D56");
            Material farMat = RuntimeAssets.Toon(far, far * 0.65f, 0f);
            Material midMat = RuntimeAssets.Toon(mid, mid * 0.6f, 0f);

            for (int i = 0; i < 18; i++)
            {
                float angle = i / 18f * Mathf.PI * 2f + (float)random.NextDouble() * 0.15f;
                float radius = 78f + (float)random.NextDouble() * 120f;
                float height = 18f + (float)random.NextDouble() * 42f;
                float baseRadius = 10f + (float)random.NextDouble() * 19f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, GameConfig.WaterHeight, Mathf.Sin(angle) * radius);
                var builder = new MeshBuilder(true);
                builder.Cone(Vector3.zero, baseRadius, height, 7 + random.Next(3),
                    Color.Lerp(far, mid, (radius - 78f) / 120f), far, true);
                WorldFactory.MeshObject("distant peak", world.Environment, builder.Build("peak"), farMat, pos,
                    Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), null, false, false);
            }

            // Near peaks framing the island.
            Color near = GameConfig.Hex("34423F");
            Material nearMat = RuntimeAssets.Toon(near, near * 0.6f, 0f);
            for (int i = 0; i < 6; i++)
            {
                float angle = (i / 6f) * Mathf.PI * 2f + 0.4f;
                float radius = 52f + (float)random.NextDouble() * 18f;
                float height = 12f + (float)random.NextDouble() * 16f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, GameConfig.WaterHeight, Mathf.Sin(angle) * radius);
                var builder = new MeshBuilder(true);
                builder.Cone(Vector3.zero, 9f + (float)random.NextDouble() * 9f, height, 8, near, Color.Lerp(near, GameConfig.Hex("57645D"), 0.4f));
                WorldFactory.MeshObject("near peak", world.Environment, builder.Build("near peak"), nearMat, pos,
                    Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), null, false, false);
            }

            BuildPagoda(world, new Vector3(-63f, 8f, 96f), 1.2f);
            BuildDock(world);
            var birds = WorldFactory.Empty("Birds", world.Environment, new Vector3(0f, 24f, -40f));
            birds.AddComponent<BirdFlock>();
        }

        static void BuildPagoda(WorldRefs world, Vector3 position, float scale)
        {
            var root = WorldFactory.Empty("Distant pagoda", world.Environment, position, Quaternion.Euler(0f, 18f, 0f));
            Material dark = RuntimeAssets.Toon(GameConfig.Hex("2B2E30"), 0f);
            for (int i = 0; i < 5; i++)
            {
                float y = i * 3.2f;
                float width = 5.5f - i * 0.75f;
                WorldFactory.Primitive("body", root.transform, PrimitiveType.Cube, new Vector3(0f, y + 1.8f, 0f),
                    new Vector3(width * 0.55f, 1.6f, width * 0.55f), null, RuntimeAssets.Toon(GameConfig.Hex("6A4A35"), 0f));
                var roof = new MeshBuilder(true);
                roof.Cone(new Vector3(0f, y + 2.6f, 0f), width * 0.8f, 1.3f, 4, GameConfig.Hex("2B2E30"), GameConfig.Hex("3A4044"), false);
                WorldFactory.MeshObject("roof", root.transform, roof.Build("pagoda roof"), dark, Vector3.zero, Quaternion.Euler(0f, 45f, 0f), null, false, false);
            }
            var lantern = RuntimeAssets.Toon(GameConfig.Hex("E5A05A"), 0f);
            WorldFactory.Primitive("lantern", root.transform, PrimitiveType.Sphere, new Vector3(0f, 16.6f, 0f),
                Vector3.one * 0.55f, null, lantern, false, false);
        }

        static void BuildDock(WorldRefs world)
        {
            var root = WorldFactory.Empty("Dock", world.Props, new Vector3(-15f, GameConfig.WaterHeight + 0.5f, -35f), Quaternion.Euler(0f, 15f, 0f));
            Material wood = RuntimeAssets.Toon(GameConfig.Hex("4C3A2A"), 0.005f);
            for (int i = 0; i < 9; i++)
            {
                WorldFactory.Primitive("plank", root.transform, PrimitiveType.Cube, new Vector3(0f, 0f, i * 1.1f),
                    new Vector3(2.6f, 0.12f, 0.9f), null, wood);
                WorldFactory.Primitive("post", root.transform, PrimitiveType.Cylinder, new Vector3(-1.2f, -0.8f, i * 1.1f),
                    new Vector3(0.11f, 1.1f, 0.11f), null, wood);
                WorldFactory.Primitive("post", root.transform, PrimitiveType.Cylinder, new Vector3(1.2f, -0.8f, i * 1.1f),
                    new Vector3(0.11f, 1.1f, 0.11f), null, wood);
            }
            var boat = WorldFactory.Empty("Boat", root.transform, new Vector3(3.2f, 0.1f, 5.5f), Quaternion.Euler(0f, 28f, 0f));
            WorldFactory.Primitive("hull", boat.transform, PrimitiveType.Cube, Vector3.zero, new Vector3(1.4f, 0.55f, 3.6f),
                null, RuntimeAssets.Toon(GameConfig.Hex("3A2A20"), 0.005f));
            WorldFactory.Primitive("mast", boat.transform, PrimitiveType.Cylinder, new Vector3(0f, 2.2f, 0.4f),
                new Vector3(0.08f, 2.2f, 0.08f), null, wood);
            WorldFactory.Primitive("sail", boat.transform, PrimitiveType.Cube, new Vector3(0f, 2.7f, 0.4f),
                new Vector3(0.06f, 1.8f, 1.5f), null, RuntimeAssets.Toon(GameConfig.Hex("C9B590"), 0f));
            var lamp = WorldFactory.Primitive("lamp", boat.transform, PrimitiveType.Sphere, new Vector3(0f, 1.1f, 1.6f),
                Vector3.one * 0.25f, null, RuntimeAssets.Toon(GameConfig.Hex("FFB45C"), 0f, false));
            var sway = boat.AddComponent<Sway>();
            sway.amplitude = 1.1f;
            sway.speed = 0.5f;
            sway.swayX = true;
            sway.swayZ = true;
        }
    }
}
